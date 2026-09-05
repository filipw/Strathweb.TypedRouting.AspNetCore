using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;

namespace Strathweb.TypedRouting.AspNetCore
{
    /// <summary>
    /// Expression based link generation for minimal API endpoints, e.g.
    /// <c>linkGenerator.GetPathByHandler(httpContext, () => ItemHandlers.GetById(5))</c>.
    /// The handler must be a method group - a lambda handler compiles to a method that cannot
    /// be referenced from anywhere else, so it can never be the target of a generated link.
    /// </summary>
    public static class TypedHandlerLinkGeneratorExtensions
    {
        public static string? GetPathByHandler(this LinkGenerator linkGenerator, HttpContext httpContext,
            Expression<Action> handler, object? values = null)
        {
            var (method, routeValues) = Resolve(httpContext, handler, values);
            return linkGenerator.GetPathByAddress(httpContext, method, routeValues);
        }

        public static string? GetPathByHandler(this LinkGenerator linkGenerator, HttpContext httpContext,
            Expression<Func<Task>> handler, object? values = null)
        {
            var (method, routeValues) = Resolve(httpContext, handler, values);
            return linkGenerator.GetPathByAddress(httpContext, method, routeValues);
        }

        public static string? GetUriByHandler(this LinkGenerator linkGenerator, HttpContext httpContext,
            Expression<Action> handler, object? values = null)
        {
            var (method, routeValues) = Resolve(httpContext, handler, values);
            return linkGenerator.GetUriByAddress(httpContext, method, routeValues,
                scheme: httpContext.Request.Scheme, host: httpContext.Request.Host);
        }

        public static string? GetUriByHandler(this LinkGenerator linkGenerator, HttpContext httpContext,
            Expression<Func<Task>> handler, object? values = null)
        {
            var (method, routeValues) = Resolve(httpContext, handler, values);
            return linkGenerator.GetUriByAddress(httpContext, method, routeValues,
                scheme: httpContext.Request.Scheme, host: httpContext.Request.Host);
        }

        internal static (MethodInfo Method, RouteValueDictionary Values) Resolve(HttpContext httpContext,
            LambdaExpression handler, object? values)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var services = httpContext.RequestServices;

            if (services.GetService<IEndpointAddressScheme<MethodInfo>>() == null)
            {
                throw new InvalidOperationException(
                    "Typed link generation for minimal API endpoints requires services.AddTypedLinkGeneration() to be called during startup.");
            }

            var call = ExpressionHelper.GetMethodCall(handler);
            var routeValues = values == null ? new RouteValueDictionary() : new RouteValueDictionary(values);

            AddArguments(call, FindRoutePattern(services, call.Method), routeValues);

            return (call.Method, routeValues);
        }

        private static RoutePattern? FindRoutePattern(IServiceProvider services, MethodInfo method)
        {
            var dataSource = services.GetService<EndpointDataSource>();

            return dataSource?.Endpoints
                .OfType<RouteEndpoint>()
                .FirstOrDefault(x => x.Metadata.GetMetadata<MethodInfo>() == method)?
                .RoutePattern;
        }

        private static void AddArguments(MethodCallExpression call, RoutePattern? routePattern, RouteValueDictionary values)
        {
            var parameters = call.Method.GetParameters();

            for (var i = 0; i < parameters.Length && i < call.Arguments.Count; i++)
            {
                var parameter = parameters[i];
                var argument = call.Arguments[i];

                if (parameter.Name == null || ExpressionHelper.IsParamPlaceholder(argument))
                {
                    continue;
                }

                // a parameter that appears in the template is a route value; anything else is only
                // useful as a query string value, and then only if it has a meaningful string form.
                // everything left over is bound from the body, DI or the request itself
                var isRouteParameter = routePattern?.Parameters.Any(x => x.Name == parameter.Name) == true;
                if (!isRouteParameter && !TypedLinkResolver.IsSimpleType(parameter.ParameterType))
                {
                    continue;
                }

                var value = ExpressionHelper.Evaluate(argument);
                if (value != null)
                {
                    values[parameter.Name] = value;
                }
            }
        }
    }
}
