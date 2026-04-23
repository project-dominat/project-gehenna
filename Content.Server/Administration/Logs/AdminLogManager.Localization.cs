using System.Text.RegularExpressions;
using Content.Shared.Database;

namespace Content.Server.Administration.Logs;

public sealed partial class AdminLogManager
{
    private static string LocalizeMessage(LogType type, string rawMessage)
    {
        return type switch
        {
            LogType.Chat => LocalizeChatMessage(rawMessage),
            LogType.Vote => LocalizeVoteMessage(rawMessage),
            LogType.AdminCommands => LocalizeAdminCommandMessage(rawMessage),
            LogType.AdminMessage => LocalizeAdminMessageLog(rawMessage),
            LogType.Action => LocalizeActionMessage(rawMessage),
            LogType.Anomaly => LocalizeAnomalyMessage(rawMessage),
            LogType.AntagSelection => LocalizeAntagSelectionMessage(rawMessage),
            LogType.Asphyxiation => LocalizeAsphyxiationMessage(rawMessage),
            LogType.AtmosDeviceSetting => LocalizeAtmosDeviceSettingMessage(rawMessage),
            LogType.AtmosFilterChanged => LocalizeAtmosFilterChangedMessage(rawMessage),
            LogType.AtmosPowerChanged => LocalizeAtmosPowerChangedMessage(rawMessage),
            LogType.AtmosPressureChanged => LocalizeAtmosPressureChangedMessage(rawMessage),
            LogType.AtmosRatioChanged => LocalizeAtmosRatioChangedMessage(rawMessage),
            LogType.AtmosVolumeChanged => LocalizeAtmosVolumeChangedMessage(rawMessage),
            LogType.Barotrauma => LocalizeBarotraumaMessage(rawMessage),
            LogType.Botany => LocalizeBotanyMessage(rawMessage),
            LogType.BulletHit => LocalizeBulletHitMessage(rawMessage),
            LogType.CableCut => LocalizeCableCutMessage(rawMessage),
            LogType.CanisterPurged => LocalizeCanisterPurgedMessage(rawMessage),
            LogType.Connection => LocalizeConnectionMessage(rawMessage),
            LogType.Construction => LocalizeConstructionMessage(rawMessage),
            LogType.CrayonDraw => LocalizeCrayonDrawMessage(rawMessage),
            LogType.ChemicalReaction => LocalizeChemicalReactionMessage(rawMessage),
            LogType.Damaged => LocalizeDamagedMessage(rawMessage),
            LogType.DeviceLinking => LocalizeDeviceLinkingMessage(rawMessage),
            LogType.DeviceNetwork => LocalizeDeviceNetworkMessage(rawMessage),
            LogType.Electrocution => LocalizeElectrocutionMessage(rawMessage),
            LogType.EmergencyShuttle => LocalizeEmergencyShuttleMessage(rawMessage),
            LogType.EntityDelete => LocalizeEntityDeleteMessage(rawMessage),
            LogType.EntitySpawn => LocalizeEntitySpawnMessage(rawMessage),
            LogType.EventAnnounced or LogType.EventStarted or LogType.EventStopped or LogType.EventRan => LocalizeEventMessage(type, rawMessage),
            LogType.Explosion => LocalizeExplosionMessage(rawMessage),
            LogType.ExplosionHit => LocalizeExplosionHitMessage(rawMessage),
            LogType.ExplosiveDepressurization => LocalizeExplosiveDepressurizationMessage(rawMessage),
            LogType.FieldGeneration => LocalizeFieldGenerationMessage(rawMessage),
            LogType.Flammable => LocalizeFlammableMessage(rawMessage),
            LogType.ForceFeed => LocalizeForceFeedMessage(rawMessage),
            LogType.Gib => LocalizeGibMessage(rawMessage),
            LogType.GhostRoleTaken => LocalizeGhostRoleTakenMessage(rawMessage),
            LogType.GhostWarp => LocalizeGhostWarpMessage(rawMessage),
            LogType.Identity => LocalizeIdentityMessage(rawMessage),
            LogType.ItemConfigure => LocalizeItemConfigureMessage(rawMessage),
            LogType.Landed => LocalizeLandedMessage(rawMessage),
            LogType.LateJoin => LocalizeLateJoinMessage(rawMessage),
            LogType.Mind => LocalizeMindMessage(rawMessage),
            LogType.PdaInteract => LocalizePdaInteractMessage(rawMessage),
            LogType.Respawn => LocalizeRespawnMessage(rawMessage),
            LogType.RoundStartJoin => LocalizeRoundStartJoinMessage(rawMessage),
            LogType.ShuttleCalled => LocalizeShuttleCalledMessage(rawMessage),
            LogType.ShuttleImpact => LocalizeShuttleImpactMessage(rawMessage),
            LogType.ShuttleRecalled => LocalizeShuttleRecalledMessage(rawMessage),
            LogType.StorePurchase => LocalizeStorePurchaseMessage(rawMessage),
            LogType.StoreRefund => LocalizeStoreRefundMessage(rawMessage),
            LogType.Teleport => LocalizeTeleportMessage(rawMessage),
            LogType.Temperature => LocalizeTemperatureMessage(rawMessage),
            LogType.Tile => LocalizeTileMessage(rawMessage),
            LogType.ThrowHit => LocalizeThrowHitMessage(rawMessage),
            LogType.Unknown => LocalizeUnknownMessage(rawMessage),
            LogType.Verb => LocalizeVerbMessage(rawMessage),
            LogType.WireHacking => LocalizeWireHackingMessage(rawMessage),
            _ => rawMessage
        };
    }

