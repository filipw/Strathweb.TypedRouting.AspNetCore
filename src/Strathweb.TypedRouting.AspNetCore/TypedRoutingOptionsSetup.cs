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
    public class TypedRoutingOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private IServiceProvider _serviceProvider;

        public TypedRoutingOptionsSetup(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Configure(MvcOptions options)
        {
            options.Conventions.Add(new TypedRoutingApplicationModelConvention(_serviceProvider));
        }
    }
}
