using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Strathweb.TypedRouting.AspNetCore
{
    public class TypedRoutingOptions
    {
        internal Dictionary<TypeInfo, List<TypedRoute>> Routes = new Dictionary<TypeInfo, List<TypedRoute>>();

        public TypedRoute Get([StringSyntax("Route")] string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup).ForHttpMethods("GET");
        }

        public TypedRoute Post([StringSyntax("Route")] string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup).ForHttpMethods("POST");
        }

        public TypedRoute Put([StringSyntax("Route")] string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup).ForHttpMethods("PUT");
        }

        public TypedRoute Delete([StringSyntax("Route")] string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup).ForHttpMethods("DELETE");
        }

        public TypedRoute Route([StringSyntax("Route")] string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup);
        }

        private TypedRoute AddRoute([StringSyntax("Route")] string template, Action<TypedRoute> configSetup)
        {
            var route = new TypedRoute(template);
            configSetup(route);

            if (route.ControllerType == null)
            {
                throw new InvalidOperationException($"No controller action was configured for the route template '{template}'.");
            }

            if (Routes.ContainsKey(route.ControllerType))
            {
                var controllerActions = Routes[route.ControllerType];
                controllerActions.Add(route);
            }
            else
            {
                var controllerActions = new List<TypedRoute> { route };
                Routes.Add(route.ControllerType, controllerActions);
            }

            return route;
        }
    }
}
