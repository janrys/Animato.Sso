namespace Animato.Sso.Application.Common.Interfaces;

public interface IAssetTransformationFactory
{
    Task<IEnumerable<IAssetTransformation>> LoadTransformations();
    IEnumerable<IAssetTransformation> GetTransformations();
    IEnumerable<IAssetTransformation> GetTransformations(string assetType);
    IEnumerable<IAssetTransformation> GetTransformations(string assetType, string code);
}
