using System.Text.RegularExpressions;

namespace Content.Server.Administration.Logs;

public sealed partial class AdminLogManager
{
    private static string LocalizeChemicalReactionMessage(string rawMessage)
    {
        var chemicalReaction = ChemicalReactionRegex().Match(rawMessage);
        if (chemicalReaction.Success)
        {
            return $"Химический payload {chemicalReaction.Groups["payload"].Value} на {chemicalReaction.Groups["location"].Value} смешивает два раствора: {chemicalReaction.Groups["solutionA"].Value} и {chemicalReaction.Groups["solutionB"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeExplosionMessage(string rawMessage)
    {
        var exploded = ExplosionDirectRegex().Match(rawMessage);
        if (exploded.Success)
        {
            return $"{exploded.Groups["entity"].Value} взорвался ({exploded.Groups["type"].Value}) на позиции {exploded.Groups["position"].Value} с интенсивностью {exploded.Groups["intensity"].Value} и спадом {exploded.Groups["slope"].Value}";
        }

        var causedExplosion = ExplosionCausedRegex().Match(rawMessage);
        if (causedExplosion.Success)
        {
            return $"{causedExplosion.Groups["user"].Value} заставил {causedExplosion.Groups["entity"].Value} взорваться ({causedExplosion.Groups["type"].Value}) на позиции {causedExplosion.Groups["position"].Value} с интенсивностью {causedExplosion.Groups["intensity"].Value} и спадом {causedExplosion.Groups["slope"].Value}";
        }

        var spawnedExplosion = ExplosionSpawnedRegex().Match(rawMessage);
        if (spawnedExplosion.Success)
        {
            return $"Взрыв ({spawnedExplosion.Groups["type"].Value}) появился на {spawnedExplosion.Groups["position"].Value} с интенсивностью {spawnedExplosion.Groups["intensity"].Value} и спадом {spawnedExplosion.Groups["slope"].Value}";
        }

        if (TryTranslateSingleSubjectMessage(ExplosionRiggedRegex(), rawMessage, "был подготовлен к взрыву при использовании.", out var rigged))
            return rigged;

        var defused = ExplosionDefusedRegex().Match(rawMessage);
        if (defused.Success)
        {
            return $"{defused.Groups["user"].Value} обезвредил {defused.Groups["entity"].Value}!";
        }

        return rawMessage;
    }

    private static string LocalizeExplosionHitMessage(string rawMessage)
    {
        var explosionCauseHit = ExplosionHitCauseRegex().Match(rawMessage);
        if (explosionCauseHit.Success)
        {
            return $"Взрыв {explosionCauseHit.Groups["cause"].Value} нанёс {explosionCauseHit.Groups["damage"].Value} урона {explosionCauseHit.Groups["target"].Value}";
        }

        var explosionEpicenterHit = ExplosionHitEpicenterRegex().Match(rawMessage);
        if (explosionEpicenterHit.Success)
        {
            return $"Взрыв в точке {explosionEpicenterHit.Groups["epicenter"].Value} нанёс {explosionEpicenterHit.Groups["damage"].Value} урона {explosionEpicenterHit.Groups["target"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeGibMessage(string rawMessage)
    {
        var singularity = GibSingularityRegex().Match(rawMessage);
        if (singularity.Success)
        {
            return $"{singularity.Groups["first"].Value} и {singularity.Groups["second"].Value} создали сингулярность на X:{singularity.Groups["x"].Value} Y:{singularity.Groups["y"].Value}";
        }

        var gibbed = GibEntityRegex().Match(rawMessage);
        if (gibbed.Success)
        {
            return $"Сущность {gibbed.Groups["entity"].Value} расчленила {gibbed.Groups["target"].Value} на X:{gibbed.Groups["x"].Value} Y:{gibbed.Groups["y"].Value}";
        }

        var butchered = GibButcheredRegex().Match(rawMessage);
        if (butchered.Success)
        {
            return $"{butchered.Groups["user"].Value} разделал {butchered.Groups["target"].Value} с помощью {butchered.Groups["knife"].Value}";
        }

        var reclaimerGib = GibByEntityRegex().Match(rawMessage);
        if (reclaimerGib.Success)
        {
            return $"{reclaimerGib.Groups["victim"].Value} был расчленён {reclaimerGib.Groups["entity"].Value}";
        }

        var shuttleGib = GibByShuttleRegex().Match(rawMessage);
        if (shuttleGib.Success)
        {
            return $"{shuttleGib.Groups["player"].Value} был расчленён шаттлом {shuttleGib.Groups["shuttle"].Value}, прибывшим из FTL в {shuttleGib.Groups["coordinates"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeLandedMessage(string rawMessage)
    {
        var landedIn = LandedInContainerRegex().Match(rawMessage);
        if (landedIn.Success)
        {
            return $"{landedIn.Groups["entity"].Value}, брошенный {landedIn.Groups["player"].Value}, приземлился в {landedIn.Groups["container"].Value}";
        }

        var splashed = LandedSplashedRegex().Match(rawMessage);
        if (splashed.Success)
        {
            return $"{splashed.Groups["user"].Value} бросил {splashed.Groups["entity"].Value}, который расплескал раствор {splashed.Groups["solution"].Value} на {splashed.Groups["target"].Value}";
        }

        var spilled = LandedSpilledRegex().Match(rawMessage);
        if (spilled.Success)
        {
            return $"{spilled.Groups["entity"].Value} расплескал раствор {spilled.Groups["solution"].Value} при падении";
        }

        return rawMessage;
    }

    private static string LocalizeTileMessage(string rawMessage)
    {
        var actorTile = TileByActorRegex().Match(rawMessage);
        if (actorTile.Success)
        {
            return $"{actorTile.Groups["actor"].Value} использовал систему размещения, чтобы установить тайл {actorTile.Groups["tile"].Value} на {actorTile.Groups["coordinates"].Value}";
        }

        var userTile = TileByUserRegex().Match(rawMessage);
        if (userTile.Success)
        {
            return $"{userTile.Groups["user"].Value} использовал систему размещения, чтобы установить тайл {userTile.Groups["tile"].Value} на {userTile.Groups["coordinates"].Value}";
        }

        var systemTile = TileBySystemRegex().Match(rawMessage);
        if (systemTile.Success)
        {
            return $"Система размещения установила тайл {systemTile.Groups["tile"].Value} на {systemTile.Groups["coordinates"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeUnknownMessage(string rawMessage)
    {
        var debugLog = UnknownDebugRegex().Match(rawMessage);
        if (debugLog.Success)
        {
            return $"Отладочный лог добавлен {debugLog.Groups["player"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeVerbMessage(string rawMessage)
    {
        var verb = VerbRegex().Match(rawMessage);
        if (verb.Success)
        {
            var forced = verb.Groups["forced"].Value == "was forced to execute" ? "принудительно выполнил" : "выполнил";
            var held = verb.Groups["held"].Success ? $" при удержании {verb.Groups["held"].Value}" : string.Empty;
            return $"{verb.Groups["user"].Value} {forced} верб [{verb.Groups["verb"].Value}] по цели {verb.Groups["target"].Value}{held}";
        }

        return rawMessage;
    }

    [GeneratedRegex("^Chemical bomb payload (?<payload>.+?) at (?<location>.+?) is combining two solutions: (?<solutionA>.+?) and (?<solutionB>.+)$")]
    private static partial Regex ChemicalReactionRegex();

    [GeneratedRegex("^(?<entity>.+?) exploded \\((?<type>.+?)\\) at Pos:(?<position>.+?) with intensity (?<intensity>.+?) slope (?<slope>.+)$")]
    private static partial Regex ExplosionDirectRegex();

    [GeneratedRegex("^(?<user>.+?) caused (?<entity>.+?) to explode \\((?<type>.+?)\\) at Pos:(?<position>.+?) with intensity (?<intensity>.+?) slope (?<slope>.+)$")]
    private static partial Regex ExplosionCausedRegex();

    [GeneratedRegex("^Explosion \\((?<type>.+?)\\) spawned at (?<position>.+?) with intensity (?<intensity>.+?) slope (?<slope>.+)$")]
    private static partial Regex ExplosionSpawnedRegex();

    [GeneratedRegex("^(?<player>.+?) has been rigged up to explode when used\\.$")]
    private static partial Regex ExplosionRiggedRegex();

    [GeneratedRegex("^(?<user>.+?) has defused (?<entity>.+?)!$")]
    private static partial Regex ExplosionDefusedRegex();

    [GeneratedRegex("^Explosion of (?<cause>.+?) dealt (?<damage>.+?) damage to (?<target>.+)$")]
    private static partial Regex ExplosionHitCauseRegex();

    [GeneratedRegex("^Explosion at (?<epicenter>.+?) dealt (?<damage>.+?) damage to (?<target>.+)$")]
    private static partial Regex ExplosionHitEpicenterRegex();

    [GeneratedRegex("^(?<first>.+?) and (?<second>.+?) created singularity at X:(?<x>.+?) Y:(?<y>.+)$")]
    private static partial Regex GibSingularityRegex();

    [GeneratedRegex("^Entity (?<entity>.+?) gibbed (?<target>.+?) at X:(?<x>.+?) Y:(?<y>.+)$")]
    private static partial Regex GibEntityRegex();

    [GeneratedRegex("^(?<user>.+?) has butchered (?<target>.+?) with (?<knife>.+)$")]
    private static partial Regex GibButcheredRegex();

    [GeneratedRegex("^(?<victim>.+?) was gibbed by (?<entity>.+?)\\s*$")]
    private static partial Regex GibByEntityRegex();

    [GeneratedRegex("^(?<player>.+?) got gibbed by the shuttle (?<shuttle>.+?) arriving from FTL at (?<coordinates>.+)$")]
    private static partial Regex GibByShuttleRegex();

    [GeneratedRegex("^(?<entity>.+?) thrown by (?<player>.+?) landed in (?<container>.+)$")]
    private static partial Regex LandedInContainerRegex();

    [GeneratedRegex("^(?<user>.+?) threw (?<entity>.+?) which splashed a solution (?<solution>.+?) onto (?<target>.+)$")]
    private static partial Regex LandedSplashedRegex();

    [GeneratedRegex("^(?<entity>.+?) spilled a solution (?<solution>.+?) on landing$")]
    private static partial Regex LandedSpilledRegex();

    [GeneratedRegex("^(?<actor>.+?) used placement system to set tile (?<tile>.+?) at (?<coordinates>.+)$")]
    private static partial Regex TileByActorRegex();

    [GeneratedRegex("^(?<user>.+?) used placement system to set tile (?<tile>.+?) at (?<coordinates>.+)$")]
    private static partial Regex TileByUserRegex();

    [GeneratedRegex("^Placement system set tile (?<tile>.+?) at (?<coordinates>.+)$")]
    private static partial Regex TileBySystemRegex();

    [GeneratedRegex("^Debug log added by (?<player>.+)$")]
    private static partial Regex UnknownDebugRegex();

    [GeneratedRegex("^(?<user>.+?) (?<forced>was forced to execute|executed) the \\[(?<verb>.+?)\\] verb targeting (?<target>.+?)(?: while holding (?<held>.+))?$")]
    private static partial Regex VerbRegex();
}
