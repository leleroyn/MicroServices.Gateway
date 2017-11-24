﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace MicroServices.Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {           
            var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
            try
            {              
                BuildWebHost(args).Run();
            }
            catch (Exception e)
            {               
                logger.Error(e, "Stopped program because of exception");
                throw;
            }

        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseNLog()
                .Build();
    }
}
