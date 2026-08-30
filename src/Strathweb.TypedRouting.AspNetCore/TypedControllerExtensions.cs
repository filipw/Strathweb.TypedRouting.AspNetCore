using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Strathweb.TypedRouting.AspNetCore
{
    /// <summary>
    /// Typed counterparts of the built in action results that need to point at another action,
    /// replacing the string based <c>CreatedAtRoute</c> / <c>AcceptedAtRoute</c> calls.
    /// </summary>
    public static class TypedControllerExtensions
    {
        public static ActionResult CreatedAtAction<TController>(this ControllerBase controller,
            Expression<Action<TController>> action, object? value = null, object? values = null) =>
            Created(controller, action, value, values);

        public static ActionResult CreatedAtAction<TController>(this ControllerBase controller,
            Expression<Func<TController, Task>> action, object? value = null, object? values = null) =>
            Created(controller, action, value, values);

        public static ActionResult AcceptedAtAction<TController>(this ControllerBase controller,
            Expression<Action<TController>> action, object? value = null, object? values = null) =>
            Accepted(controller, action, value, values);

        public static ActionResult AcceptedAtAction<TController>(this ControllerBase controller,
            Expression<Func<TController, Task>> action, object? value = null, object? values = null) =>
            Accepted(controller, action, value, values);

        private static ActionResult Created(ControllerBase controller, LambdaExpression action, object? value, object? values)
        {
            var link = Resolve(controller, action, values);

            return link.HasRouteName
                ? new CreatedAtRouteResult(link.RouteName, link.Values, value)
                : new CreatedAtActionResult(link.Action, link.Controller, link.AllValues, value);
        }

        private static ActionResult Accepted(ControllerBase controller, LambdaExpression action, object? value, object? values)
        {
            var link = Resolve(controller, action, values);

            return link.HasRouteName
                ? new AcceptedAtRouteResult(link.RouteName, link.Values, value)
                : new AcceptedAtActionResult(link.Action, link.Controller, link.AllValues, value);
        }

        private static ResolvedLink Resolve(ControllerBase controller, LambdaExpression action, object? values)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (action == null) throw new ArgumentNullException(nameof(action));

            return TypedLinkResolver.Resolve(controller.HttpContext.RequestServices, action, values);
        }
    }
}
