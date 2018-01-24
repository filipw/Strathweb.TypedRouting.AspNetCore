using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Strathweb.TypedRouting.AspNetCore
{
    public class TypedRoutingOptions
    {
        internal Dictionary<TypeInfo, List<TypedRoute>> Routes = new Dictionary<TypeInfo, List<TypedRoute>>();

        public TypedRoute Get(string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup).ForHttpMethods("GET");
        }

        public TypedRoute Post(string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup).ForHttpMethods("POST");
        }

        public TypedRoute Put(string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup).ForHttpMethods("PUT");
        }

        public TypedRoute Delete(string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup).ForHttpMethods("DELETE");
        }

        public TypedRoute Route(string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup);
        }

        private TypedRoute AddRoute(string template, Action<TypedRoute> configSetup)
        {
            var route = new TypedRoute(template);
            configSetup(route);

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
