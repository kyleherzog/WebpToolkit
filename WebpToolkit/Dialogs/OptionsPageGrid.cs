using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace WebpToolkit.Dialogs
{
    public class OptionsPageGrid : DialogPage
    {
        private int bestCompression = 50;
        private int bestQuality = 80;

        [Category("Image Quality")]
        [DisplayName("Best Compression")]
        [Description("The quality level (1 - 100) to use for best compression file generations.")]
        public int BestCompression
        {
            get
            {
                return bestCompression;
            }

            set
            {
                if (value > 100)
                {
                    bestCompression = 100;
                }
                else if (value < 1)
                {
                    bestCompression = 1;
                }
                else
                {
                    bestCompression = value;
                }
            }
        }

        [Category("Image Quality")]
        [DisplayName("Best Quality")]
        [Description("The quality level (1 - 100) to use for best quality file generations.")]
        public int BestQuality
        {
            get
            {
                return bestQuality;
            }

            set
            {
                if (value > 100)
                {
                    bestQuality = 100;
                }
                else if (value < 1)
                {
                    bestQuality = 1;
                }
                else
                {
                    bestQuality = value;
                }
            }
        }

        [Category("Target File Exists Behavior")]
        [DisplayName("Overwrite")]
        [Description("Overwrite the target WebP file if it already exists.")]
        public bool IsOverwriteEnabled { get; set; } = true;
    }
}