    private static string LocalizeChatMessage(string rawMessage)
    {
        if (TryTranslateNamedMessage(ServerAnnouncementRegex(), rawMessage, "Сообщение сервера", out var serverAnnouncement))
            return serverAnnouncement;

        if (TryTranslatePlayerMessage(ServerMessageRegex(), rawMessage, "Сообщение сервера игроку", out var serverMessage))
            return serverMessage;

        if (TryTranslateNamedMessage(AdminAnnouncementRegex(), rawMessage, "Объявление администратора", out var adminAnnouncement))
            return adminAnnouncement;

        if (TryTranslatePlayerMessage(HookOocRegex(), rawMessage, "Перехваченный OOC от", out var hookOoc))
            return hookOoc;

        if (TryTranslatePlayerMessage(HookAdminRegex(), rawMessage, "Перехваченный админ-чат от", out var hookAdmin))
            return hookAdmin;

        if (TryTranslatePlayerMessage(OocRegex(), rawMessage, "OOC от", out var ooc))
            return ooc;

        if (TryTranslateSingleSubjectMessage(NotAdminRegex(), rawMessage, "попытался отправить админ-сообщение, не являясь администратором", out var notAdmin))
            return notAdmin;

        if (TryTranslatePlayerMessage(AdminChatRegex(), rawMessage, "Админ-чат от", out var adminChat))
            return adminChat;

        if (TryTranslatePlayerMessage(GlobalStationAnnouncementRegex(), rawMessage, "Глобальное объявление станции от", out var globalAnnouncement))
            return globalAnnouncement;

        if (TryTranslatePlayerMessage(StationAnnouncementRegex(), rawMessage, "Объявление станции от", out var stationAnnouncement))
            return stationAnnouncement;

        var stationAnnouncementOn = StationAnnouncementOnRegex().Match(rawMessage);
        if (stationAnnouncementOn.Success)
        {
            return $"Объявление станции на {stationAnnouncementOn.Groups["station"].Value} от {stationAnnouncementOn.Groups["sender"].Value}: {stationAnnouncementOn.Groups["message"].Value}";
        }

        var sayAsTransformed = SayAsTransformedRegex().Match(rawMessage);
        if (sayAsTransformed.Success)
        {
            return $"Речь от {sayAsTransformed.Groups["source"].Value} как {sayAsTransformed.Groups["name"].Value}, оригинал: {sayAsTransformed.Groups["original"].Value}, после преобразования: {sayAsTransformed.Groups["transformed"].Value}.";
        }

        var sayTransformed = SayTransformedRegex().Match(rawMessage);
        if (sayTransformed.Success)
        {
            return $"Речь от {sayTransformed.Groups["source"].Value}, оригинал: {sayTransformed.Groups["original"].Value}, после преобразования: {sayTransformed.Groups["transformed"].Value}.";
        }

        var sayAs = SayAsRegex().Match(rawMessage);
        if (sayAs.Success)
        {
            return $"Речь от {sayAs.Groups["source"].Value} как {sayAs.Groups["name"].Value}: {sayAs.Groups["message"].Value}.";
        }

        var say = SayRegex().Match(rawMessage);
        if (say.Success)
        {
            return $"Речь от {say.Groups["source"].Value}: {say.Groups["message"].Value}.";
        }

        var whisperAs = WhisperAsRegex().Match(rawMessage);
        if (whisperAs.Success)
        {
            return $"Шёпот от {whisperAs.Groups["source"].Value} как {whisperAs.Groups["name"].Value}: {whisperAs.Groups["message"].Value}.";
        }

        var whisperAsTransformed = WhisperAsTransformedRegex().Match(rawMessage);
        if (whisperAsTransformed.Success)
        {
            return $"Шёпот от {whisperAsTransformed.Groups["source"].Value} как {whisperAsTransformed.Groups["name"].Value}, оригинал: {whisperAsTransformed.Groups["original"].Value}, после преобразования: {whisperAsTransformed.Groups["transformed"].Value}.";
        }

        var whisper = WhisperRegex().Match(rawMessage);
        if (whisper.Success)
        {
            return $"Шёпот от {whisper.Groups["source"].Value}: {whisper.Groups["message"].Value}.";
        }

        var whisperTransformed = WhisperTransformedRegex().Match(rawMessage);
        if (whisperTransformed.Success)
        {
            return $"Шёпот от {whisperTransformed.Groups["source"].Value}, оригинал: {whisperTransformed.Groups["original"].Value}, после преобразования: {whisperTransformed.Groups["transformed"].Value}.";
        }

        var emoteAs = EmoteAsRegex().Match(rawMessage);
        if (emoteAs.Success)
        {
            return $"Эмоция от {emoteAs.Groups["source"].Value} как {emoteAs.Groups["name"].Value}: {emoteAs.Groups["action"].Value}";
        }

        var emote = EmoteRegex().Match(rawMessage);
        if (emote.Success)
        {
            return $"Эмоция от {emote.Groups["source"].Value}: {emote.Groups["action"].Value}";
        }

        if (TryTranslatePlayerMessage(LoocRegex(), rawMessage, "LOOC от", out var looc))
            return looc;

        if (TryTranslatePlayerMessage(AdminDeadChatRegex(), rawMessage, "Админский мёртвый чат от", out var adminDeadChat))
            return adminDeadChat;

        if (TryTranslatePlayerMessage(DeadChatRegex(), rawMessage, "Мёртвый чат от", out var deadChat))
            return deadChat;

        var clonedBody = ClonedBodyRegex().Match(rawMessage);
        if (clonedBody.Success)
        {
            return $"Тело {clonedBody.Groups["original"].Value} было клонировано как {clonedBody.Groups["clone"].Value}";
        }

        var globalConsoleAnnouncement = GlobalConsoleAnnouncementRegex().Match(rawMessage);
        if (globalConsoleAnnouncement.Success)
        {
            return $"{globalConsoleAnnouncement.Groups["player"].Value} отправил следующее глобальное объявление: {globalConsoleAnnouncement.Groups["message"].Value}";
        }

        var stationConsoleAnnouncement = StationConsoleAnnouncementRegex().Match(rawMessage);
        if (stationConsoleAnnouncement.Success)
        {
            return $"{stationConsoleAnnouncement.Groups["player"].Value} отправил следующее объявление станции: {stationConsoleAnnouncement.Groups["message"].Value}";
        }

        if (TryTranslateSingleSubjectMessage(ClearMotdRegex(), rawMessage, "очистил MOTD сервера.", out var clearMotd))
            return clearMotd;

        var setMotd = SetMotdRegex().Match(rawMessage);
        if (setMotd.Success)
        {
            return $"{setMotd.Groups["player"].Value} установил MOTD сервера на \"{setMotd.Groups["motd"].Value}\"";
        }

        var warDeclaration = WarDeclarationRegex().Match(rawMessage);
        if (warDeclaration.Success)
        {
            return $"{warDeclaration.Groups["player"].Value} объявил войну со следующим текстом: {warDeclaration.Groups["message"].Value}";
        }

        var radioMessageAs = RadioMessageAsRegex().Match(rawMessage);
        if (radioMessageAs.Success)
        {
            return $"Радиосообщение от {radioMessageAs.Groups["user"].Value} как {radioMessageAs.Groups["name"].Value} на канале {radioMessageAs.Groups["channel"].Value}: {radioMessageAs.Groups["message"].Value}";
        }

        var radioMessage = RadioMessageRegex().Match(rawMessage);
        if (radioMessage.Success)
        {
            return $"Радиосообщение от {radioMessage.Groups["user"].Value} на канале {radioMessage.Groups["channel"].Value}: {radioMessage.Groups["message"].Value}";
        }

        var telephoneMessageAs = TelephoneMessageAsRegex().Match(rawMessage);
        if (telephoneMessageAs.Success)
        {
            return $"Телефонное сообщение от {telephoneMessageAs.Groups["user"].Value} как {telephoneMessageAs.Groups["name"].Value} на {telephoneMessageAs.Groups["source"].Value}: {telephoneMessageAs.Groups["message"].Value}";
        }

        var telephoneMessage = TelephoneMessageRegex().Match(rawMessage);
        if (telephoneMessage.Success)
        {
            return $"Телефонное сообщение от {telephoneMessage.Groups["user"].Value} на {telephoneMessage.Groups["source"].Value}: {telephoneMessage.Groups["message"].Value}";
        }

        var parrotLearnedPhrase = ParrotLearnedPhraseRegex().Match(rawMessage);
        if (parrotLearnedPhrase.Success)
        {
            return $"Подражающая сущность {parrotLearnedPhrase.Groups["entity"].Value} выучила фразу \"{parrotLearnedPhrase.Groups["message"].Value}\" от {parrotLearnedPhrase.Groups["speaker"].Value}";
        }

        var cameraRenamed = CameraRenamedRegex().Match(rawMessage);
        if (cameraRenamed.Success)
        {
            return $"{cameraRenamed.Groups["actor"].Value} установил имя для {cameraRenamed.Groups["camera"].Value}: \"{cameraRenamed.Groups["name"].Value}\".";
        }

        return rawMessage;
    }

