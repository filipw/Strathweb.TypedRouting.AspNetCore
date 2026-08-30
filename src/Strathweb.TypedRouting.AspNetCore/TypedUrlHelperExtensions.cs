using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Strathweb.TypedRouting.AspNetCore
{
    /// <summary>
    /// Expression based link generation for <see cref="IUrlHelper"/>, so that URLs are produced from
    /// the action itself rather than from action, controller or route names spelled out as strings.
    /// </summary>
    public static class TypedUrlHelperExtensions
    {
        /// <summary>
        /// Generates a relative URL to the given action, e.g. <c>Url.Action&lt;ItemsController&gt;(x => x.Get(5))</c>.
        /// </summary>
        public static string? Action<TController>(this IUrlHelper urlHelper, Expression<Action<TController>> action, object? values = null) =>
            GenerateUrl(urlHelper, action, values, protocol: null);

        /// <inheritdoc cref="Action{TController}(IUrlHelper, Expression{Action{TController}}, object)"/>
        public static string? Action<TController>(this IUrlHelper urlHelper, Expression<Func<TController, Task>> action, object? values = null) =>
            GenerateUrl(urlHelper, action, values, protocol: null);

        /// <summary>
        /// Generates an absolute URL to the given action, using the scheme and host of the current request.
        /// </summary>
        public static string? Link<TController>(this IUrlHelper urlHelper, Expression<Action<TController>> action, object? values = null) =>
            GenerateUrl(urlHelper, action, values, protocol: CurrentProtocol(urlHelper));

        /// <inheritdoc cref="Link{TController}(IUrlHelper, Expression{Action{TController}}, object)"/>
        public static string? Link<TController>(this IUrlHelper urlHelper, Expression<Func<TController, Task>> action, object? values = null) =>
            GenerateUrl(urlHelper, action, values, protocol: CurrentProtocol(urlHelper));

        private static string? GenerateUrl(IUrlHelper urlHelper, LambdaExpression action, object? values, string? protocol)
        {
            if (urlHelper == null) throw new ArgumentNullException(nameof(urlHelper));
            if (action == null) throw new ArgumentNullException(nameof(action));

            var link = TypedLinkResolver.Resolve(urlHelper.ActionContext.HttpContext.RequestServices, action, values);

            if (link.HasRouteName)
            {
                return urlHelper.RouteUrl(new UrlRouteContext
                {
                    RouteName = link.RouteName,
                    Values = link.Values,
                    Protocol = protocol
                });
            }

            return urlHelper.Action(new UrlActionContext
            {
                Action = link.Action,
                Controller = link.Controller,
                Values = link.AllValues,
                Protocol = protocol
            });
        }

        private static string CurrentProtocol(IUrlHelper urlHelper) =>
            urlHelper.ActionContext.HttpContext.Request.Scheme;
    }
}
