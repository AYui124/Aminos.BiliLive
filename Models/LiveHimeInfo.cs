using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Models
{
    public class LiveHimeInfo(string version, string build)
    {
        public string Version { get; set; } = version;

        public string Build { get; set; } = build;
    }
}