    private static string LocalizeVoteMessage(string rawMessage)
    {
        if (TryTranslateSingleSubjectMessage(OpenedVoteMenuRegex(), rawMessage, "открыл меню голосований", out var openedVoteMenu))
            return openedVoteMenu;

        var initiatedVote = InitiatedVoteRegex().Match(rawMessage);
        if (initiatedVote.Success)
        {
            var voteType = LocalizeVoteTypeName(initiatedVote.Groups["type"].Value);
            return $"{initiatedVote.Groups["initiator"].Value} запустил голосование типа {voteType}";
        }

        var initiatedVoteWithArgs = InitiatedVoteWithArgsRegex().Match(rawMessage);
        if (initiatedVoteWithArgs.Success)
        {
            var voteType = initiatedVoteWithArgs.Groups["type"].Value;
            return $"{initiatedVoteWithArgs.Groups["initiator"].Value} запустил голосование типа {LocalizeVoteTypeName(voteType)} с аргументами: {LocalizeVoteArguments(voteType, initiatedVoteWithArgs.Groups["args"].Value)}";
        }

        var initiatedVoteByServer = InitiatedVoteByServerRegex().Match(rawMessage);
        if (initiatedVoteByServer.Success)
        {
            return $"Запущено голосование типа {LocalizeVoteTypeName(initiatedVoteByServer.Groups["type"].Value)}";
        }

        var failedToStartVote = FailedToStartVoteRegex().Match(rawMessage);
        if (failedToStartVote.Success)
        {
            return $"{failedToStartVote.Groups["player"].Value} не смог запустить голосование типа {LocalizeVoteTypeName(failedToStartVote.Groups["type"].Value)}";
        }

        var restartVoteAdminOnline = RestartVoteAdminOnlineRegex().Match(rawMessage);
        if (restartVoteAdminOnline.Success)
        {
            return $"Голосование за рестарт набрало достаточно голосов, но было отменено, потому что онлайн был администратор. {FormatVoteCounts(restartVoteAdminOnline.Groups["yes"].Value, restartVoteAdminOnline.Groups["no"].Value)}";
        }

        var restartVoteSucceeded = RestartVoteSucceededRegex().Match(rawMessage);
        if (restartVoteSucceeded.Success)
        {
            return $"Голосование за рестарт прошло: {restartVoteSucceeded.Groups["yes"].Value}/{restartVoteSucceeded.Groups["no"].Value}";
        }

        var restartVoteFailed = RestartVoteFailedRegex().Match(rawMessage);
        if (restartVoteFailed.Success)
        {
            return $"Голосование за рестарт провалилось: {restartVoteFailed.Groups["yes"].Value}/{restartVoteFailed.Groups["no"].Value}";
        }

        var restartVoteGhostRequirement = RestartVoteGhostRequirementRegex().Match(rawMessage);
        if (restartVoteGhostRequirement.Success)
        {
            return $"Голосование за рестарт провалилось: текущий процент игроков-призраков {restartVoteGhostRequirement.Groups["current"].Value}% не достигает {restartVoteGhostRequirement.Groups["required"].Value}%";
        }

        var presetVoteFinished = PresetVoteFinishedRegex().Match(rawMessage);
        if (presetVoteFinished.Success)
        {
            return $"Голосование за пресет завершено: {presetVoteFinished.Groups["picked"].Value}";
        }

        var mapVoteFinished = MapVoteFinishedRegex().Match(rawMessage);
        if (mapVoteFinished.Success)
        {
            return $"Голосование за карту завершено: {mapVoteFinished.Groups["picked"].Value}";
        }

        var initiatedCustomVote = InitiatedCustomVoteRegex().Match(rawMessage);
        if (initiatedCustomVote.Success)
        {
            return $"{initiatedCustomVote.Groups["player"].Value} запустил пользовательское голосование: {initiatedCustomVote.Groups["title"].Value} - {initiatedCustomVote.Groups["options"].Value}";
        }

        var initiatedCustomVoteByServer = InitiatedCustomVoteByServerRegex().Match(rawMessage);
        if (initiatedCustomVoteByServer.Success)
        {
            return $"Запущено пользовательское голосование: {initiatedCustomVoteByServer.Groups["title"].Value} - {initiatedCustomVoteByServer.Groups["options"].Value}";
        }

        var customVoteTie = CustomVoteTieRegex().Match(rawMessage);
        if (customVoteTie.Success)
        {
            return $"Пользовательское голосование {customVoteTie.Groups["title"].Value} завершилось ничьей: {customVoteTie.Groups["ties"].Value}";
        }

        var customVoteFinished = CustomVoteFinishedRegex().Match(rawMessage);
        if (customVoteFinished.Success)
        {
            return $"Пользовательское голосование {customVoteFinished.Groups["title"].Value} завершилось: {customVoteFinished.Groups["winner"].Value}";
        }

        var canceledVote = CanceledVoteRegex().Match(rawMessage);
        if (canceledVote.Success)
        {
            return $"{canceledVote.Groups["player"].Value} отменил голосование: {canceledVote.Groups["title"].Value}";
        }

        var canceledVoteByServer = CanceledVoteByServerRegex().Match(rawMessage);
        if (canceledVoteByServer.Success)
        {
            return $"Голосование отменено: {canceledVoteByServer.Groups["title"].Value}";
        }

        var votekickInitiatorIneligible = VotekickInitiatorIneligibleRegex().Match(rawMessage);
        if (votekickInitiatorIneligible.Success)
        {
            return $"{votekickInitiatorIneligible.Groups["initiator"].Value} попытался запустить голосование за кик, но не имеет на это права";
        }

        var votekickTargetNotFound = VotekickTargetNotFoundRegex().Match(rawMessage);
        if (votekickTargetNotFound.Success)
        {
            return $"{votekickTargetNotFound.Groups["initiator"].Value} попытался запустить голосование за кик для игрока \"{votekickTargetNotFound.Groups["target"].Value}\", но цель не была найдена";
        }

        var votekickSelf = VotekickSelfRegex().Match(rawMessage);
        if (votekickSelf.Success)
        {
            return $"{votekickSelf.Groups["initiator"].Value} попытался запустить голосование за кик самого себя. Голосование отменено.";
        }

        var votekickNotEnoughGhostRoles = VotekickNotEnoughGhostRolesRegex().Match(rawMessage);
        if (votekickNotEnoughGhostRoles.Success)
        {
            return $"{votekickNotEnoughGhostRoles.Groups["initiator"].Value} попытался запустить голосование за кик для {votekickNotEnoughGhostRoles.Groups["target"].Value}, но не хватило подходящих призраков для голосования: требуется {votekickNotEnoughGhostRoles.Groups["required"].Value}, найдено {votekickNotEnoughGhostRoles.Groups["found"].Value}";
        }

        var votekickTargetIneligible = VotekickTargetIneligibleRegex().Match(rawMessage);
        if (votekickTargetIneligible.Success)
        {
            return $"{votekickTargetIneligible.Groups["initiator"].Value} попытался запустить голосование за кик для {votekickTargetIneligible.Groups["target"].Value}, но цель не подлежит кику голосованием";
        }

        var votekickStarted = VotekickStartedRegex().Match(rawMessage);
        if (votekickStarted.Success)
        {
            return $"Запущено голосование за кик {votekickStarted.Groups["username"].Value} ({votekickStarted.Groups["targetName"].Value}) по причине {LocalizeVotekickReason(votekickStarted.Groups["reason"].Value)}. Инициатор: {votekickStarted.Groups["initiator"].Value}.";
        }

        var votekickAdminOnline = VotekickAdminOnlineRegex().Match(rawMessage);
        if (votekickAdminOnline.Success)
        {
            return $"Голосование за кик {votekickAdminOnline.Groups["username"].Value} набрало достаточно голосов, но было отменено, потому что онлайн был администратор. {FormatVoteBreakdown(votekickAdminOnline.Groups["yes"].Value, votekickAdminOnline.Groups["no"].Value, votekickAdminOnline.Groups["yesVoters"].Value, votekickAdminOnline.Groups["noVoters"].Value)}";
        }

        var votekickCancelledAntag = VotekickCancelledAntagRegex().Match(rawMessage);
        if (votekickCancelledAntag.Success)
        {
            return $"Голосование за кик {votekickCancelledAntag.Groups["username"].Value} по причине {LocalizeVotekickReason(votekickCancelledAntag.Groups["reason"].Value)}, созданное {votekickCancelledAntag.Groups["initiator"].Value}, было отменено, потому что цель является антагонистом.";
        }

        var votekickCancelledAdmin = VotekickCancelledAdminRegex().Match(rawMessage);
        if (votekickCancelledAdmin.Success)
        {
            return $"Голосование за кик {votekickCancelledAdmin.Groups["username"].Value} по причине {LocalizeVotekickReason(votekickCancelledAdmin.Groups["reason"].Value)}, созданное {votekickCancelledAdmin.Groups["initiator"].Value}, было отменено, потому что цель является деадминенным администратором.";
        }

        var votekickSucceeded = VotekickSucceededRegex().Match(rawMessage);
        if (votekickSucceeded.Success)
        {
            return $"Голосование за кик {votekickSucceeded.Groups["username"].Value} прошло. {FormatVoteBreakdown(votekickSucceeded.Groups["yes"].Value, votekickSucceeded.Groups["no"].Value, votekickSucceeded.Groups["yesVoters"].Value, votekickSucceeded.Groups["noVoters"].Value)}";
        }

        var votekickFailed = VotekickFailedRegex().Match(rawMessage);
        if (votekickFailed.Success)
        {
            return $"Голосование за кик провалилось. {FormatVoteBreakdown(votekickFailed.Groups["yes"].Value, votekickFailed.Groups["no"].Value, votekickFailed.Groups["yesVoters"].Value, votekickFailed.Groups["noVoters"].Value)}";
        }

        return rawMessage;
    }

