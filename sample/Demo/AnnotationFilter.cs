using Microsoft.AspNetCore.Mvc.Filters;

namespace Demo
{
    public class AnnotationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Response.Headers.Append("FilterBefore",typeof(AnnotationFilter).ToString());

        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.Response.Headers.Append("FilterAfter", typeof(AnnotationFilter).ToString());
        }
    }
}
