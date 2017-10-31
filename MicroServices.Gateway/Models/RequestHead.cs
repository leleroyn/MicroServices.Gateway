using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MicroServices.Gateway.Models
{
    public class RequestHead
    {
        /// <summary>
        /// 请求渠道
        /// </summary>
        public string RequestFrom { get; set; }
        /// <summary>
        /// 请求时间
        /// </summary>
        public string RequestTime { get; set; }

        /// <summary>
        /// 请求流水号
        /// </summary>
        public string RequestSN { get; set; }

        /// <summary>
        /// 请求结果缓存时间
        /// </summary>
        public int?  Expire { get; set; }

        /// <summary>
        /// 业务编码
        /// </summary>
        public string BusinessCode { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }

        public RequestHead()
        {
            Version = "1.0.0";
            RequestSN = Guid.NewGuid().ToString();
            RequestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Expire = 0;
        }
    }
}