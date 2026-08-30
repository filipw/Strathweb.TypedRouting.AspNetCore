using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Strathweb.TypedRouting.AspNetCore
{
    /// <summary>
    /// Expression based link generation for <see cref="LinkGenerator"/>, for use outside of controllers
    /// (middleware, filters, background components with an <see cref="HttpContext"/> at hand).
    /// </summary>
    public static class TypedLinkGeneratorExtensions
    {
        public static string? GetPathByAction<TController>(this LinkGenerator linkGenerator, HttpContext httpContext,
            Expression<Action<TController>> action, object? values = null) =>
            GetPath(linkGenerator, httpContext, action, values);

        public static string? GetPathByAction<TController>(this LinkGenerator linkGenerator, HttpContext httpContext,
            Expression<Func<TController, Task>> action, object? values = null) =>
            GetPath(linkGenerator, httpContext, action, values);

        public static string? GetUriByAction<TController>(this LinkGenerator linkGenerator, HttpContext httpContext,
            Expression<Action<TController>> action, object? values = null) =>
            GetUri(linkGenerator, httpContext, action, values);

        public static string? GetUriByAction<TController>(this LinkGenerator linkGenerator, HttpContext httpContext,
            Expression<Func<TController, Task>> action, object? values = null) =>
            GetUri(linkGenerator, httpContext, action, values);

        private static string? GetPath(LinkGenerator linkGenerator, HttpContext httpContext, LambdaExpression action, object? values)
        {
            var link = Resolve(linkGenerator, httpContext, action, values);

            return link.HasRouteName
                ? linkGenerator.GetPathByRouteValues(httpContext, link.RouteName, link.Values)
                : linkGenerator.GetPathByRouteValues(httpContext, routeName: null, link.AllValues);
        }

        private static string? GetUri(LinkGenerator linkGenerator, HttpContext httpContext, LambdaExpression action, object? values)
        {
            var link = Resolve(linkGenerator, httpContext, action, values);

            return link.HasRouteName
                ? linkGenerator.GetUriByRouteValues(httpContext, link.RouteName, link.Values)
                : linkGenerator.GetUriByRouteValues(httpContext, routeName: null, link.AllValues);
        }

        private static ResolvedLink Resolve(LinkGenerator linkGenerator, HttpContext httpContext, LambdaExpression action, object? values)
        {
            if (linkGenerator == null) throw new ArgumentNullException(nameof(linkGenerator));
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (action == null) throw new ArgumentNullException(nameof(action));

            return TypedLinkResolver.Resolve(httpContext.RequestServices, action, values);
        }
    }
}
