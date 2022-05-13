namespace Animato.Sso.Infrastructure.Transformations;
using System.IO;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

public class WatermarkAssetTransformation : BaseTransformation, IAssetTransformation
{
    public WatermarkAssetTransformation() : base("Watermark", "Apply watermark on image. Parameters (text=animato)", "image/jpg", "image/jpeg", "image/png", "image/bmp")
    {

    }
    public override async Task<Stream> Transform(Stream asset, string parameters = null)
    {
        var parsedParameters = ParseParameters(parameters);
        var watermarkParameter = parsedParameters?.FirstOrDefault(p => p.Key.Equals("text", StringComparison.OrdinalIgnoreCase));

        if (!watermarkParameter.HasValue || string.IsNullOrEmpty(watermarkParameter.Value.Value))
        {
            throw new ArgumentNullException(nameof(parameters), "text");
        }

        var watermark = watermarkParameter.Value.Value;

        asset.Position = 0;
        using var image = Image.Load(asset, out var format);
        var font = SystemFonts.CreateFont("Arial", 10);

        using var imgWatermark = image.Clone(ctx => ApplyScalingWaterMark(ctx, font, watermark, Color.WhiteSmoke, 5, true));
        var ms = new MemoryStream();
        await imgWatermark.SaveAsync(ms, format);
        ms.Position = 0;
        return ms;
    }

    private static IImageProcessingContext ApplyScalingWaterMark(IImageProcessingContext processingContext,
            Font font,
            string text,
            Color color,
            float padding,
            bool wordwrap)
    {
        if (wordwrap)
        {
            return ApplyScalingWaterMarkWordWrap(processingContext, font, text, color, padding);
        }
        else
        {
            return ApplyScalingWaterMarkSimple(processingContext, font, text, color, padding);
        }
    }

    private static IImageProcessingContext ApplyScalingWaterMarkSimple(IImageProcessingContext processingContext,
        Font font,
        string text,
        Color color,
        float padding)
    {
        var imgSize = processingContext.GetCurrentSize();

        var targetWidth = imgSize.Width - (padding * 2);

        // measure the text size
        var size = TextMeasurer.Measure(text, new TextOptions(font));

        //find out how much we need to scale the text to fill the space (up or down)
        var scalingFactor = Math.Min(imgSize.Width / size.Width, imgSize.Height / size.Height);

        //create a new font
        var scaledFont = new Font(font, scalingFactor * font.Size);

        var center = new PointF(imgSize.Width / 2, imgSize.Height / 2);

        var textOptions = new TextOptions(scaledFont)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            WrappingLength = targetWidth,
            Origin = center
        };

        return processingContext.DrawText(textOptions, text, color);
    }

    private static IImageProcessingContext ApplyScalingWaterMarkWordWrap(IImageProcessingContext processingContext,
        Font font,
        string text,
        Color color,
        float padding)
    {
        var imgSize = processingContext.GetCurrentSize();
        var targetWidth = imgSize.Width - (padding * 2);
        var targetHeight = imgSize.Height - (padding * 2);

        var targetMinHeight = imgSize.Height - (padding * 3); // must be with in a margin width of the target height

        // now we are working i 2 dimensions at once and can't just scale because it will cause the text to
        // reflow we need to just try multiple times

        var scaledFont = font;
        var s = new FontRectangle(0, 0, float.MaxValue, float.MaxValue);

        var scaleFactor = scaledFont.Size / 2; // every time we change direction we half this size
        var trapCount = (int)scaledFont.Size * 2;
        if (trapCount < 10)
        {
            trapCount = 10;
        }

        var isTooSmall = false;

        while ((s.Height > targetHeight || s.Height < targetMinHeight) && trapCount > 0)
        {
            if (s.Height > targetHeight)
            {
                if (isTooSmall)
                {
                    scaleFactor /= 2;
                }

                scaledFont = new Font(scaledFont, scaledFont.Size - scaleFactor);
                isTooSmall = false;
            }

            if (s.Height < targetMinHeight)
            {
                if (!isTooSmall)
                {
                    scaleFactor /= 2;
                }
                scaledFont = new Font(scaledFont, scaledFont.Size + scaleFactor);
                isTooSmall = true;
            }
            trapCount--;

            s = TextMeasurer.Measure(text, new TextOptions(scaledFont)
            {
                WrappingLength = targetWidth
            });
        }

        var center = new PointF(padding, imgSize.Height / 2);

        var textOptions = new TextOptions(scaledFont)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            WrappingLength = targetWidth,
            Origin = center
        };

        return processingContext.DrawText(textOptions, text, color);
    }
}
