using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LocMp.Catalog.Api.Filters;

[AttributeUsage(AttributeTargets.Method)]
public sealed class InternalApiKeyAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expected = config["InternalApiKey"];

        if (string.IsNullOrEmpty(expected))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue("X-Internal-Key", out var provided)
            || provided != expected)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}