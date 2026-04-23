using System.Text;
using Content.Shared.Database;

namespace Content.Client.Administration.UI.Logs;

public static class AdminLogText
{
    public static string GetImpactText(LogImpact impact)
    {
        return impact switch
        {
            LogImpact.Low => "Низкое",
            LogImpact.Medium => "Среднее",
            LogImpact.High => "Высокое",
            LogImpact.Extreme => "Критическое",
            _ => Humanize(impact.ToString())
        };
    }

    public static string GetTypeText(LogType type)
    {
        return type switch
        {
            LogType.Unknown => "Неизвестно",
            LogType.Damaged => "Урон",
            LogType.Healed => "Лечение",
            LogType.Slip => "Поскальзывание",
            LogType.EventAnnounced => "Анонс события",
            LogType.EventStarted => "Запуск события",
            LogType.EventRan => "Ход события",
            LogType.EventStopped => "Остановка события",
            LogType.Verb => "Верб",
            LogType.ShuttleCalled => "Вызов шаттла",
            LogType.ShuttleRecalled => "Отзыв шаттла",
            LogType.ExplosiveDepressurization => "Взрывная разгерметизация",
            LogType.Respawn => "Респавн",
            LogType.RoundStartJoin => "Вход в начале раунда",
            LogType.LateJoin => "Поздний вход",
            LogType.ChemicalReaction => "Химическая реакция",
            LogType.EntityEffect => "Эффект сущности",
            LogType.CanisterValve => "Клапан канистры",
            LogType.CanisterPressure => "Давление канистры",
            LogType.CanisterPurged => "Сброс канистры",
            LogType.CanisterTankEjected => "Извлечение баллона из канистры",
            LogType.CanisterTankInserted => "Вставка баллона в канистру",
            LogType.DisarmedAction => "Разоружение",
            LogType.DisarmedKnockdown => "Нокдаун разоружением",
            LogType.AttackArmedClick => "Точечный вооружённый удар",
            LogType.AttackArmedWide => "Широкий вооружённый удар",
            LogType.AttackUnarmedClick => "Точечный безоружный удар",
            LogType.AttackUnarmedWide => "Широкий безоружный удар",
            LogType.InteractHand => "Взаимодействие в руке",
            LogType.InteractActivate => "Активация",
            LogType.Throw => "Бросок",
            LogType.Landed => "Приземление",
            LogType.ThrowHit => "Попадание брошенным предметом",
            LogType.Pickup => "Поднятие",
            LogType.Drop => "Сброс",
            LogType.BulletHit => "Попадание пулей",
            LogType.ForceFeed => "Насильное кормление",
            LogType.Ingestion => "Проглатывание",
            LogType.MeleeHit => "Попадание в ближнем бою",
            LogType.HitScanHit => "Попадание хитсканом",
            LogType.Mind => "Разум",
            LogType.Explosion => "Взрыв",
            LogType.Radiation => "Радиация",
            LogType.Barotrauma => "Баротравма",
            LogType.Flammable => "Горение",
            LogType.Asphyxiation => "Удушье",
            LogType.Temperature => "Температура",
            LogType.Hunger => "Голод",
            LogType.Thirst => "Жажда",
            LogType.Electrocution => "Удар током",
            LogType.CrayonDraw => "Рисование мелком",
            LogType.AtmosPressureChanged => "Изменение давления в атмосе",
            LogType.AtmosPowerChanged => "Изменение мощности в атмосе",
            LogType.AtmosVolumeChanged => "Изменение объёма в атмосе",
            LogType.AtmosFilterChanged => "Изменение фильтра в атмосе",
            LogType.AtmosRatioChanged => "Изменение соотношения в атмосе",
            LogType.FieldGeneration => "Генерация поля",
            LogType.GhostRoleTaken => "Взята роль призрака",
            LogType.Chat => "Чат",
            LogType.Action => "Действие",
            LogType.RCD => "RCD",
            LogType.Construction => "Строительство",
            LogType.Trigger => "Триггер",
            LogType.Anchor => "Закрепление",
            LogType.Unanchor => "Открепление",
            LogType.EmergencyShuttle => "Аварийный шаттл",
            LogType.Emag => "Емаг",
            LogType.Gib => "Гиб",
            LogType.Identity => "Личность",
            LogType.CableCut => "Перерезание кабеля",
            LogType.StorePurchase => "Покупка в магазине",
            LogType.LatticeCut => "Резка решётки",
            LogType.Stripping => "Экипировка и раздевание",
            LogType.Stamina => "Выносливость",
            LogType.EntitySpawn => "Создание сущности",
            LogType.AdminMessage => "Админ-сообщение",
            LogType.Anomaly => "Аномалия",
            LogType.WireHacking => "Взлом проводки",
            LogType.Teleport => "Телепорт",
            LogType.EntityDelete => "Удаление сущности",
            LogType.Vote => "Голосование",
            LogType.ItemConfigure => "Настройка предмета",
            LogType.DeviceLinking => "Связывание устройств",
            LogType.Tile => "Плитка",
            LogType.ChatRateLimited => "Ограничение чата",
            LogType.AtmosTemperatureChanged => "Изменение температуры в атмосе",
            LogType.DeviceNetwork => "Сеть устройств",
            LogType.StoreRefund => "Возврат из магазина",
            LogType.RateLimited => "Ограничение по частоте",
            LogType.InteractUsing => "Использование предметом",
            LogType.Storage => "Хранилище",
            LogType.ExplosionHit => "Попадание взрывом",
            LogType.GhostWarp => "Варп призрака",
            LogType.PdaInteract => "Взаимодействие с PDA",
            LogType.AtmosDeviceSetting => "Настройка атмосферного устройства",
            LogType.AdminCommands => "Админ-команды",
            LogType.AntagSelection => "Выбор антага",
            LogType.Botany => "Ботаника",
            LogType.ArtifactNode => "Узел артефакта",
            LogType.ShuttleImpact => "Удар шаттла",
            LogType.Instrument => "Инструмент",
            LogType.Connection => "Подключение",
            _ => Humanize(type.ToString())
        };
    }

    private static string Humanize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var builder = new StringBuilder(value.Length + 8);
        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i];
            if (i > 0 && char.IsUpper(ch) && !char.IsUpper(value[i - 1]))
                builder.Append(' ');

            builder.Append(ch);
        }

        return builder.ToString();
    }
}
