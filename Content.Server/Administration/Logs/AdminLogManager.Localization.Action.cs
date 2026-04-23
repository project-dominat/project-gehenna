using System.Text.RegularExpressions;

namespace Content.Server.Administration.Logs;

public sealed partial class AdminLogManager
{
    private static string LocalizeActionMessage(string rawMessage)
    {
        if (TryTranslateSingleSubjectMessage(ActionUnfrozeRegex(), rawMessage, "разморозил", out var unfroze))
            return unfroze;

        if (TryTranslateSingleSubjectMessage(ActionFrozeAndMutedRegex(), rawMessage, "заморозил и заглушил", out var frozeAndMuted))
            return frozeAndMuted;

        if (TryTranslateSingleSubjectMessage(ActionFrozeRegex(), rawMessage, "заморозил", out var froze))
            return froze;

        var openedLogs = ActionOpenedLogsRegex().Match(rawMessage);
        if (openedLogs.Success)
        {
            return $"{openedLogs.Groups["player"].Value} открыл логи на {openedLogs.Groups["subject"].Value}";
        }

        if (TryTranslateSingleSubjectMessage(ActionRejuvenatedRegex(), rawMessage, "омолодил", out var rejuvenated))
            return rejuvenated;

        if (TryTranslateSingleSubjectMessage(ActionDeletedRegex(), rawMessage, "удалил", out var deleted))
            return deleted;

        var renamedFax = ActionRenamedFaxRegex().Match(rawMessage);
        if (renamedFax.Success)
        {
            return $"{renamedFax.Groups["user"].Value} переименовал {renamedFax.Groups["tool"].Value} с \"{renamedFax.Groups["oldName"].Value}\" на \"{renamedFax.Groups["newName"].Value}\"";
        }

        var faxPrintJob = ActionFaxPrintJobRegex().Match(rawMessage);
        if (faxPrintJob.Success)
        {
            return $"{faxPrintJob.Groups["actor"].Value} добавил задание печати в \"{faxPrintJob.Groups["fax"].Value}\" {faxPrintJob.Groups["tool"].Value} для {faxPrintJob.Groups["subject"].Value}: {faxPrintJob.Groups["content"].Value}";
        }

        var faxCopyJob = ActionFaxCopyJobRegex().Match(rawMessage);
        if (faxCopyJob.Success)
        {
            return $"{faxCopyJob.Groups["actor"].Value} добавил задание копирования в \"{faxCopyJob.Groups["fax"].Value}\" {faxCopyJob.Groups["tool"].Value} для {faxCopyJob.Groups["subject"].Value}: {faxCopyJob.Groups["content"].Value}";
        }

        var faxSend = ActionFaxSendRegex().Match(rawMessage);
        if (faxSend.Success)
        {
            return $"{faxSend.Groups["actor"].Value} отправил факс из \"{faxSend.Groups["sourceFax"].Value}\" {faxSend.Groups["tool"].Value} в \"{faxSend.Groups["destFax"].Value}\" ({faxSend.Groups["address"].Value}) для {faxSend.Groups["subject"].Value}: {faxSend.Groups["content"].Value}";
        }

        var faxPrinted = ActionFaxPrintedRegex().Match(rawMessage);
        if (faxPrinted.Success)
        {
            return $"\"{faxPrinted.Groups["fax"].Value}\" {faxPrinted.Groups["tool"].Value} распечатал {faxPrinted.Groups["subject"].Value}: {faxPrinted.Groups["content"].Value}";
        }

        var insertedOrderSlip = ActionInsertedOrderSlipRegex().Match(rawMessage);
        if (insertedOrderSlip.Success)
        {
            return $"{insertedOrderSlip.Groups["user"].Value} вставил бланк заказа [{insertedOrderSlip.Groups["order"].Value}]";
        }

        var approvedOrder = ActionApprovedOrderRegex().Match(rawMessage);
        if (approvedOrder.Success)
        {
            return $"{approvedOrder.Groups["user"].Value} одобрил заказ [{approvedOrder.Groups["order"].Value}] на аккаунте {approvedOrder.Groups["account"].Value} с балансом {approvedOrder.Groups["balance"].Value}";
        }

        var addedOrder = ActionAddedOrderRegex().Match(rawMessage);
        if (addedOrder.Success)
        {
            return $"{addedOrder.Groups["user"].Value} добавил заказ [{addedOrder.Groups["order"].Value}]";
        }

        var addAndApproveOrder = ActionAddAndApproveOrderRegex().Match(rawMessage);
        if (addAndApproveOrder.Success)
        {
            return $"AddAndApproveOrder {addAndApproveOrder.Groups["description"].Value} добавил заказ [{addAndApproveOrder.Groups["order"].Value}]";
        }

        var removedCryoItem = ActionRemovedCryoItemRegex().Match(rawMessage);
        if (removedCryoItem.Success)
        {
            return $"{removedCryoItem.Groups["player"].Value} извлёк предмет {removedCryoItem.Groups["item"].Value} из игрока {removedCryoItem.Groups["contained"].Value}, находящегося в криохранилище {removedCryoItem.Groups["cryo"].Value}";
        }

        var enteredCryo = ActionEnteredCryoRegex().Match(rawMessage);
        if (enteredCryo.Success)
        {
            return $"{enteredCryo.Groups["player"].Value} был помещён в криохранилище внутри {enteredCryo.Groups["cryo"].Value}";
        }

        var reenteredCryo = ActionReenteredCryoRegex().Match(rawMessage);
        if (reenteredCryo.Success)
        {
            return $"{reenteredCryo.Groups["player"].Value} вернулся в игру из криохранилища {reenteredCryo.Groups["cryo"].Value}";
        }

        var queuedLathe = ActionQueuedLatheRegex().Match(rawMessage);
        if (queuedLathe.Success)
        {
            return $"{queuedLathe.Groups["player"].Value} поставил в очередь {queuedLathe.Groups["quantity"].Value} {queuedLathe.Groups["recipe"].Value} на {queuedLathe.Groups["lathe"].Value}";
        }

        var deletedLatheJob = ActionDeletedLatheJobRegex().Match(rawMessage);
        if (deletedLatheJob.Success)
        {
            return $"{deletedLatheJob.Groups["player"].Value} удалил задание станка для ({deletedLatheJob.Groups["printed"].Value}/{deletedLatheJob.Groups["requested"].Value}) {deletedLatheJob.Groups["recipe"].Value} на {deletedLatheJob.Groups["lathe"].Value}";
        }

        var abortedLathe = ActionAbortedLatheRegex().Match(rawMessage);
        if (abortedLathe.Success)
        {
            return $"{abortedLathe.Groups["player"].Value} прервал печать {abortedLathe.Groups["recipe"].Value} на {abortedLathe.Groups["lathe"].Value}";
        }

        var insertedVoiceControl = ActionInsertedVoiceControlRegex().Match(rawMessage);
        if (insertedVoiceControl.Success)
        {
            return $"{insertedVoiceControl.Groups["source"].Value} вставил {insertedVoiceControl.Groups["item"].Value} в {insertedVoiceControl.Groups["storage"].Value} через голосовое управление";
        }

        var failedInsertVoiceControl = ActionFailedInsertVoiceControlRegex().Match(rawMessage);
        if (failedInsertVoiceControl.Success)
        {
            return $"{failedInsertVoiceControl.Groups["source"].Value} не смог вставить {failedInsertVoiceControl.Groups["item"].Value} в {failedInsertVoiceControl.Groups["storage"].Value} через голосовое управление";
        }

        var retrievedVoiceControl = ActionRetrievedVoiceControlRegex().Match(rawMessage);
        if (retrievedVoiceControl.Success)
        {
            return $"{retrievedVoiceControl.Groups["source"].Value} извлёк {retrievedVoiceControl.Groups["item"].Value} из {retrievedVoiceControl.Groups["storage"].Value} через голосовое управление";
        }

        var burntId = ActionBurntIdRegex().Match(rawMessage);
        if (burntId.Success)
        {
            return $"{burntId.Groups["microwave"].Value} сжёг {burntId.Groups["entity"].Value}";
        }

        var clearedAccess = ActionClearedAccessRegex().Match(rawMessage);
        if (clearedAccess.Success)
        {
            return $"{clearedAccess.Groups["microwave"].Value} очистил доступы на {clearedAccess.Groups["entity"].Value}";
        }

        var addedAccess = ActionAddedAccessRegex().Match(rawMessage);
        if (addedAccess.Success)
        {
            return $"{addedAccess.Groups["microwave"].Value} добавил доступ {addedAccess.Groups["access"].Value} к {addedAccess.Groups["entity"].Value}";
        }

        var modifiedAllowedHolders = ActionModifiedAllowedHoldersRegex().Match(rawMessage);
        if (modifiedAllowedHolders.Success)
        {
            return $"{modifiedAllowedHolders.Groups["player"].Value} изменил {modifiedAllowedHolders.Groups["entity"].Value}. Изменения уровней доступа: [{modifiedAllowedHolders.Groups["changes"].Value}] [{modifiedAllowedHolders.Groups["current"].Value}]";
        }

        var modifiedAccesses = ActionModifiedAccessesRegex().Match(rawMessage);
        if (modifiedAccesses.Success)
        {
            return $"{modifiedAccesses.Groups["player"].Value} изменил {modifiedAccesses.Groups["target"].Value}. Изменения доступов: [{modifiedAccesses.Groups["changes"].Value}] [{modifiedAccesses.Groups["current"].Value}]";
        }

        var turnedOnOff = ActionTurnedOnOffRegex().Match(rawMessage);
        if (turnedOnOff.Success)
        {
            return $"{turnedOnOff.Groups["player"].Value} {(turnedOnOff.Groups["state"].Value == "on" ? "включил" : "выключил")} {turnedOnOff.Groups["entity"].Value}";
        }

        var setStrength = ActionSetStrengthRegex().Match(rawMessage);
        if (setStrength.Success)
        {
            return $"{setStrength.Groups["player"].Value} установил мощность {setStrength.Groups["entity"].Value} на {setStrength.Groups["strength"].Value}";
        }

        var bountyFulfilled = ActionBountyFulfilledRegex().Match(rawMessage);
        if (bountyFulfilled.Success)
        {
            return $"Баунти \"{bountyFulfilled.Groups["bounty"].Value}\" (id:{bountyFulfilled.Groups["id"].Value}) выполнено";
        }

        var bountyAdded = ActionBountyAddedRegex().Match(rawMessage);
        if (bountyAdded.Success)
        {
            return $"Добавлено баунти \"{bountyAdded.Groups["bounty"].Value}\" (id:{bountyAdded.Groups["id"].Value}) на станцию {bountyAdded.Groups["station"].Value}";
        }

        var ameMode = ActionAmeModeRegex().Match(rawMessage);
        if (ameMode.Success)
        {
            return $"{ameMode.Groups["player"].Value} установил режим AME на {LocalizeAmeState(ameMode.Groups["state"].Value)}";
        }

        var ameInject = ActionAmeInjectRegex().Match(rawMessage);
        if (ameInject.Success)
        {
            return $"{ameInject.Groups["player"].Value} установил инъекцию AME на {ameInject.Groups["amount"].Value} при режиме {LocalizeAmeState(ameInject.Groups["state"].Value)}";
        }

        var printedPill = ActionPrintedPillRegex().Match(rawMessage);
        if (printedPill.Success)
        {
            return $"{printedPill.Groups["user"].Value} напечатал {printedPill.Groups["pill"].Value} {printedPill.Groups["solution"].Value}";
        }

        var bottled = ActionBottledRegex().Match(rawMessage);
        if (bottled.Success)
        {
            return $"{bottled.Groups["user"].Value} разлил в {bottled.Groups["bottle"].Value} {bottled.Groups["solution"].Value}";
        }

        var cursedMaskTakeover = ActionCursedMaskTakeoverRegex().Match(rawMessage);
        if (cursedMaskTakeover.Success)
        {
            return $"{cursedMaskTakeover.Groups["player"].Value} потерял контроль над телом и стал врагом из-за проклятой маски {cursedMaskTakeover.Groups["mask"].Value}";
        }

        var cursedMaskRestore = ActionCursedMaskRestoreRegex().Match(rawMessage);
        if (cursedMaskRestore.Success)
        {
            return $"{cursedMaskRestore.Groups["player"].Value} вернулся в своё тело после снятия {cursedMaskRestore.Groups["mask"].Value}.";
        }

        var calledShuttle = ActionCalledShuttleRegex().Match(rawMessage);
        if (calledShuttle.Success)
        {
            return $"{calledShuttle.Groups["player"].Value} вызвал шаттл.";
        }

        var recalledShuttle = ActionRecalledShuttleRegex().Match(rawMessage);
        if (recalledShuttle.Success)
        {
            return $"{recalledShuttle.Groups["player"].Value} отозвал шаттл.";
        }

        var triggeredSignaller = ActionTriggeredSignallerRegex().Match(rawMessage);
        if (triggeredSignaller.Success)
        {
            return $"{triggeredSignaller.Groups["actor"].Value} активировал сигнальщик {triggeredSignaller.Groups["tool"].Value}";
        }

        var successfulClone = ActionSuccessfulCloneRegex().Match(rawMessage);
        if (successfulClone.Success)
        {
            return $"{successfulClone.Groups["console"].Value} успешно клонировал {successfulClone.Groups["body"].Value}.";
        }

        var triedListRules = ActionTriedListRulesRegex().Match(rawMessage);
        if (triedListRules.Success)
        {
            return $"{triedListRules.Groups["player"].Value} попытался получить список геймправил через команду";
        }

        var forcedSpeech = ActionForcedSpeechRegex().Match(rawMessage);
        if (forcedSpeech.Success)
        {
            return $"{forcedSpeech.Groups["admin"].Value} принудил {forcedSpeech.Groups["entity"].Value} к {forcedSpeech.Groups["kind"].Value}: {forcedSpeech.Groups["message"].Value}";
        }

        var biomassReclaimer = ActionBiomassReclaimerRegex().Match(rawMessage);
        if (biomassReclaimer.Success)
        {
            return $"{biomassReclaimer.Groups["player"].Value} использовал переработчик биомассы, чтобы расчленить {biomassReclaimer.Groups["target"].Value} в {biomassReclaimer.Groups["reclaimer"].Value}";
        }

        var pointedAt = ActionPointedAtRegex().Match(rawMessage);
        if (pointedAt.Success)
        {
            return $"{pointedAt.Groups["player"].Value} указал на {pointedAt.Groups["target"].Value} {pointedAt.Groups["position"].Value}";
        }

        var setTargetState = ActionSetTargetStateRegex().Match(rawMessage);
        if (setTargetState.Success)
        {
            return $"{setTargetState.Groups["player"].Value} установил {setTargetState.Groups["target"].Value} в состояние {LocalizeOnOffState(setTargetState.Groups["state"].Value)}";
        }

        var unlockedTechnology = ActionUnlockedTechnologyRegex().Match(rawMessage);
        if (unlockedTechnology.Success)
        {
            return $"{unlockedTechnology.Groups["player"].Value} разблокировал {unlockedTechnology.Groups["technology"].Value} (дисциплина: {unlockedTechnology.Groups["discipline"].Value}, уровень: {unlockedTechnology.Groups["tier"].Value}) на {unlockedTechnology.Groups["client"].Value} для сервера {unlockedTechnology.Groups["server"].Value}.";
        }

        var disabledBorg = ActionDisabledBorgRegex().Match(rawMessage);
        if (disabledBorg.Success)
        {
            return $"{disabledBorg.Groups["user"].Value} отключил борга {disabledBorg.Groups["name"].Value} с адресом {disabledBorg.Groups["address"].Value}";
        }

        var destroyedBorg = ActionDestroyedBorgRegex().Match(rawMessage);
        if (destroyedBorg.Success)
        {
            return $"{destroyedBorg.Groups["user"].Value} уничтожил борга {destroyedBorg.Groups["name"].Value} с адресом {destroyedBorg.Groups["address"].Value}";
        }

        var setVoiceMask = ActionSetVoiceMaskRegex().Match(rawMessage);
        if (setVoiceMask.Success)
        {
            return $"{setVoiceMask.Groups["player"].Value} установил голос {setVoiceMask.Groups["mask"].Value}: {setVoiceMask.Groups["name"].Value}";
        }

        var insertedMaterial = ActionInsertedMaterialRegex().Match(rawMessage);
        if (insertedMaterial.Success)
        {
            return $"{insertedMaterial.Groups["player"].Value} вставил {insertedMaterial.Groups["count"].Value} {insertedMaterial.Groups["inserted"].Value} в {insertedMaterial.Groups["receiver"].Value}";
        }

        var unsafeCookingExplosion = ActionUnsafeCookingExplosionRegex().Match(rawMessage);
        if (unsafeCookingExplosion.Success)
        {
            return $"{unsafeCookingExplosion.Groups["entity"].Value} взорвался из-за небезопасной готовки!";
        }

        var meteorStrike = ActionMeteorStrikeRegex().Match(rawMessage);
        if (meteorStrike.Success)
        {
            return $"{meteorStrike.Groups["player"].Value} был поражён метеором {meteorStrike.Groups["meteor"].Value} и мгновенно погиб.";
        }

        var lubed = ActionLubedRegex().Match(rawMessage);
        if (lubed.Success)
        {
            return $"{lubed.Groups["actor"].Value} смазал {lubed.Groups["subject"].Value} с помощью {lubed.Groups["tool"].Value}";
        }

        var navBeacon = ActionConfiguredNavBeaconRegex().Match(rawMessage);
        if (navBeacon.Success)
        {
            return $"{navBeacon.Groups["player"].Value} настроил NavMapBeacon '{navBeacon.Groups["entity"].Value}' с текстом '{navBeacon.Groups["text"].Value}', цветом {navBeacon.Groups["color"].Value} и состоянием {LocalizeEnabledDisabledWord(navBeacon.Groups["state"].Value)}.";
        }

        var inputBreaker = ActionInputBreakerRegex().Match(rawMessage);
        if (inputBreaker.Success)
        {
            return $"{inputBreaker.Groups["actor"].Value} установил входной автомат {inputBreaker.Groups["target"].Value} в состояние {LocalizeBooleanWord(inputBreaker.Groups["state"].Value)}";
        }

        var outputBreaker = ActionOutputBreakerRegex().Match(rawMessage);
        if (outputBreaker.Success)
        {
            return $"{outputBreaker.Groups["actor"].Value} установил выходной автомат {outputBreaker.Groups["target"].Value} в состояние {LocalizeBooleanWord(outputBreaker.Groups["state"].Value)}";
        }

        var successfulBreeding = ActionSuccessfulBreedingRegex().Match(rawMessage);
        if (successfulBreeding.Success)
        {
            return $"{successfulBreeding.Groups["carrier"].Value} и {successfulBreeding.Groups["partner"].Value} успешно размножились.";
        }

        var gaveBirth = ActionGaveBirthRegex().Match(rawMessage);
        if (gaveBirth.Success)
        {
            return $"{gaveBirth.Groups["parent"].Value} родил {gaveBirth.Groups["offspring"].Value}.";
        }

        return rawMessage;
    }

