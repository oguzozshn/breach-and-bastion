using Godot;

namespace BreachAndBastion.Game;

/// <summary>Period-inspired, readable HUD (02-visual-style): ink on parchment panels.</summary>
public partial class Hud : CanvasLayer
{
    private static readonly Color Parchment = new(0.93f, 0.89f, 0.78f, 0.93f);
    private static readonly Color Ink = new(0.17f, 0.12f, 0.07f);

    private Label _status = null!;
    private Label _rangeTable = null!;
    private Label _banner = null!;

    public override void _Ready()
    {
        var font = new SystemFont { FontNames = ["Consolas", "Courier New", "monospace"] };
        _status = AddPanel(font, new Vector2(16, 16));
        _rangeTable = AddPanel(font, new Vector2(-16, 16), alignRight: true);

        _banner = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _banner.AddThemeFontSizeOverride("font_size", 34);
        _banner.AddThemeColorOverride("font_color", new Color(0.62f, 0.08f, 0.05f));
        _banner.AddThemeColorOverride("font_outline_color", Parchment);
        _banner.AddThemeConstantOverride("outline_size", 8);
        _banner.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
        _banner.Position = new Vector2(-300, 40);
        _banner.Size = new Vector2(600, 50);
        AddChild(_banner);
    }

    // StringBuilder.AppendLine writes CR LF on Windows, which a Label shows as two breaks.
    public void SetStatus(string text) => _status.Text = text.Replace("\r", "");
    public void SetRangeTable(string text) => _rangeTable.Text = text.Replace("\r", "");
    public void SetBanner(string text) => _banner.Text = text;

    private Label AddPanel(Font font, Vector2 offset, bool alignRight = false)
    {
        var panel = new PanelContainer();
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = Parchment,
            BorderColor = Ink,
            BorderWidthBottom = 2, BorderWidthTop = 2, BorderWidthLeft = 2, BorderWidthRight = 2,
            ContentMarginLeft = 12, ContentMarginRight = 12, ContentMarginTop = 8, ContentMarginBottom = 8,
        });
        var label = new Label();
        label.AddThemeFontOverride("font", font);
        label.AddThemeFontSizeOverride("font_size", 15);
        label.AddThemeColorOverride("font_color", Ink);
        panel.AddChild(label);
        AddChild(panel);

        if (alignRight)
        {
            // Anchored to the top-right corner, growing leftwards as the text widens.
            panel.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            panel.GrowHorizontal = Control.GrowDirection.Begin;
            panel.OffsetLeft = offset.X;
            panel.OffsetRight = offset.X;
            panel.OffsetTop = offset.Y;
        }
        else
        {
            panel.Position = offset;
        }
        return label;
    }
}
