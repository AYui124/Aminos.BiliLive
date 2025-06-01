using Aminos.BiliLive.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using System.Dynamic;
using System.Net;
using System.Web;
using ZXing.Aztec.Internal;

namespace Aminos.BiliLive.Services
{
    public class ManageLiveService : ISingtonService
    {
        private readonly UserDataService _userDataService;
        private readonly HttpClientService _httpClientService;

        public ManageLiveService(
            UserDataService userDataService,
            HttpClientService httpClientService)
        {
            _userDataService = userDataService;
            _httpClientService = httpClientService;
        }

        public async Task<BizResult<string>> GetRoomIdAsync(string userId)
        {
            var response = await _httpClientService.GetAsync("https://api.live.bilibili.com", "/live_user/v1/Master/info?uid="+ userId);
            if (!response.IsSuccess)
            {
                return BizResult<string>.AsFail(code: response.StatusCode.GetHashCode(), message: "Http通讯失败");
            }
            var data = response.Content;
            if (string.IsNullOrEmpty(data))
            {
                return BizResult<string>.AsFail(code: 204, message: "返回内容为空");
            }
            var obj = JsonSerializer.Deserialize<JsonObject>(data);
            var code = obj?["code"]?.GetValue<int>() ?? -1;
            var message = obj?["message"]?.GetValue<string>() ?? "";
            var roomId = obj?["data"]?["room_id"]?.GetValue<int>() ?? 0;
            return code != 0
                ? BizResult<string>.AsFail(code: code, message: message)
                : BizResult<string>.AsSuccess(roomId.ToString());
        }
        
        public async Task<BizResult<LivingInfo>> GetStatusAsync(string userId)
        {
            var response = await _httpClientService.GetAsync("https://api.live.bilibili.com", "/room/v1/Room/get_status_info_by_uids?uids[]=" + userId);
            if (!response.IsSuccess)
            {
                return BizResult<LivingInfo>.AsFail(code: response.StatusCode.GetHashCode(), message: "Http通讯失败");
            }
            var data = response.Content;
            if (string.IsNullOrEmpty(data))
            {
                return BizResult<LivingInfo>.AsFail(code: 204, message: "返回内容为空");
            }
            var obj = JsonSerializer.Deserialize<JsonObject>(data);
            var code = obj?["code"]?.GetValue<int>() ?? -1;
            var message = obj?["message"]?.GetValue<string>() ?? "";
            var status = obj?["data"]?[userId]?["live_status"]?.GetValue<int>() ?? 0;
            var area = obj?["data"]?[userId]?["area_v2_id"]?.GetValue<int>() ?? 0;
            var parentArea = obj?["data"]?[userId]?["area_v2_parent_id"]?.GetValue<int>() ?? 0;
            var title = obj?["data"]?[userId]?["title"]?.GetValue<string>() ?? "";
            var livingInfo = new LivingInfo() 
            { 
                IsLiving = status == 1, 
                LastLiveArea = area,
                ParentArea = parentArea,
                RoomName = title,
            };
            return code != 0
                ? BizResult<LivingInfo>.AsFail(code: code, message: message)
                : BizResult<LivingInfo>.AsSuccess(livingInfo);
        }

        public async Task<BizResult> SetRoomTitleAsync(string title)
        {
            var roomId = _userDataService.GetRoomId();
            var cookies = _userDataService.GetCookies();
            var csrf = _userDataService.GetCsrfToken();
            var requestform = new Dictionary<string, string>
            {
                ["room_id"] = roomId,
                ["title"] = title,
                ["csrf_token"] = csrf,
                ["csrf"] = csrf,
                ["visit_id"] = ""
            };
            var content = new FormUrlEncodedContent(requestform);
            var response = await _httpClientService.PostAsync("https://api.live.bilibili.com", "/room/v1/Room/update", content, cookies);
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
            return code == 0 
                ? BizResult.AsSuccess() 
                : BizResult.AsFail(code: code, message: message);
        }

        public async Task<BizResult<RtmpInfo>> StartLiveAsync(string area)
        {
            var path = "/room/v1/Room/startLive";
            return await PostLiveAsync(path, area);
        }

        public async Task<BizResult<RtmpInfo>> StopLiveAsync()
        {
            var path = "/room/v1/Room/stopLive";
            return await PostLiveAsync(path);
        }

        private async Task<BizResult<RtmpInfo>> PostLiveAsync(string path, string area = "")
        {
            var roomId = _userDataService.GetRoomId();
            var cookies = _userDataService.GetCookies();
            var csrf = _userDataService.GetCsrfToken();

            var requestform = new Dictionary<string, string>
            {
                ["room_id"] = roomId,
                ["platform"] = "pc_link",
                ["csrf_token"] = csrf,
                ["csrf"] = csrf,
                ["visit_id"] = ""
            };
            if (!string.IsNullOrEmpty(area))
            {
                requestform["area_v2"] = area;
            }
            var content = new FormUrlEncodedContent(requestform);
            var response = await _httpClientService.PostAsync("https://api.live.bilibili.com", path, content, cookies);

            if (!response.IsSuccess)
            {
                return BizResult<RtmpInfo>.AsFail(code: response.StatusCode.GetHashCode(), message: "Http通讯失败");
            }
            var data = response.Content;
            if (string.IsNullOrEmpty(data))
            {
                return BizResult<RtmpInfo>.AsFail(code: 204, message: "返回内容为空");
            }
            var obj = JsonSerializer.Deserialize<JsonObject>(data);
            var code = obj?["code"]?.GetValue<int>() ?? -1;
            var message = obj?["message"]?.GetValue<string>() ?? "";
            if (code != 0)
            {
                return BizResult<RtmpInfo>.AsFail(code: code, message: message);
            }
            var url = obj?["data"]?["rtmp"]?["addr"]?.GetValue<string>() ?? "";
            var key = obj?["data"]?["rtmp"]?["code"]?.GetValue<string>() ?? "";

            var info = new RtmpInfo
            {
                RtmpUrl = url,
                RtmpKey = key.Replace(@"\u0026", "&")
            };

            return BizResult<RtmpInfo>.AsSuccess(info);
        }
    }
}
