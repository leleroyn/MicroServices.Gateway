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
using System.Net.Http;

namespace MicroServices.Gateway.Common
{
    public static class HttpHelper
    {
        private static HttpClient client = new HttpClient();
        public static async Task<HttpResult> PostAsync(string url, string requestContent)
        {
            HttpResult result = new HttpResult();
            var resp =  await client.PostAsync(url, new StringContent(requestContent));           
            result.Content = await resp.Content.ReadAsStringAsync();
            result.HttpStatus = (int)resp.StatusCode;
            return result;
        }
    }   
}