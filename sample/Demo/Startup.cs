using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Strathweb.TypedRouting.AspNetCore;
using Demo.Controllers;

namespace Demo
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {}

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(opt =>
            {
                opt.EnableTypedRouting();
                opt.Get("api/values", c => c.Action<ValuesController>(x => x.Get()));
                opt.Get("api/values/{id}", c => c.Action<ValuesController>(x => x.Get(Param<int>.Any))).WithName("GetValueById");
                opt.Post("api/values", c => c.Action<ValuesController>(x => x.Post(Param<string>.Any)));
                opt.Put("api/values/{id}", c => c.Action<ValuesController>(x => x.Put(Param<int>.Any, Param<string>.Any)));
                opt.Delete("api/values/{id}", c => c.Action<ValuesController>(x => x.Delete(Param<int>.Any)));

                opt.Get("api/other", c => c.Action<OtherController>(x => x.Action1())).WithConstraints(new MandatoryHeaderConstraint("CustomHeader"));
                opt.Get("api/other/{id:int}", c => c.Action<OtherController>(x => x.Action2(Param<int>.Any)));
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();
            app.UseMvc();
        }
    }
}
