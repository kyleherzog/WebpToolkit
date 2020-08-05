using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using WebpToolkit.Dialogs;

namespace WebpToolkit.Commands
{
    internal class GenerateCommand
    {
        private readonly IMenuCommandService commandService;
        private readonly DTE2 dte;
        private bool isProcessing;

        private GenerateCommand(DTE2 dte, IMenuCommandService commandService)
        {
            this.dte = dte;
            this.commandService = commandService;

            AddCommand(
                PackageIds.cmdGenerateLossless,
                (s, e) => GenerateImageAsync(false, e).ConfigureAwait(true),
                (s, e) =>
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    GenerateBeforeQueryStatus(s);
                });
            AddCommand(
                PackageIds.cmdGenerateLossy,
                (s, e) => GenerateImageAsync(true, e).ConfigureAwait(true),
                (s, e) =>
                {
                    GenerateBeforeQueryStatus(s);
                });
        }

        public static async Task<IEnumerable<string>> GetSelectedFilePathsAsync(DTE2 dte)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return GetSelectedItemPaths(dte)
                .SelectMany(p => Directory.Exists(p)
                                 ? Directory.EnumerateFiles(p, "*", SearchOption.AllDirectories)
                                 : new[] { p });
        }

        public static IEnumerable<string> GetSelectedItemPaths(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var items = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;

            foreach (UIHierarchyItem selItem in items)
            {
                if (selItem.Object is ProjectItem item && item.Properties != null)
                {
                    yield return item.Properties.Item("FullPath").Value.ToString();
                }
                else if (selItem.Object is Project project && project.Kind != "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")
                {
                    yield return project.GetRootFolder();
                }
                else if (selItem.Object is Solution solution && !string.IsNullOrEmpty(solution.FullName))
                {
                    yield return Path.GetDirectoryName(solution.FullName);
                }
            }
        }

        public static async System.Threading.Tasks.Task<GenerateCommand> InitializeAsync(IAsyncServiceProvider package)
        {
            var menuCommandService = await package.GetServiceAsync(typeof(IMenuCommandService)).ConfigureAwait(true) as IMenuCommandService;
            var dte2 = await package.GetServiceAsync(typeof(DTE)).ConfigureAwait(true) as DTE2;

            return new GenerateCommand(dte2, menuCommandService);
        }

        private void AddCommand(int commandId, EventHandler invokeHandler, EventHandler beforeQueryStatus)
        {
            var cmdId = new CommandID(PackageGuids.guidWebpGeneratorCmdSet, commandId);
            var menuCmd = new OleMenuCommand(invokeHandler, cmdId);
            menuCmd.BeforeQueryStatus += beforeQueryStatus;
            menuCmd.ParametersDescription = "*";
            commandService.AddCommand(menuCmd);
        }

        private async System.Threading.Tasks.Task DisplayEndResultAsync(IList<ConversionResult> list, TimeSpan elapsed, bool isLossy)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var savings = list.Where(r => r?.Processed ?? false).Sum(r => r.Saving);
            var originals = list.Where(r => r?.Processed ?? false).Sum(r => r.OriginalFileSize);
            var results = list.Where(r => r?.Processed ?? false).Sum(r => r.ResultFileSize);

            if (savings > 0)
            {
                var successfulGenerations = list.Count(x => x != null);
                var percent = Math.Round(100 - ((double)results / (double)originals * 100), 1, MidpointRounding.AwayFromZero);
                var image = successfulGenerations == 1 ? "image" : "images";
                var lossyLabel = isLossy ? "lossy" : "lossless";
                var msg = $"{successfulGenerations} {image} generated using {lossyLabel} settings in {Math.Round(elapsed.TotalMilliseconds / 1000, 2)} seconds. Total saving of {savings:N0} bytes / {percent}%";

                dte.StatusBar.Text = msg;
                await Logger.LogToOutputWindowAsync(msg + Environment.NewLine).ConfigureAwait(false);

                var namesSeparator = " -> ";
                var filesGenerated = list.Where(r => r != null && r.Saving > 0);
                var maxLength = filesGenerated.Max(r => Path.GetFileName(r.OriginalFileName).Length + Path.GetFileName(r.ResultFileName).Length + namesSeparator.Length);

                foreach (var result in filesGenerated)
                {
                    var originalName = Path.GetFileName(result.OriginalFileName);
                    var generatedName = Path.GetFileName(result.ResultFileName).PadRight(maxLength);
                    if (result.Processed)
                    {
                        var p = Math.Round(100 - ((double)result.ResultFileSize / (double)result.OriginalFileSize * 100), 1, MidpointRounding.AwayFromZero);
                        await Logger.LogToOutputWindowAsync($"  {originalName}{namesSeparator}{generatedName}\t saving {result.Saving:N0} bytes / {p}%").ConfigureAwait(false);
                    }
                    else
                    {
                        await Logger.LogToOutputWindowAsync($"  {originalName}{namesSeparator}{generatedName} skipped").ConfigureAwait(false);
                    }
                }
            }
            else
            {
                var message = "All images have already been generated.";
                dte.StatusBar.Text = message;
                await Logger.LogToOutputWindowAsync(message).ConfigureAwait(false);
            }
        }

        private void GenerateBeforeQueryStatus(object sender)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var button = (OleMenuCommand)sender;
            var paths = GetSelectedItemPaths(dte);

            button.Visible = paths.Any();
            button.Enabled = true;

            if (button.Visible && isProcessing)
            {
                button.Enabled = false;
            }
        }

        private async System.Threading.Tasks.Task GenerateImageAsync(bool isLossy, EventArgs e)
        {
            isProcessing = true;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IEnumerable<string> files = null;

            // Check command parameters first
            if (e is OleMenuCmdEventArgs cmdArgs && cmdArgs.InValue is string arg)
            {
                var filePath = arg.Trim('"', '\'');

                if (Converter.IsFileSupported(filePath) && File.Exists(filePath))
                {
                    files = new[] { filePath };
                }
            }

            // Then check selected items
            if (files == null)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var filePaths = await GetSelectedFilePathsAsync(dte).ConfigureAwait(true);
                files = filePaths.Where(f => Converter.IsFileSupported(f)).ToArray();
            }

            if (!files.Any())
            {
                isProcessing = false;

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                dte.StatusBar.Text = "No images found to convert.";
                return;
            }

            var list = new ConversionResult[files.Count()];
            var stopwatch = Stopwatch.StartNew();
            var count = files.Count();

            await System.Threading.Tasks.Task.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    var text = count == 1 ? " image" : " images";
                    dte.StatusBar.Progress(true, "Generating " + count + text + "...", AmountCompleted: 1, Total: count + 1);

                    for (var i = 0; i < files.Count(); i++)
                    {
                        var file = files.ElementAt(i);
                        var converter = new Converter
                        {
                            LossyQualityLevel = WebpToolkitPackage.OptionsPage.LossyQuality,
                            AllowOverwrite = WebpToolkitPackage.OptionsPage.IsOverwriteEnabled,
                            AllowNearLossless = WebpToolkitPackage.OptionsPage.AllowNearLossless,
                        };

                        var result = converter.ConvertToWebp(file, isLossy);
                        await HandleResultAsync(result, i + 1).ConfigureAwait(true);

                        if (result.Saving > 0 && result.ResultFileSize > 0 && !string.IsNullOrEmpty(result.ResultFileName))
                        {
                            list[i] = result;
                        }
                    }
                }
                finally
                {
                    dte.StatusBar.Progress(false);
                    stopwatch.Stop();
                    await DisplayEndResultAsync(list, stopwatch.Elapsed, isLossy).ConfigureAwait(false);
                    isProcessing = false;
                }
            }).ConfigureAwait(false);
        }

        private async System.Threading.Tasks.Task HandleResultAsync(ConversionResult result, int count)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var originalName = Path.GetFileName(result.OriginalFileName);
            var generatedName = Path.GetFileName(result.ResultFileName);

            if (result.Processed)
            {
                if (dte.SourceControl.IsItemUnderSCC(result.ResultFileName) && !dte.SourceControl.IsItemCheckedOut(result.ResultFileName))
                {
                    dte.SourceControl.CheckOutItem(result.ResultFileName);
                }

                var text = $"Compressed {originalName} to {generatedName} saving {result.Saving} bytes / {result.Percent}%";
                dte.StatusBar.Progress(true, text, count, count + 1);
            }
            else
            {
                dte.StatusBar.Progress(true, generatedName + " already exists", AmountCompleted: count, Total: count + 1);
            }
        }
    }
}