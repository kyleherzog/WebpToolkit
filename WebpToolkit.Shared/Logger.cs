#nullable enable

using System;
using System.Diagnostics;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Shell = Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace WebpToolkit
{
    internal static class Logger
    {
        private static string? name;
        private static IVsOutputWindow? output;
        private static IVsOutputWindowPane? pane;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "VSSDK006:Check services exist", Justification = "False positive.")]
        public static async Task InitializeAsync(Microsoft.VisualStudio.Shell.IAsyncServiceProvider provider, string name)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            output = await provider.GetServiceAsync(typeof(SVsOutputWindow)).ConfigureAwait(true) as IVsOutputWindow;
            Assumes.Present(output);
            Logger.name = name;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Have no idea what exception to expect here.")]
        public static async Task LogToOutputWindowAsync(object message)
        {
            try
            {
                await Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (EnsurePane())
                {
                    pane?.OutputStringThreadSafe(message + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        private static bool EnsurePane()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (pane == null)
            {
                var guid = Guid.NewGuid();
                output?.CreatePane(ref guid, name, 1, 1);
                output?.GetPane(ref guid, out pane);
            }

            return pane != null;
        }
    }
}