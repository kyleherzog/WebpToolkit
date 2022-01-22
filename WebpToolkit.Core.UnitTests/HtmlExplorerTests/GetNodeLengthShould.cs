using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebpToolkit.Core.UnitTests.HtmlExplorerTests;

[TestClass]
public class GetNodeLengthShould
{
    [DataTestMethod]
    [DataRow("div1", 333)]
    [DataRow("img1", 62)]
    [DataRow("b1", 19)]
    public void ReturnXGivenNodeId(string nodeId, int expected)
    {
        var explorer = new HtmlExplorer(Properties.Resources.HtmlFragment);
        var node = explorer.Document.GetElementbyId(nodeId);
        var result = explorer.GetNodeLength(node);
        Assert.AreEqual(expected, result);
    }
}