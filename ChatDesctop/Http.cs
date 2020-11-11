using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Hepler
{
    public static class Http
    {
        private static readonly HttpClient _HttpClient;


        public static HttpClientHandler HttpClientHandler { get; private set; }

        static Http()
        {
            HttpClientHandler = new HttpClientHandler();
            _HttpClient = new HttpClient(HttpClientHandler);
        }


        public static async Task<HttpResponseMessage> SendAsync(string url, Dictionary<string, string> httpHeaders,
            string body, HttpMethod httpMethodType, CancellationToken cancellationToken)
        {
            _HttpClient?.DefaultRequestHeaders?.Clear();
            using HttpRequestMessage rq = new HttpRequestMessage();
            rq.Method = httpMethodType;
            if(!string.IsNullOrWhiteSpace(body))
                rq.Content = new StringContent(body, Encoding.UTF8, "application/json");
            rq.RequestUri = new Uri(url);
            if(httpHeaders != null) 
            {
                foreach (var headers in httpHeaders)
                {
                    rq.Headers.Add(headers.Key, headers.Value);
                }
            }
            _HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return await _HttpClient.SendAsync(rq);
        }


    }
}