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
            services.AddSingleton<TimerFilter>();
            services.AddMvc(opt =>
            {
                //opt.EnableTypedRouting();
                opt.Get("api/items", c => c.Action<ItemsController>(x => x.Get()));
                opt.Get("api/items/{id}", c => c.Action<ItemsController>(x => x.Get(Param<int>.Any))).WithName("GetItemById");
                opt.Post("api/items", c => c.Action<ItemsController>(x => x.Post(Param<Item>.Any)));
                opt.Put("api/items/{id}", c => c.Action<ItemsController>(x => x.Put(Param<int>.Any, Param<Item>.Any)));
                opt.Delete("api/items/{id}", c => c.Action<ItemsController>(x => x.Delete(Param<int>.Any)));

                opt.Get("api/other", c => c.Action<OtherController>(x => x.Action1())).
                    WithConstraints(new MandatoryHeaderConstraint("CustomHeader"));

                opt.Get("api/other/{id:int}", c => c.Action<OtherController>(x => x.Action2(Param<int>.Any)));
            }).EnableTypedRouting();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();
            app.UseMvc();
        }
    }
}
