# Интерфейс

## Окно

air-alarm-ui-title = Воздушная тревога

air-alarm-ui-access-denied = Недостаточно прав!

air-alarm-ui-window-pressure-label = Давление
air-alarm-ui-window-temperature-label = Температура
air-alarm-ui-window-alarm-state-label = Статус

air-alarm-ui-window-address-label = Адрес
air-alarm-ui-window-device-count-label = Всего устройств
air-alarm-ui-window-resync-devices-label = Пересинхронизировать

air-alarm-ui-window-mode-label = Режим
air-alarm-ui-window-mode-select-locked-label = [bold][color=red] Сбой выбора режима! [/color][/bold]
air-alarm-ui-window-auto-mode-label = Авторежим

-air-alarm-state-name = { $state ->
    [normal] Норма
    [warning] Внимание
    [danger] Опасность
    [emagged] Взломано
   *[invalid] Неверно
}

air-alarm-ui-window-listing-title = { $address } : { -air-alarm-state-name(state: $state) }
air-alarm-ui-window-pressure = { $pressure } кПа
air-alarm-ui-window-pressure-indicator = Давление: [color={ $color }]{ $pressure } кПа[/color]
air-alarm-ui-window-temperature = { $tempC } °C ({ $temperature } K)
air-alarm-ui-window-temperature-indicator = Температура: [color={ $color }]{ $tempC } °C ({ $temperature } K)[/color]
air-alarm-ui-window-alarm-state = [color={ $color }]{ -air-alarm-state-name(state: $state) }[/color]
air-alarm-ui-window-alarm-state-indicator = Статус: [color={ $color }]{ -air-alarm-state-name(state: $state) }[/color]

air-alarm-ui-window-tab-vents = Вентиляция
air-alarm-ui-window-tab-scrubbers = Скрубберы
air-alarm-ui-window-tab-sensors = Датчики

air-alarm-ui-gases = { $gas }: { $amount } моль ({ $percentage }%)
air-alarm-ui-gases-indicator = { $gas }: [color={ $color }]{ $amount } моль ({ $percentage }%)[/color]

air-alarm-ui-mode-filtering = Фильтрация
air-alarm-ui-mode-wide-filtering = Фильтрация (широкая)
air-alarm-ui-mode-fill = Заполнение
air-alarm-ui-mode-panic = Паника
air-alarm-ui-mode-none = Нет

air-alarm-ui-pump-direction-siphoning = Откачка
air-alarm-ui-pump-direction-scrubbing = Очистка
air-alarm-ui-pump-direction-releasing = Подача

air-alarm-ui-pressure-bound-nobound = Без ограничения
air-alarm-ui-pressure-bound-internalbound = Внутреннее ограничение
air-alarm-ui-pressure-bound-externalbound = Внешнее ограничение
air-alarm-ui-pressure-bound-both = Оба

air-alarm-ui-widget-gas-filters = Газовые фильтры

## Виджеты

### Общие

air-alarm-ui-widget-enable = Включено
air-alarm-ui-widget-copy = Скопировать настройки на аналогичные устройства
air-alarm-ui-widget-copy-tooltip = Копирует настройки этого устройства на все устройства в данной вкладке воздушной тревоги.
air-alarm-ui-widget-ignore = Игнорировать
air-alarm-ui-atmos-net-device-label = Адрес: { $address }

### Вентиляционные насосы

air-alarm-ui-vent-pump-label = Направление вентиляции
air-alarm-ui-vent-pressure-label = Ограничение давления
air-alarm-ui-vent-external-bound-label = Внешнее ограничение
air-alarm-ui-vent-internal-bound-label = Внутреннее ограничение

### Скрубберы

air-alarm-ui-scrubber-pump-direction-label = Направление
air-alarm-ui-scrubber-volume-rate-label = Расход (л)
air-alarm-ui-scrubber-wide-net-label = Широкий охват
air-alarm-ui-scrubber-select-all-gases-label = Выбрать все
air-alarm-ui-scrubber-deselect-all-gases-label = Снять выбор

### Пороги

air-alarm-ui-sensor-gases = Газы
air-alarm-ui-sensor-thresholds = Пороги
air-alarm-ui-thresholds-pressure-title = Пороги (кПа)
air-alarm-ui-thresholds-temperature-title = Пороги (K)
air-alarm-ui-thresholds-gas-title = Пороги (%)
air-alarm-ui-thresholds-upper-bound = Опасность выше
air-alarm-ui-thresholds-lower-bound = Опасность ниже
air-alarm-ui-thresholds-upper-warning-bound = Внимание выше
air-alarm-ui-thresholds-lower-warning-bound = Внимание ниже
air-alarm-ui-thresholds-copy = Скопировать пороги на все устройства
air-alarm-ui-thresholds-copy-tooltip = Копирует пороги датчика этого устройства на все устройства в данной вкладке воздушной тревоги.
