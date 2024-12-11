using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Utils
{
    public class RsaTool
    {
        private const string PublicKey = """
            -----BEGIN PUBLIC KEY----- 
            MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDLgd2OAkcGVtoE3ThUREbio0Eg
            Uc/prcajMKXvkCKFCWhJYJcLkcM2DKKcSeFpD/j6Boy538YXnR6VhcuUJOhH2x71
            nzPjfdTcqMz7djHum0qSZA0AyCBDABUqCrfNgCiJ00Ra7GmRj+YCK1NJEuewlb40
            JNrRuoEUXpabUzGB8QIDAQAB
            -----END PUBLIC KEY-----
            """;

        public static string Encrypt(string content)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(PublicKey);
            var bytes = Encoding.UTF8.GetBytes(content);
            var buffer = rsa.Encrypt(bytes, RSAEncryptionPadding.OaepSHA256);
            return ToHexString(buffer);
        }

        public static string ToHexString(byte[] bytes)
        {
            return string.Join("", bytes.Select(b => b.ToString("x2")));
        }
    }
}