    private static string LocalizeAmeState(string rawState)
    {
        return rawState switch
        {
            "Inject" => "инъекция",
            "Not inject" => "без инъекции",
            _ => rawState
        };
    }

    [GeneratedRegex("^(?<player>.+?) unfroze (?<subject>.+)$")]
    private static partial Regex ActionUnfrozeRegex();

    [GeneratedRegex("^(?<player>.+?) froze and muted (?<subject>.+)$")]
    private static partial Regex ActionFrozeAndMutedRegex();

    [GeneratedRegex("^(?<player>.+?) froze (?<subject>.+)$")]
    private static partial Regex ActionFrozeRegex();

    [GeneratedRegex("^(?<player>.+?) opened logs on (?<subject>.+)$")]
    private static partial Regex ActionOpenedLogsRegex();

    [GeneratedRegex("^(?<player>.+?) rejuvenated (?<subject>.+)$")]
    private static partial Regex ActionRejuvenatedRegex();

    [GeneratedRegex("^(?<player>.+?) deleted (?<subject>.+)$")]
    private static partial Regex ActionDeletedRegex();

    [GeneratedRegex("^(?<user>.+?) renamed (?<tool>.+?) from \\\"(?<oldName>.*)\\\" to \\\"(?<newName>.*)\\\"$")]
    private static partial Regex ActionRenamedFaxRegex();

