using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Extensions
{
    public static class TimeExtensions
    {
        public static long ToSecondTimeStamp(this DateTime time, TimeZoneInfo? timeZoneInfo = null)
        {
            var finalTImeZone = timeZoneInfo ?? TimeZoneInfo.Local;
            var dtStart = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1, 0, 0, 0), finalTImeZone);
            long timeStamp = Convert.ToInt32((time - dtStart).TotalSeconds);
            return timeStamp;
        }
    }
}
