using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Models
{
    public enum LoginState
    {
        Preparing,
        QrImage,
        Success,
        Error = -1
    }
}
