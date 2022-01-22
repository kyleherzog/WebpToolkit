using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebpToolkit.Core.UnitTests.HtmlExplorerTests;

[TestClass]
public class GetNodeAtShould
{
    [DataTestMethod]
    [DataRow(5, 4, "#text")]
    [DataRow(5, 20, "img")]
    [DataRow(7, 13, "img")]
    [DataRow(9, 1, "#text")]
    [DataRow(9, 6, "b")]
    [DataRow(10, 10, "img")]
    [DataRow(11, 6, "i")]
    [DataRow(11, 34, "i")]
    public void ReturnTagGivenPositionInFragment(int line, int linePosition, string expectedTagName)
    {
        var explorer = new HtmlExplorer(Properties.Resources.HtmlFragment);
        var result = explorer.GetNodeAt(line, linePosition);
        Assert.AreEqual(expectedTagName, result?.Name);
    }
}