    private static string LocalizeAdminCommandMessage(string rawMessage)
    {
        var cvarChange = CvarChangedRegex().Match(rawMessage);
        if (cvarChange.Success)
        {
            return $"{cvarChange.Groups["player"].Value} ({cvarChange.Groups["userId"].Value}) изменил CVAR {cvarChange.Groups["cvar"].Value} с {cvarChange.Groups["oldValue"].Value} на {cvarChange.Groups["newValue"].Value}";
        }

        return rawMessage;
    }

    private static bool TryTranslateNamedMessage(Regex regex, string rawMessage, string prefix, out string message)
    {
        var match = regex.Match(rawMessage);
        if (!match.Success)
        {
            message = string.Empty;
            return false;
        }

        message = $"{prefix}: {match.Groups["message"].Value}";
        return true;
    }

    private static bool TryTranslatePlayerMessage(Regex regex, string rawMessage, string prefix, out string message)
    {
        var match = regex.Match(rawMessage);
        if (!match.Success)
        {
            message = string.Empty;
            return false;
        }

        message = $"{prefix} {match.Groups["player"].Value}: {match.Groups["message"].Value}";
        return true;
    }

    private static bool TryTranslateSingleSubjectMessage(Regex regex, string rawMessage, string suffix, out string message)
    {
        var match = regex.Match(rawMessage);
        if (!match.Success)
        {
            message = string.Empty;
            return false;
        }

        message = $"{match.Groups["player"].Value} {suffix}";
        return true;
    }

