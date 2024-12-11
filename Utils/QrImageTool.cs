using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing.QrCode;
using ZXing;
using System.Drawing;

namespace Aminos.BiliLive.Utils
{
    internal static class QrImageTool
    {
        public static byte[] GenerateQrImage(string content, int size = 200)
        {
            var writer = new ZXing.SkiaSharp.BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
            };
            QrCodeEncodingOptions options = new()
            {
                //禁用ECI，UTF-8需为true
                DisableECI = true,
                //内容编码格式  
                CharacterSet = "UTF-8",
                //二维码的宽高  
                Width = size,
                Height = size,
                //二维码边距  
                Margin = 2,
                //版本
                QrVersion = 10,
                //错误修正率
                ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.Q,
            };
            writer.Options = options;
            //导出图片  
            using var image = writer.Write(content);
            
            using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 90);
            using var  ms = new MemoryStream();
            data.SaveTo(ms);
            return ms.ToArray();
        }
    }
}
