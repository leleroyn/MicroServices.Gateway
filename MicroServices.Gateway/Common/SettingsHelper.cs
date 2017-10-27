using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace MicroServices.Gateway.Common
{
    public static class SettingsHelper
    {
        /// <summary>
        /// 获取验签密钥
        /// </summary>
        /// <param name="requestFrom"></param>
        /// <returns></returns>
        public static string GetDesKey(string requestFrom)
        {
            NameValueCollection webDesKeys = (NameValueCollection)ConfigurationManager.GetSection("webDesKey");
            return webDesKeys[requestFrom.ToLower()];            
        }

        /// <summary>
        /// 是否忽略日志
        /// </summary>
        /// <param name="requestFrom"></param>
        /// <returns></returns>
        public static bool IgnoreLogChannel(string requestFrom)
        {
            string ignoreLogChannel = ConfigurationManager.AppSettings["IgnoreLogChannel"];
            if (string.IsNullOrWhiteSpace(ignoreLogChannel))
                return false;
            return ignoreLogChannel.Split(',').Any(o => o.Equals(requestFrom, StringComparison.OrdinalIgnoreCase));
        }       

        /// <summary>
        /// 程序根目录
        /// </summary>
        public static string RootPath { get; set; }
               
    }
}