using System.Text.RegularExpressions;
using Content.Shared.Database;

namespace Content.Server.Administration.Logs;

public sealed partial class AdminLogManager
{
    private static string LocalizeMindMessage(string rawMessage)
    {
        if (TryTranslateSingleSubjectMessage(MindAttemptedSuicideRegex(), rawMessage, "пытается совершить самоубийство", out var attemptedSuicide))
            return attemptedSuicide;

        if (TryTranslateSingleSubjectMessage(MindSuicidedRegex(), rawMessage, "совершил самоубийство.", out var suicided))
            return suicided;

        var revolutionaryConversion = RevolutionaryConversionRegex().Match(rawMessage);
        if (revolutionaryConversion.Success)
        {
            return $"{revolutionaryConversion.Groups["user"].Value} обратил {revolutionaryConversion.Groups["target"].Value} в революционера";
        }

        if (TryTranslateSingleSubjectMessage(MindDeconvertedHeadRevRegex(), rawMessage, "был деконвертирован из-за смерти всех глав революции.", out var deconvertedHeadRev))
            return deconvertedHeadRev;

        if (TryTranslateSingleSubjectMessage(MindForcedGhostRegex(), rawMessage, "был принудительно переведён в призрака через команду", out var forcedGhost))
            return forcedGhost;

        if (TryTranslateSingleSubjectMessage(MindAttemptGhostRegex(), rawMessage, "пытается стать призраком через команду", out var attemptGhost))
            return attemptGhost;

        var ghosted = MindGhostedRegex().Match(rawMessage);
        if (ghosted.Success)
        {
            var nonReturnable = ghosted.Groups["suffix"].Value.Length > 0 ? " (без возврата)" : string.Empty;
            return $"{ghosted.Groups["player"].Value} стал призраком{nonReturnable}";
        }

        var returnedToBody = MindReturnedToBodyRegex().Match(rawMessage);
        if (returnedToBody.Success)
        {
            return $"{returnedToBody.Groups["player"].Value} вернулся в {returnedToBody.Groups["entity"].Value}";
        }

        if (TryTranslateSingleSubjectMessage(MindDeconvertedMindshieldRegex(), rawMessage, "был деконвертирован из-за имплантации майндшилда.", out var deconvertedMindshield))
            return deconvertedMindshield;

        var ionStormLawChange = MindIonStormLawChangeRegex().Match(rawMessage);
        if (ionStormLawChange.Success)
        {
            return $"{ionStormLawChange.Groups["silicon"].Value} получил изменённые ионным штормом законы: {ionStormLawChange.Groups["laws"].Value}";
        }

        var ionStormSpawn = MindIonStormSpawnRegex().Match(rawMessage);
        if (ionStormSpawn.Success)
        {
            return $"{ionStormSpawn.Groups["silicon"].Value} появился с законами ионного шторма: {ionStormSpawn.Groups["laws"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeConnectionMessage(string rawMessage)
    {
        var connected = ConnectionConnectedRegex().Match(rawMessage);
        if (connected.Success)
        {
            return $"Пользователь {connected.Groups["player"].Value}, привязанный к {connected.Groups["entity"].Value}, подключился к игре.";
        }

        var disconnected = ConnectionDisconnectedRegex().Match(rawMessage);
        if (disconnected.Success)
        {
            return $"Пользователь {disconnected.Groups["player"].Value}, привязанный к {disconnected.Groups["entity"].Value}, отключился от игры.";
        }

        if (TryTranslateSingleSubjectMessage(ConnectionFuckRulesRegex(), rawMessage, "использовал команду fuckrules.", out var fuckRules))
            return fuckRules;

        return rawMessage;
    }

    private static string LocalizeAdminMessageLog(string rawMessage)
    {
        var subtleMessage = AdminSubtleMessageRegex().Match(rawMessage);
        if (subtleMessage.Success)
        {
            return $"{subtleMessage.Groups["player"].Value} получил тонкое сообщение от {subtleMessage.Groups["source"].Value}: {subtleMessage.Groups["message"].Value}";
        }

        var prayer = AdminPrayerRegex().Match(rawMessage);
        if (prayer.Success)
        {
            return $"{prayer.Groups["player"].Value} отправил молитву ({prayer.Groups["prefix"].Value}): {prayer.Groups["message"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeConstructionMessage(string rawMessage)
    {
        var changedNode = ConstructionChangedNodeRegex().Match(rawMessage);
        if (changedNode.Success)
        {
            return $"{changedNode.Groups["player"].Value} изменил узел {changedNode.Groups["entity"].Value} с \"{changedNode.Groups["oldNode"].Value}\" на \"{changedNode.Groups["newNode"].Value}\"";
        }

        var completedGhost = ConstructionCompletedGhostRegex().Match(rawMessage);
        if (completedGhost.Success)
        {
            return $"{completedGhost.Groups["player"].Value} превратил призрак конструкции {completedGhost.Groups["prototype"].Value} в {completedGhost.Groups["entity"].Value} на {completedGhost.Groups["coordinates"].Value}";
        }

        var placedCable = ConstructionPlacedRegex().Match(rawMessage);
        if (placedCable.Success)
        {
            return $"{placedCable.Groups["player"].Value} разместил {placedCable.Groups["entity"].Value} на {placedCable.Groups["coordinates"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeTeleportMessage(string rawMessage)
    {
        var shuffled = TeleportShuffledRegex().Match(rawMessage);
        if (shuffled.Success)
        {
            return $"{shuffled.Groups["entity"].Value} был перемешан в {shuffled.Groups["target"].Value} аномалией {shuffled.Groups["source"].Value} на {shuffled.Groups["coordinates"].Value}";
        }

        var teleportedByAnomaly = TeleportByAnomalyRegex().Match(rawMessage);
        if (teleportedByAnomaly.Success)
        {
            return $"{teleportedByAnomaly.Groups["entity"].Value} был телепортирован в {teleportedByAnomaly.Groups["target"].Value} сверхкритической {teleportedByAnomaly.Groups["source"].Value} на {teleportedByAnomaly.Groups["coordinates"].Value}";
        }

        var teleportedViaPortal = TeleportViaPortalRegex().Match(rawMessage);
        if (teleportedViaPortal.Success)
        {
            return $"{teleportedViaPortal.Groups["player"].Value} телепортировался через {teleportedViaPortal.Groups["portal"].Value} из {teleportedViaPortal.Groups["source"].Value} в {teleportedViaPortal.Groups["target"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeAnomalyMessage(string rawMessage)
    {
        if (TryTranslateSingleSubjectMessage(AnomalyHostRegex(), rawMessage, "стал носителем аномалии.", out var anomalyHost))
            return anomalyHost;

        if (TryTranslateSingleSubjectMessage(AnomalyNoLongerHostRegex(), rawMessage, "больше не является носителем аномалии.", out var noLongerHost))
            return noLongerHost;

        return rawMessage;
    }

    private static string LocalizeAntagSelectionMessage(string rawMessage)
    {
        var preSelected = AntagPreselectedRegex().Match(rawMessage);
        if (preSelected.Success)
        {
            return $"Предварительно выбран антагонист {preSelected.Groups["player"].Value}: {preSelected.Groups["entity"].Value}";
        }

        var tryingToMakeAntag = AntagTryingToMakeRegex().Match(rawMessage);
        if (tryingToMakeAntag.Success)
        {
            return $"Начата попытка сделать {tryingToMakeAntag.Groups["player"].Value} антагонистом: {tryingToMakeAntag.Groups["entity"].Value}";
        }

        var noEntity = AntagNoEntityRegex().Match(rawMessage);
        if (noEntity.Success)
        {
            return $"Попытка сделать {noEntity.Groups["player"].Value} антагонистом в геймправиле {noEntity.Groups["entity"].Value} провалилась: у игрока не было подходящей сущности.";
        }

        var missingSpawner = AntagMissingSpawnerRegex().Match(rawMessage);
        if (missingSpawner.Success)
        {
            return $"Спавнер антагониста {missingSpawner.Groups["player"].Value} в геймправиле {missingSpawner.Groups["entity"].Value} не сработал: отсутствует GhostRoleAntagSpawnerComponent.";
        }

        var assigned = AntagAssignedRegex().Match(rawMessage);
        if (assigned.Success)
        {
            return $"Назначен антагонист {assigned.Groups["mind"].Value}: {assigned.Groups["entity"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeAsphyxiationMessage(string rawMessage)
    {
        if (TryTranslateSingleSubjectMessage(AsphyxiationStartedRegex(), rawMessage, "начал задыхаться", out var started))
            return started;

        if (TryTranslateSingleSubjectMessage(AsphyxiationStoppedRegex(), rawMessage, "перестал задыхаться", out var stopped))
            return stopped;

        return rawMessage;
    }

    private static string LocalizeBarotraumaMessage(string rawMessage)
    {
        if (TryTranslateSingleSubjectMessage(BarotraumaLowRegex(), rawMessage, "начал получать урон от низкого давления", out var low))
            return low;

        if (TryTranslateSingleSubjectMessage(BarotraumaHighRegex(), rawMessage, "начал получать урон от высокого давления", out var high))
            return high;

        if (TryTranslateSingleSubjectMessage(BarotraumaStoppedRegex(), rawMessage, "перестал получать урон от давления", out var stopped))
            return stopped;

        return rawMessage;
    }

    private static string LocalizeFlammableMessage(string rawMessage)
    {
        var atmosIgnition = FlammableAtmosIgnitionRegex().Match(rawMessage);
        if (atmosIgnition.Success)
        {
            return $"Тепло/искра от {atmosIgnition.Groups["source"].Value} вызвала атмосферное воспламенение газа: {atmosIgnition.Groups["gas"].Value}";
        }

        if (TryTranslateSingleSubjectMessage(FlammableStoppedRegex(), rawMessage, "перестал получать урон от огня", out var stopped))
            return stopped;

        var setOnFireWithTool = FlammableSetOnFireWithToolRegex().Match(rawMessage);
        if (setOnFireWithTool.Success)
        {
            return $"{setOnFireWithTool.Groups["target"].Value} был подожжён {setOnFireWithTool.Groups["actor"].Value} с помощью {setOnFireWithTool.Groups["tool"].Value}";
        }

        var setOnFire = FlammableSetOnFireRegex().Match(rawMessage);
        if (setOnFire.Success)
        {
            return $"{setOnFire.Groups["target"].Value} был подожжён {setOnFire.Groups["actor"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeBotanyMessage(string rawMessage)
    {
        var autoHarvested = BotanyAutoHarvestRegex().Match(rawMessage);
        if (autoHarvested.Success)
        {
            return $"Автособран урожай {autoHarvested.Groups["seed"].Value} на позиции {autoHarvested.Groups["position"].Value}.";
        }

        var harvested = BotanyHarvestRegex().Match(rawMessage);
        if (harvested.Success)
        {
            return $"{harvested.Groups["player"].Value} собрал {harvested.Groups["seed"].Value} на позиции {harvested.Groups["position"].Value}.";
        }

        var planted = BotanyPlantRegex().Match(rawMessage);
        if (planted.Success)
        {
            return $"{planted.Groups["player"].Value} посадил {planted.Groups["seed"].Value} на позиции {planted.Groups["position"].Value}.";
        }

        return rawMessage;
    }

    private static string LocalizeBulletHitMessage(string rawMessage)
    {
        var bulletHit = BulletHitRegex().Match(rawMessage);
        if (bulletHit.Success)
        {
            return $"Снаряд {bulletHit.Groups["projectile"].Value}, выпущенный {bulletHit.Groups["user"].Value}, попал в {bulletHit.Groups["target"].Value} и нанёс {bulletHit.Groups["damage"].Value} урона";
        }

        return rawMessage;
    }

    private static string LocalizeCableCutMessage(string rawMessage)
    {
        var cableCut = CableCutRegex().Match(rawMessage);
        if (cableCut.Success)
        {
            return $"{cableCut.Groups["cable"].Value} на {cableCut.Groups["coordinates"].Value} был перерезан {cableCut.Groups["user"].Value}.";
        }

        return rawMessage;
    }

    private static string LocalizeCanisterPurgedMessage(string rawMessage)
    {
        var canisterPurged = CanisterPurgedRegex().Match(rawMessage);
        if (canisterPurged.Success)
        {
            return $"Канистра {canisterPurged.Groups["canister"].Value} стравила своё содержимое {canisterPurged.Groups["gas"].Value} в окружающую среду.";
        }

        var portableScrubberPurged = PortableScrubberPurgedRegex().Match(rawMessage);
        if (portableScrubberPurged.Success)
        {
            return $"Переносной скруббер {portableScrubberPurged.Groups["canister"].Value} стравил своё содержимое {portableScrubberPurged.Groups["gas"].Value} в окружающую среду.";
        }

        return rawMessage;
    }

    private static string LocalizeCrayonDrawMessage(string rawMessage)
    {
        var drewCrayon = CrayonDrewRegex().Match(rawMessage);
        if (drewCrayon.Success)
        {
            return $"{drewCrayon.Groups["user"].Value} нарисовал {drewCrayon.Groups["color"].Value} {drewCrayon.Groups["state"].Value}";
        }

        var paintedDecal = CrayonPaintedRegex().Match(rawMessage);
        if (paintedDecal.Success)
        {
            return $"{paintedDecal.Groups["user"].Value} нарисовал {paintedDecal.Groups["decal"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeDamagedMessage(string rawMessage)
    {
        var welderDamage = DamagedWelderRegex().Match(rawMessage);
        if (welderDamage.Success)
        {
            return $"{welderDamage.Groups["user"].Value} использовал {welderDamage.Groups["used"].Value} как сварочный аппарат и нанёс {welderDamage.Groups["damage"].Value} урона {welderDamage.Groups["target"].Value}";
        }

        var toolDamage = DamagedToolRegex().Match(rawMessage);
        if (toolDamage.Success)
        {
            return $"{toolDamage.Groups["user"].Value} использовал {toolDamage.Groups["used"].Value} как инструмент и нанёс {toolDamage.Groups["damage"].Value} урона {toolDamage.Groups["target"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeDeviceLinkingMessage(string rawMessage)
    {
        var savedDevice = DeviceLinkingSavedRegex().Match(rawMessage);
        if (savedDevice.Success)
        {
            return $"{savedDevice.Groups["actor"].Value} сохранил {savedDevice.Groups["subject"].Value} в {savedDevice.Groups["tool"].Value}";
        }

        var removedBuffered = DeviceLinkingRemovedBufferedRegex().Match(rawMessage);
        if (removedBuffered.Success)
        {
            return $"{removedBuffered.Groups["actor"].Value} удалил буферизованное устройство {removedBuffered.Groups["subject"].Value} из {removedBuffered.Groups["tool"].Value}";
        }

        var clearedBuffered = DeviceLinkingClearedBufferedRegex().Match(rawMessage);
        if (clearedBuffered.Success)
        {
            return $"{clearedBuffered.Groups["actor"].Value} очистил буферизованные устройства в {clearedBuffered.Groups["tool"].Value}";
        }

        var clearedLinksBetween = DeviceLinkingClearedLinksBetweenRegex().Match(rawMessage);
        if (clearedLinksBetween.Success)
        {
            return $"{clearedLinksBetween.Groups["actor"].Value} очистил связи между {clearedLinksBetween.Groups["subject"].Value} и {clearedLinksBetween.Groups["subject2"].Value} с помощью {clearedLinksBetween.Groups["tool"].Value}";
        }

        var setDeviceLinks = DeviceLinkingSetRegex().Match(rawMessage);
        if (setDeviceLinks.Success)
        {
            return $"{setDeviceLinks.Groups["actor"].Value} установил ссылки устройств в {setDeviceLinks.Groups["subject"].Value} с помощью {setDeviceLinks.Groups["tool"].Value}";
        }

        var addedDeviceLinks = DeviceLinkingAddedRegex().Match(rawMessage);
        if (addedDeviceLinks.Success)
        {
            return $"{addedDeviceLinks.Groups["actor"].Value} добавил ссылки устройств в {addedDeviceLinks.Groups["subject"].Value} с помощью {addedDeviceLinks.Groups["tool"].Value}";
        }

        var clearedDeviceLinks = DeviceLinkingClearedRegex().Match(rawMessage);
        if (clearedDeviceLinks.Success)
        {
            return $"{clearedDeviceLinks.Groups["actor"].Value} очистил ссылки устройств в {clearedDeviceLinks.Groups["subject"].Value} с помощью {clearedDeviceLinks.Groups["tool"].Value}";
        }

        var copiedDevices = DeviceLinkingCopiedRegex().Match(rawMessage);
        if (copiedDevices.Success)
        {
            return $"{copiedDevices.Groups["actor"].Value} скопировал устройства из {copiedDevices.Groups["subject"].Value} в {copiedDevices.Groups["tool"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeDeviceNetworkMessage(string rawMessage)
    {
        var broadcast = DeviceNetworkBroadcastRegex().Match(rawMessage);
        if (broadcast.Success)
        {
            return $"{broadcast.Groups["player"].Value} отправил следующее широковещательное сообщение: {broadcast.Groups["message"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeElectrocutionMessage(string rawMessage)
    {
        var electrocution = ElectrocutionRegex().Match(rawMessage);
        if (electrocution.Success)
        {
            var source = electrocution.Groups["source"].Success ? $" от {electrocution.Groups["source"].Value}" : string.Empty;
            return $"{electrocution.Groups["entity"].Value} получил {electrocution.Groups["damage"].Value} урона от электрического удара{source}";
        }

        return rawMessage;
    }

    private static string LocalizeEmergencyShuttleMessage(string rawMessage)
    {
        var repealAll = EmergencyShuttleRepealAllRegex().Match(rawMessage);
        if (repealAll.Success)
        {
            return $"Экстренный шаттл: полная отмена раннего запуска от {repealAll.Groups["user"].Value}";
        }

        var repeal = EmergencyShuttleRepealRegex().Match(rawMessage);
        if (repeal.Success)
        {
            return $"Экстренный шаттл: отмена раннего запуска от {repeal.Groups["user"].Value}";
        }

        var auth = EmergencyShuttleAuthRegex().Match(rawMessage);
        if (auth.Success)
        {
            return $"Экстренный шаттл: ранний запуск авторизован {auth.Groups["user"].Value}";
        }

        if (rawMessage == "Emergency shuttle launch authorized")
            return "Экстренный шаттл: запуск авторизован";

        if (rawMessage == "Round ended, showing summary")
            return "Раунд завершён, показывается сводка";

        return rawMessage;
    }

    private static string LocalizeEntityDeleteMessage(string rawMessage)
    {
        var eventHorizonDelete = EntityDeleteEventHorizonRegex().Match(rawMessage);
        if (eventHorizonDelete.Success)
        {
            return $"{eventHorizonDelete.Groups["player"].Value} вошёл в горизонт событий {eventHorizonDelete.Groups["entity"].Value} и был удалён";
        }

        var closedPortals = EntityDeleteClosedPortalsRegex().Match(rawMessage);
        if (closedPortals.Success)
        {
            return $"{closedPortals.Groups["player"].Value} закрыл {closedPortals.Groups["portals"].Value} с помощью {closedPortals.Groups["tool"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeEntitySpawnMessage(string rawMessage)
    {
        var logProbe = EntitySpawnLogProbeRegex().Match(rawMessage);
        if (logProbe.Success)
        {
            return $"{logProbe.Groups["user"].Value} распечатал логи LogProbe ({logProbe.Groups["paper"].Value}) для {logProbe.Groups["entity"].Value}";
        }

        var spawnFromUse = EntitySpawnFromUseRegex().Match(rawMessage);
        if (spawnFromUse.Success)
        {
            return $"{spawnFromUse.Groups["user"].Value} использовал {spawnFromUse.Groups["spawner"].Value}, что создало {spawnFromUse.Groups["spawned"].Value}";
        }

        var openedPortal = EntitySpawnOpenedPortalRegex().Match(rawMessage);
        if (openedPortal.Success)
        {
            return $"{openedPortal.Groups["player"].Value} открыл {openedPortal.Groups["portal"].Value} на {openedPortal.Groups["coordinates"].Value} с помощью {openedPortal.Groups["tool"].Value}";
        }

        var openedLinkedPortal = EntitySpawnOpenedLinkedPortalRegex().Match(rawMessage);
        if (openedLinkedPortal.Success)
        {
            return $"{openedLinkedPortal.Groups["player"].Value} открыл {openedLinkedPortal.Groups["portal"].Value} на {openedLinkedPortal.Groups["coordinates"].Value}, связанный с {openedLinkedPortal.Groups["linked"].Value}, используя {openedLinkedPortal.Groups["tool"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeEventMessage(LogType type, string rawMessage)
    {
        var eventAddedOrAnnounced = EventAddedOrAnnouncedRegex().Match(rawMessage);
        if (eventAddedOrAnnounced.Success)
        {
            return $"Событие добавлено / объявлено: {eventAddedOrAnnounced.Groups["entity"].Value}";
        }

        var eventStarted = EventStartedRegex().Match(rawMessage);
        if (eventStarted.Success)
        {
            return $"Событие началось: {eventStarted.Groups["entity"].Value}";
        }

        var eventEnded = EventEndedRegex().Match(rawMessage);
        if (eventEnded.Success)
        {
            return $"Событие завершилось: {eventEnded.Groups["entity"].Value}";
        }

        var codewordsGenerated = EventCodewordsRegex().Match(rawMessage);
        if (codewordsGenerated.Success)
        {
            return $"Для фракции {codewordsGenerated.Groups["faction"].Value} сгенерированы кодовые слова: {codewordsGenerated.Groups["codewords"].Value}";
        }

        var addedGameRule = EventAddedGameRuleRegex().Match(rawMessage);
        if (addedGameRule.Success)
        {
            return $"Добавлено геймправило {addedGameRule.Groups["entity"].Value}";
        }

        var queuedGameRule = EventQueuedGameRuleRegex().Match(rawMessage);
        if (queuedGameRule.Success)
        {
            return $"Запланирован запуск геймправила {queuedGameRule.Groups["entity"].Value} с задержкой {queuedGameRule.Groups["delay"].Value}";
        }

        var startedGameRule = EventStartedGameRuleRegex().Match(rawMessage);
        if (startedGameRule.Success)
        {
            return $"Запущено геймправило {startedGameRule.Groups["entity"].Value}";
        }

        var addGameRuleViaCommand = EventAddGameRuleViaCommandRegex().Match(rawMessage);
        if (addGameRuleViaCommand.Success)
        {
            return $"{addGameRuleViaCommand.Groups["player"].Value} попытался добавить геймправило [{addGameRuleViaCommand.Groups["rule"].Value}] через команду";
        }

        var unknownAddGameRule = EventUnknownAddGameRuleRegex().Match(rawMessage);
        if (unknownAddGameRule.Success)
        {
            return $"Неизвестный источник попытался добавить геймправило [{unknownAddGameRule.Groups["rule"].Value}] через команду";
        }

        var endGameRuleViaCommand = EventEndGameRuleViaCommandRegex().Match(rawMessage);
        if (endGameRuleViaCommand.Success)
        {
            return $"{endGameRuleViaCommand.Groups["player"].Value} попытался завершить геймправило [{endGameRuleViaCommand.Groups["rule"].Value}] через команду";
        }

        var unknownEndGameRule = EventUnknownEndGameRuleRegex().Match(rawMessage);
        if (unknownEndGameRule.Success)
        {
            return $"Неизвестный источник попытался завершить геймправило [{unknownEndGameRule.Groups["rule"].Value}] через команду";
        }

        var triggerAddedGameRule = EventTriggerAddedGameRuleRegex().Match(rawMessage);
        if (triggerAddedGameRule.Success)
        {
            return $"{triggerAddedGameRule.Groups["user"].Value} добавил геймправило [{triggerAddedGameRule.Groups["rule"].Value}] через триггер на {triggerAddedGameRule.Groups["entity"].Value}.";
        }

        var triggerStartedGameRule = EventTriggerStartedGameRuleRegex().Match(rawMessage);
        if (triggerStartedGameRule.Success)
        {
            return $"{triggerStartedGameRule.Groups["user"].Value} запустил геймправило [{triggerStartedGameRule.Groups["rule"].Value}].";
        }

        var selectedSecretPreset = EventSecretPresetRegex().Match(rawMessage);
        if (selectedSecretPreset.Success)
        {
            return $"В качестве секретного пресета выбран {selectedSecretPreset.Groups["preset"].Value}.";
        }

        var ruleRanWithCost = EventRanWithCostRegex().Match(rawMessage);
        if (ruleRanWithCost.Success)
        {
            return $"{ruleRanWithCost.Groups["entity"].Value} выполнил правило {ruleRanWithCost.Groups["rule"].Value} стоимостью {ruleRanWithCost.Groups["cost"].Value} при бюджете {ruleRanWithCost.Groups["budget"].Value}.";
        }

        var ruleRanWithoutCost = EventRanWithoutCostRegex().Match(rawMessage);
        if (ruleRanWithoutCost.Success)
        {
            return $"{ruleRanWithoutCost.Groups["entity"].Value} выполнил правило {ruleRanWithoutCost.Groups["rule"].Value} без стоимости.";
        }

        return rawMessage;
    }

    private static string LocalizeExplosiveDepressurizationMessage(string rawMessage)
    {
        var depressurization = ExplosiveDepressurizationRegex().Match(rawMessage);
        if (depressurization.Success)
        {
            return $"Взрывная разгерметизация удалила {depressurization.Groups["moles"].Value} молей из {depressurization.Groups["tiles"].Value} тайлов, начиная с позиции {depressurization.Groups["position"].Value} на гриде {depressurization.Groups["grid"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeFieldGenerationMessage(string rawMessage)
    {
        if (TryTranslateSingleSubjectMessage(FieldGenerationLostConnectionsRegex(), rawMessage, "потерял полевые соединения", out var lostConnections))
            return lostConnections;

        var toggledEmitter = FieldGenerationToggledEmitterRegex().Match(rawMessage);
        if (toggledEmitter.Success)
        {
            return $"{toggledEmitter.Groups["player"].Value} переключил {toggledEmitter.Groups["emitter"].Value} в состояние {LocalizeOnOffState(toggledEmitter.Groups["state"].Value)}";
        }

        return rawMessage;
    }

    private static string LocalizeForceFeedMessage(string rawMessage)
    {
        var ingestedSmoke = ForceFeedSmokeRegex().Match(rawMessage);
        if (ingestedSmoke.Success)
        {
            return $"{ingestedSmoke.Groups["target"].Value} вдохнул дым {ingestedSmoke.Groups["solution"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeGhostRoleTakenMessage(string rawMessage)
    {
        var ghostRole = GhostRoleTakenRegex().Match(rawMessage);
        if (ghostRole.Success)
        {
            return $"{ghostRole.Groups["player"].Value} занял роль призрака {ghostRole.Groups["role"].Value}: {ghostRole.Groups["entity"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeGhostWarpMessage(string rawMessage)
    {
        var ghostWarp = GhostWarpRegex().Match(rawMessage);
        if (ghostWarp.Success)
        {
            return $"{ghostWarp.Groups["ghost"].Value} телепортировался призраком к {ghostWarp.Groups["target"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeIdentityMessage(string rawMessage)
    {
        var criminalStatus = IdentityCriminalStatusRegex().Match(rawMessage);
        if (criminalStatus.Success)
        {
            return $"{criminalStatus.Groups["actor"].Value} изменил криминальный статус для {criminalStatus.Groups["name"].Value} на \"{criminalStatus.Groups["status"].Value}\"";
        }

        return rawMessage;
    }

    private static string LocalizeItemConfigureMessage(string rawMessage)
    {
        var breakerState = ItemConfigureMainBreakerRegex().Match(rawMessage);
        if (breakerState.Success)
        {
            return $"{breakerState.Groups["user"].Value} установил состояние главного выключателя {breakerState.Groups["entity"].Value} на {LocalizeEnabledDisabledState(breakerState.Groups["state"].Value)}.";
        }

        var battlecry = ItemConfigureBattlecryRegex().Match(rawMessage);
        if (battlecry.Success)
        {
            return $"Боевой клич {battlecry.Groups["entity"].Value} изменён на {battlecry.Groups["battlecry"].Value}";
        }

        var turretState = ItemConfigureTurretStateRegex().Match(rawMessage);
        if (turretState.Success)
        {
            return $"{turretState.Groups["user"].Value} установил {turretState.Groups["entity"].Value} в состояние {turretState.Groups["state"].Value}";
        }

        var turretAuthorization = ItemConfigureTurretAuthorizationRegex().Match(rawMessage);
        if (turretAuthorization.Success)
        {
            return $"{turretAuthorization.Groups["user"].Value} установил авторизацию {turretAuthorization.Groups["exemption"].Value} для {turretAuthorization.Groups["entity"].Value} в состояние {LocalizeBooleanWord(turretAuthorization.Groups["enabled"].Value)}";
        }

        return rawMessage;
    }

    private static string LocalizeLateJoinMessage(string rawMessage)
    {
        var lateJoin = LateJoinRegex().Match(rawMessage);
        if (lateJoin.Success)
        {
            return $"Игрок {lateJoin.Groups["player"].Value} поздно присоединился как {lateJoin.Groups["character"].Value} на станции {lateJoin.Groups["station"].Value}, используя {lateJoin.Groups["entity"].Value} в роли {lateJoin.Groups["job"].Value}.";
        }

        var observerLateJoin = LateJoinObserverRegex().Match(rawMessage);
        if (observerLateJoin.Success)
        {
            return $"{observerLateJoin.Groups["player"].Value} поздно присоединился к раунду как наблюдатель с {observerLateJoin.Groups["entity"].Value}.";
        }

        return rawMessage;
    }

    private static string LocalizePdaInteractMessage(string rawMessage)
    {
        var addedNote = PdaAddedNoteRegex().Match(rawMessage);
        if (addedNote.Success)
        {
            return $"{addedNote.Groups["actor"].Value} добавил заметку в PDA: '{addedNote.Groups["note"].Value}' в {addedNote.Groups["pda"].Value}";
        }

        var removedNote = PdaRemovedNoteRegex().Match(rawMessage);
        if (removedNote.Success)
        {
            return $"{removedNote.Groups["actor"].Value} удалил заметку из PDA: '{removedNote.Groups["note"].Value}' из {removedNote.Groups["pda"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeRespawnMessage(string rawMessage)
    {
        var respawnedPlayer = RespawnedPlayerRegex().Match(rawMessage);
        if (respawnedPlayer.Success)
        {
            return $"Игрок {respawnedPlayer.Groups["player"].Value} был возрождён.";
        }

        var specialRespawn = SpecialRespawnRegex().Match(rawMessage);
        if (specialRespawn.Success)
        {
            return $"{specialRespawn.Groups["oldEntity"].Value} был удалён и возрождён на {specialRespawn.Groups["coordinates"].Value} как {specialRespawn.Groups["newEntity"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeRoundStartJoinMessage(string rawMessage)
    {
        var roundStartJoin = RoundStartJoinRegex().Match(rawMessage);
        if (roundStartJoin.Success)
        {
            return $"Игрок {roundStartJoin.Groups["player"].Value} присоединился как {roundStartJoin.Groups["character"].Value} на станции {roundStartJoin.Groups["station"].Value}, используя {roundStartJoin.Groups["entity"].Value} в роли {roundStartJoin.Groups["job"].Value}.";
        }

        return rawMessage;
    }

    private static string LocalizeShuttleCalledMessage(string rawMessage)
    {
        var shuttleCalledBy = ShuttleCalledByRegex().Match(rawMessage);
        if (shuttleCalledBy.Success)
        {
            return $"Шаттл вызван {shuttleCalledBy.Groups["player"].Value}{shuttleCalledBy.Groups["suffix"].Value}";
        }

        var shuttleCalled = ShuttleCalledRegex().Match(rawMessage);
        if (shuttleCalled.Success)
        {
            return $"Шаттл вызван{shuttleCalled.Groups["suffix"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeShuttleImpactMessage(string rawMessage)
    {
        var shuttleImpact = ShuttleImpactRegex().Match(rawMessage);
        if (shuttleImpact.Success)
        {
            return $"Столкновение шаттла {shuttleImpact.Groups["our"].Value} с {shuttleImpact.Groups["other"].Value} в точке {shuttleImpact.Groups["point"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeShuttleRecalledMessage(string rawMessage)
    {
        var shuttleRecalledBy = ShuttleRecalledByRegex().Match(rawMessage);
        if (shuttleRecalledBy.Success)
        {
            return $"Шаттл отозван {shuttleRecalledBy.Groups["player"].Value}{shuttleRecalledBy.Groups["suffix"].Value}";
        }

        var shuttleRecalled = ShuttleRecalledRegex().Match(rawMessage);
        if (shuttleRecalled.Success)
        {
            return $"Шаттл отозван{shuttleRecalled.Groups["suffix"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeStorePurchaseMessage(string rawMessage)
    {
        var storePurchase = StorePurchaseRegex().Match(rawMessage);
        if (storePurchase.Success)
        {
            var extra = LocalizeStorePurchaseExtra(storePurchase.Groups["extra"].Value);
            return $"{storePurchase.Groups["player"].Value} приобрёл лот \"{storePurchase.Groups["listing"].Value}\" в {storePurchase.Groups["store"].Value}{extra}.";
        }

        return rawMessage;
    }

    private static string LocalizeStoreRefundMessage(string rawMessage)
    {
        var refund = StoreRefundRegex().Match(rawMessage);
        if (refund.Success)
        {
            return $"{refund.Groups["player"].Value} оформил возврат своих покупок в {refund.Groups["store"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeTemperatureMessage(string rawMessage)
    {
        if (TryTranslateSingleSubjectMessage(TemperatureHighRegex(), rawMessage, "начал получать урон от высокой температуры", out var high))
            return high;

        if (TryTranslateSingleSubjectMessage(TemperatureLowRegex(), rawMessage, "начал получать урон от низкой температуры", out var low))
            return low;

        if (TryTranslateSingleSubjectMessage(TemperatureStoppedRegex(), rawMessage, "перестал получать урон от температуры", out var stopped))
            return stopped;

        return rawMessage;
    }

    private static string LocalizeThrowHitMessage(string rawMessage)
    {
        var throwHit = ThrowHitRegex().Match(rawMessage);
        if (throwHit.Success)
        {
            return $"{throwHit.Groups["target"].Value} получил {throwHit.Groups["damage"].Value} урона от столкновения";
        }

        return rawMessage;
    }

    private static string LocalizeWireHackingMessage(string rawMessage)
    {
        var wireAction = WireHackingRegex().Match(rawMessage);
        if (wireAction.Success)
        {
            return $"{wireAction.Groups["player"].Value} {wireAction.Groups["verb"].Value} {wireAction.Groups["color"].Value} провод {wireAction.Groups["name"].Value} ({wireAction.Groups["action"].Value}) в {wireAction.Groups["owner"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeOnOffState(string rawState)
    {
        return rawState switch
        {
            "on" => "вкл",
            "off" => "выкл",
            _ => rawState
        };
    }

    private static string LocalizeEnabledDisabledState(string rawState)
    {
        return rawState switch
        {
            "Enabled" => "включено",
            "Disabled" => "выключено",
            _ => rawState
        };
    }

    private static string LocalizeBooleanWord(string rawValue)
    {
        return rawValue switch
        {
            "True" => "включено",
            "False" => "выключено",
            "true" => "включено",
            "false" => "выключено",
            _ => rawValue
        };
    }

    private static string LocalizeStorePurchaseExtra(string rawExtra)
    {
        return rawExtra switch
        {
            "" => string.Empty,
            ", but was not from an expected faction" => ", но не принадлежал ожидаемой фракции",
            ", but was not from an expected faction while also possessing a mindshield" => ", но не принадлежал ожидаемой фракции и при этом имел майндшилд",
            _ => rawExtra
        };
    }

    [GeneratedRegex("^(?<player>.+?) is attempting to suicide$")]
    private static partial Regex MindAttemptedSuicideRegex();

    [GeneratedRegex("^(?<player>.+?) suicided\\.$")]
    private static partial Regex MindSuicidedRegex();

    [GeneratedRegex("^(?<user>.+?) converted (?<target>.+?) into a Revolutionary$")]
    private static partial Regex RevolutionaryConversionRegex();

    [GeneratedRegex("^(?<player>.+?) was deconverted due to all Head Revolutionaries dying\\.$")]
    private static partial Regex MindDeconvertedHeadRevRegex();

    [GeneratedRegex("^(?<player>.+?) was forced to ghost via command$")]
    private static partial Regex MindForcedGhostRegex();

    [GeneratedRegex("^(?<player>.+?) is attempting to ghost via command$")]
    private static partial Regex MindAttemptGhostRegex();

    [GeneratedRegex("^(?<player>.+?) ghosted(?<suffix> \\(non-returnable\\))?$")]
    private static partial Regex MindGhostedRegex();

    [GeneratedRegex("^(?<player>.+?) returned to (?<entity>.+)$")]
    private static partial Regex MindReturnedToBodyRegex();

    [GeneratedRegex("^(?<player>.+?) was deconverted due to being implanted with a Mindshield\\.$")]
    private static partial Regex MindDeconvertedMindshieldRegex();

    [GeneratedRegex("^(?<silicon>.+?) had its laws changed by an ion storm to (?<laws>.*)$")]
    private static partial Regex MindIonStormLawChangeRegex();

    [GeneratedRegex("^(?<silicon>.+?) spawned with ion stormed laws: (?<laws>.*)$")]
    private static partial Regex MindIonStormSpawnRegex();

    [GeneratedRegex("^User (?<player>.+?) attached to (?<entity>.+?) connected to the game\\.$")]
    private static partial Regex ConnectionConnectedRegex();

    [GeneratedRegex("^User (?<player>.+?) attached to (?<entity>.+?) disconnected from the game\\.$")]
    private static partial Regex ConnectionDisconnectedRegex();

    [GeneratedRegex("^Player (?<player>.+?) used the fuckrules command\\.$")]
    private static partial Regex ConnectionFuckRulesRegex();

    [GeneratedRegex("^(?<player>.+?) received subtle message from (?<source>.+?): (?<message>.*)$")]
    private static partial Regex AdminSubtleMessageRegex();

    [GeneratedRegex("^(?<player>.+?) sent prayer \\((?<prefix>.+?)\\): (?<message>.*)$")]
    private static partial Regex AdminPrayerRegex();

    [GeneratedRegex("^(?<player>.+?) changed (?<entity>.+?)'s node from \\\"(?<oldNode>.*)\\\" to \\\"(?<newNode>.*)\\\"$")]
    private static partial Regex ConstructionChangedNodeRegex();

    [GeneratedRegex("^(?<player>.+?) has turned a (?<prototype>.+?) construction ghost into (?<entity>.+?) at (?<coordinates>.*)$")]
    private static partial Regex ConstructionCompletedGhostRegex();

    [GeneratedRegex("^(?<player>.+?) placed (?<entity>.+?) at (?<coordinates>.*)$")]
    private static partial Regex ConstructionPlacedRegex();

    [GeneratedRegex("^(?<entity>.+?) has been shuffled to (?<target>.+?) by the (?<source>.+?) at (?<coordinates>.+)$")]
    private static partial Regex TeleportShuffledRegex();

    [GeneratedRegex("^(?<entity>.+?) has been teleported to (?<target>.+?) by the supercritical (?<source>.+?) at (?<coordinates>.+)$")]
    private static partial Regex TeleportByAnomalyRegex();

    [GeneratedRegex("^(?<player>.+?) teleported via (?<portal>.+?) from (?<source>.+?) to (?<target>.+)$")]
    private static partial Regex TeleportViaPortalRegex();

    [GeneratedRegex("^(?<player>.+?) became anomaly host\\.$")]
    private static partial Regex AnomalyHostRegex();

    [GeneratedRegex("^(?<player>.+?) is no longer a host for the anomaly\\.$")]
    private static partial Regex AnomalyNoLongerHostRegex();

    [GeneratedRegex("^Pre-selected (?<player>.+?) as antagonist: (?<entity>.+)$")]
    private static partial Regex AntagPreselectedRegex();

    [GeneratedRegex("^Start trying to make (?<player>.+?) become the antagonist: (?<entity>.+)$")]
    private static partial Regex AntagTryingToMakeRegex();

    [GeneratedRegex("^Attempted to make (?<player>.+?) antagonist in gamerule (?<entity>.+?) but there was no valid entity for player\\.$")]
    private static partial Regex AntagNoEntityRegex();

    [GeneratedRegex("^Antag spawner (?<player>.+?) in gamerule (?<entity>.+?) failed due to not having GhostRoleAntagSpawnerComponent\\.$")]
    private static partial Regex AntagMissingSpawnerRegex();

    [GeneratedRegex("^Assigned (?<mind>.+?) as antagonist: (?<entity>.+)$")]
    private static partial Regex AntagAssignedRegex();

    [GeneratedRegex("^(?<player>.+?) started suffocating$")]
    private static partial Regex AsphyxiationStartedRegex();

    [GeneratedRegex("^(?<player>.+?) stopped suffocating$")]
    private static partial Regex AsphyxiationStoppedRegex();

    [GeneratedRegex("^(?<player>.+?) started taking low pressure damage$")]
    private static partial Regex BarotraumaLowRegex();

    [GeneratedRegex("^(?<player>.+?) started taking high pressure damage$")]
    private static partial Regex BarotraumaHighRegex();

    [GeneratedRegex("^(?<player>.+?) stopped taking pressure damage$")]
    private static partial Regex BarotraumaStoppedRegex();

    [GeneratedRegex("^Heat/spark of (?<source>.+?) caused atmos ignition of gas: (?<gas>.*)$")]
    private static partial Regex FlammableAtmosIgnitionRegex();

    [GeneratedRegex("^(?<player>.+?) stopped being on fire damage$")]
    private static partial Regex FlammableStoppedRegex();

    [GeneratedRegex("^(?<target>.+?) set on fire by (?<actor>.+?) with (?<tool>.+)$")]
    private static partial Regex FlammableSetOnFireWithToolRegex();

    [GeneratedRegex("^(?<target>.+?) set on fire by (?<actor>.+)$")]
    private static partial Regex FlammableSetOnFireRegex();

    [GeneratedRegex("^Auto-harvested (?<seed>.+?) at Pos:(?<position>.*)\\.$")]
    private static partial Regex BotanyAutoHarvestRegex();

    [GeneratedRegex("^(?<player>.+?) harvested (?<seed>.+?) at Pos:(?<position>.*)\\.$")]
    private static partial Regex BotanyHarvestRegex();

    [GeneratedRegex("^(?<player>.+?) planted\\s+(?<seed>.+?) at Pos:(?<position>.*)\\.$")]
    private static partial Regex BotanyPlantRegex();

    [GeneratedRegex("^Projectile (?<projectile>.+?) shot by (?<user>.+?) hit (?<target>.+?) and dealt (?<damage>.+?) damage$")]
    private static partial Regex BulletHitRegex();

    [GeneratedRegex("^The (?<cable>.+?) at (?<coordinates>.+?) was cut by (?<user>.+?)\\.$")]
    private static partial Regex CableCutRegex();

    [GeneratedRegex("^Canister (?<canister>.+?) purged its contents of (?<gas>.+?) into the environment\\.$")]
    private static partial Regex CanisterPurgedRegex();

    [GeneratedRegex("^Portable scrubber (?<canister>.+?) purged its contents of (?<gas>.+?) into the environment\\.$")]
    private static partial Regex PortableScrubberPurgedRegex();

    [GeneratedRegex("^(?<user>.+?) drew a (?<color>.+?) (?<state>.+)$")]
    private static partial Regex CrayonDrewRegex();

    [GeneratedRegex("^(?<user>.+?) painted a (?<decal>.+)$")]
    private static partial Regex CrayonPaintedRegex();

    [GeneratedRegex("^(?<user>.+?) used (?<used>.+?) as a welder to deal (?<damage>.+?) damage to (?<target>.+)$")]
    private static partial Regex DamagedWelderRegex();

    [GeneratedRegex("^(?<user>.+?) used (?<used>.+?) as a tool to deal (?<damage>.+?) damage to (?<target>.+)$")]
    private static partial Regex DamagedToolRegex();

    [GeneratedRegex("^(?<actor>.+?) saved (?<subject>.+?) to (?<tool>.+)$")]
    private static partial Regex DeviceLinkingSavedRegex();

    [GeneratedRegex("^(?<actor>.+?) removed buffered device (?<subject>.+?) from (?<tool>.+)$")]
    private static partial Regex DeviceLinkingRemovedBufferedRegex();

    [GeneratedRegex("^(?<actor>.+?) cleared buffered devices from (?<tool>.+)$")]
    private static partial Regex DeviceLinkingClearedBufferedRegex();

    [GeneratedRegex("^(?<actor>.+?) cleared links between (?<subject>.+?) and (?<subject2>.+?) with (?<tool>.+)$")]
    private static partial Regex DeviceLinkingClearedLinksBetweenRegex();

    [GeneratedRegex("^(?<actor>.+?) set device links to (?<subject>.+?) with (?<tool>.+)$")]
    private static partial Regex DeviceLinkingSetRegex();

    [GeneratedRegex("^(?<actor>.+?) added device links to (?<subject>.+?) with (?<tool>.+)$")]
    private static partial Regex DeviceLinkingAddedRegex();

    [GeneratedRegex("^(?<actor>.+?) cleared device links from (?<subject>.+?) with (?<tool>.+)$")]
    private static partial Regex DeviceLinkingClearedRegex();

    [GeneratedRegex("^(?<actor>.+?) copied devices from (?<subject>.+?) to (?<tool>.+)$")]
    private static partial Regex DeviceLinkingCopiedRegex();

    [GeneratedRegex("^(?<player>.+?) has sent the following broadcast: (?<message>.*)$")]
    private static partial Regex DeviceNetworkBroadcastRegex();

    [GeneratedRegex("^(?<entity>.+?) received (?<damage>.+?) powered electrocution damage(?: from (?<source>.+))?$")]
    private static partial Regex ElectrocutionRegex();

    [GeneratedRegex("^Emergency shuttle early launch REPEAL ALL by (?<user>.+)$")]
    private static partial Regex EmergencyShuttleRepealAllRegex();

    [GeneratedRegex("^Emergency shuttle early launch REPEAL by (?<user>.+)$")]
    private static partial Regex EmergencyShuttleRepealRegex();

    [GeneratedRegex("^Emergency shuttle early launch AUTH by (?<user>.+)$")]
    private static partial Regex EmergencyShuttleAuthRegex();

    [GeneratedRegex("^(?<player>.+?) entered the event horizon of (?<entity>.+?) and was deleted$")]
    private static partial Regex EntityDeleteEventHorizonRegex();

    [GeneratedRegex("^(?<player>.+?) closed (?<portals>.+?) with (?<tool>.+)$")]
    private static partial Regex EntityDeleteClosedPortalsRegex();

    [GeneratedRegex("^(?<user>.+?) printed out LogProbe logs \\((?<paper>.+?)\\) of (?<entity>.+)$")]
    private static partial Regex EntitySpawnLogProbeRegex();

    [GeneratedRegex("^(?<user>.+?) used (?<spawner>.+?) which spawned (?<spawned>.+)$")]
    private static partial Regex EntitySpawnFromUseRegex();

    [GeneratedRegex("^(?<player>.+?) opened (?<portal>.+?) at (?<coordinates>.+?) using (?<tool>.+)$")]
    private static partial Regex EntitySpawnOpenedPortalRegex();

    [GeneratedRegex("^(?<player>.+?) opened (?<portal>.+?) at (?<coordinates>.+?) linked to (?<linked>.+?) using (?<tool>.+)$")]
    private static partial Regex EntitySpawnOpenedLinkedPortalRegex();

    [GeneratedRegex("^Event added / announced: (?<entity>.+)$")]
    private static partial Regex EventAddedOrAnnouncedRegex();

    [GeneratedRegex("^Event started: (?<entity>.+)$")]
    private static partial Regex EventStartedRegex();

    [GeneratedRegex("^Event ended: (?<entity>.+)$")]
    private static partial Regex EventEndedRegex();

    [GeneratedRegex("^Codewords generated for faction (?<faction>.+?): (?<codewords>.*)$")]
    private static partial Regex EventCodewordsRegex();

    [GeneratedRegex("^Added game rule (?<entity>.+)$")]
    private static partial Regex EventAddedGameRuleRegex();

    [GeneratedRegex("^Queued start for game rule (?<entity>.+?) with delay (?<delay>.+)$")]
    private static partial Regex EventQueuedGameRuleRegex();

    [GeneratedRegex("^Started game rule (?<entity>.+)$")]
    private static partial Regex EventStartedGameRuleRegex();

    [GeneratedRegex("^(?<player>.+?) tried to add game rule \\[(?<rule>.+?)\\] via command$")]
    private static partial Regex EventAddGameRuleViaCommandRegex();

    [GeneratedRegex("^Unknown tried to add game rule \\[(?<rule>.+?)\\] via command$")]
    private static partial Regex EventUnknownAddGameRuleRegex();

    [GeneratedRegex("^(?<player>.+?) tried to end game rule \\[(?<rule>.+?)\\] via command$")]
    private static partial Regex EventEndGameRuleViaCommandRegex();

    [GeneratedRegex("^Unknown tried to end game rule \\[(?<rule>.+?)\\] via command$")]
    private static partial Regex EventUnknownEndGameRuleRegex();

    [GeneratedRegex("^(?<user>.+?) added a game rule \\[(?<rule>.+?)\\] via a trigger on (?<entity>.+?)\\.$")]
    private static partial Regex EventTriggerAddedGameRuleRegex();

    [GeneratedRegex("^(?<user>.+?) started game rule \\[(?<rule>.+?)\\]\\.$")]
    private static partial Regex EventTriggerStartedGameRuleRegex();

    [GeneratedRegex("^Selected (?<preset>.+?) as the secret preset\\.$")]
    private static partial Regex EventSecretPresetRegex();

    [GeneratedRegex("^(?<entity>.+?) ran rule (?<rule>.+?) with cost (?<cost>.+?) on budget (?<budget>.+?)\\.$")]
    private static partial Regex EventRanWithCostRegex();

    [GeneratedRegex("^(?<entity>.+?) ran rule (?<rule>.+?) which had no cost\\.$")]
    private static partial Regex EventRanWithoutCostRegex();

    [GeneratedRegex("^Explosive depressurization removed (?<moles>.+?) moles from (?<tiles>.+?) tiles starting from position (?<position>.+?) on grid ID (?<grid>.+)$")]
    private static partial Regex ExplosiveDepressurizationRegex();

    [GeneratedRegex("^(?<player>.+?) lost field connections$")]
    private static partial Regex FieldGenerationLostConnectionsRegex();

    [GeneratedRegex("^(?<player>.+?) toggled (?<emitter>.+?) to (?<state>.+)$")]
    private static partial Regex FieldGenerationToggledEmitterRegex();

    [GeneratedRegex("^(?<target>.+?) ingested smoke (?<solution>.*)$")]
    private static partial Regex ForceFeedSmokeRegex();

    [GeneratedRegex("^(?<player>.+?) took the (?<role>.+?) ghost role (?<entity>.+)$")]
    private static partial Regex GhostRoleTakenRegex();

    [GeneratedRegex("^(?<ghost>.+?) ghost warped to (?<target>.+)$")]
    private static partial Regex GhostWarpRegex();

    [GeneratedRegex("^(?<actor>.+?) changed criminal status for (?<name>.+?) to \\\"(?<status>.*)\\\"$")]
    private static partial Regex IdentityCriminalStatusRegex();

    [GeneratedRegex("^(?<user>.+?) set the main breaker state of (?<entity>.+?) to (?<state>.+?)\\.$")]
    private static partial Regex ItemConfigureMainBreakerRegex();

    [GeneratedRegex("^ (?<entity>.+?)'s battlecry has been changed to (?<battlecry>.*)$")]
    private static partial Regex ItemConfigureBattlecryRegex();

    [GeneratedRegex("^(?<user>.+?) set (?<entity>.+?) to (?<state>.+)$")]
    private static partial Regex ItemConfigureTurretStateRegex();

    [GeneratedRegex("^(?<user>.+?) set (?<entity>.+?) authorization of (?<exemption>.+?) to (?<enabled>.+)$")]
    private static partial Regex ItemConfigureTurretAuthorizationRegex();

    [GeneratedRegex("^Player (?<player>.+?) late joined as (?<character>.+?) on station (?<station>.+?) with (?<entity>.+?) as a (?<job>.+?)\\.$")]
    private static partial Regex LateJoinRegex();

    [GeneratedRegex("^(?<player>.+?) late joined the round as an Observer with (?<entity>.+?)\\.$")]
    private static partial Regex LateJoinObserverRegex();

    [GeneratedRegex("^(?<actor>.+?) added a note to PDA: '(?<note>.*)' contained on: (?<pda>.+)$")]
    private static partial Regex PdaAddedNoteRegex();

    [GeneratedRegex("^(?<actor>.+?) removed a note from PDA: '(?<note>.*)' was contained on: (?<pda>.+)$")]
    private static partial Regex PdaRemovedNoteRegex();

    [GeneratedRegex("^Player (?<player>.+?) was respawned\\.$")]
    private static partial Regex RespawnedPlayerRegex();

    [GeneratedRegex("^(?<oldEntity>.+?) was deleted and was respawned at (?<coordinates>.+?) as (?<newEntity>.+)$")]
    private static partial Regex SpecialRespawnRegex();

    [GeneratedRegex("^Player (?<player>.+?) joined as (?<character>.+?) on station (?<station>.+?) with (?<entity>.+?) as a (?<job>.+?)\\.$")]
    private static partial Regex RoundStartJoinRegex();

    [GeneratedRegex("^Shuttle called by (?<player>.+?)(?<suffix>.*)$")]
    private static partial Regex ShuttleCalledByRegex();

    [GeneratedRegex("^Shuttle called(?<suffix>.*)$")]
    private static partial Regex ShuttleCalledRegex();

    [GeneratedRegex("^Shuttle impact of (?<our>.+?) with (?<other>.+?) at (?<point>.+)$")]
    private static partial Regex ShuttleImpactRegex();

    [GeneratedRegex("^Shuttle recalled by (?<player>.+?)(?<suffix>.*)$")]
    private static partial Regex ShuttleRecalledByRegex();

    [GeneratedRegex("^Shuttle recalled(?<suffix>.*)$")]
    private static partial Regex ShuttleRecalledRegex();

    [GeneratedRegex("^(?<player>.+?) purchased listing \\\"(?<listing>.+?)\\\" from (?<store>.+?)(?<extra>, but was not from an expected faction(?: while also possessing a mindshield)?)?\\.$")]
    private static partial Regex StorePurchaseRegex();

    [GeneratedRegex("^(?<player>.+?) has refunded their purchases from (?<store>.+)$")]
    private static partial Regex StoreRefundRegex();

    [GeneratedRegex("^(?<player>.+?) started taking high temperature damage$")]
    private static partial Regex TemperatureHighRegex();

    [GeneratedRegex("^(?<player>.+?) started taking low temperature damage$")]
    private static partial Regex TemperatureLowRegex();

    [GeneratedRegex("^(?<player>.+?) stopped taking temperature damage$")]
    private static partial Regex TemperatureStoppedRegex();

    [GeneratedRegex("^(?<target>.+?) received (?<damage>.+?) damage from collision$")]
    private static partial Regex ThrowHitRegex();

    [GeneratedRegex("^(?<player>.+?) (?<verb>.+?) (?<color>.+?) (?<name>.+?) wire \\((?<action>.+?)\\) in (?<owner>.+)$")]
    private static partial Regex WireHackingRegex();
}
