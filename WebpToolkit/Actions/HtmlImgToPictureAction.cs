using System;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.WebTools.Languages.Html.Editor.SuggestedActions;
using Microsoft.WebTools.Languages.Html.Tree.Nodes;

namespace WebpToolkit.Actions
{
    public class HtmlImgToPictureAction : HtmlSuggestedActionBase
    {
        public HtmlImgToPictureAction(ITextView textView, ITextBuffer textBuffer, ElementNode element)
            : base(textView, textBuffer, element, "Convert to WebP Picture Element", Guid.Empty)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD001:Avoid legacy thread switching APIs", Justification = "False positive.")]
        public override void Invoke(CancellationToken cancellationToken)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                try
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    WebpToolkitPackage.Dte.UndoContext.Open(DisplayText);
                    var img = Element.GetText(Element.OuterRange).Trim();

                    var src = Element.GetAttribute("src");
                    var dataSrc = Element.GetAttribute("data-src");

                    var sourceBulder = new StringBuilder();
                    sourceBulder.Append("<source");
                    if (src?.HasValue() ?? false)
                    {
                        var webpSrc = src.Value;
                        webpSrc = webpSrc.Substring(0, webpSrc.LastIndexOf('.'));
                        sourceBulder.Append($" srcset=\"{webpSrc}.webp\"");
                    }

                    if (dataSrc?.HasValue() ?? false)
                    {
                        var webpSrc = dataSrc.Value;
                        webpSrc = webpSrc.Substring(0, webpSrc.LastIndexOf('.'));
                        sourceBulder.Append($" data-srcset=\"{webpSrc}.webp\"");
                    }

                    sourceBulder.Append(" type=\"image/webp\" />");

                    var picture = $"<picture>\n{sourceBulder}\n{img}</picture>\n";

                    using var edit = TextBuffer.CreateEdit();
                    edit.Replace(new Span(Element.Start, Element.Length), picture);
                    edit.Apply();

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    Dispatcher.CurrentDispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            WebpToolkitPackage.Dte.ExecuteCommand("Edit.FormatDocument");
                        }),
                        DispatcherPriority.ApplicationIdle,
                        null).Task.FileAndForget("Edit.FormatDocument");
                }
                finally
                {
                    WebpToolkitPackage.Dte.UndoContext.Close();
                }
            });
        }
    }
}