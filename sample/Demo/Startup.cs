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
using Microsoft.AspNetCore.Authorization;

namespace Demo
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {}

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<TimerFilter>();
            services.AddSingleton<AnnotationFilter>();

            services.AddAuthorization(o =>
            {
                o.AddPolicy("MyPolicy", b => b.RequireAuthenticatedUser());
            });
            services.AddMvc(opt =>
            {
                opt.AddStrathwebGet("strathweb", () =>
                {
                    return "wow!";
                });

                opt.AddStrathwebGet("strathweb/{test}", test =>
                {
                    return "wow! " + test;
                });

                /* opt.Get("api/items", c => c.Action<ItemsController>(x => x.Get())).
                     WithFilters(new AnnotationFilter());

                 opt.Get("api/items/{id}", c => c.Action<ItemsController>(x => x.Get(Param<int>.Any))).
                     WithName("GetItemById").
                     WithFilter<AnnotationFilter>();

                 opt.Post("api/items", c => c.Action<ItemsController>(x => x.Post(Param<Item>.Any)));
                 opt.Put("api/items/{id}", c => c.Action<ItemsController>(x => x.Put(Param<int>.Any, Param<Item>.Any)));
                 opt.Delete("api/items/{id}", c => c.Action<ItemsController>(x => x.Delete(Param<int>.Any)));

                 opt.Get("api/other", c => c.Action<OtherController>(x => x.Action1())).
                     WithConstraints(new MandatoryHeaderConstraint("CustomHeader"));

                 opt.Get("api/other/{id:int}", c => c.Action<OtherController>(x => x.Action2(Param<int>.Any)));

                 opt.Get("api/secure_string", c => c.Action<OtherController>(x => x.Unreachable()).
                     WithAuthorizationPolicy("MyPolicy"));

                 opt.Get("api/secure_instance", c => c.Action<OtherController>(x => x.Unreachable()).
                     WithAuthorizationPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));*/
            });
            services.AddStrathweb();

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();
            app.UseJwtBearerAuthentication();
            app.UseMvc();
        }
    }
}
