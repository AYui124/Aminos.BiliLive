using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Models
{
    public class LiveArea
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public List<SubArea> Areas { get; set; } = new();
    }

    public class SubArea
    {
        public string Id { get; set; } = "";

        public string Name { get; set; } = "";

        public string ParentId { get; set; } = "";
    }
}
