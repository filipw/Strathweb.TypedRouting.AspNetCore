using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace Strathweb.TypedRouting.AspNetCore
{
    /// <summary>
    /// Teaches the built-in <see cref="LinkGenerator"/> to address an endpoint by the
    /// <see cref="MethodInfo"/> of the method handling it. Minimal API endpoints carry that
    /// <see cref="MethodInfo"/> in their metadata, which makes them addressable even when they
    /// were never given a name via <c>WithName</c>.
    /// </summary>
    public sealed class MethodInfoAddressScheme : IEndpointAddressScheme<MethodInfo>
    {
        private readonly EndpointDataSource _endpointDataSource;

        public MethodInfoAddressScheme(EndpointDataSource endpointDataSource)
        {
            _endpointDataSource = endpointDataSource;
        }

        public IEnumerable<Endpoint> FindEndpoints(MethodInfo address)
        {
            if (address == null)
            {
                return Enumerable.Empty<Endpoint>();
            }

            return _endpointDataSource.Endpoints.Where(endpoint => Matches(endpoint, address));
        }

        private static bool Matches(Endpoint endpoint, MethodInfo address)
        {
            // minimal API endpoints expose the handler method directly
            var handler = endpoint.Metadata.GetMetadata<MethodInfo>();
            if (handler != null && (handler == address || handler.GetBaseDefinition() == address.GetBaseDefinition()))
            {
                return true;
            }

            // controller actions expose it through the action descriptor
            var action = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            return action != null &&
                   (action.MethodInfo == address || action.MethodInfo.GetBaseDefinition() == address.GetBaseDefinition());
        }
    }
}