    private static string FormatVoteCounts(string yes, string no)
    {
        return $"Да: {yes} / Нет: {no}";
    }

    private static string FormatVoteBreakdown(string yes, string no, string yesVoters, string noVoters)
    {
        return $"{FormatVoteCounts(yes, no)}. Да: {yesVoters} / Нет: {noVoters}";
    }

    private static string LocalizeVoteTypeName(string rawType)
    {
        return rawType switch
        {
            "Restart" => "рестарт",
            "Preset" => "пресет",
            "Map" => "карта",
            "Votekick" => "кик игрока",
            _ => rawType
        };
    }

    private static string LocalizeVoteArguments(string rawType, string rawArguments)
    {
        if (rawType != "Votekick")
            return rawArguments;

        var parts = rawArguments.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            return rawArguments;

        return $"цель: {parts[0]}, причина: {LocalizeVotekickReason(parts[1])}";
    }

    private static string LocalizeVotekickReason(string rawReason)
    {
        return rawReason switch
        {
            "Raiding" => "рейд",
            "Cheating" => "читы",
            "Spam" => "спам",
            _ => rawReason
        };
    }

    [GeneratedRegex("^Server announcement: (?<message>.*)$")]
    private static partial Regex ServerAnnouncementRegex();

    [GeneratedRegex("^Server message to (?<player>.+?): (?<message>.*)$")]
    private static partial Regex ServerMessageRegex();

