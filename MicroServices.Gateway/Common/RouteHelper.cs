using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Newtonsoft.Json;
using MicroServices.Gateway.Models;

namespace MicroServices.Gateway.Common
{
    public static class RouteHelper
    {
        private static readonly string RouteDataPath = Path.Combine(SettingsHelper.RootPath, "App_Data/route");
        private static readonly string RouteDataCacheKey = "routes.json";
        private static readonly string HostDataPath = Path.Combine(SettingsHelper.RootPath, "App_Data/servicehost");
        private static readonly string HostDataCacheKey = "hosts.json";

        /// <summary>
        /// 获取路由配置
        /// </summary>
        public static Dictionary<string, List<CustomRouteData>> GetRouteDatas()
        {
            var routeDic = HttpRuntime.Cache[RouteDataCacheKey] as Dictionary<string, List<CustomRouteData>>;
            if (routeDic == null)
            {
                //加载配置目录下所有的json文件
                string[] files = Directory.GetFiles(RouteDataPath, "*.json", SearchOption.AllDirectories);

                routeDic = new Dictionary<string, List<CustomRouteData>>();
                foreach (var file in files)
                {
                    var routeContent = File.ReadAllText(file);
                    var routeSet = JsonConvert.DeserializeObject<List<CustomRouteData>>(routeContent);

                    var singleDic = routeSet.GroupBy(o => o.BusinessCode).ToDictionary(
                          k => k.Key,
                          v => v.Select(o => o).ToList()
                         );

                    //跨配置文件BusinessCode必须保持唯一
                    foreach (var route in singleDic)
                        routeDic.Add(route.Key, route.Value);
                }

                CacheHelper.Set(RouteDataCacheKey, routeDic, files);
            }
            return routeDic;
        }

        /// <summary>
        /// 获取Host配置
        /// </summary>
        /// <returns></returns>
        public static List<ServiceHostData> GetHostDatas()
        {
            var hostDatas = HttpRuntime.Cache[HostDataCacheKey] as List<ServiceHostData>;
            if (hostDatas == null)
            {
                //加载配置目录下所有的json文件
                string[] files = Directory.GetFiles(HostDataPath, "*.json", SearchOption.AllDirectories);

                hostDatas = new List<ServiceHostData>();
                foreach (var file in files)
                {
                    var content = File.ReadAllText(file);
                    var data = JsonConvert.DeserializeObject<List<ServiceHostData>>(content);
                    hostDatas.AddRange(data);
                }

                CacheHelper.Set(HostDataCacheKey, hostDatas, files);
            }
            return hostDatas;
        }

        /// <summary>
        /// 路由负载均衡
        /// </summary>
        /// <param name="routeData"></param>
        /// <returns></returns>
        public static CustomRouteData RoutingLoadBalance(
            CustomRouteData routeData)
        {
            var route = new CustomRouteData
            {             
                BusinessCode = routeData.BusinessCode,
                MicroService = routeData.MicroService,
                Description = routeData.Description,
                RequestFrom = routeData.RequestFrom,
                Version = routeData.Version
            };
            var hostData = GetHostDatas().FirstOrDefault(x => routeData.MicroService == x.ApplicationId);
            if (hostData != null)
            {
                var randomHost = RandomHelper.GetRandomList(hostData.Hosts.ToList(), 1).First();
                route.Handle = string.Concat(randomHost.ServiceUrl, routeData.Handle);
            }
            return route;
        }

        /// <summary>
        /// 获取最优路由
        /// </summary>
        /// <param name="businessCode"></param>
        /// <param name="version"></param>
        /// <param name="requestFrom"></param>
        /// <returns></returns>
        public static CustomRouteData GetOptimalRoute(string businessCode, string version, string requestFrom)
        {
            var routeDatas = GetRouteDatas();
            var routes = routeDatas.FirstOrDefault(x => string.Equals(x.Key, businessCode, StringComparison.OrdinalIgnoreCase));
            if (routes.Value == null)
                return null;

            if (routes.Value.Count == 1)
                return routes.Value.First();

            IEnumerable<CustomRouteData> routeList = routes.Value;

            List<Expression<Func<CustomRouteData, bool>>> expressions = new List<Expression<Func<CustomRouteData, bool>>>();
            if (!string.IsNullOrEmpty(version))
            {
                expressions.Add(x => x.Version == version);
            }
            if (!string.IsNullOrEmpty(requestFrom))
            {
                expressions.Add(x => string.Equals(x.RequestFrom, requestFrom, StringComparison.OrdinalIgnoreCase));
            }

            routeList = expressions.Aggregate(routeList, (current, item) => current.Where(item.Compile()));

            if (routeList.Any())
            {
                return routeList.OrderBy(x => x.Version).ThenBy(x => x.RequestFrom).FirstOrDefault();
            }

            return routes.Value
                    .Where(x => string.IsNullOrEmpty(x.Version) || string.IsNullOrEmpty(x.RequestFrom))
                    .OrderBy(x => x.Version).ThenBy(x => x.RequestFrom)
                    .FirstOrDefault();
        }       
    }
}