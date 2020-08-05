using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace WebpToolkit.Dialogs
{
    public class OptionsPageGrid : DialogPage
    {
        private int lossyQuality = 90;

        [Category("Lossless Images")]
        [DisplayName("Allow Near-Lossless")]
        [Description("Try to use some preprocessing to help compressibility but with minimal impact to the visual quality.")]
        public bool AllowNearLossless { get; set; } = true;

        [Category("Lossy Images")]
        [DisplayName("Quality Level")]
        [Description("The quality level (1 - 100) to use for lossy file generations.")]
        public int LossyQuality
        {
            get
            {
                return lossyQuality;
            }

            set
            {
                if (value > 100)
                {
                    lossyQuality = 100;
                }
                else
                {
                    lossyQuality = value < 1 ? 1 : value;
                }
            }
        }

        [Category("Target File Exists Behavior")]
        [DisplayName("Overwrite")]
        [Description("Overwrite the target WebP file if it already exists.")]
        public bool IsOverwriteEnabled { get; set; } = true;
    }
}