    [GeneratedRegex("^(?<actor>.+?) added print job to \\\"(?<fax>.+?)\\\" (?<tool>.+?) of (?<subject>.+?): (?<content>.*)$")]
    private static partial Regex ActionFaxPrintJobRegex();

    [GeneratedRegex("^(?<actor>.+?) added copy job to \\\"(?<fax>.+?)\\\" (?<tool>.+?) of (?<subject>.+?): (?<content>.*)$")]
    private static partial Regex ActionFaxCopyJobRegex();

    [GeneratedRegex("^(?<actor>.+?) sent fax from \\\"(?<sourceFax>.+?)\\\" (?<tool>.+?) to \\\"(?<destFax>.+?)\\\" \\((?<address>.+?)\\) of (?<subject>.+?): (?<content>.*)$")]
    private static partial Regex ActionFaxSendRegex();

    [GeneratedRegex("^\\\"(?<fax>.+?)\\\" (?<tool>.+?) printed (?<subject>.+?): (?<content>.*)$")]
    private static partial Regex ActionFaxPrintedRegex();

    [GeneratedRegex("^(?<user>.+?) inserted order slip \\[(?<order>.*)\\]$")]
    private static partial Regex ActionInsertedOrderSlipRegex();

    [GeneratedRegex("^(?<user>.+?) approved order \\[(?<order>.*)\\] on account (?<account>.+?) with balance at (?<balance>.+)$")]
    private static partial Regex ActionApprovedOrderRegex();

