using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Models
{
    public class CookieRefreshInfo
    {
        public Dictionary<string, string> Cookies { get; set; } = new();

        public string NewToken { get; set; } = "";
    }
}
