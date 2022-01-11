using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using HtmlAgilityPack;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace WebpToolkit;

internal class ImgToPictureSuggestedAction : ISuggestedAction
{
    private bool hasDisposed;

    public ImgToPictureSuggestedAction(ITrackingSpan span, HtmlNode node)
    {
        Span = span;
        Snapshot = span.TextBuffer.CurrentSnapshot;
        LoadPictureElement(node);
        Node = node;
    }

    ~ImgToPictureSuggestedAction()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public string DisplayText => "Convert to WebP picture tag";

    public bool HasActionSets => false;

    public bool HasPreview => true;

    public string IconAutomationText => null;

    public ImageMoniker IconMoniker => default;

    public string InputGestureText => null;

    public HtmlNode Node { get; }

    public string PictureElement { get; private set; }

    public string PictureElementPreview { get; private set; }

    public ITextSnapshot Snapshot { get; }

    public ITrackingSpan Span { get; }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<SuggestedActionSet>>(null);
    }

    public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
    {
        var textBlock = new TextBlock
        {
            Padding = new Thickness(5),
        };
        textBlock.Inlines.Add(new Run() { Text = PictureElementPreview });
        return Task.FromResult<object>(textBlock);
    }

    public void Invoke(CancellationToken cancellationToken)
    {
        Span.TextBuffer.Replace(Span.GetSpan(Snapshot), PictureElement);
    }

    public bool TryGetTelemetryId(out Guid telemetryId)
    {
        telemetryId = Guid.Empty;
        return false;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!hasDisposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects)
            // TODO: set large fields to null
            hasDisposed = true;
        }
    }

    private void GeneratePreview(HtmlNode node)
    {
        var builder = new StringBuilder();
        var padding = new string(' ', node.LinePosition);
        using var reader = new StringReader(PictureElement);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith(padding, StringComparison.Ordinal))
            {
                line = line.Substring(padding.Length);
            }

            builder.AppendLine(line);
        }

        PictureElementPreview = builder.ToString();
    }

    private HtmlTextNode GetLineReturn(HtmlDocument doc, int whiteSpace, int extraWhiteSpace = 0)
    {
        var builder = new StringBuilder(whiteSpace);

        builder.AppendLine();

        for (var i = 0; i < whiteSpace; i++)
        {
            builder.Append(" ");
        }

        var tabWidth = 4;
        for (var i = 0; i < extraWhiteSpace * tabWidth; i++)
        {
            builder.Append(" ");
        }

        return doc.CreateTextNode(builder.ToString());
    }

    private void LoadPictureElement(HtmlNode node)
    {
        var doc = new HtmlDocument
        {
            OptionWriteEmptyNodes = true,
        };
        doc.LoadHtml(node.OuterHtml);
        var img = doc.DocumentNode.SelectSingleNode("/img");

        img.Remove();
        var pic = doc.CreateElement("picture");

        var source = doc.CreateElement("source");
        source.SetAttributeValue("type", "image/webp");

        var src = node.GetAttributeValue("src", null);
        if (!string.IsNullOrEmpty(src))
        {
            source.SetAttributeValue("src", SwapExtensionToWebp(src));
        }

        var dataSrc = node.GetAttributeValue("data-src", null);
        if (!string.IsNullOrEmpty(dataSrc))
        {
            source.SetAttributeValue("data-src", SwapExtensionToWebp(dataSrc));
        }

        pic.AppendChild(GetLineReturn(doc, node.LinePosition, 1));
        pic.AppendChild(source);
        pic.AppendChild(GetLineReturn(doc, node.LinePosition, 1));
        pic.AppendChild(img);
        pic.AppendChild(GetLineReturn(doc, node.LinePosition, 0));

        PictureElement = pic.OuterHtml;
        GeneratePreview(node);
    }

    private string SwapExtensionToWebp(string original)
    {
        var parts = original.Split('?');
        var extension = Path.GetExtension(parts[0]);

        if (Converter.SupportedFileTypes.Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase)))
        {
            parts[0] = Path.ChangeExtension(parts[0], ".webp");
        }

        return string.Join("?", parts);
    }
}