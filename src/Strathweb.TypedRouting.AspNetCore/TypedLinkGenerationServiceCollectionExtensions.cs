using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Strathweb.TypedRouting.AspNetCore
{
    public static class TypedLinkGenerationServiceCollectionExtensions
    {
        /// <summary>
        /// Enables typed link generation for minimal API endpoints, by registering a
        /// <see cref="MethodInfoAddressScheme"/> with the routing infrastructure.
        /// </summary>
        public static IServiceCollection AddTypedLinkGeneration(this IServiceCollection services)
        {
            services.TryAddSingleton<IEndpointAddressScheme<MethodInfo>, MethodInfoAddressScheme>();
            return services;
        }
    }
}
