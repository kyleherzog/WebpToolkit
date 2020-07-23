using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;

namespace WebpToolkit
{
    public static class Converter
    {
        private static readonly string[] supportedFileTypes = { ".png", ".jpg", ".jpeg", ".gif" };

        public static bool IsFileSupported(string fileName)
        {
            var ext = Path.GetExtension(fileName);

            return supportedFileTypes.Any(s => s.Equals(ext, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<ConversionResult> ConvertToWebpAsync(string fileName, int quality)
        {
            var stopwatch = Stopwatch.StartNew();

            using var bmp = SKBitmap.Decode(fileName);
            using var webp = bmp.Encode(SKEncodedImageFormat.Webp, quality).AsStream(true);

            var webpFileName = $"{Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName))}.webp";
            if (File.Exists(webpFileName))
            {
                if (WebpToolkitPackage.OptionsPage.IsOverwriteEnabled)
                {
                    File.Delete(webpFileName);
                }
                else
                {
                    return new ConversionResult(fileName, webpFileName, stopwatch.Elapsed, false);
                }
            }

            using var fileStream = File.Create(webpFileName);
            await webp.CopyToAsync(fileStream).ConfigureAwait(false);
            await fileStream.FlushAsync().ConfigureAwait(false);
            stopwatch.Stop();
            return new ConversionResult(fileName, webpFileName, stopwatch.Elapsed);
        }
    }
}