    [GeneratedRegex("^(?<user>.+?) added order \\[(?<order>.*)\\]$")]
    private static partial Regex ActionAddedOrderRegex();

    [GeneratedRegex("^AddAndApproveOrder (?<description>.+?) added order \\[(?<order>.*)\\]$")]
    private static partial Regex ActionAddAndApproveOrderRegex();

    [GeneratedRegex("^(?<player>.+?) removed item (?<item>.+?) from cryostorage-contained player (?<contained>.+?), stored in cryostorage (?<cryo>.+)$")]
    private static partial Regex ActionRemovedCryoItemRegex();

    [GeneratedRegex("^(?<player>.+?) was entered into cryostorage inside of (?<cryo>.+)$")]
    private static partial Regex ActionEnteredCryoRegex();

    [GeneratedRegex("^(?<player>.+?) re-entered the game from cryostorage (?<cryo>.+)$")]
    private static partial Regex ActionReenteredCryoRegex();

    [GeneratedRegex("^(?<player>.+?) queued (?<quantity>.+?) (?<recipe>.+?) at (?<lathe>.+)$")]
    private static partial Regex ActionQueuedLatheRegex();

    [GeneratedRegex("^(?<player>.+?) deleted a lathe job for \\((?<printed>.+?)/(?<requested>.+?)\\) (?<recipe>.+?) at (?<lathe>.+)$")]
    private static partial Regex ActionDeletedLatheJobRegex();

