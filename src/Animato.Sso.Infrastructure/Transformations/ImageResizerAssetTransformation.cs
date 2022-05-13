namespace Animato.Sso.Infrastructure.Transformations;
using System.IO;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

public class ImageResizerAssetTransformation : BaseTransformation, IAssetTransformation
{
    public ImageResizerAssetTransformation() : base("Resize", "Resize image to specified width and height. Parameters (width=xx,height=yy)", "image/jpg", "image/jpeg", "image/png", "image/bmp")
    {

    }
    public override async Task<Stream> Transform(Stream asset, string parameters = null)
    {
        var parsedParameters = ParseParameters(parameters);
        var widthParameter = parsedParameters?.FirstOrDefault(p => p.Key.Equals("width", StringComparison.OrdinalIgnoreCase));
        var heightParameter = parsedParameters?.FirstOrDefault(p => p.Key.Equals("height", StringComparison.OrdinalIgnoreCase));

        if (!widthParameter.HasValue || string.IsNullOrEmpty(widthParameter.Value.Value)
            || !int.TryParse(widthParameter.Value.Value, out var width))
        {
            throw new ArgumentNullException(nameof(parameters), "width");
        }

        if (!heightParameter.HasValue || string.IsNullOrEmpty(heightParameter.Value.Value)
            || !int.TryParse(heightParameter.Value.Value, out var height))
        {
            throw new ArgumentNullException(nameof(parameters), "height");
        }

        asset.Position = 0;
        using var image = Image.Load(asset, out var format);
        image.Mutate(x => x.Resize(width, height));
        var ms = new MemoryStream();
        await image.SaveAsync(ms, format);
        ms.Position = 0;
        return ms;
    }
}
