using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Blog.API.Swagger;

public class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (AllowsAnonymous(context.MethodInfo))
        {
            return;
        }

        if (!RequiresAuthorization(context.MethodInfo))
        {
            return;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference("Bearer", hostDocument: null, externalResource: null)
            ] = []
        });
    }

    private static bool AllowsAnonymous(MethodInfo methodInfo)
    {
        return methodInfo.IsDefined(typeof(AllowAnonymousAttribute), inherit: true)
            || methodInfo.DeclaringType?.IsDefined(typeof(AllowAnonymousAttribute), inherit: true) == true;
    }

    private static bool RequiresAuthorization(MethodInfo methodInfo)
    {
        return methodInfo.IsDefined(typeof(AuthorizeAttribute), inherit: true)
            || methodInfo.DeclaringType?.IsDefined(typeof(AuthorizeAttribute), inherit: true) == true;
    }
}
