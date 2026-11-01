using System.Linq;
using QmTui.UI;
using Xunit;

namespace QmTui.Tests;

/// <summary>
/// 帮助弹窗文案回归：锁住本次补上的键位不丢失，并保证同组内键位标签唯一、文案不被弹窗宽度裁掉。
/// </summary>
public class HelpDialogTests
{
    // ── 过去有功能却从未被提示过的键位 ───────────────────────────

    [Theory]
    [InlineData("Space")]
    [InlineData("Tab / S-Tab")]
    [InlineData("/  F3  C-V")]
    [InlineData("Q")]
    [InlineData("Backspace")]
    [InlineData("Home")]
    [InlineData("N")]
    public void PreviouslyUnadvertisedKeys_AreListedInHelp(string keyLabel)
    {
        Assert.Contains(HelpDialog.KeysHelpLines.SelectMany(g => g.Rows), r => r.Key == keyLabel);
    }

    // ── 文案完整性 ───────────────────────────────────────────────

    [Fact]
    public void KeyLabels_AreUniqueWithinEachGroup()
    {
        foreach (var group in HelpDialog.KeysHelpLines)
        {
            var labels = group.Rows.Select(r => r.Key).ToList();
            Assert.NotEmpty(labels);
            Assert.Equal(labels.Count, labels.Distinct().Count());
        }
    }

    [Fact]
    public void HelpLines_FitDialogWidth()
    {
        // 弹窗宽 76 列，CJK 按 2 列折算，超出部分会被终端裁掉
        int tooWide = HelpDialog.BuildDisplayLines()
                                .Count(line => line.Sum(ch => ch > 127 ? 2 : 1) > 74);

        Assert.Equal(0, tooWide);
    }
}
