using System.Linq;
using Content.Client.Stylesheets.Sheetlets;
using Robust.Client.UserInterface;

namespace Content.Client.Stylesheets.Stylesheets;

/// <summary>
///     Основная тема «Гехенна / Доминат Ордината».
///     Наследует структуру правил Nanotrasen, но переопределяет палитру на
///     «уголь / золото / кровь / пергамент» (см. Пример-дизай ДОМИНАТ).
/// </summary>
public sealed partial class GehennaStylesheet : NanotrasenStylesheet
{
    public override string StylesheetName => "Gehenna";

    public GehennaStylesheet(object config, StylesheetManager man) : base(config, man)
    {
        Stylesheet = new Stylesheet(
            Stylesheet.Rules
                .Concat(GetSheetletRules<GehennaStylesheet>(typeof(GehennaThemeSheetlet), man))
                .ToArray());
    }
}
