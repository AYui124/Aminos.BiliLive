using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Models
{
    public class QrLoginStatus
    {
        public QrStatus Status { get; set; }

        public string Token { get; set; } = "";

        public string SsoUrl { get; set; } = "";

        public Dictionary<string, string> Cookies { get; set; } = new();

        public void SetStatus(int code)
        {
            Status = code switch
            {
                86101 => QrStatus.NotScanned,
                86090 => QrStatus.Scanned,
                0 => QrStatus.Success,
                _ => QrStatus.OutofDate
            };
        }
    }

    public enum QrStatus
    {
        NotScanned,
        Scanned,
        Success,
        OutofDate,
    }
}
