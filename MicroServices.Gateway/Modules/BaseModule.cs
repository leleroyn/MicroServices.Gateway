using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Newtonsoft.Json;
using MicroServices.Gateway.Models;
using MicroServices.Gateway.Common;

namespace MicroServices.Gateway.Modules
{
    public class BaseModule : NancyModule
    {
        protected RequestHead HeadData;
        protected CustomRouteData OptimalRoute;
        protected bool FromCache;
        protected string RequestContent;

        public BaseModule()
        {
            DateTime elapsedTime = DateTime.Now;
            bool ignoreLog = false;
            Before += ctx =>
            { 
                var route = GetRequestData(ctx.Request);
                OptimalRoute = route;
                ignoreLog = SettingsHelper.IgnoreLogChannel(HeadData.RequestFrom);
                return null;
            };

            After += ctx =>
            {
                if (!ignoreLog)
                {
                    string response;
                    using (MemoryStream respData = new MemoryStream())
                    {
                        ctx.Response.Contents(respData);
                        response = Encoding.UTF8.GetString(respData.ToArray());
                    }
                    string requestInfo = string.Format(
                        "Success, Elapsed:{0}(s), RequestBody:{1}, ResponseBody:{2}, RouteData:{3}, FromCahle:{4}",
                        (DateTime.Now - elapsedTime).TotalSeconds, RequestContent, response,
                        JsonConvert.SerializeObject(OptimalRoute), 
                        FromCache);
                    LogHelper.Info(HeadData.BusinessCode, requestInfo);                      
                }
            };

            OnError += (ctx, ex) =>
            {              
                    LogHelper.Error(
                        HeadData == null ? "" : HeadData.BusinessCode,
                        string.Format(
                            "Error, ErrorTime:{0}, RequestBody:{1}, RouteData:{2}, ErrorMessage:{3}",
                            DateTime.Now, RequestContent, 
                            JsonConvert.SerializeObject(OptimalRoute), ex.Message), ex);
               
               throw ex;
            };
        }

        /// <summary>
        /// 生成缓存Key
        /// </summary>    
        protected string GeneralCacheKey()
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

        /// <summary>
        /// 请求处理
        /// </summary>
        protected async Task<string> HandleRequest(CustomRouteData route)
        {
            string response;
            int expire = HeadData.Expire.GetValueOrDefault();

            if (expire > 0)
            {
                string key = GeneralCacheKey();
                var cacheValue = CacheHelper.Get(key);
                if (cacheValue != null)
                {
                    response = cacheValue;
                    FromCache = true;
                }
                else
                {
                    response = await HttpClient.PostAsync(route.Handle, RequestContent);
                    CacheHelper.Set(key, response, TimeSpan.FromSeconds(expire));
                    FromCache = false;
                }
            }
            else
            {
                response = await HttpClient.PostAsync(route.Handle, RequestContent);
                FromCache = false;
            }
            return response;
        }


        /// <summary>
        /// 获取请求信息
        /// </summary>
        private CustomRouteData GetRequestData(Request request)
        {
            var requestLength = request.Body.Length;
            var requestBites = new Byte[requestLength];
            request.Body.Read(requestBites, 0, (int)requestLength);
            RequestContent = Encoding.UTF8.GetString(requestBites);      

            var requestObj = JsonConvert.DeserializeObject<dynamic>(RequestContent);
            var requestHeadObj = JsonConvert.DeserializeObject<dynamic>(Convert.ToString(requestObj.RequestHead));

            HeadData = new RequestHead { BusinessCode = requestHeadObj.BusinessCode, Expire = requestHeadObj.Expire, RequestFrom = requestHeadObj.RequestFrom, RequestSN = requestHeadObj.RequestSN, RequestTime = requestHeadObj.RequestTime, Version = requestHeadObj.Version };
           
            CustomRouteData route = RouteHelper.GetOptimalRoute(HeadData.BusinessCode, HeadData.Version, HeadData.RequestFrom);
            if (route == null)
                throw new Exception("请求路由不存在");
            route = RouteHelper.RoutingLoadBalance(route);
            return route;
        }
    }
}