    [GeneratedRegex("^Admin announcement: (?<message>.*)$")]
    private static partial Regex AdminAnnouncementRegex();

    [GeneratedRegex("^Hook OOC from (?<player>.+?): (?<message>.*)$")]
    private static partial Regex HookOocRegex();

    [GeneratedRegex("^Hook admin from (?<player>.+?): (?<message>.*)$")]
    private static partial Regex HookAdminRegex();

    [GeneratedRegex("^OOC from (?<player>.+?): (?<message>.*)$")]
    private static partial Regex OocRegex();

    [GeneratedRegex("^(?<player>.+?) attempted to send admin message but was not admin$")]
    private static partial Regex NotAdminRegex();

    [GeneratedRegex("^Admin chat from (?<player>.+?): (?<message>.*)$")]
    private static partial Regex AdminChatRegex();

    [GeneratedRegex("^Global station announcement from (?<player>.+?): (?<message>.*)$")]
    private static partial Regex GlobalStationAnnouncementRegex();

    [GeneratedRegex("^Station Announcement from (?<player>.+?): (?<message>.*)$")]
    private static partial Regex StationAnnouncementRegex();

    [GeneratedRegex("^Station Announcement on (?<station>.+?) from (?<sender>.+?): (?<message>.*)$")]
    private static partial Regex StationAnnouncementOnRegex();

    [GeneratedRegex("^Say from (?<source>.+?) as (?<name>.+?), original: (?<original>.*), transformed: (?<transformed>.*)\\.$")]
    private static partial Regex SayAsTransformedRegex();

