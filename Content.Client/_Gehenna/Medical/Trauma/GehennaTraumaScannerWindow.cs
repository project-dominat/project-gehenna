using System.Numerics;
using Content.Shared._Gehenna.Medical.Trauma;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Utility;

namespace Content.Client._Gehenna.Medical.Trauma;

public sealed class GehennaTraumaScannerWindow : DefaultWindow
{
    private readonly Label _scanMode;
    private readonly Label _name;
    private readonly Label _species;
    private readonly Label _status;
    private readonly Label _blood;
    private readonly Label _bleeding;
    private readonly GehennaBodyMapControl _bodyMap;
    private readonly BoxContainer _wounds;

    public GehennaTraumaScannerWindow()
    {
        MinSize = SetSize = new Vector2(650, 460);

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            Margin = new Thickness(8),
        };

        Contents.AddChild(root);

        var left = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            MinWidth = 230,
            SeparationOverride = 6,
        };

        var right = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 4,
        };

        root.AddChild(left);
        root.AddChild(new Control { MinWidth = 8 });
        root.AddChild(right);

        _scanMode = AddLine(left, "gehenna-trauma-scanner-scan-mode", string.Empty);
        _name = AddLine(left, "gehenna-trauma-scanner-name", string.Empty);
        _species = AddLine(left, "gehenna-trauma-scanner-species", string.Empty);
        _status = AddLine(left, "gehenna-trauma-scanner-status", string.Empty);
        _blood = AddLine(left, "gehenna-trauma-scanner-blood", string.Empty);
        _bleeding = AddLine(left, "gehenna-trauma-scanner-bleeding", string.Empty);

        _bodyMap = new GehennaBodyMapControl();
        left.AddChild(_bodyMap);

        right.AddChild(new Label
        {
            Text = Loc.GetString("gehenna-trauma-scanner-wounds"),
            StyleClasses = { "LabelHeading" },
        });

        _wounds = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 4,
        };

        right.AddChild(_wounds);
    }

    public void Populate(GehennaTraumaScannerUiState state)
    {
        _scanMode.Text = Loc.GetString(state.ScanMode
            ? "health-analyzer-window-scan-mode-active"
            : "health-analyzer-window-scan-mode-inactive");
        _name.Text = state.Name;
        _species.Text = state.Species;
        _status.Text = state.MobState == null
            ? Loc.GetString("health-analyzer-window-entity-unknown-text")
            : GetMobState(state.MobState.Value);
        _blood.Text = float.IsNaN(state.BloodLevel)
            ? Loc.GetString("health-analyzer-window-entity-unknown-value-text")
            : $"{state.BloodLevel * 100:F1}%";
        _bleeding.Text = Loc.GetString(state.Bleeding
            ? "gehenna-trauma-scanner-yes"
            : "gehenna-trauma-scanner-no");
        _bodyMap.SetWounds(state.Wounds);

        _wounds.RemoveAllChildren();

        if (state.Wounds.Count == 0)
        {
            _wounds.AddChild(new Label { Text = Loc.GetString("gehenna-trauma-scanner-no-wounds") });
            return;
        }

        foreach (var wound in state.Wounds)
        {
            _wounds.AddChild(CreateWoundLabel(wound));
        }
    }

    private static Label AddLine(BoxContainer parent, string titleKey, string value)
    {
        parent.AddChild(new Label
        {
            Text = Loc.GetString(titleKey),
            StyleClasses = { "LabelSubText" },
        });

        var label = new Label { Text = value };
        parent.AddChild(label);
        return label;
    }

    private static Control CreateWoundLabel(GehennaTraumaScannerEntry wound)
    {
        var message = new FormattedMessage();
        message.PushColor(GetStateColor(wound.State));
        message.AddText(Loc.GetString("gehenna-trauma-scanner-wound-line",
            ("zone", Loc.GetString($"gehenna-target-zone-{wound.Zone.ToString().ToLowerInvariant()}")),
            ("type", Loc.GetString($"gehenna-trauma-type-{wound.Type.ToString().ToLowerInvariant()}")),
            ("state", Loc.GetString($"gehenna-wound-state-{wound.State.ToString().ToLowerInvariant()}")),
            ("severity", wound.Severity)));
        message.Pop();
        message.AddText("\n");
        message.AddText(Loc.GetString("gehenna-trauma-scanner-treatment-line", ("treatment", Loc.GetString(wound.Treatment))));

        var label = new RichTextLabel
        {
            MinSize = new Vector2(280, 44),
        };
        label.SetMessage(message);
        return label;
    }

    private static Color GetStateColor(GehennaWoundState state)
    {
        return state switch
        {
            GehennaWoundState.Open => Color.DeepSkyBlue,
            GehennaWoundState.Bandaged => Color.LightSkyBlue,
            GehennaWoundState.Rotting => Color.Orange,
            GehennaWoundState.Septic => Color.Red,
            _ => Color.White,
        };
    }

    private static string GetMobState(MobState state)
    {
        return state switch
        {
            MobState.Alive => Loc.GetString("health-analyzer-window-entity-alive-text"),
            MobState.Critical => Loc.GetString("health-analyzer-window-entity-critical-text"),
            MobState.Dead => Loc.GetString("health-analyzer-window-entity-dead-text"),
            _ => Loc.GetString("health-analyzer-window-entity-unknown-text"),
        };
    }
}
