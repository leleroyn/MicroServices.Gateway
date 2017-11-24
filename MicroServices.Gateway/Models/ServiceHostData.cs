using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MicroServices.Gateway.Models
{
    public class ServiceHostData
    {
        public string Description { get; set; }
        public string MicroService { get; set; }
        public MicroServiceRandomObject[] Hosts { get; set; }
    }
}