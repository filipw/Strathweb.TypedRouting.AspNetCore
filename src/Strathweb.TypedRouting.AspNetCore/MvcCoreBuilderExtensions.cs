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
        public static IMvcCoreBuilder EnableTypedRouting(this IMvcCoreBuilder builder)
        {
            builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, TypedRoutingOptionsSetup>();
            return builder;
        }
    }
}
