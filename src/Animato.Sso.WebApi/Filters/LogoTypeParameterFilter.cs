namespace Animato.Sso.WebApi.Filters;

using Animato.Sso.Application.Models;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class LogoTypeParameterFilter : IParameterFilter
{

    public LogoTypeParameterFilter()
    {
    }

    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (parameter.Name.Equals("logoType", StringComparison.OrdinalIgnoreCase))
        {
            parameter.Schema.Enum = LogoType.GetAll().Select(p => new OpenApiString(p.Name)).ToList<IOpenApiAny>();
        }
    }
}
