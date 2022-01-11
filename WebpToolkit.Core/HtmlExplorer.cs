using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

namespace WebpToolkit.Core;

public class HtmlExplorer
{
    private IList<HtmlNode>? allNodes;

    private IList<string>? sourceLines;

    public HtmlExplorer(string html)
    {
        Document = new HtmlDocument();
        Document.LoadHtml(html);
    }

    public HtmlExplorer(HtmlDocument document)
    {
        Document = document;
    }

    public IList<HtmlNode> AllNodes
    {
        get
        {
            if (allNodes == null)
            {
                var docNode = Document.DocumentNode;
                var descendants = docNode.DescendantsAndSelf();
                var endNodes = descendants.Where(x => x.EndNode != x).Select(x => x.EndNode);

                var nodes = new List<HtmlNode>();
                nodes.AddRange(descendants);
                nodes.AddRange(endNodes);
                nodes = nodes.OrderBy(x => x.Line).ThenBy(x => x.LinePosition).ToList();

                allNodes = nodes;
            }

            return allNodes;
        }
    }

    public HtmlDocument Document { get; }

    public IList<string> SourceLines
    {
        get
        {
            if (sourceLines == null)
            {
                sourceLines = new List<string>();
                using var reader = new StringReader(Document.Text);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    sourceLines.Add(line);
                }
            }

            return sourceLines;
        }
    }

    public int GetLineStartIndex(int lineNumber)
    {
        if (lineNumber < 1 || lineNumber > SourceLines.Count)
        {
            return 0;
        }

        if (lineNumber == 1)
        {
            return 1;
        }

        return SourceLines.Take(lineNumber - 1).Sum(x => x.Length)
            + (lineNumber - 1) * 2 // \r\n
            + 1;
    }

    public HtmlNode? GetNextNode(HtmlNode? node)
    {
        if (node == null || node == AllNodes.Last())
        {
            return null;
        }

        var index = AllNodes.IndexOf(node);
        if (index == -1)
        {
            return null;
        }

        return AllNodes[index + 1];
    }

    public HtmlNode? GetNodeAt(int line, int linePosition)
    {
        var nodes = AllNodes.Where(x => x.Line == line && x.LinePosition + 1 <= linePosition);
        while (!nodes.Any() && line-- > 0)
        {
            nodes = AllNodes.Where(x => x.Line == line);
        }
        return nodes.OrderByDescending(x => x.LinePosition).First();
    }

    public int GetNodeLength(HtmlNode node)
    {
        if (node == null)
        {
            return 0;
        }

        var start = GetPositionOf(node);
        var nextNode = GetNextNode(node.EndNode);

        var end = nextNode == null
            ? Document.Text.Length + 1
            : GetPositionOf(nextNode);

        return end - start;
    }

    public int GetPositionOf(HtmlNode node)
    {
        if (node == null)
        {
            return 0;
        }

        var lineStart = GetLineStartIndex(node.Line);

        if (lineStart == 0)
        {
            return 0;
        }

        return lineStart + node.LinePosition;
    }
}