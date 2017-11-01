﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nancy;
using Newtonsoft.Json;

namespace MicroServices.Gateway.Modules
{
    public class HomeModule : BaseModule
    {
        public HomeModule()
        {
             Post["/Api", true] = async (x, ct) =>
             {             
                 var requestResult = await HandleRequest(OptimalRoute); 
                 if(requestResult.HttpStatus != (int)HttpStatusCode.OK)
                 {
                     throw new RequestExecutionException(new Exception (requestResult.Content));
                 }                            
                 return requestResult.Content;
             };      
        }
    }
}