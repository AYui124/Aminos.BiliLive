using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Models
{
    public class RtmpInfo
    {
        public string RtmpUrl { get; set; } = "";

        public string RtmpKey { get; set; } = "";

        public static RtmpInfo Default { get; } = new ();
    }
}
