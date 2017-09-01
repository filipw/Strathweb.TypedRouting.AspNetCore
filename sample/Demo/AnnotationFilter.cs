using Microsoft.AspNetCore.Mvc.Filters;

namespace Demo
{
    public class AnnotationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add("FilterBefore",typeof(AnnotationFilter).ToString());

        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.Response.Headers.Add("FilterAfter", typeof(AnnotationFilter).ToString());
        }
    }
}
