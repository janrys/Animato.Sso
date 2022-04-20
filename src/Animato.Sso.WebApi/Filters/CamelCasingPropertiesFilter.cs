namespace Animato.Sso.WebApi.Filters;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// Set schema model properties to camelCase naming convention
/// </summary>
public class CamelCasingPropertiesFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        => schema.Properties =
            schema.Properties.ToDictionary(d => string.Concat(d.Key[..1].ToLower(System.Globalization.CultureInfo.InvariantCulture), d.Key.AsSpan(1)), d => d.Value);
}
