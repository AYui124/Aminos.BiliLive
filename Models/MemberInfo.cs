using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Models
{
    public class MemberInfo
    {
        public string Nickname { get; set; } = "";

        public string Uid { get; set; } = "";

        public string FaceUrl { get; set; } = "";

        public string CurrentLevel { get; set; } = "";

        public static MemberInfo Anonymous { get; } = new();
    }
}
