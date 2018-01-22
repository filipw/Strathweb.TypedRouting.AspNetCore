using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Strathweb.TypedRouting.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddStrathweb(this IServiceCollection services)
        {
            services.AddTransient<IActionDescriptorProvider, StrathwebActionDescriptorProvider>();
            services.AddTransient<IActionInvokerProvider, StrathwebActionInvokerProvider>();
        }
    }

    public class StrathwebActionDescriptor : ActionDescriptor
    {
        public StrathwebActionDescriptor(StrathwebActionInfo actionInfo)
        {
            ActionConstraints = new List<IActionConstraintMetadata>()
                    {
                        new HttpMethodActionConstraint(new [] { actionInfo.Method }),
                    };

            AttributeRouteInfo = new AttributeRouteInfo
            {
                Template = actionInfo.Template
            };

            ActionInfo = actionInfo;

            var parameterModels = new List<ParameterModel>();
            foreach (var parameterInfo in actionInfo.Action.GetMethodInfo().GetParameters())
            {
                var parameterModel = CreateParameterModel(parameterInfo);
                if (parameterModel != null)
                {
                    parameterModels.Add(parameterModel);
                }
            }

            Parameters = parameterModels.Select(x => new ParameterDescriptor { Name = x.ParameterName, BindingInfo = x.BindingInfo, ParameterType = x.ParameterInfo.ParameterType }).ToList();
        }

        public StrathwebActionInfo ActionInfo { get; private set; }

        private ParameterModel CreateParameterModel(ParameterInfo parameterInfo)
        {
            if (parameterInfo == null)
            {
                throw new ArgumentNullException(nameof(parameterInfo));
            }

            // CoreCLR returns IEnumerable<Attribute> from GetCustomAttributes - the OfType<object>
            // is needed to so that the result of ToArray() is object
            var attributes = parameterInfo.GetCustomAttributes(inherit: true);
            var parameterModel = new ParameterModel(parameterInfo, attributes);

            var bindingInfo = BindingInfo.GetBindingInfo(attributes);
            parameterModel.BindingInfo = bindingInfo;

            parameterModel.ParameterName = parameterInfo.Name;

            return parameterModel;
        }
    }

    //public class StrathwebApplicationModelProvider : DefaultApplicationModelProvider
    //{
    //    public override void OnProvidersExecuting(ApplicationModelProviderContext context)
    //    {
    //        base.OnProvidersExecuting(context);
    //    }
    //}

    public class StrathwebActionDescriptorProvider : IActionDescriptorProvider
    {
        public int Order => -9999;

        public void OnProvidersExecuted(ActionDescriptorProviderContext context)
        {
            foreach(var action in MvcOptionsExtensions.StrathwebActions)
            {
                context.Results.Add(new StrathwebActionDescriptor(action));
            }
        }

        public void OnProvidersExecuting(ActionDescriptorProviderContext context)
        {
        }
    }

    public class StrathwebActionInvokerProvider : IActionInvokerProvider
    {
        private readonly IReadOnlyList<IFilterProvider> _filterProviders;

        public StrathwebActionInvokerProvider(IEnumerable<IFilterProvider> filterProviders)
        {
            _filterProviders = filterProviders.OrderBy(p => p.Order).ToList();
        }


        public int Order => -100000;

        public void OnProvidersExecuting(ActionInvokerProviderContext context)
        {
            var actionDescriptor = context.ActionContext.ActionDescriptor as StrathwebActionDescriptor;

            if (actionDescriptor != null)
            {
                context.Result = new StrathwebActionInvoker(
                    _filterProviders,
                    context.ActionContext);
            }
        }

        public void OnProvidersExecuted(ActionInvokerProviderContext context)
        {
        }
    }

    public class StrathwebActionInvoker : IActionInvoker
    {
        private readonly IReadOnlyList<IFilterProvider> _filterProviders;
        private readonly ActionContext _actionContext;

        public StrathwebActionInvoker(
            IReadOnlyList<IFilterProvider> filterProviders,
            ActionContext actionContext)
        {
            _filterProviders = filterProviders;
            _actionContext = actionContext;
        }

        public async Task InvokeAsync()
        {
            var actionDescriptor = _actionContext.ActionDescriptor as StrathwebActionDescriptor;
            if (actionDescriptor != null)
            {
                var returnValue = actionDescriptor.ActionInfo.Action();
                var result = CreateActionResult(returnValue);
                await result.ExecuteResultAsync(_actionContext);
            }
        }

        private IActionResult CreateActionResult(object value)
        {
            var actionResult = value as IActionResult;
            if (actionResult != null)
            {
                return actionResult;
            }
            else if (value == null)
            {
                return new NoContentResult();
            }
            else
            {
                return new ObjectResult(value);
            }
        }
    }

    public class StrathwebActionInfo
    {
        public StrathwebActionInfo(string template, string method, Func<object> action)
        {
            Template = template;
            Method = method;
            Action = action;
        }

        public string Template { get; }
        public string Method { get; }
        public Func<object> Action { get; }
    }

    public static class MvcOptionsExtensions
    {
        public static List<StrathwebActionInfo> StrathwebActions = new List<StrathwebActionInfo>();

        public static void AddStrathwebGet(this MvcOptions opts, string template, Func<string, object> action)
        {
            var strathwebAction = new StrathwebActionInfo(template, "GET", action);
            StrathwebActions.Add(strathwebAction);
        }

        public static void AddStrathwebGet(this MvcOptions opts, string template, Func<object> action)
        {
            var strathwebAction = new StrathwebActionInfo(template, "GET", action);
            StrathwebActions.Add(strathwebAction);
        }

        public static TypedRoute Get(this MvcOptions opts, string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup).ForHttpMethods("GET");
        }

        public static TypedRoute Post(this MvcOptions opts, string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup).ForHttpMethods("POST");
        }

        public static TypedRoute Put(this MvcOptions opts, string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup).ForHttpMethods("PUT");
        }

        public static TypedRoute Delete(this MvcOptions opts, string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup).ForHttpMethods("DELETE");
        }

        public static TypedRoute Route(this MvcOptions opts, string template, Action<TypedRoute> configSetup)
        {
            return AddRoute(template, configSetup);
        }

        private static TypedRoute AddRoute(string template, Action<TypedRoute> configSetup)
        {
            var route = new TypedRoute(template);
            configSetup(route);

            if (TypedRoutingApplicationModelConvention.Routes.ContainsKey(route.ControllerType))
            {
                var controllerActions = TypedRoutingApplicationModelConvention.Routes[route.ControllerType];
                controllerActions.Add(route);
            }
            else
            {
                var controllerActions = new List<TypedRoute> { route };
                TypedRoutingApplicationModelConvention.Routes.Add(route.ControllerType, controllerActions);
            }

            return route;
        }

        [Obsolete("Please use EnableTypedRouting() on IMvcBuilder or IMvcCoreBuilder instead.")]
        public static void EnableTypedRouting(this MvcOptions opts)
        {
            opts.Conventions.Add(new TypedRoutingApplicationModelConvention(null));
        }
    }
}
