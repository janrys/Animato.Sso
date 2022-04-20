namespace Animato.Sso.Infrastructure.Transformations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;

public class StaticAssetTransformationFactory : IAssetTransformationFactory
{
    private readonly List<IAssetTransformation> transformations;
    public StaticAssetTransformationFactory(IEnumerable<IAssetTransformation> transformations) : this(transformations.ToArray())
    {

    }

    public StaticAssetTransformationFactory(params IAssetTransformation[] transformations)
    {
        this.transformations = new List<IAssetTransformation>();

        if (transformations is not null && transformations.Any())
        {
            this.transformations.AddRange(transformations);
        }
    }

    public virtual Task<IEnumerable<IAssetTransformation>> LoadTransformations() => Task.FromResult(GetTransformations());

    public IEnumerable<IAssetTransformation> GetTransformations() => transformations;
    public IEnumerable<IAssetTransformation> GetTransformations(string assetType) => transformations.Where(t => t.CanTransform(assetType));
    public IEnumerable<IAssetTransformation> GetTransformations(string assetType, string code)
    {
        code = RemoveParameters(code);
        return transformations.Where(t => t.CanTransform(assetType) && t.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    private string RemoveParameters(string codeWithParameters)
    {
        if (string.IsNullOrEmpty(codeWithParameters) || !codeWithParameters.Contains('(', StringComparison.OrdinalIgnoreCase))
        {
            return codeWithParameters;
        }

        return codeWithParameters[..codeWithParameters.IndexOf('(')];
    }
}
