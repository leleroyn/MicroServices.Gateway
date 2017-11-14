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
using Polly;
using Microsoft.Extensions.Logging;

namespace MicroServices.Gateway.Controllers
{
    [Route("api")]
    public class ApiController : Controller
    {
        private IMemoryCache cache;
        private IHostingEnvironment env;

        private RequestHead requestHead;
        private CustomRouteData optimalRoute;
        private bool fromCache;
        private string requestBody;
        private string authorizationHeadValue;
        private RouteHelper routeHelper;
        private ILogger logger;

        public ApiController(IMemoryCache cacheProvider, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<ApiController>();            
            env = hostingEnvironment;
            cache = cacheProvider;
            routeHelper = new RouteHelper(env.ContentRootPath);
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
            var routeSetting = await GetRequestData(Request);
            var requestResult = new HttpResult();
            if (routeSetting.RetryTimes > 0)
            {
                var policyHandle = Policy.HandleResult<HttpResult>(o => o.HttpStatus != 200)
                    .Retry(routeSetting.RetryTimes, (ex, count) =>
                    {
                        logger.LogError("执行失败! 重试次数 {0}", count);
                        logger.LogError("异常来自 {0},错误码 {1}", ex.Result.Content, ex.Result.HttpStatus);
                    });
                requestResult = policyHandle.Execute(() =>
                {
                    optimalRoute = GetLoadBalanceRoute(routeSetting);
                    return HandleRequest(optimalRoute).Result;
                });
            }
            else
            {
                optimalRoute = GetLoadBalanceRoute(routeSetting);
                requestResult = await HandleRequest(optimalRoute);
            }

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
        private async Task<CustomRouteData> GetRequestData(HttpRequest request)
        {
            string requestBodyStr = "";
            using (var buffer = new MemoryStream())
            {
                await Request.Body.CopyToAsync(buffer);
                requestBodyStr = Encoding.UTF8.GetString(buffer.ToArray());
            }
            requestBody = requestBodyStr;

            //Get authorization information in the request headers, it needs  send to the micro service.
            authorizationHeadValue = Request.Headers[Const.HEAD_NAME_AUTHORIZATION];

            var requestHeadStr = Request.Headers[Const.HEAD_NAME_ROUTE_INFO];
            var requestHeadDic = HttpHelper.GetFormDictionary(requestHeadStr);

            requestHead = new RequestHead { BusinessCode = requestHeadDic["BusinessCode"], Ttl = Convert.ToInt32(requestHeadDic.GetValueOrDefault("Ttl", "0")), Channel = requestHeadDic.GetValueOrDefault("Channel",""), Version = requestHeadDic.GetValueOrDefault("Version","") };

            CustomRouteData route = routeHelper.GetOptimalRoute(requestHead.BusinessCode, requestHead.Version, requestHead.Channel);
            if (route == null)
                throw new Exception("请求路由不存在");
            return route;
        }

        private CustomRouteData GetLoadBalanceRoute(CustomRouteData route)
        {
            return routeHelper.RoutingLoadBalance(route);
        }

        /// <summary>
        /// 生成缓存Key
        /// </summary>    
        string GeneralCacheKey()
        {
            string key = string.Join("_", requestHead.Channel, requestHead.BusinessCode, requestHead.Version);
            if (!string.IsNullOrWhiteSpace(requestBody))
            {
                var requestBodyDic = HttpHelper.GetFormDictionary(requestBody).OrderBy(o => o.Key);
                foreach (var p in requestBodyDic)
                {
                    key = string.Join("_", key, string.Join("_", p.Key, p.Value));
                }
            }
            return key.ToLower();
        }

        async Task<HttpResult> HandleRequest(CustomRouteData route)
        {
            HttpResult result = new HttpResult();
            int expire = requestHead.Ttl.GetValueOrDefault();

            if (expire > 0)
            {
                string key = GeneralCacheKey();
                var cacheValue = cache.Get(key);
                if (cacheValue != null)
                {
                    result.Content = cacheValue.ToString();
                    result.HttpStatus = 200;
                    fromCache = true;
                }
                else
                {
                    result = await HttpHelper.PostAsync(route.Handle, requestBody, authorizationHeadValue);
                    if (result.HttpStatus == 200)
                    {
                        cache.Set(key, result.Content, TimeSpan.FromSeconds(expire));
                    }
                    fromCache = false;
                }
            }
            else
            {
                result = await HttpHelper.PostAsync(route.Handle, requestBody, authorizationHeadValue);
                fromCache = false;
            }
            return result;
        }

        #endregion

    }
}
