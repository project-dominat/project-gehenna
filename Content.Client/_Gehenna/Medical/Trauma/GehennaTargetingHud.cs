using System.Numerics;
using Content.Shared._Gehenna.Medical.Trauma;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._Gehenna.Medical.Trauma;

public sealed class GehennaTargetingHud : PanelContainer
{
    public event Action<GehennaBodyZone>? ZonePressed;

    private readonly Dictionary<GehennaBodyZone, Button> _buttons = new();
    private readonly Label _currentZone;

    public GehennaTargetingHud()
    {
        MinSize = new Vector2(142, 170);
        Margin = new Thickness(0, 0, 10, 190);

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new Thickness(6),
            SeparationOverride = 4,
        };

        AddChild(root);

        _currentZone = new Label
        {
            Text = Loc.GetString("gehenna-target-zone-torso"),
            HorizontalAlignment = HAlignment.Center,
            StyleClasses = { "LabelHeading" },
        };
        root.AddChild(_currentZone);

        root.AddChild(CreateRow((GehennaBodyZone.Head, "H")));
        root.AddChild(CreateRow((GehennaBodyZone.LeftArm, "LA"), (GehennaBodyZone.Torso, "T"), (GehennaBodyZone.RightArm, "RA")));
        root.AddChild(CreateRow((GehennaBodyZone.LeftLeg, "LL"), (GehennaBodyZone.RightLeg, "RL")));
    }

    public void SetZone(GehennaBodyZone zone)
    {
        _currentZone.Text = Loc.GetString($"gehenna-target-zone-{zone.ToString().ToLowerInvariant()}");

        foreach (var (buttonZone, button) in _buttons)
        {
            button.Pressed = buttonZone == zone;
        }
    }

    private BoxContainer CreateRow(params (GehennaBodyZone Zone, string Text)[] entries)
    {
        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalAlignment = HAlignment.Center,
            SeparationOverride = 4,
        };

        foreach (var (zone, text) in entries)
        {
            var button = new Button
            {
                Text = text,
                ToggleMode = true,
                SetSize = new Vector2(40, 32),
                ToolTip = Loc.GetString($"gehenna-target-zone-{zone.ToString().ToLowerInvariant()}"),
            };

            button.OnPressed += _ => ZonePressed?.Invoke(zone);
            _buttons[zone] = button;
            row.AddChild(button);
        }

        return row;
    }
}
