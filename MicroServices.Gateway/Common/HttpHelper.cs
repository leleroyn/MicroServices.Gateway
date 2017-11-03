using MicroServices.Gateway.Models;
using System.Net.Http;
using System.Threading.Tasks;

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