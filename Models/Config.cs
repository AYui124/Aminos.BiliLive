using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Models
{
    internal class Config
    {
        public int BaseTheme { get; set; }

        public string ThemeColor { get; set; } = "Red";

        public static Config Default { get; } = new() { BaseTheme = 1, ThemeColor = "Red" };
    }
}
