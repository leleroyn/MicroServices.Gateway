using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroServices.Gateway.Models
{
    public class AppConfig
    {   
        public string[] IgnoreLogChannel { get; set; }
    }
}
