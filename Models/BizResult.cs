using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aminos.BiliLive.Models
{
    public class ResultBase
    {
        public int Code { get; set; }

        public bool Success { get; set; }

        public string Message { get; set; } = "";
    }

    public class BizResult : ResultBase
    {
        public static BizResult AsSuccess(int code = 200, string message = "")
        {
            return new BizResult { Success = true, Code = code, Message = message };
        }

        public static BizResult AsFail(int code = 500, string message = "失败")
        {
            return new BizResult { Success = false, Code = code, Message = message };
        }
    }

    public class BizResult<T> : ResultBase
    {
        public T? Data { get; set; }

        public static BizResult<T> AsSuccess(T data, int code = 200, string message = "")
        {
            return new BizResult<T> { Data = data, Success = true, Code = code, Message = message };
        }

        public static BizResult<T> AsFail(T? data = default, int code = 500, string message = "失败")
        {
            return new BizResult<T> { Data = data, Success = true, Code = code, Message = message };
        }
    }
}