    [GeneratedRegex("^Say from (?<source>.+?), original: (?<original>.*), transformed: (?<transformed>.*)\\.$")]
    private static partial Regex SayTransformedRegex();

    [GeneratedRegex("^Say from (?<source>.+?) as (?<name>.+?): (?<message>.*)\\.$")]
    private static partial Regex SayAsRegex();

    [GeneratedRegex("^Say from (?<source>.+?): (?<message>.*)\\.$")]
    private static partial Regex SayRegex();

    [GeneratedRegex("^Whisper from (?<source>.+?) as (?<name>.+?): (?<message>.*)\\.$")]
    private static partial Regex WhisperAsRegex();

    [GeneratedRegex("^Whisper from (?<source>.+?) as (?<name>.+?), original: (?<original>.*), transformed: (?<transformed>.*)\\.$")]
    private static partial Regex WhisperAsTransformedRegex();

    [GeneratedRegex("^Whisper from (?<source>.+?): (?<message>.*)\\.$")]
    private static partial Regex WhisperRegex();

    [GeneratedRegex("^Whisper from (?<source>.+?), original: (?<original>.*), transformed: (?<transformed>.*)\\.$")]
    private static partial Regex WhisperTransformedRegex();

    [GeneratedRegex("^Emote from (?<source>.+?) as (?<name>.+?): (?<action>.*)$")]
    private static partial Regex EmoteAsRegex();

    [GeneratedRegex("^Emote from (?<source>.+?): (?<action>.*)$")]
    private static partial Regex EmoteRegex();

    [GeneratedRegex("^LOOC from (?<player>.+?): (?<message>.*)$")]
    private static partial Regex LoocRegex();

    [GeneratedRegex("^Admin dead chat from (?<player>.+?): (?<message>.*)$")]
    private static partial Regex AdminDeadChatRegex();

    [GeneratedRegex("^Dead chat from (?<player>.+?): (?<message>.*)$")]
    private static partial Regex DeadChatRegex();

    [GeneratedRegex("^The body of (?<original>.+?) was cloned as (?<clone>.+)$")]
    private static partial Regex ClonedBodyRegex();

    [GeneratedRegex("^(?<player>.+?) has sent the following global announcement: (?<message>.*)$")]
    private static partial Regex GlobalConsoleAnnouncementRegex();

    [GeneratedRegex("^(?<player>.+?) has sent the following station announcement: (?<message>.*)$")]
    private static partial Regex StationConsoleAnnouncementRegex();

    [GeneratedRegex("^(?<player>.+?) cleared the MOTD for the server\\.$")]
    private static partial Regex ClearMotdRegex();

    [GeneratedRegex("^(?<player>.+?) set the MOTD for the server to \\\"(?<motd>.*)\\\"$")]
    private static partial Regex SetMotdRegex();

    [GeneratedRegex("^(?<player>.+?) has declared war with this text: (?<message>.*)$")]
    private static partial Regex WarDeclarationRegex();

    [GeneratedRegex("^Radio message from (?<user>.+?) as (?<name>.+?) on (?<channel>.+?): (?<message>.*)$")]
    private static partial Regex RadioMessageAsRegex();

    [GeneratedRegex("^Radio message from (?<user>.+?) on (?<channel>.+?): (?<message>.*)$")]
    private static partial Regex RadioMessageRegex();

    [GeneratedRegex("^Telephone message from (?<user>.+?) as (?<name>.+?) on (?<source>.+?): (?<message>.*)$")]
    private static partial Regex TelephoneMessageAsRegex();

    [GeneratedRegex("^Telephone message from (?<user>.+?) on (?<source>.+?): (?<message>.*)$")]
    private static partial Regex TelephoneMessageRegex();

    [GeneratedRegex("^Parroting entity (?<entity>.+?) learned the phrase \\\"(?<message>.*)\\\" from (?<speaker>.+)$")]
    private static partial Regex ParrotLearnedPhraseRegex();

    [GeneratedRegex("^(?<actor>.+?) set the name of (?<camera>.+?) to \\\"(?<name>.*)\\.\\\"$")]
    private static partial Regex CameraRenamedRegex();

    [GeneratedRegex("^(?<player>.+?) opened vote menu$")]
    private static partial Regex OpenedVoteMenuRegex();

    [GeneratedRegex("^(?<initiator>.+?) initiated a (?<type>.+?) vote$")]
    private static partial Regex InitiatedVoteRegex();

    [GeneratedRegex("^(?<initiator>.+?) initiated a (?<type>.+?) vote with the arguments: (?<args>.*)$")]
    private static partial Regex InitiatedVoteWithArgsRegex();

    [GeneratedRegex("^Initiated a (?<type>.+?) vote$")]
    private static partial Regex InitiatedVoteByServerRegex();

    [GeneratedRegex("^(?<player>.+?) failed to start (?<type>.+?) vote$")]
    private static partial Regex FailedToStartVoteRegex();

