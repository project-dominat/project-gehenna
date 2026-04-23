using System.Text.RegularExpressions;

namespace Content.Server.Administration.Logs;

public sealed partial class AdminLogManager
{
    private static string LocalizeAtmosDeviceSettingMessage(string rawMessage)
    {
        var changedMode = AtmosDeviceChangedModeRegex().Match(rawMessage);
        if (changedMode.Success)
        {
            return $"{changedMode.Groups["actor"].Value} изменил режим {changedMode.Groups["device"].Value} на {changedMode.Groups["mode"].Value}";
        }

        var changedAutoMode = AtmosDeviceChangedAutoModeRegex().Match(rawMessage);
        if (changedAutoMode.Success)
        {
            return $"{changedAutoMode.Groups["actor"].Value} изменил автоматический режим {changedAutoMode.Groups["device"].Value} на {LocalizeBooleanWord(changedAutoMode.Groups["enabled"].Value)}";
        }

        var changedGasThreshold = AtmosDeviceChangedGasThresholdRegex().Match(rawMessage);
        if (changedGasThreshold.Success)
        {
            return $"{changedGasThreshold.Groups["actor"].Value} изменил порог {changedGasThreshold.Groups["type"].Value} для газа {changedGasThreshold.Groups["gas"].Value} на устройстве {changedGasThreshold.Groups["address"].Value} через {changedGasThreshold.Groups["device"].Value}";
        }

        var changedThreshold = AtmosDeviceChangedThresholdRegex().Match(rawMessage);
        if (changedThreshold.Success)
        {
            return $"{changedThreshold.Groups["actor"].Value} изменил порог {changedThreshold.Groups["type"].Value} на устройстве {changedThreshold.Groups["address"].Value} через {changedThreshold.Groups["device"].Value}";
        }

        var changedSettings = AtmosDeviceChangedSettingsRegex().Match(rawMessage);
        if (changedSettings.Success)
        {
            return $"{changedSettings.Groups["actor"].Value} изменил настройки {changedSettings.Groups["address"].Value} через {changedSettings.Groups["device"].Value}";
        }

        var copiedToVent = AtmosDeviceCopiedToVentRegex().Match(rawMessage);
        if (copiedToVent.Success)
        {
            return $"{copiedToVent.Groups["actor"].Value} скопировал настройки в вент {copiedToVent.Groups["address"].Value}";
        }

        var copiedToScrubber = AtmosDeviceCopiedToScrubberRegex().Match(rawMessage);
        if (copiedToScrubber.Success)
        {
            return $"{copiedToScrubber.Groups["actor"].Value} скопировал настройки в скруббер {copiedToScrubber.Groups["address"].Value}";
        }

        var attemptedAccess = AtmosDeviceAttemptedAccessRegex().Match(rawMessage);
        if (attemptedAccess.Success)
        {
            return $"{attemptedAccess.Groups["user"].Value} попытался получить доступ к {attemptedAccess.Groups["device"].Value} без доступа";
        }

        var toggled = AtmosDeviceToggledRegex().Match(rawMessage);
        if (toggled.Success)
        {
            return $"{toggled.Groups["device"].Value}: {LocalizeEnabledDisabledWord(toggled.Groups["state"].Value)}";
        }

        var directionChanged = AtmosDeviceDirectionChangedRegex().Match(rawMessage);
        if (directionChanged.Success)
        {
            return $"{directionChanged.Groups["device"].Value}: направление изменено на {directionChanged.Groups["direction"].Value}";
        }

        var pressureCheckChanged = AtmosDevicePressureCheckChangedRegex().Match(rawMessage);
        if (pressureCheckChanged.Success)
        {
            return $"{pressureCheckChanged.Groups["device"].Value}: проверка давления изменена на {pressureCheckChanged.Groups["checks"].Value}";
        }

        var externalPressure = AtmosDeviceExternalPressureRegex().Match(rawMessage);
        if (externalPressure.Success)
        {
            return $"{externalPressure.Groups["device"].Value}: внешний предел давления изменён с {externalPressure.Groups["old"].Value} kPa на {externalPressure.Groups["new"].Value} kPa";
        }

        var internalPressure = AtmosDeviceInternalPressureRegex().Match(rawMessage);
        if (internalPressure.Success)
        {
            return $"{internalPressure.Groups["device"].Value}: внутренний предел давления изменён с {internalPressure.Groups["old"].Value} kPa на {internalPressure.Groups["new"].Value} kPa";
        }

        var lockoutOverride = AtmosDeviceLockoutOverrideRegex().Match(rawMessage);
        if (lockoutOverride.Success)
        {
            return $"{lockoutOverride.Groups["device"].Value}: override pressure lockout {LocalizeEnabledDisabledWord(lockoutOverride.Groups["state"].Value)}";
        }

        var gasFilterDisabled = AtmosDeviceGasFilterDisabledRegex().Match(rawMessage);
        if (gasFilterDisabled.Success)
        {
            return $"{gasFilterDisabled.Groups["device"].Value}: фильтрация {gasFilterDisabled.Groups["gas"].Value} отключена";
        }

        var gasFilterEnabled = AtmosDeviceGasFilterEnabledRegex().Match(rawMessage);
        if (gasFilterEnabled.Success)
        {
            return $"{gasFilterEnabled.Groups["device"].Value}: фильтрация {gasFilterEnabled.Groups["gas"].Value} включена";
        }

        var volumeRateChanged = AtmosDeviceVolumeRateChangedRegex().Match(rawMessage);
        if (volumeRateChanged.Success)
        {
            return $"{volumeRateChanged.Groups["device"].Value}: скорость объёма изменена с {volumeRateChanged.Groups["old"].Value} L на {volumeRateChanged.Groups["new"].Value} L";
        }

        var wideNet = AtmosDeviceWideNetRegex().Match(rawMessage);
        if (wideNet.Success)
        {
            return $"{wideNet.Groups["device"].Value}: WideNet {LocalizeEnabledDisabledWord(wideNet.Groups["state"].Value)}";
        }

        return rawMessage;
    }

