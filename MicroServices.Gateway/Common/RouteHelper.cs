using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Newtonsoft.Json;
using MicroServices.Gateway.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MicroServices.Gateway.Common
{
    public class RouteHelper
    {
        string _rootPath;
        public RouteHelper(string rootPath)
        {
            _rootPath = rootPath;

        }
        private RouteHelper() { }

        /// <summary>
        /// 获取路由配置
        /// </summary>
        public Dictionary<string, List<CustomRouteData>> GetRouteDatas()
        {
            var routeDic = new Dictionary<string, List<CustomRouteData>>();
            var routeSet = JsonConfigurationHelper.GetAppSettings<List<CustomRouteData>>(Path.Combine(_rootPath, "App_Data"), "routesettings.json", "RouteTable");

            var singleDic = routeSet.GroupBy(o => o.BusinessCode).ToDictionary(
                       k => k.Key,
                       v => v.Select(o => o).ToList()
                      );

            //跨配置文件BusinessCode必须保持唯一
            foreach (var route in singleDic)
                routeDic.Add(route.Key, route.Value);
            return routeDic;
        }

        /// <summary>
        /// 获取Host配置
        /// </summary>
        /// <returns></returns>
        public List<ServiceHostData> GetHostDatas()
        {
            return JsonConfigurationHelper.GetAppSettings<List<ServiceHostData>>(Path.Combine(_rootPath, "App_Data"), "hostsettings.json", "HostTable");
        }

        /// <summary>
        /// 路由负载均衡
        /// </summary>
        /// <param name="routeData"></param>
        /// <returns></returns>
        public CustomRouteData RoutingLoadBalance(
            CustomRouteData routeData)
        {
            var route = new CustomRouteData
            {
                BusinessCode = routeData.BusinessCode,
                MicroService = routeData.MicroService,
                Description = routeData.Description,
                Channel = routeData.Channel,
                Version = routeData.Version
            };
            var hostData = GetHostDatas().FirstOrDefault(x => routeData.MicroService == x.MicroService);
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
        public CustomRouteData GetOptimalRoute(string businessCode, string version, string requestFrom)
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
                expressions.Add(x => string.Equals(x.Channel, requestFrom, StringComparison.OrdinalIgnoreCase));
            }

            routeList = expressions.Aggregate(routeList, (current, item) => current.Where(item.Compile()));

            if (routeList.Any())
            {
                return routeList.OrderBy(x => x.Version).ThenBy(x => x.Channel).FirstOrDefault();
            }

            return routes.Value
                    .Where(x => string.IsNullOrEmpty(x.Version) || string.IsNullOrEmpty(x.Channel))
                    .OrderBy(x => x.Version).ThenBy(x => x.Channel)
                    .FirstOrDefault();
        }
    }
}