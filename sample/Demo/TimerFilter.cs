using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Demo
{
    public class TimerFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var start = DateTimeOffset.UtcNow;
            context.HttpContext.Items.Add("start", start);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Items.TryGetValue("start", out var start))
            {
                var end = DateTimeOffset.UtcNow;
                context.HttpContext.Response.Headers.Add("ActionDuration", (end - (DateTimeOffset)start).ToString());
            }
        }
    }
}