    [GeneratedRegex("^Restart vote attempted to pass, but an admin was online\\. (?<yes>.+?)/(?<no>.+)$")]
    private static partial Regex RestartVoteAdminOnlineRegex();

    [GeneratedRegex("^Restart vote succeeded: (?<yes>.+?)/(?<no>.+)$")]
    private static partial Regex RestartVoteSucceededRegex();

    [GeneratedRegex("^Restart vote failed: (?<yes>.+?)/(?<no>.+)$")]
    private static partial Regex RestartVoteFailedRegex();

    [GeneratedRegex("^Restart vote failed: Current Ghost player percentage:(?<current>.+?)% does not meet (?<required>.+?)%$")]
    private static partial Regex RestartVoteGhostRequirementRegex();

    [GeneratedRegex("^Preset vote finished: (?<picked>.*)$")]
    private static partial Regex PresetVoteFinishedRegex();

    [GeneratedRegex("^Map vote finished: (?<picked>.*)$")]
    private static partial Regex MapVoteFinishedRegex();

    [GeneratedRegex("^(?<player>.+?) initiated a custom vote: (?<title>.+?) - (?<options>.*)$")]
    private static partial Regex InitiatedCustomVoteRegex();

    [GeneratedRegex("^Initiated a custom vote: (?<title>.+?) - (?<options>.*)$")]
    private static partial Regex InitiatedCustomVoteByServerRegex();

    [GeneratedRegex("^Custom vote (?<title>.+?) finished as tie: (?<ties>.*)$")]
    private static partial Regex CustomVoteTieRegex();

    [GeneratedRegex("^Custom vote (?<title>.+?) finished: (?<winner>.*)$")]
    private static partial Regex CustomVoteFinishedRegex();

    [GeneratedRegex("^(?<player>.+?) canceled vote: (?<title>.*)$")]
    private static partial Regex CanceledVoteRegex();

    [GeneratedRegex("^Canceled vote: (?<title>.*)$")]
    private static partial Regex CanceledVoteByServerRegex();

    [GeneratedRegex("^Votekick attempted by (?<initiator>.+?), but they are not eligible to votekick!$")]
    private static partial Regex VotekickInitiatorIneligibleRegex();

    [GeneratedRegex("^Votekick attempted by (?<initiator>.+?) for player string (?<target>.+?), but they could not be found!$")]
    private static partial Regex VotekickTargetNotFoundRegex();

    [GeneratedRegex("^Votekick attempted by (?<initiator>.+?) for themselves\\? Votekick cancelled\\.$")]
    private static partial Regex VotekickSelfRegex();

    [GeneratedRegex("^Votekick attempted by (?<initiator>.+?) for player (?<target>.+?), but there were not enough ghost roles! (?<required>.+?) required, (?<found>.+?) found\\.$")]
    private static partial Regex VotekickNotEnoughGhostRolesRegex();

    [GeneratedRegex("^Votekick attempted by (?<initiator>.+?) for player (?<target>.+?), but they are not eligible to be votekicked!$")]
    private static partial Regex VotekickTargetIneligibleRegex();

    [GeneratedRegex("^Votekick for (?<username>.+?) \\((?<targetName>.+?)\\) due to (?<reason>.+?) started, initiated by (?<initiator>.+?)\\.$")]
    private static partial Regex VotekickStartedRegex();

    [GeneratedRegex("^Votekick for (?<username>.+?) attempted to pass, but an admin was online\\. Yes: (?<yes>.+?) / No: (?<no>.+?)\\. Yes: (?<yesVoters>.*) / No: (?<noVoters>.*)$")]
    private static partial Regex VotekickAdminOnlineRegex();

    [GeneratedRegex("^Votekick for (?<username>.+?) due to (?<reason>.+?) finished, created by (?<initiator>.+?), but was cancelled due to the target being an antagonist\\.$")]
    private static partial Regex VotekickCancelledAntagRegex();

    [GeneratedRegex("^Votekick for (?<username>.+?) due to (?<reason>.+?) finished, created by (?<initiator>.+?), but was cancelled due to the target being a de-admined admin\\.$")]
    private static partial Regex VotekickCancelledAdminRegex();

    [GeneratedRegex("^Votekick for (?<username>.+?) succeeded:\\s+Yes: (?<yes>.+?) / No: (?<no>.+?)\\. Yes: (?<yesVoters>.*) / No: (?<noVoters>.*)$")]
    private static partial Regex VotekickSucceededRegex();

    [GeneratedRegex("^Votekick failed: Yes: (?<yes>.+?) / No: (?<no>.+?)\\. Yes: (?<yesVoters>.*) / No: (?<noVoters>.*)$")]
    private static partial Regex VotekickFailedRegex();

    [GeneratedRegex("^(?<player>.+?) \\((?<userId>.+?)\\) changed CVAR (?<cvar>.+?) from (?<oldValue>.+?) to (?<newValue>.*)$")]
    private static partial Regex CvarChangedRegex();
}
