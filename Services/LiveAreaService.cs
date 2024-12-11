using Aminos.BiliLive.Models;
using Aminos.BiliLive.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace Aminos.BiliLive.Services
{
    public class LiveAreaService : ISingtonService
    {
        private readonly List<LiveArea> _liveAreas = new ();
        private readonly string _configFileName =
            Path.Combine(PathTool.DocumentPath, "Aminos", "area.json");

        private readonly HttpClientService _httpClientService;

        public LiveAreaService(HttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }


        public async Task<BizResult<List<LiveArea>>> GetLiveAreaInfoAsync()
        {
            var response = await _httpClientService.GetAsync("https://api.live.bilibili.com", "/room/v1/Area/getList");
            
            if (!response.IsSuccess)
            {
                return BizResult<List<LiveArea>>.AsFail(code: response.StatusCode.GetHashCode(), message: "Http通讯失败");
            }
            var data = response.Content;
            if (string.IsNullOrEmpty(data))
            {
                return BizResult<List<LiveArea>>.AsFail(code: 204, message: "返回内容为空");
            }
            var obj = JsonSerializer.Deserialize<JsonObject>(data);
            var code = obj?["code"]?.GetValue<int>() ?? -1;
            var message = obj?["message"]?.GetValue<string>() ?? "";
            var list = obj?["data"]?.AsArray() ?? new JsonArray();
            var areas = new List<LiveArea>();
            foreach (var item in list)
            {
                var area = new LiveArea
                {
                    Id = item?["id"]?.GetValue<int>() ?? 0,
                    Name = item?["name"]?.GetValue<string>() ?? ""
                };
                var subList = item?["list"]?.AsArray() ?? new JsonArray();
                area.Areas = new List<SubArea>();
                foreach (var sub in subList)
                {
                    var subArea = new SubArea
                    {
                        Id = sub?["id"]?.GetValue<string>() ?? "",
                        ParentId = sub?["parent_id"]?.GetValue<string>() ?? "",
                        Name = sub?["name"]?.GetValue<string>() ?? ""
                    };
                    area.Areas.Add(subArea);
                }
                areas.Add(area);
            }
            return code != 0
                ? BizResult<List<LiveArea>>.AsFail(code: code, message: message)
                : BizResult<List<LiveArea>>.AsSuccess(areas);
        }

        public ImmutableList<LiveArea> GetAreas()
        {
            LiveArea[] res = new LiveArea[_liveAreas.Count];
            _liveAreas.CopyTo(res);
            return ImmutableList.Create(res);
        }

        public async Task RefreshAsync()
        {
            var result = await GetLiveAreaInfoAsync();
            if (result is { Success: true, Data.Count: > 0 })
            {
                _liveAreas.AddRange(result.Data);
                await SaveAsync();
            }
        }

        public async Task LoadAsync()
        {
            if (File.Exists(_configFileName))
            {
                await using var fs = File.OpenRead(_configFileName);
                var areas = await JsonSerializer.DeserializeAsync<List<LiveArea>>(fs);
                if (areas != null)
                {
                    _liveAreas.AddRange(areas);
                }
            }
            if (_liveAreas.Count <= 0)
            {
                await RefreshAsync();
            }
        }

        public async Task SaveAsync()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configFileName)!);
            await using var fs = File.Create(_configFileName);
            await JsonSerializer.SerializeAsync(fs, _liveAreas);
        }

        public async Task ClearAsync()
        {
            _liveAreas.Clear();
            await SaveAsync();
        }
    }
}
