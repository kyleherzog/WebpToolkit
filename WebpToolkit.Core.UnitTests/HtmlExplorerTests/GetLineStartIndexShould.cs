using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebpToolkit.Core.UnitTests.HtmlExplorerTests;

[TestClass]
public class GetLineStartIndexShould
{
    [DataTestMethod]
    [DataRow(1, 1)]
    [DataRow(2, 23)]
    [DataRow(12, 375)]
    [DataRow(0, 0)]
    [DataRow(-5, 0)]
    [DataRow(99999, 0)]
    public void ReturnXGivenLineNumber(int lineNumber, int expected)
    {
        var explorer = new HtmlExplorer(Properties.Resources.HtmlFragment);
        var result = explorer.GetLineStartIndex(lineNumber);
        Assert.AreEqual(expected, result);
    }
}