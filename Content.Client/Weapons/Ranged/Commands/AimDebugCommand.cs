using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Client.Weapons.Ranged.Commands;

public sealed class AimDebugCommand : LocalizedCommands
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public override string Command => "aimdebug";

    public override string Description => "Toggles the weapon aim debug overlay CVar.";

    public override string Help => $"Usage: {Command} [true|false]";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length > 1)
        {
            shell.WriteError(Help);
            return;
        }

        var enabled = _cfg.GetCVar(CCVars.WeaponAimDebugOverlay);

        if (args.Length == 0)
        {
            enabled = !enabled;
        }
        else if (!bool.TryParse(args[0], out enabled))
        {
            shell.WriteError("Argument must be true or false.");
            return;
        }

        _cfg.SetCVar(CCVars.WeaponAimDebugOverlay, enabled);
        shell.WriteLine($"weapon.aim.debug_overlay = {enabled}");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length == 1
            ? CompletionResult.FromOptions(new[] { "true", "false" })
            : CompletionResult.Empty;
    }
}
