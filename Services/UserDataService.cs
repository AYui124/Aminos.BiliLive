using Aminos.BiliLive.Models;
using Aminos.BiliLive.Utils;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;

namespace Aminos.BiliLive.Services
{
    public class UserDataService : ISingtonService
    {
        private UserData _user = UserData.Anonymous;
        private readonly string _configFileName =
            Path.Combine(PathTool.DocumentPath, "Aminos", "user.json");

        private readonly HttpClientService _httpClientService;

        public UserDataService(HttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        public string GetUserId()
        {
            return _user.Id;
        }

        public string GetRoomId()
        {
            return _user.LiveRoomId;
        }

        public string GetCsrfToken()
        {
            return _user.GetCsrfToken();
        }

        public Dictionary<string, string> GetCookies()
        {
            var dict = new Dictionary<string, string>();
            foreach (var item in _user.Cookies)
            {
                dict.Add(item.Key, item.Value);
            }
            return dict;
        }

        public byte[]? GetCachedAvatar()
        {
            if (string.IsNullOrEmpty(_user.Avatar))
            {
                return null;
            }
            return Convert.FromBase64String(_user.Avatar);
        }

        public bool HasLoginBefore()
        {
            return !string.IsNullOrEmpty(_user.Id);
        }

        public bool IsValidUser()
        {
            return !string.IsNullOrEmpty(_user.Id) && _user.Validated;
        }

        public bool HasCheckedRecently()
        {
            return _user.LastRefreshDate == DateOnly.FromDateTime(DateTime.Today);
        }

        public async Task<BizResult<QrLoginInfo>> GetQrLoginAsync()
        {
            var response = await _httpClientService.GetAsync("https://passport.bilibili.com", "/x/passport-login/web/qrcode/generate");
            if (!response.IsSuccess)
            {
                return BizResult<QrLoginInfo>.AsFail(code: response.StatusCode.GetHashCode(), message: "Http通讯失败");
            }
            var data = response.Content;
            if (string.IsNullOrEmpty(data))
            {
                return BizResult<QrLoginInfo>.AsFail(code: 204, message: "返回内容为空");
            }
            var obj = JsonSerializer.Deserialize<JsonObject>(data);
            var code = obj?["code"]?.GetValue<int>() ?? -1;
            var message = obj?["message"]?.GetValue<string>() ?? "";
            var url = obj?["data"]?["url"]?.GetValue<string>() ?? "";
            var key = obj?["data"]?["qrcode_key"]?.GetValue<string>() ?? "";
            return code != 0
                ? BizResult<QrLoginInfo>.AsFail(code: code, message: message)
                : BizResult<QrLoginInfo>.AsSuccess(new QrLoginInfo { QrKey = key, Url = url });
        }

        public async Task<BizResult<QrLoginStatus>> GetQrStatusAsync(string qrKey)
        {
            var response = await _httpClientService.GetAsync("https://passport.bilibili.com", "/x/passport-login/web/qrcode/poll?qrcode_key=" + qrKey);

            if (!response.IsSuccess)
            {
                return BizResult<QrLoginStatus>.AsFail(code: response.StatusCode.GetHashCode(), message: "Http通讯失败");
            }
            var data = response.Content;
            if (string.IsNullOrEmpty(data))
            {
                return BizResult<QrLoginStatus>.AsFail(code: 204, message: "返回内容为空");
            }
            var obj = JsonSerializer.Deserialize<JsonObject>(data);
            var code = obj?["code"]?.GetValue<int>() ?? -1;
            var message = obj?["message"]?.GetValue<string>() ?? "";
            if (code != 0)
            {
                return BizResult<QrLoginStatus>.AsFail(code: code, message: message);
            }
            var stateCode = obj?["data"]?["code"]?.GetValue<int>() ?? -1;

            var token = obj?["data"]?["refresh_token"]?.GetValue<string>() ?? "";
            var url = obj?["data"]?["url"]?.GetValue<string>() ?? "";
            var status = new QrLoginStatus
            {
                SsoUrl = url,
                Token = token
            };
           
            status.SetStatus(stateCode);
            if (status.Status == QrStatus.Success)
            {
                var newCookies = response.Cookies;
                foreach (var cookie in newCookies)
                {
                    status.Cookies[cookie.Key] = cookie.Value;
                }
            }
            return BizResult<QrLoginStatus>.AsSuccess(status);
        }

        public async Task<BizResult> RefreshCookiesAsync()
        {
            var checkResult = await CheckNeedRefreshAsync();
            if (!checkResult.Success || checkResult.Data == null)
            {
                return BizResult.AsFail();
            }
            if (!checkResult.Data.Refresh)
            {
                // 不需要刷新，直接返回
                SetUserValidated();
                await UpdateLastDateAsync();
                return BizResult.AsSuccess();
            }
            // 获取新csrf
            var csrfResult = await GetRefreshCsrfAsync(checkResult.Data.Timestamp);
            if (!csrfResult.Success || string.IsNullOrEmpty(csrfResult.Data))
            {
                return BizResult.AsFail();
            }
            // 获取新token和新cookies
            var refreshResult = await PostCookieRefreshAsync(csrfResult.Data);
            if (!refreshResult.Success || refreshResult.Data == null)
            {
                return BizResult.AsFail();
            }
            // 确认使原token和cookies作废
            var confirmResult = await ComfirmCookieRefreshAsync(refreshResult.Data.Cookies);
            if (!confirmResult.Success)
            {
                return BizResult.AsFail();
            }
            // 保存新token和新cookies
            await UpdateRefreshAsync(refreshResult.Data);
            return BizResult.AsSuccess();
        }
        private async Task<BizResult<TokenRefreshInfo>> CheckNeedRefreshAsync()
        {
            var csrf = GetCsrfToken();
            var cookies = GetCookies();
            var response = await _httpClientService.GetAsync("https://passport.bilibili.com", "/x/passport-login/web/cookie/info?csrf=" + csrf, cookies);
            
            if (!response.IsSuccess)
            {
                return BizResult<TokenRefreshInfo>.AsFail(code: response.StatusCode.GetHashCode(), message: "Http通讯失败");
            }
            var data = response.Content;
            if (string.IsNullOrEmpty(data))
            {
                return BizResult<TokenRefreshInfo>.AsFail(code: 204, message: "返回内容为空");
            }
            var obj = JsonSerializer.Deserialize<JsonObject>(data);
            var code = obj?["code"]?.GetValue<int>() ?? -1;
            var message = obj?["message"]?.GetValue<string>() ?? "";
            var refresh = obj?["data"]?["refresh"]?.GetValue<bool>() ?? false;
            var timestamp = obj?["data"]?["timestamp"]?.GetValue<long>() ?? 0L;
            return code != 0 || (refresh == false && timestamp == 0)
                ? BizResult<TokenRefreshInfo>.AsFail(code: code, message: message)
                : BizResult<TokenRefreshInfo>.AsSuccess(new TokenRefreshInfo { Refresh = refresh, Timestamp = timestamp });
        }

        private async Task<BizResult<string>> GetRefreshCsrfAsync(long timestamp)
        {
            var correspondPath = RsaTool.Encrypt($"refresh_{timestamp}");
            var cookies = GetCookies();
            var response = await _httpClientService.GetAsync("https://www.bilibili.com", "/correspond/1/" + correspondPath, cookies);

            if (!response.IsSuccess)
            {
                return BizResult<string>.AsFail(code: response.StatusCode.GetHashCode(), message: "Http通讯失败");
            }
            var html = response.Content;
            if (string.IsNullOrEmpty(html))
            {
                return BizResult<string>.AsFail(code: 204, message: "返回内容为空");
            }

            // 解析html
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var node = doc.GetElementbyId("1-name");
            return node == null
                ? BizResult<string>.AsFail("无法获取")
                : BizResult<string>.AsSuccess(node.InnerText);
        }

        private async Task<BizResult<CookieRefreshInfo>> PostCookieRefreshAsync(string refreshCsrf)
        {
            var csrf = GetCsrfToken();
            var token = _user.Token;
            var cookies = GetCookies();
            var requestform = new Dictionary<string, string>
            {
                ["csrf"] = csrf,
                ["refresh_csrf"] = refreshCsrf,
                ["source"] = "main_web",
                ["refresh_token"] = token
            };
            var response = await _httpClientService.PostAsync("https://passport.bilibili.com", "/x/passport-login/web/cookie/refresh", new FormUrlEncodedContent(requestform), cookies);

            if (!response.IsSuccess)
            {
                return BizResult<CookieRefreshInfo>.AsFail(code: response.StatusCode.GetHashCode(), message: "Http通讯失败");
            }
            var data = response.Content;
            if (string.IsNullOrEmpty(data))
            {
                return BizResult<CookieRefreshInfo>.AsFail(code: 204, message: "返回内容为空");
            }
            var obj = JsonSerializer.Deserialize<JsonObject>(data);
            var code = obj?["code"]?.GetValue<int>() ?? -1;
            var message = obj?["message"]?.GetValue<string>() ?? "";
            if (code != 0)
            {
                return BizResult<CookieRefreshInfo>.AsFail(code: code, message: message);
            }
            var newCookies = new Dictionary<string, string>();
            foreach (var cookie in response.Cookies)
            {
                newCookies[cookie.Key] = cookie.Value;
            }
            var newToken = obj?["data"]?["refresh_token"]?.GetValue<string>() ?? "";
            return string.IsNullOrEmpty(newToken)
                ? BizResult<CookieRefreshInfo>.AsFail(code: code, message: message)
                : BizResult<CookieRefreshInfo>.AsSuccess(new CookieRefreshInfo { Cookies = newCookies, NewToken = newToken });

        }

        private async Task<BizResult> ComfirmCookieRefreshAsync(Dictionary<string,string> newCookies)
        {
            var oldToken = _user.Token;
            var csrf = newCookies["bili_jct"];
            var requestform = new Dictionary<string, string>
            {
                ["csrf"] = csrf,
                ["refresh_token"] = oldToken
            };
            var response = await _httpClientService.PostAsync("https://passport.bilibili.com", "/x/passport-login/web/confirm/refresh", new FormUrlEncodedContent(requestform), newCookies);

            if (!response.IsSuccess)
            {
                return BizResult.AsFail(code: response.StatusCode.GetHashCode(), message: "Http通讯失败");
            }
            var data = response.Content;
            if (string.IsNullOrEmpty(data))
            {
                return BizResult.AsFail(code: 204, message: "返回内容为空");
            }
            var obj = JsonSerializer.Deserialize<JsonObject>(data);
            var code = obj?["code"]?.GetValue<int>() ?? -1;
            var message = obj?["message"]?.GetValue<string>() ?? "";
            return code != 0
                ? BizResult.AsFail(code: code, message: message)
                : BizResult.AsSuccess();
        }

        public async Task<BizResult<MemberInfo>> GetMemberInfoAsync()
        {
            var cookies = GetCookies();
            var response = await _httpClientService.GetAsync("https://api.bilibili.com", "/x/web-interface/nav", cookies);

            if (!response.IsSuccess)
            {
                return BizResult<MemberInfo>.AsFail(code: response.StatusCode.GetHashCode(), message: "Http通讯失败");
            }
            var data = response.Content;
            if (string.IsNullOrEmpty(data))
            {
                return BizResult<MemberInfo>.AsFail(code: 204, message: "返回内容为空");
            }
            var obj = JsonSerializer.Deserialize<JsonObject>(data);
            var code = obj?["code"]?.GetValue<int>() ?? -1;
            var message = obj?["message"]?.GetValue<string>() ?? "";
            var hasLogin = obj?["data"]?["isLogin"]?.GetValue<bool>() ?? false;
            var nickname = obj?["data"]?["uname"]?.GetValue<string>() ?? "";
            var face = obj?["data"]?["face"]?.GetValue<string>() ?? "";
            var uid = obj?["data"]?["mid"]?.GetValue<int>() ?? 0;
            var level = obj?["data"]?["level_info"]?["current_level"]?.GetValue<int>() ?? 0;
            return code != 0 || !hasLogin
                ? BizResult<MemberInfo>.AsFail(code: code, message: message)
                : BizResult<MemberInfo>.AsSuccess(new MemberInfo 
                {
                    Uid = uid.ToString(),
                    Nickname = nickname,
                    FaceUrl = face,
                    CurrentLevel = level.ToString(),
                });
        }

        

        public async Task<BizResult<byte[]>> GetAvatarAsync(string url)
        {
            var uri = new Uri(url);
            var baseUrl = $"{uri.Scheme}://{uri.Host}";
            var path = uri.PathAndQuery;
            var response = await _httpClientService.GetAsync(baseUrl, path);

            if (!response.IsSuccess)
            {
                return BizResult<byte[]>.AsFail(code: response.StatusCode.GetHashCode(), message: "Http通讯失败");
            }
            var data = response.Data;
            return data is { Length: > 0 } 
                ? BizResult<byte[]>.AsSuccess(data) 
                : BizResult<byte[]>.AsFail(code: 500, message: "数据为空");
        }

        public async Task LoadAsync()
        {
            if (!File.Exists(_configFileName))
            {
                return;
            }
            await using var fs = File.OpenRead(_configFileName);
            var user = await JsonSerializer.DeserializeAsync<UserData>(fs);
            if (user != null)
            {
                _user = user;
            }
        }

        public async Task SaveAsync()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configFileName)!);
            await using var fs = File.Create(_configFileName);
            await JsonSerializer.SerializeAsync(fs, _user);
        }

        public async Task BuildUserInfoAsync(QrLoginStatus data)
        {
            _user = new UserData
            {
                Token = data.Token,
                LastRefreshDate = DateOnly.FromDateTime(DateTime.Now),
                Validated = true    // 新登录默认已验证通过
            };

            #region 旧代码 从url获取cookies
            //var uri = new Uri(data.SsoUrl);
            //NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
            //var keys = query.AllKeys;
            //foreach (var key in keys)
            //{
            //    if (!string.IsNullOrEmpty(key))
            //    {
            //        var value = query[key] ?? "";
            //        _user.Cookies.TryAdd(key, value);
            //    }
            //}
            #endregion

            foreach (var kv in data.Cookies)
            {
                _user.Cookies[kv.Key] = kv.Value;
            }
            await SaveAsync();
        }

        public async Task SaveRoomIdAsync(string roomId)
        {
            _user.LiveRoomId = roomId;
            await SaveAsync();
        }

        public async Task UpdateLastDateAsync()
        {
            _user.LastRefreshDate = DateOnly.FromDateTime(DateTime.Now);
            await SaveAsync();
        }

        public async Task ClearAsync()
        {
            _user = UserData.Anonymous;
            await SaveAsync();
        }

        public void SetUserValidated()
        {
            _user.Validated = true;
        }

        public async Task UpdateRefreshAsync(CookieRefreshInfo info)
        {
            _user.Validated = true;
            _user.Token = info.NewToken;
            foreach (var kv in info.Cookies) 
            {
                _user.Cookies[kv.Key] = kv.Value;
            }
            _user.LastRefreshDate = DateOnly.FromDateTime(DateTime.Now);
            await SaveAsync();
        }

        public async Task SaveAvatarAsync(byte[] faceData)
        {
            if (faceData is { Length: > 0 })
            {
                _user.Avatar = Convert.ToBase64String(faceData);
                await SaveAsync();
            }
        }
    }
}