    private static string LocalizeAtmosPowerChangedMessage(string rawMessage)
    {
        var power = AtmosPowerChangedRegex().Match(rawMessage);
        if (power.Success)
        {
            return $"{power.Groups["player"].Value} установил питание на {power.Groups["device"].Value} в состояние {LocalizeBooleanWord(power.Groups["enabled"].Value)}";
        }

        return rawMessage;
    }

    private static string LocalizeAtmosPressureChangedMessage(string rawMessage)
    {
        var pressure = AtmosPressureChangedRegex().Match(rawMessage);
        if (pressure.Success)
        {
            return $"{pressure.Groups["player"].Value} установил давление на {pressure.Groups["device"].Value} на {pressure.Groups["pressure"].Value} kPa";
        }

        return rawMessage;
    }

    private static string LocalizeAtmosRatioChangedMessage(string rawMessage)
    {
        var ratio = AtmosRatioChangedRegex().Match(rawMessage);
        if (ratio.Success)
        {
            return $"{ratio.Groups["player"].Value} установил соотношение на {ratio.Groups["device"].Value} на {ratio.Groups["ratio"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeAtmosVolumeChangedMessage(string rawMessage)
    {
        var volume = AtmosVolumeChangedRegex().Match(rawMessage);
        if (volume.Success)
        {
            return $"{volume.Groups["player"].Value} установил скорость переноса на {volume.Groups["device"].Value} на {volume.Groups["rate"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeAtmosFilterChangedMessage(string rawMessage)
    {
        var filter = AtmosFilterChangedRegex().Match(rawMessage);
        if (filter.Success)
        {
            return $"{filter.Groups["player"].Value} установил фильтр на {filter.Groups["device"].Value} на {filter.Groups["gas"].Value}";
        }

        return rawMessage;
    }

    private static string LocalizeEnabledDisabledWord(string rawValue)
    {
        return rawValue switch
        {
            "enabled" => "включено",
            "disabled" => "отключено",
            _ => rawValue
        };
    }

    [GeneratedRegex("^(?<actor>.+?) changed (?<device>.+?) mode to (?<mode>.+)$")]
    private static partial Regex AtmosDeviceChangedModeRegex();

    [GeneratedRegex("^(?<actor>.+?) changed (?<device>.+?) auto mode to (?<enabled>.+)$")]
    private static partial Regex AtmosDeviceChangedAutoModeRegex();

    [GeneratedRegex("^(?<actor>.+?) changed (?<address>.+?) (?<gas>.+?) (?<type>.+?) threshold using (?<device>.+)$")]
    private static partial Regex AtmosDeviceChangedGasThresholdRegex();

    [GeneratedRegex("^(?<actor>.+?) changed (?<address>.+?) (?<type>.+?) threshold using (?<device>.+)$")]
    private static partial Regex AtmosDeviceChangedThresholdRegex();

    [GeneratedRegex("^(?<actor>.+?) changed (?<address>.+?) settings using (?<device>.+)$")]
    private static partial Regex AtmosDeviceChangedSettingsRegex();

    [GeneratedRegex("^(?<actor>.+?) copied settings to vent (?<address>.+)$")]
    private static partial Regex AtmosDeviceCopiedToVentRegex();

    [GeneratedRegex("^(?<actor>.+?) copied settings to scrubber (?<address>.+)$")]
    private static partial Regex AtmosDeviceCopiedToScrubberRegex();

    [GeneratedRegex("^(?<user>.+?) attempted to access (?<device>.+?) without access$")]
    private static partial Regex AtmosDeviceAttemptedAccessRegex();

    [GeneratedRegex("^(?<device>.+?) (?<state>enabled|disabled)$")]
    private static partial Regex AtmosDeviceToggledRegex();

    [GeneratedRegex("^(?<device>.+?) direction changed to (?<direction>.+)$")]
    private static partial Regex AtmosDeviceDirectionChangedRegex();

    [GeneratedRegex("^(?<device>.+?) pressure check changed to (?<checks>.+)$")]
    private static partial Regex AtmosDevicePressureCheckChangedRegex();

    [GeneratedRegex("^(?<device>.+?) external pressure bound changed from (?<old>.+?) kPa to (?<new>.+?) kPa$")]
    private static partial Regex AtmosDeviceExternalPressureRegex();

    [GeneratedRegex("^(?<device>.+?) internal pressure bound changed from (?<old>.+?) kPa to (?<new>.+?) kPa$")]
    private static partial Regex AtmosDeviceInternalPressureRegex();

    [GeneratedRegex("^(?<device>.+?) pressure lockout override (?<state>enabled|disabled)$")]
    private static partial Regex AtmosDeviceLockoutOverrideRegex();

    [GeneratedRegex("^(?<device>.+?) (?<gas>.+?) filtering disabled$")]
    private static partial Regex AtmosDeviceGasFilterDisabledRegex();

    [GeneratedRegex("^(?<device>.+?) (?<gas>.+?) filtering enabled$")]
    private static partial Regex AtmosDeviceGasFilterEnabledRegex();

    [GeneratedRegex("^(?<device>.+?) volume rate changed from (?<old>.+?) L to (?<new>.+?) L$")]
    private static partial Regex AtmosDeviceVolumeRateChangedRegex();

    [GeneratedRegex("^(?<device>.+?) WideNet (?<state>enabled|disabled)$")]
    private static partial Regex AtmosDeviceWideNetRegex();

    [GeneratedRegex("^(?<player>.+?) set the power on (?<device>.+?) to (?<enabled>.+)$")]
    private static partial Regex AtmosPowerChangedRegex();

    [GeneratedRegex("^(?<player>.+?) set the pressure on (?<device>.+?) to (?<pressure>.+?)kPa$")]
    private static partial Regex AtmosPressureChangedRegex();

    [GeneratedRegex("^(?<player>.+?) set the ratio on (?<device>.+?) to (?<ratio>.+)$")]
    private static partial Regex AtmosRatioChangedRegex();

    [GeneratedRegex("^(?<player>.+?) set the transfer rate on (?<device>.+?) to (?<rate>.+)$")]
    private static partial Regex AtmosVolumeChangedRegex();

    [GeneratedRegex("^(?<player>.+?) set the filter on (?<device>.+?) to (?<gas>.+)$")]
    private static partial Regex AtmosFilterChangedRegex();
}
