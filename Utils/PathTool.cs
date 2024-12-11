using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Utils
{
    internal static class PathTool
    {
        public static string DocumentPath => GetDocumentsPath();

        private static string GetDocumentsPath()
        {
            var homePath = Environment.GetEnvironmentVariable("HOME");
            return Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                PlatformID.Unix or PlatformID.MacOSX =>
                    // 处理 HOME 环境变量可能为空的情况
                    homePath != null && !string.IsNullOrEmpty(homePath)
                        ? Path.Combine(homePath, "Documents")
                        : AppContext.BaseDirectory
                ,
                _ => throw new PlatformNotSupportedException("不支持的操作系统平台。")
            };
        }
    }
}