    [GeneratedRegex("^(?<player>.+?) aborted printing (?<recipe>.+?) at (?<lathe>.+)$")]
    private static partial Regex ActionAbortedLatheRegex();

    [GeneratedRegex("^(?<source>.+?) inserted (?<item>.+?) into (?<storage>.+?) via voice control$")]
    private static partial Regex ActionInsertedVoiceControlRegex();

    [GeneratedRegex("^(?<source>.+?) failed to insert (?<item>.+?) into (?<storage>.+?) via voice control$")]
    private static partial Regex ActionFailedInsertVoiceControlRegex();

    [GeneratedRegex("^(?<source>.+?) retrieved (?<item>.+?) from (?<storage>.+?) via voice control$")]
    private static partial Regex ActionRetrievedVoiceControlRegex();

    [GeneratedRegex("^(?<microwave>.+?) burnt (?<entity>.+)$")]
    private static partial Regex ActionBurntIdRegex();

    [GeneratedRegex("^(?<microwave>.+?) cleared access on (?<entity>.+)$")]
    private static partial Regex ActionClearedAccessRegex();

    [GeneratedRegex("^(?<microwave>.+?) added (?<access>.+?) access to (?<entity>.+)$")]
    private static partial Regex ActionAddedAccessRegex();

    [GeneratedRegex("^(?<player>.+?) has modified (?<entity>.+?) with the following allowed access level holders: \\[(?<changes>.*)\\] \\[(?<current>.*)\\]$")]
    private static partial Regex ActionModifiedAllowedHoldersRegex();

