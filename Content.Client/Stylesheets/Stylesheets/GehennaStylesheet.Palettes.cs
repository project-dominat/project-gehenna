using Content.Client.Stylesheets.Palette;

namespace Content.Client.Stylesheets.Stylesheets;

public sealed partial class GehennaStylesheet
{
    // Тёмный уголь вместо темно-синего Navy.
    public override ColorPalette PrimaryPalette => Palettes.Coal;
    // Вторичная — тот же уголь, но «альт»-блок светлее (решается через sheetlet'ы).
    public override ColorPalette SecondaryPalette => Palettes.Neutral;
    public override ColorPalette PositivePalette => Palettes.Green;
    // Кровь: ошибки / запреты / классификационная плашка.
    public override ColorPalette NegativePalette => Palettes.Blood;
    // Золото домината: активные элементы, рамки, заголовки.
    public override ColorPalette HighlightPalette => Palettes.DeepGold;
}
