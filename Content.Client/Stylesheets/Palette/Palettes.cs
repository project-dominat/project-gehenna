namespace Content.Client.Stylesheets.Palette;

/// <summary>
///     Stores all style palettes in one accessible location
/// </summary>
/// <remarks>
///     Technically not limited to only colors, can store like, standard padding amounts, and font sizes, maybe?
/// </remarks>
public static class Palettes
{
    // muted tones
    public static readonly ColorPalette Navy = ColorPalette.FromHexBase("#4f5376", lightnessShift: 0.05f, chromaShift: 0.0045f);
    public static readonly ColorPalette Cyan = ColorPalette.FromHexBase("#42586a", lightnessShift: 0.05f, chromaShift: 0.0045f);
    public static readonly ColorPalette Slate = ColorPalette.FromHexBase("#545562");
    public static readonly ColorPalette Neutral = ColorPalette.FromHexBase("#555555");

    // status tones
    public static readonly ColorPalette Red = ColorPalette.FromHexBase("#b62124", chromaShift: 0.02f);
    public static readonly ColorPalette Amber = ColorPalette.FromHexBase("#c18e36");
    public static readonly ColorPalette Green = ColorPalette.FromHexBase("#3c854a");
    public static readonly StatusPalette Status = new([Red.Base, Amber.Base, Green.Base]);

    // highlight tones
    public static readonly ColorPalette Gold = ColorPalette.FromHexBase("#a88b5e");
    public static readonly ColorPalette Maroon = ColorPalette.FromHexBase("#9b2236");

    // Gehenna / Dominat Ordinata palette — «тоталитарный архив»
    // Coal: глубокий чёрный/уголь, базовый фон панелей
    public static readonly ColorPalette Coal = ColorPalette.FromHexBase("#14141a", lightnessShift: 0.035f);
    // DeepGold: золото Домината — акцент, рамки, активные элементы
    public static readonly ColorPalette DeepGold = ColorPalette.FromHexBase("#b08840", lightnessShift: 0.05f, chromaShift: 0.005f);
    // Blood: запрет/угроза, классификационная плашка
    public static readonly ColorPalette Blood = ColorPalette.FromHexBase("#6b0f0f", lightnessShift: 0.05f, chromaShift: 0.01f);
    // Parchment: пергаментный текст (теплый кремовый)
    public static readonly ColorPalette Parchment = ColorPalette.FromHexBase("#d4cdb8");

    // Intended to be used with `ModulateSelf` to darken / lighten something
    public static readonly ColorPalette AlphaModulate = ColorPalette.FromHexBase("#ffffff");

}
