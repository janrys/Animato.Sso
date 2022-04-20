namespace Animato.Sso.Application.Common.Interfaces;
using System.Threading.Tasks;

public interface IAssetTransformation
{
    string Code { get; }
    string Description { get; }
    IEnumerable<string> AssetTypes { get; }
    bool CanTransform(string assetType);
    Task<Stream> Transform(Stream asset, string parameters = null);
}
