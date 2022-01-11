using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using WebpToolkit.Core;

namespace WebpToolkit;

internal class ImgToPictureSuggestedActionSource : ISuggestedActionsSource
{
    private readonly ITextView currentView;
    private bool hasDisposed;

    public ImgToPictureSuggestedActionSource(ITextView textView)
    {
        currentView = textView;
    }

    ~ImgToPictureSuggestedActionSource()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public event EventHandler<EventArgs> SuggestedActionsChanged;

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
    {
        var explorer = new HtmlExplorer(currentView.TextBuffer.CurrentSnapshot.GetText());
        var node = GetConvertableImgNode(explorer);
        if (node != null)
        {
            var trackingSpan = currentView.TextBuffer.CurrentSnapshot.CreateTrackingSpan(explorer.GetPositionOf(node) - 1, explorer.GetNodeLength(node), SpanTrackingMode.EdgeInclusive);
            var pictureAction = new ImgToPictureSuggestedAction(trackingSpan, node);
            return new SuggestedActionSet[] { new SuggestedActionSet(PredefinedSuggestedActionCategoryNames.Refactoring, new ISuggestedAction[] { pictureAction }) };
        }

        return Enumerable.Empty<SuggestedActionSet>();
    }

    public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(
            () =>
            {
                var explorer = new HtmlExplorer(currentView.TextBuffer.CurrentSnapshot.GetText());
                var node = GetConvertableImgNode(explorer);
                return node != null;
            },
            cancellationToken,
            TaskCreationOptions.None,
            TaskScheduler.Default);
    }

    public bool TryGetTelemetryId(out Guid telemetryId)
    {
        // This is a sample provider and doesn't participate in LightBulb telemetry
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

    private HtmlNode GetConvertableImgNode(HtmlExplorer explorer)
    {
        var caret = currentView.Caret;

        if (caret.Position.BufferPosition < 1)
        {
            return null;
        }

        var point = caret.Position.BufferPosition - 1;

        var snapshot = currentView.TextBuffer.CurrentSnapshot;
        var line = snapshot.GetLineFromPosition(point);
        var linePosition = point - line.Start;

        var node = explorer.GetNodeAt(line.LineNumber + 1, linePosition);

        if (node?.Name != "img" || node.ParentNode?.Name == "picture")
        {
            return null;
        }

        var src = node.GetAttributeValue("src", null)
            ?? node.GetAttributeValue("data-src", null);

        if (string.IsNullOrEmpty(src))
        {
            return null;
        }

        if (src.Contains("?"))
        {
            src = src.Substring(0, src.IndexOf("?"));
        }

        var ext = Path.GetExtension(src);
        if (ext.Equals(".webp", StringComparison.OrdinalIgnoreCase) || ext.Equals(".svg", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return node;
    }
}