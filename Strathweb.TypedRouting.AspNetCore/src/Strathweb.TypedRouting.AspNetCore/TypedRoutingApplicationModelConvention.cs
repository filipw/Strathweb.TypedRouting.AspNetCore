using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Strathweb.TypedRouting.AspNetCore
{
    public class TypedRoutingApplicationModelConvention : IApplicationModelConvention
    {
        internal static readonly Dictionary<TypeInfo, List<TypedRoute>> Routes = new Dictionary<TypeInfo, List<TypedRoute>>();

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

                        var selectorModel = new SelectorModel
                        {
                            AttributeRouteModel = route,
                        };

                        foreach(var constraint in route.Constraints)
                        {
                            selectorModel.ActionConstraints.Add(constraint);
                        }

                        action?.Selectors.Clear();
                        action?.Selectors.Insert(0, selectorModel);
                    }
                }
            }
        }
    }
}
