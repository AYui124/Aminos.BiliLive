using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Models
{
    public class LivingInfo
    {
        public int LastLiveArea { get; set; }

        public int ParentArea { get; set; }

        public string RoomName { get; set; } = string.Empty;

        public bool IsLiving { get; set; } = false;

        public static LivingInfo Default { get; } = new ();
    }
}