    [GeneratedRegex("^(?<player>.+?) has modified (?<target>.+?) with the following accesses: \\[(?<changes>.*)\\] \\[(?<current>.*)\\]$")]
    private static partial Regex ActionModifiedAccessesRegex();

    [GeneratedRegex("^(?<player>.+?) has turned (?<entity>.+?) (?<state>on|off)$")]
    private static partial Regex ActionTurnedOnOffRegex();

    [GeneratedRegex("^(?<player>.+?) has set the strength of (?<entity>.+?) to (?<strength>.+)$")]
    private static partial Regex ActionSetStrengthRegex();

    [GeneratedRegex("^Bounty \\\"(?<bounty>.+?)\\\" \\(id:(?<id>.+?)\\) was fulfilled$")]
    private static partial Regex ActionBountyFulfilledRegex();

    [GeneratedRegex("^Added bounty \\\"(?<bounty>.+?)\\\" \\(id:(?<id>.+?)\\) to station (?<station>.+)$")]
    private static partial Regex ActionBountyAddedRegex();

    [GeneratedRegex("^(?<player>.+?) has set the AME to (?<state>.+)$")]
    private static partial Regex ActionAmeModeRegex();

    [GeneratedRegex("^(?<player>.+?) has set the AME to inject (?<amount>.+?) while set to (?<state>.+)$")]
    private static partial Regex ActionAmeInjectRegex();

