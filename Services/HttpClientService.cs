using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Aminos.BiliLive.Models;

namespace Aminos.BiliLive.Services
{
    public class HttpClientService : ISingtonService
    {
        public async Task<HttpResponse> PostAsync(string url, string path, HttpContent? content, Dictionary<string,string>? cookies = null)
        {
            return await SendAsync(url, path, HttpMethod.Post, content, cookies: cookies);
        }

        public async Task<HttpResponse> GetAsync(string url, string path, Dictionary<string, string>? cookies = null)
        {
           return await SendAsync(url, path, HttpMethod.Get, cookies: cookies);
        }

        private async Task<HttpResponse> SendAsync(string url, string path, HttpMethod method, HttpContent? content = null, Dictionary<string, string>? cookies = null)
        {
            var baseAddress = new Uri(url);
            var cookieContainer = SetCookie(baseAddress, cookies);
            using var client = CreateClient(baseAddress, cookieContainer);
            var request = new HttpRequestMessage(method, path);
            if(content != null)
            {
                request.Content = content;
            }
            try
            {
                var response = await client.SendAsync(request);
                var result = new HttpResponse();
                result.StatusCode = response.StatusCode.GetHashCode();
                var buffer = await response.Content.ReadAsByteArrayAsync();
                result.Data = buffer;
                var newCookies = GetCookie(baseAddress, cookieContainer);
                foreach (var cookie in newCookies)
                {
                    result.Cookies.Add(cookie.Key, cookie.Value);
                }
                return result;
            }
            catch (Exception e)
            {
                return new HttpResponse { StatusCode = -1, WebException = e };
            }
        }

        private HttpClient CreateClient(Uri baseAddress, CookieContainer cookieContainer)
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = cookieContainer;
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            var client = new HttpClient(handler);
            client.BaseAddress = baseAddress;
            return client;
        }

        private CookieContainer SetCookie(Uri baseAddress, Dictionary<string, string>? cookies = null)
        {
            var cookieContainer = new CookieContainer();
            if (cookies != null)
            {
                foreach (var kv in cookies)
                {
                    var cookie = new Cookie(kv.Key, HttpUtility.UrlEncode(kv.Value));
                    cookieContainer.Add(baseAddress, cookie);
                }
            }
            return cookieContainer;
        }

        private Dictionary<string, string> GetCookie(Uri baseAddress, CookieContainer cookieContainer)
        {
            var httpCookies = cookieContainer.GetCookies(baseAddress);
            var result = new Dictionary<string, string>();
            foreach (var cookie in httpCookies.OrderBy(o => o.Expires))
            {
                result[cookie.Name] = HttpUtility.UrlDecode(cookie.Value);
            }
            return result;
        }
    }
}
