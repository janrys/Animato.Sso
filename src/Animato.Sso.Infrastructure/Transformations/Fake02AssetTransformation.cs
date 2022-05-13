namespace Animato.Sso.Infrastructure.Transformations;
using System.IO;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;

public class Fake02AssetTransformation : BaseTransformation, IAssetTransformation
{
    public Fake02AssetTransformation() : base("Fake02", "Fake transformation, do nothing. No parameters", ASSET_TYPE_ANY)
    {

    }

    public override async Task<Stream> Transform(Stream asset, string parameters = null)
    {
        var ms = new MemoryStream();
        await asset.CopyToAsync(ms);
        return ms;
    }
}
