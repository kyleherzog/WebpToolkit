#nullable enable

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace WebpToolkit;

[Export(typeof(ISuggestedActionsSourceProvider))]
[Name("Img to Picture Suggested Actions")]
[ContentType("Razor")]
[ContentType("HTML")]
internal class ImgToPictureSuggestedActionsSourceProvider : ISuggestedActionsSourceProvider
{
    [Import(typeof(ITextStructureNavigatorSelectorService))]
    internal ITextStructureNavigatorSelectorService? NavigatorService { get; set; }

    public ISuggestedActionsSource? CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
    {
        if (textBuffer == null || textView == null)
        {
            return null;
        }

        return new ImgToPictureSuggestedActionSource(textView);
    }
}