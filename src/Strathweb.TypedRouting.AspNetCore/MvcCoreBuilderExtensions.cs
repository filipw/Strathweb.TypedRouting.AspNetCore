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
    public static class MvcCoreBuilderExtensions
    {
        public static IMvcCoreBuilder AddTypedRouting(this IMvcCoreBuilder builder, Action<TypedRoutingOptions> typedRoutingOptionsConfiguration)
        {
            var typedRoutingOptions = new TypedRoutingOptions();
            typedRoutingOptionsConfiguration(typedRoutingOptions);

            builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, TypedRoutingOptionsSetup>();
            builder.Services.AddSingleton<TypedRoutingOptions>(typedRoutingOptions);

            return builder;
        }

        public static IMvcCoreBuilder AddTypedRouting(this IMvcCoreBuilder builder, TypedRoutingOptions typedRoutingOptions)
        {
            builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, TypedRoutingOptionsSetup>();
            builder.Services.AddSingleton<TypedRoutingOptions>(typedRoutingOptions);
            return builder;
        }
    }
}
