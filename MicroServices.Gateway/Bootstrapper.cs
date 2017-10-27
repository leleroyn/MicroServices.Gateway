using Nancy;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Nancy.Bootstrapper;
using Nancy.Json;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using MicroServices.Gateway.Common;

namespace MicroServices.Gateway
{

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            if (string.IsNullOrWhiteSpace(SettingsHelper.RootPath))
            {
                SettingsHelper.RootPath = RootPathProvider.GetRootPath();
            }
            JsonSettings.RetainCasing = true;
        
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            pipelines.OnError += (ctx, ex) =>
            {
                LogHelper.Error("Route request error[Global]", string.Format("Route request error，Message:{0}", ex.Message), ex);
                throw ex;
            };
        }
    }
}