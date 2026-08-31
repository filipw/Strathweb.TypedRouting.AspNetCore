using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Strathweb.TypedRouting.AspNetCore
{
    /// <summary>
    /// Resolves an action expression such as <c>x => x.Get(5)</c> into the route name, action/controller
    /// names and route values that the ASP.NET Core link generation infrastructure expects.
    /// </summary>
    internal static class TypedLinkResolver
    {
        // keyed on the action descriptor collection itself, so the cache is dropped automatically
        // whenever the collection is rebuilt (e.g. by an application part change)
        private static readonly ConditionalWeakTable<ActionDescriptorCollection, ConcurrentDictionary<MethodInfo, ControllerActionDescriptor?>> Lookups = new();

        private static readonly HashSet<Type> AdditionalSimpleTypes = new()
        {
            typeof(string), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset),
            typeof(TimeSpan), typeof(Guid), typeof(Uri)
        };

        public static ResolvedLink Resolve(IServiceProvider services, LambdaExpression expression, object? additionalValues)
        {
            var call = ExpressionHelper.GetMethodCall(expression);
            var descriptor = FindDescriptor(services, call.Method);

            if (descriptor == null)
            {
                throw new InvalidOperationException(
                    $"No controller action could be found for '{call.Method.DeclaringType?.FullName}.{call.Method.Name}'. " +
                    "Make sure the controller is discovered by MVC and that the action is reachable via a route.");
            }

            var values = additionalValues == null
                ? new RouteValueDictionary()
                : new RouteValueDictionary(additionalValues);

            AddArguments(call, descriptor, values);

            // area (and any other ambient route value) is needed to select the right endpoint,
            // but must not be passed along a named route, where it would leak into the query string
            var ambientValues = new RouteValueDictionary();
            foreach (var routeValue in descriptor.RouteValues)
            {
                if (routeValue.Key == "action" || routeValue.Key == "controller" || routeValue.Value == null)
                {
                    continue;
                }

                ambientValues[routeValue.Key] = routeValue.Value;
            }

            descriptor.RouteValues.TryGetValue("action", out var action);
            descriptor.RouteValues.TryGetValue("controller", out var controller);

            return new ResolvedLink(descriptor.AttributeRouteInfo?.Name, action, controller, values, ambientValues);
        }

        private static ControllerActionDescriptor? FindDescriptor(IServiceProvider services, MethodInfo method)
        {
            var provider = services.GetService<IActionDescriptorCollectionProvider>();
            if (provider == null)
            {
                throw new InvalidOperationException(
                    $"'{nameof(IActionDescriptorCollectionProvider)}' is not available. Typed link generation requires MVC to be registered (e.g. via AddControllers()).");
            }

            var collection = provider.ActionDescriptors;
            var lookup = Lookups.GetValue(collection, _ => new ConcurrentDictionary<MethodInfo, ControllerActionDescriptor?>());

            return lookup.GetOrAdd(method, static (m, items) =>
            {
                var candidates = items.OfType<ControllerActionDescriptor>().ToArray();

                return candidates.FirstOrDefault(x => x.MethodInfo == m)
                    // the expression may have been written against a base class declaring the action as virtual
                    ?? candidates.FirstOrDefault(x => x.MethodInfo.GetBaseDefinition() == m.GetBaseDefinition());
            }, collection.Items);
        }

        private static void AddArguments(MethodCallExpression call, ControllerActionDescriptor descriptor, RouteValueDictionary values)
        {
            var parameters = call.Method.GetParameters();

            for (var i = 0; i < parameters.Length && i < call.Arguments.Count; i++)
            {
                var parameter = parameters[i];
                var argument = call.Arguments[i];

                // Param<T>.Any is a placeholder used when declaring routes - it carries no value
                if (ExpressionHelper.IsParamPlaceholder(argument))
                {
                    continue;
                }

                var parameterDescriptor = descriptor.Parameters.FirstOrDefault(x => x.Name == parameter.Name);
                if (!IsRoutable(parameterDescriptor, parameter.ParameterType))
                {
                    continue;
                }

                var value = ExpressionHelper.Evaluate(argument);
                if (value == null)
                {
                    continue;
                }

                values[parameterDescriptor?.BindingInfo?.BinderModelName ?? parameter.Name!] = value;
            }
        }

        private static bool IsRoutable(ParameterDescriptor? parameterDescriptor, Type parameterType)
        {
            var source = parameterDescriptor?.BindingInfo?.BindingSource;

            // anything bound from the request body, DI, headers or form data can never be part of a URL
            if (source == BindingSource.Body || source == BindingSource.Services || source == BindingSource.Form ||
                source == BindingSource.FormFile || source == BindingSource.Header || source == BindingSource.Special)
            {
                return false;
            }

            if (source == BindingSource.Path || source == BindingSource.Query)
            {
                return true;
            }

            // with no explicit binding source we only take values that have a meaningful string form
            return IsSimpleType(parameterType);
        }

        internal static bool IsSimpleType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            return underlying.IsPrimitive || underlying.IsEnum || AdditionalSimpleTypes.Contains(underlying);
        }
    }

    internal sealed class ResolvedLink
    {
        public ResolvedLink(string? routeName, string? action, string? controller, RouteValueDictionary values, RouteValueDictionary ambientValues)
        {
            RouteName = routeName;
            Action = action;
            Controller = controller;
            Values = values;
            AmbientValues = ambientValues;
        }

        public string? RouteName { get; }

        public string? Action { get; }

        public string? Controller { get; }

        public RouteValueDictionary Values { get; }

        public RouteValueDictionary AmbientValues { get; }

        /// <summary>
        /// A named route identifies the endpoint exactly, so it is preferred over action/controller
        /// matching, which cannot tell overloaded actions apart.
        /// </summary>
        public bool HasRouteName => !string.IsNullOrEmpty(RouteName);

        public RouteValueDictionary AllValues
        {
            get
            {
                var all = new RouteValueDictionary(Values);
                foreach (var ambient in AmbientValues)
                {
                    if (!all.ContainsKey(ambient.Key))
                    {
                        all[ambient.Key] = ambient.Value;
                    }
                }

                return all;
            }
        }
    }
}
