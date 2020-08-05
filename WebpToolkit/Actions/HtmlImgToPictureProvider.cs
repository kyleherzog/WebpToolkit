using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.WebTools.Languages.Html.Editor.SuggestedActions;
using Microsoft.WebTools.Languages.Html.Tree.Nodes;
using Microsoft.WebTools.Languages.Html.Tree.Utility;
using Microsoft.WebTools.Languages.Shared.ContentTypes;

namespace WebpToolkit.Actions
{
    [Export(typeof(IHtmlSuggestedActionProvider))]
    [ContentType(HtmlContentTypeDefinition.HtmlContentType)]
    [Name("HTML Image to Picture Light Bulb Provider")]
    internal class HtmlImgToPictureProvider : IHtmlSuggestedActionProvider
    {
        public IEnumerable<ISuggestedAction> GetSuggestedActions(ITextView textView, ITextBuffer textBuffer, int caretPosition, ElementNode element, AttributeNode attribute, HtmlPositionType positionType)
        {
            return new ISuggestedAction[]
            {
                new HtmlImgToPictureAction(textView, textBuffer, element),
            };
        }

        public bool HasSuggestedActions(ITextView textView, ITextBuffer textBuffer, int caretPosition, ElementNode element, AttributeNode attribute, HtmlPositionType positionType)
        {
            if (!element.IsElement("img"))
            {
                return false;
            }

            if (element.Parent.IsElement("picture"))
            {
                return false;
            }

            var src = element.GetAttribute("src");
            if (string.IsNullOrEmpty(src?.Value))
            {
                src = element.GetAttribute("data-src");
            }

            var srcValue = src?.Value?.ToUpperInvariant() ?? string.Empty;

            return !string.IsNullOrEmpty(srcValue) && !srcValue.Contains(".SVG");
        }
    }
}