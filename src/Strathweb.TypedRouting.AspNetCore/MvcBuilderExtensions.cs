using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Strathweb.TypedRouting.AspNetCore
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddTypedRouting(this IMvcBuilder builder, Action<TypedRoutingOptions> typedRoutingOptionsConfiguration)
        {
            var typedRoutingOptions = new TypedRoutingOptions();
            typedRoutingOptionsConfiguration(typedRoutingOptions);

            builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, TypedRoutingOptionsSetup>();
            builder.Services.AddSingleton<TypedRoutingOptions>(typedRoutingOptions);

            return builder;
        }

        public static IMvcBuilder AddTypedRouting(this IMvcBuilder builder, TypedRoutingOptions typedRoutingOptions)
        {
            builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, TypedRoutingOptionsSetup>();
            builder.Services.AddSingleton<TypedRoutingOptions>(typedRoutingOptions);
            return builder;
        }
    }
}
