using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Strathweb.TypedRouting.AspNetCore
{
    public class TypedRoutingApplicationModelConvention : IApplicationModelConvention
    {
        private readonly IServiceProvider _serviceProvider;

        public TypedRoutingApplicationModelConvention(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        internal static Dictionary<TypeInfo, List<TypedRoute>> Routes = new Dictionary<TypeInfo, List<TypedRoute>>();

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (Routes.ContainsKey(controller.ControllerType))
                {
                    var typedRoutes = Routes[controller.ControllerType];
                    foreach (var route in typedRoutes)
                    {
                        var action = controller.Actions.FirstOrDefault(x => x.ActionMethod == route.ActionMember);
                        if (action == null) continue;

                        var selectorModel = new SelectorModel
                        {
                            AttributeRouteModel = route,
                        };

                        foreach(var constraint in route.Constraints)
                        {
                            selectorModel.ActionConstraints.Add(constraint);
                        }

                        foreach (var filter in route.Filters)
                        {
                            action.Filters.Add(filter);
                        }

                        // resolution from DI only supported when ServiceProvider is there
                        if (_serviceProvider != null)
                        {
                            foreach (var filter in route.FilterTypes)
                            {
                                var filterMetadata = _serviceProvider.GetService(filter) as IFilterMetadata;
                                if (filterMetadata != null)
                                {
                                    action.Filters.Add(filterMetadata);
                                }
                            }
                        }

                        var nonAttributeSelectors = action.Selectors.Where(x => x.AttributeRouteModel == null).ToArray();
                        foreach (var nonAttributeSelector in nonAttributeSelectors)
                        {
                            action.Selectors.Remove(nonAttributeSelector);
                        }

                        action.Selectors.Insert(0, selectorModel);
                    }
                }
            }
        }
    }
}
