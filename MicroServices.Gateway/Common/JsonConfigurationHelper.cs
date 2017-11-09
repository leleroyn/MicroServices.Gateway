using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroServices.Gateway.Common
{
    public class JsonConfigurationHelper
    {
        public static T GetAppSettings<T>( string basePath,string file, string key) where T : class, new()
        {

            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .Add(new JsonConfigurationSource { Path = file, Optional = false, ReloadOnChange = true })
                .Build();

            var appconfig = new ServiceCollection()
              .AddOptions()
              .Configure<T>(config.GetSection(key))
              .BuildServiceProvider()
              .GetService<IOptions<T>>()
              .Value;
            return appconfig;

        }

    }
}