    [GeneratedRegex("^(?<user>.+?) printed (?<pill>.+?) (?<solution>.*)$")]
    private static partial Regex ActionPrintedPillRegex();

    [GeneratedRegex("^(?<user>.+?) bottled (?<bottle>.+?) (?<solution>.*)$")]
    private static partial Regex ActionBottledRegex();

    [GeneratedRegex("^(?<player>.+?) had their body taken over and turned into an enemy through the cursed mask (?<mask>.+)$")]
    private static partial Regex ActionCursedMaskTakeoverRegex();

    [GeneratedRegex("^(?<player>.+?) was restored to their body after the removal of (?<mask>.+?)\\.$")]
    private static partial Regex ActionCursedMaskRestoreRegex();

    [GeneratedRegex("^(?<player>.+?) has called the shuttle\\.$")]
    private static partial Regex ActionCalledShuttleRegex();

    [GeneratedRegex("^(?<player>.+?) has recalled the shuttle\\.$")]
    private static partial Regex ActionRecalledShuttleRegex();

    [GeneratedRegex("^(?<actor>.+?) triggered signaler (?<tool>.+)$")]
    private static partial Regex ActionTriggeredSignallerRegex();

    [GeneratedRegex("^(?<console>.+?) successfully cloned (?<body>.+?)\\.$")]
    private static partial Regex ActionSuccessfulCloneRegex();

