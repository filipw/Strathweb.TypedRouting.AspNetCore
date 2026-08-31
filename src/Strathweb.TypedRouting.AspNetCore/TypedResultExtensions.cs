using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Strathweb.TypedRouting.AspNetCore
{
    /// <summary>
    /// Minimal API results that point at another endpoint. The built-in <c>Results.CreatedAtRoute</c>
    /// can only address an endpoint by name, which forces every link target to be named; these
    /// address the handler itself instead.
    /// </summary>
    public static class TypedResultExtensions
    {
        public static IResult CreatedAtHandler(this IResultExtensions extensions,
            Expression<Action> handler, object? value = null, object? values = null) =>
            new TypedLinkResult(handler, value, values, StatusCodes.Status201Created);

        public static IResult CreatedAtHandler(this IResultExtensions extensions,
            Expression<Func<Task>> handler, object? value = null, object? values = null) =>
            new TypedLinkResult(handler, value, values, StatusCodes.Status201Created);

        public static IResult AcceptedAtHandler(this IResultExtensions extensions,
            Expression<Action> handler, object? value = null, object? values = null) =>
            new TypedLinkResult(handler, value, values, StatusCodes.Status202Accepted);

        public static IResult AcceptedAtHandler(this IResultExtensions extensions,
            Expression<Func<Task>> handler, object? value = null, object? values = null) =>
            new TypedLinkResult(handler, value, values, StatusCodes.Status202Accepted);

        private sealed class TypedLinkResult : IResult
        {
            private readonly LambdaExpression _handler;
            private readonly object? _value;
            private readonly object? _values;
            private readonly int _statusCode;

            public TypedLinkResult(LambdaExpression handler, object? value, object? values, int statusCode)
            {
                _handler = handler ?? throw new ArgumentNullException(nameof(handler));
                _value = value;
                _values = values;
                _statusCode = statusCode;
            }

            // the link is resolved during execution, so the caller never needs an HttpContext
            public Task ExecuteAsync(HttpContext httpContext)
            {
                var (method, routeValues) = TypedHandlerLinkGeneratorExtensions.Resolve(httpContext, _handler, _values);

                var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
                var location = linkGenerator.GetUriByAddress(httpContext, method, routeValues,
                    scheme: httpContext.Request.Scheme, host: httpContext.Request.Host);

                var result = _statusCode == StatusCodes.Status201Created
                    ? Results.Created(location ?? string.Empty, _value)
                    : Results.Accepted(location, _value);

                return result.ExecuteAsync(httpContext);
            }
        }
    }
}
