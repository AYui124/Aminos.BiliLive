using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Aminos.BiliLive.Utils
{
    public class SignTool
    {
        // 与投稿工具相同
        public const string AppKey = "aae92bc66f3edfab";
        public const string AppSec = "af125a0d5279fd576c1b4418a3e8276d";

        public static Dictionary<string, string> Sign(Dictionary<string,string> requestform)
        {
            requestform.Add("appkey", AppKey);
            var sortedParams = requestform.OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);

            var query = string.Join("&", sortedParams.Select(pair =>
                $"{HttpUtility.UrlEncode(pair.Key)}={HttpUtility.UrlEncode(pair.Value)}"));

            // 计算签名（MD5）
            using var md5 = MD5.Create();
            var data = Encoding.UTF8.GetBytes(query + AppSec);
            var hashBytes = md5.ComputeHash(data);
            var sign = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            // 新增 sign 字段
            sortedParams["sign"] = sign;

            return sortedParams;
        }
    }
}
