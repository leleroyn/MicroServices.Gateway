using MicroServices.Gateway.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MicroServices.Gateway.Common
{
    public static class HttpHelper
    {
        private static HttpClient client = new HttpClient();
        public static async Task<HttpResult> PostAsync(string url, string requestContent)
        {
            HttpResult result = new HttpResult();
            var resp =  await client.PostAsync(url, new FormUrlEncodedContent(GetFormDictionary(requestContent)));           
            result.Content = await resp.Content.ReadAsStringAsync();
            result.HttpStatus = (int)resp.StatusCode;
            return result;
        }

        public static Dictionary<string,string>  GetFormDictionary(string formStr)
        {            
            var formParms = formStr.Split('&');
            var result = new Dictionary<string, string>(formParms.Length);
            foreach (var p in formParms)
            {
                var parm = p.Split('=');
                result.Add(parm[0].Trim(), parm[1]);
            }
            return result;
        }
    }   
}