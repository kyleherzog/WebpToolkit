using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebpToolkit.Core.UnitTests.HtmlExplorerTests;

[TestClass]
public class GetPositionOfShould
{
    [DataTestMethod]
    [DataRow("div1", 48)]
    [DataRow("img1", 69)]
    public void ReturnXGivenNodeId(string nodeId, int expected)
    {
        var explorer = new HtmlExplorer(Properties.Resources.HtmlFragment);
        var node = explorer.Document.GetElementbyId(nodeId);
        var result = explorer.GetPositionOf(node);
        Assert.AreEqual(expected, result);
    }
}