    [GeneratedRegex("^(?<player>.+?) tried to get list of game rules via command$")]
    private static partial Regex ActionTriedListRulesRegex();

    [GeneratedRegex("^(?<admin>.+?) forced (?<entity>.+?) to (?<kind>.+?): (?<message>.*)$")]
    private static partial Regex ActionForcedSpeechRegex();

    [GeneratedRegex("^(?<player>.+?) used a biomass reclaimer to gib (?<target>.+?) in (?<reclaimer>.+)$")]
    private static partial Regex ActionBiomassReclaimerRegex();

    [GeneratedRegex("^(?<player>.+?) pointed at (?<target>.+?) (?<position>.+)$")]
    private static partial Regex ActionPointedAtRegex();

    [GeneratedRegex("^(?<player>.+?) set (?<target>.+?) to (?<state>on|off)$")]
    private static partial Regex ActionSetTargetStateRegex();

    [GeneratedRegex("^(?<player>.+?) unlocked (?<technology>.+?) \\(discipline: (?<discipline>.+?), tier: (?<tier>.+?)\\) at (?<client>.+?), for server (?<server>.+?)\\.$")]
    private static partial Regex ActionUnlockedTechnologyRegex();

    [GeneratedRegex("^(?<user>.+?) disabled borg (?<name>.+?) with address (?<address>.+)$")]
    private static partial Regex ActionDisabledBorgRegex();

    [GeneratedRegex("^(?<user>.+?) destroyed borg (?<name>.+?) with address (?<address>.+)$")]
    private static partial Regex ActionDestroyedBorgRegex();

    [GeneratedRegex("^(?<player>.+?) set voice of (?<mask>.+?): (?<name>.+)$")]
    private static partial Regex ActionSetVoiceMaskRegex();

    [GeneratedRegex("^(?<player>.+?) inserted (?<count>.+?) (?<inserted>.+?) into (?<receiver>.+)$")]
    private static partial Regex ActionInsertedMaterialRegex();

    [GeneratedRegex("^(?<entity>.+?) exploded from unsafe cooking!$")]
    private static partial Regex ActionUnsafeCookingExplosionRegex();

    [GeneratedRegex("^(?<player>.+?) was struck by meteor (?<meteor>.+?) and killed instantly\\.$")]
    private static partial Regex ActionMeteorStrikeRegex();

    [GeneratedRegex("^(?<actor>.+?) lubed (?<subject>.+?) with (?<tool>.+)$")]
    private static partial Regex ActionLubedRegex();

    [GeneratedRegex("^(?<player>.+?) configured NavMapBeacon \\'(?<entity>.+?)\\' with text \\'(?<text>.*)\\', color (?<color>.+?), and (?<state>enabled|disabled) it\\.$")]
    private static partial Regex ActionConfiguredNavBeaconRegex();

    [GeneratedRegex("^(?<actor>.+?) set input breaker to (?<state>.+?) on (?<target>.+)$")]
    private static partial Regex ActionInputBreakerRegex();

    [GeneratedRegex("^(?<actor>.+?) set output breaker to (?<state>.+?) on (?<target>.+)$")]
    private static partial Regex ActionOutputBreakerRegex();

    [GeneratedRegex("^(?<carrier>.+?) \\(carrier\\) and (?<partner>.+?) \\(partner\\) successfully bred\\.$")]
    private static partial Regex ActionSuccessfulBreedingRegex();

    [GeneratedRegex("^(?<parent>.+?) gave birth to (?<offspring>.+?)\\.$")]
    private static partial Regex ActionGaveBirthRegex();
}
