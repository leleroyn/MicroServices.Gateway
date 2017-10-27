using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MicroServices.Gateway.Modules
{
    public class HelloMudule: Nancy.NancyModule
    {
        public HelloMudule()
        {
            Get["/"] = _ =>
            {
                return string .Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") , " Sevice is Online.");
            };
        }
            
    }
}