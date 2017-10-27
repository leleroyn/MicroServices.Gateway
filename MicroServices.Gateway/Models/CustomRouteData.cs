using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MicroServices.Gateway.Models
{
    public class CustomRouteData
    {
        public string Description { get; set; }
        public string BusinessCode { get; set; }
        public string Version { get; set; }
        public string RequestFrom { get; set; }
        public string Handle { get; set; }
        public string MicroService { get; set; }
    }
}