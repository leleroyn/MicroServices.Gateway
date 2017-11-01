using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MicroServices.Gateway.Models
{
    public class HttpResult
    {
        public string Content { get; set; }
        public int HttpStatus { get; set; }
    }
}