using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Strathweb.TypedRouting.AspNetCore
{
    public class TypedRoutingOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TypedRoutingOptions _options;

        public TypedRoutingOptionsSetup(IServiceProvider serviceProvider, TypedRoutingOptions options)
        {
            _serviceProvider = serviceProvider;
            _options = options;
        }

        public void Configure(MvcOptions options)
        {
            options.Conventions.Add(new TypedRoutingApplicationModelConvention(_serviceProvider, _options));
        }
    }
}
