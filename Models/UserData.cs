using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Models
{
    public class UserData
    {
        public Dictionary<string, string> Cookies { get; set; } = new();

        public string Id => GetUserId();

        public string Token { get; set; } = string.Empty;

        public string LiveRoomId { get; set; } = string.Empty;

        [JsonIgnore]
        public bool Validated { get; set; }

        public DateOnly LastRefreshDate { get; set; }

        public string Avatar { get; set; } = string.Empty;

        public static UserData Anonymous { get; } = new();

        public string GetUserId()
        {
            if (Cookies.TryGetValue("DedeUserID", out var id))
            {
                return id;
            }
            return string.Empty;
        }

        public string GetCsrfToken()
        {
            if (Cookies.TryGetValue("bili_jct", out var csrf))
            {
                return csrf;
            }
            return string.Empty;
        }
    }
}
