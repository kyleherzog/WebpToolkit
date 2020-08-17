using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace WebpToolkit
{
    public class Converter
    {
        private static readonly string[] supportedFileTypes = { ".png", ".jpg", ".jpeg", ".gif", ".tiff" };

        private readonly string workingDirectory;

        public Converter()
        {
            workingDirectory = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), @"Resources\Tools\");
        }

        public int LossyQualityLevel { get; set; }

        public bool AllowNearLossless { get; set; }

        public bool AllowOverwrite { get; set; }

        public static bool IsFileSupported(string fileName)
        {
            var ext = Path.GetExtension(fileName);

            return supportedFileTypes.Any(s => s.Equals(ext, StringComparison.OrdinalIgnoreCase));
        }

        public ConversionResult ConvertToWebp(string fileName, bool isLossy)
        {
            return ConvertImage(fileName, isLossy);
        }

        private static string GetGifArguments(string fileName, string targetName, bool isLossy)
        {
            var builder = new StringBuilder();
            builder.Append("/c gif2webp ");

            builder.Append($"\"{fileName}\"");

            builder.Append(" -o ");
            builder.Append($"\"{targetName}\"");

            builder.Append(" -min_size");

            if (isLossy)
            {
                builder.Append(" -mixed");
            }

            return builder.ToString();
        }

        private string GetImageArguments(string fileName, string targetName, bool isLossy, bool allowNearLossless)
        {
            var builder = new StringBuilder();
            builder.Append("/c cwebp ");

            builder.Append($"\"{fileName}\"");

            builder.Append(" -o ");
            builder.Append($"\"{targetName}\"");

            if (!isLossy)
            {
                if (allowNearLossless)
                {
                    builder.Append(" -near_lossless 50");
                }
                else
                {
                    builder.Append(" -lossless -q 100");
                }
            }
            else
            {
                builder.Append($" -q {LossyQualityLevel}");
            }

            return builder.ToString();
        }

        private ConversionResult ConvertImage(string fileName, bool isLossy)
        {
            var stopwatch = Stopwatch.StartNew();
            var targetName = Path.ChangeExtension(fileName, ".webp");
            if (!AllowOverwrite && File.Exists(targetName))
            {
                return new ConversionResult(fileName, targetName, stopwatch.Elapsed, false);
            }

            var fileExtension = Path.GetExtension(fileName);
            var isGif = fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase);
            var arguments = isGif
                ? GetGifArguments(fileName, targetName, isLossy)
                : GetImageArguments(fileName, targetName, isLossy, false);

            ConvertFile(arguments);

            // see if we can get a smaller file size using near lossless conversion
            if (!isGif && !isLossy && AllowNearLossless)
            {
                var altTargetName = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(fileName));
                arguments = GetImageArguments(fileName, altTargetName, false, true);
                ConvertFile(arguments);
                var targetInfo = new FileInfo(targetName);
                var altInfo = new FileInfo(altTargetName);
                if (altInfo.Length < targetInfo.Length)
                {
                    targetInfo.Delete();
                    altInfo.MoveTo(targetName);
                }
                else
                {
                    altInfo.Delete();
                }
            }

            stopwatch.Stop();
            return new ConversionResult(fileName, targetName, stopwatch.Elapsed);
        }

        private void ConvertFile(string arguments)
        {
            var start = new ProcessStartInfo("cmd")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(start);
            process.WaitForExit();
        }
    }
}