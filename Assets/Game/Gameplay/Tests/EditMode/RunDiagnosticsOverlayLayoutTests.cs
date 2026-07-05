using Game.Gameplay.Diagnostics;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public sealed class RunDiagnosticsOverlayLayoutTests
{
    [Test]
    public void CreatePanelRect_PortraitPhone_UsesWholeScreenWidth()
    {
        var layout = new RunDiagnosticsOverlayLayout();

        var rect = layout.CreatePanelRect(704f, 1512f);

        Assert.That(rect.x, Is.EqualTo(0f));
        Assert.That(rect.width, Is.EqualTo(704f));
        Assert.That(rect.height, Is.GreaterThan(360f));
        Assert.That(rect.height, Is.LessThanOrEqualTo(400f));
    }

    [Test]
    public void CreatePanelRect_WideScreen_DoesNotUseLegacyWidthCap()
    {
        var layout = new RunDiagnosticsOverlayLayout();

        var rect = layout.CreatePanelRect(1200f, 800f);

        Assert.That(rect.width, Is.EqualTo(1200f));
    }

    [Test]
    public void CreatePanelRect_ShortScreen_KeepsPanelInsideScreen()
    {
        var layout = new RunDiagnosticsOverlayLayout();

        var rect = layout.CreatePanelRect(360f, 240f);

        Assert.That(rect.y + rect.height, Is.LessThanOrEqualTo(240f));
    }
}
