using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using RestSharp;
using MicroServices.Gateway.Models;

namespace MicroServices.Gateway.Common
{
    public static class HttpClient
    {
        public static async Task<string> PostAsync(string url, string requestContent)
        {           
            RestRequest request = new RestRequest(Method.POST);
            request.AddBody(requestContent);
            var client = new RestClient(url)
            {
                Proxy = null,
                CookieContainer = null,
                FollowRedirects = false,
                Timeout = 60000
            };
            var respones = await client.ExecuteTaskAsync(request);
            return respones.Content;
        }       
    }
}