using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Models
{
    public class HttpResponse
    {
        public byte[]? Data { get; set; }

        public string Content => Data != null ? Encoding.UTF8.GetString(Data) : "";

        public int StatusCode { get; set; }

        public bool IsSuccess => StatusCode is >= 200 and <= 299 && WebException == null;

        public Exception? WebException { get; set; }

        public Dictionary<string, string> Cookies { get; set; } = new();
    }
}
