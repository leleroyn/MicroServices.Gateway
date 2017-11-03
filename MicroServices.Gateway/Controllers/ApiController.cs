using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MicroServices.Gateway.Common;
using MicroServices.Gateway.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using System.Web;
using System.IO;

namespace MicroServices.Gateway.Controllers
{
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly AppConfig appConfig;
        private IMemoryCache cache;
        private IHostingEnvironment env;

        private RequestHead HeadData;
        private CustomRouteData OptimalRoute;
        private bool FromCache;
        private string RequestContent;

        public ApiController(IOptions<AppConfig> optionsAccessor, IMemoryCache cacheProvider, IHostingEnvironment hostingEnvironment)
        {
            env = hostingEnvironment;
            cache = cacheProvider;
            appConfig = optionsAccessor.Value;
        }

        #region 对外接口
        [HttpGet]
        public string Get()
        {
            return string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), " Service is Online.");
        }

        [HttpPost]
        public async Task<string> Post()
        {
            GetRequestData(Request);

            var requestResult = await HandleRequest(OptimalRoute);
            if (requestResult.HttpStatus != 200)
            {
                Response.StatusCode = requestResult.HttpStatus;
            }
            return requestResult.Content;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 获取请求信息
        /// </summary>
        void GetRequestData(HttpRequest request)
        {
            RouteHelper routeHelper = new RouteHelper(env.ContentRootPath, cache);

            string reqContent = "";
            using (var buffer = new MemoryStream())
            {
                Request.Body.CopyTo(buffer);
                reqContent = Encoding.UTF8.GetString(buffer.ToArray());
            }

            reqContent = HttpUtility.UrlDecode(reqContent);
            var base64Bits = Convert.FromBase64String(reqContent);
            RequestContent = Encoding.UTF8.GetString(base64Bits);

            var requestObj = JsonConvert.DeserializeObject<dynamic>(RequestContent);
            var requestHeadObj = JsonConvert.DeserializeObject<dynamic>(Convert.ToString(requestObj.RequestHead));

            HeadData = new RequestHead { BusinessCode = requestHeadObj.BusinessCode, Expire = requestHeadObj.Expire, RequestFrom = requestHeadObj.RequestFrom, RequestSN = requestHeadObj.RequestSN, RequestTime = requestHeadObj.RequestTime, Version = requestHeadObj.Version };

            CustomRouteData route = routeHelper.GetOptimalRoute(HeadData.BusinessCode, HeadData.Version, HeadData.RequestFrom);
            if (route == null)
                throw new Exception("请求路由不存在");
            OptimalRoute = routeHelper.RoutingLoadBalance(route);
        }

        /// <summary>
        /// 生成缓存Key
        /// </summary>    
        string GeneralCacheKey()
        {
            string key = string.Join("_", HeadData.RequestFrom, HeadData.BusinessCode, HeadData.Version);
            var requestObj = JsonConvert.DeserializeObject<dynamic>(RequestContent);
            string requestBodyStr = Convert.ToString(requestObj.RequestBody);
            if (!string.IsNullOrWhiteSpace(requestBodyStr))
            {
                var requestBodyObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBodyStr).OrderBy(o => o.Key);
                foreach (var p in requestBodyObj)
                {
                    key = string.Join("_", key, string.Join("_", p.Key, p.Value));
                }
            }
            return key.ToLower();
        }

        async Task<HttpResult> HandleRequest(CustomRouteData route)
        {
            HttpResult result = new HttpResult();
            int expire = HeadData.Expire.GetValueOrDefault();

            if (expire > 0)
            {
                string key = GeneralCacheKey();
                var cacheValue = cache.Get(key);
                if (cacheValue != null)
                {
                    result.Content = cacheValue.ToString();
                    result.HttpStatus = 200;
                    FromCache = true;
                }
                else
                {
                    result = await HttpHelper.PostAsync(route.Handle, RequestContent);
                    if (result.HttpStatus == 200)
                    {
                        cache.Set(key, result.Content, TimeSpan.FromSeconds(expire));
                    }
                    FromCache = false;
                }
            }
            else
            {
                result = await HttpHelper.PostAsync(route.Handle, RequestContent);
                FromCache = false;
            }
            return result;
        }

        #endregion

    }
}
