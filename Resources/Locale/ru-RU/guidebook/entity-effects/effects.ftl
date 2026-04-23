-create-3rd-person =
    { $chance ->
        [1] Создаёт
        *[other] создать
    }

-cause-3rd-person =
    { $chance ->
        [1] Вызывает
        *[other] вызвать
    }

-satiate-3rd-person =
    { $chance ->
        [1] Утоляет
        *[other] утолить
    }

entity-effect-guidebook-spawn-entity =
    { $chance ->
        [1] Создаёт
        *[other] создать
    } { $amount ->
        [1] {$entname}
        *[other] {$amount} ед. {$entname}
    }

entity-effect-guidebook-destroy =
    { $chance ->
        [1] Уничтожает
        *[other] уничтожить
    } объект

entity-effect-guidebook-break =
    { $chance ->
        [1] Ломает
        *[other] сломать
    } объект

entity-effect-guidebook-explosion =
    { $chance ->
        [1] Вызывает
        *[other] вызвать
    } взрыв

entity-effect-guidebook-emp =
    { $chance ->
        [1] Вызывает
        *[other] вызвать
    } электромагнитный импульс

entity-effect-guidebook-flash =
    { $chance ->
        [1] Вызывает
        *[other] вызвать
    } ослепляющую вспышку

entity-effect-guidebook-foam-area =
    { $chance ->
        [1] Создаёт
        *[other] создать
    } большое количество пены

entity-effect-guidebook-smoke-area =
    { $chance ->
        [1] Создаёт
        *[other] создать
    } большое количество дыма

entity-effect-guidebook-satiate-thirst =
    { $chance ->
        [1] Утоляет
        *[other] утолить
    } жажду { $relative ->
        [1] со средней скоростью
        *[other] со скоростью {NATURALFIXED($relative, 3)}x от средней
    }

entity-effect-guidebook-satiate-hunger =
    { $chance ->
        [1] Утоляет
        *[other] утолить
    } голод { $relative ->
        [1] со средней скоростью
        *[other] со скоростью {NATURALFIXED($relative, 3)}x от средней
    }

entity-effect-guidebook-health-change =
    { $chance ->
        [1] { $healsordeals ->
                [heals] Восстанавливает
                [deals] Наносит
                *[both] Изменяет здоровье на
             }
        *[other] { $healsordeals ->
                    [heals] восстановить
                    [deals] нанести
                    *[both] изменить здоровье на
                 }
    } { $changes }

entity-effect-guidebook-even-health-change =
    { $chance ->
        [1] { $healsordeals ->
            [heals] Равномерно восстанавливает
            [deals] Равномерно наносит
            *[both] Равномерно изменяет здоровье на
        }
        *[other] { $healsordeals ->
            [heals] равномерно восстановить
            [deals] равномерно нанести
            *[both] равномерно изменить здоровье на
        }
    } { $changes }

entity-effect-guidebook-status-effect-old =
    { $type ->
        [update]{ $chance ->
                    [1] Вызывает
                    *[other] вызвать
                 } {LOC($key)} как минимум на {NATURALFIXED($time, 3)} сек. без накопления
        [add]   { $chance ->
                    [1] Вызывает
                    *[other] вызвать
                } {LOC($key)} как минимум на {NATURALFIXED($time, 3)} сек. с накоплением
        [set]   { $chance ->
                    [1] Вызывает
                    *[other] вызвать
                } {LOC($key)} на {NATURALFIXED($time, 3)} сек. без накопления
        *[remove]{ $chance ->
                    [1] Убирает
                    *[other] убрать
                } {NATURALFIXED($time, 3)} сек. эффекта {LOC($key)}
    }

entity-effect-guidebook-status-effect =
    { $type ->
        [update]{ $chance ->
                    [1] Вызывает
                    *[other] вызвать
                 } {LOC($key)} как минимум на {NATURALFIXED($time, 3)} сек. без накопления
        [add]   { $chance ->
                    [1] Вызывает
                    *[other] вызвать
                } {LOC($key)} как минимум на {NATURALFIXED($time, 3)} сек. с накоплением
        [set]   { $chance ->
                    [1] Вызывает
                    *[other] вызвать
                } {LOC($key)} как минимум на {NATURALFIXED($time, 3)} сек. без накопления
        *[remove]{ $chance ->
                    [1] Убирает
                    *[other] убрать
                } {NATURALFIXED($time, 3)} сек. эффекта {LOC($key)}
    } { $delay ->
        [0] немедленно
        *[other] через {NATURALFIXED($delay, 3)} сек. задержки
    }

entity-effect-guidebook-status-effect-indef =
    { $type ->
        [update]{ $chance ->
                    [1] Вызывает
                    *[other] вызвать
                 } постоянный эффект {LOC($key)}
        [add]   { $chance ->
                    [1] Вызывает
                    *[other] вызвать
                } постоянный эффект {LOC($key)}
        [set]   { $chance ->
                    [1] Вызывает
                    *[other] вызвать
                } постоянный эффект {LOC($key)}
        *[remove]{ $chance ->
                    [1] Убирает
                    *[other] убрать
                } эффект {LOC($key)}
    } { $delay ->
        [0] немедленно
        *[other] через {NATURALFIXED($delay, 3)} сек. задержки
    }

entity-effect-guidebook-knockdown =
    { $type ->
        [update]{ $chance ->
                    [1] Вызывает
                    *[other] вызвать
                } опрокидывание как минимум на {NATURALFIXED($time, 3)} сек. без накопления
        [add]   { $chance ->
                    [1] Вызывает
                    *[other] вызвать
                } опрокидывание как минимум на {NATURALFIXED($time, 3)} сек. с накоплением
        *[set]  { $chance ->
                    [1] Вызывает
                    *[other] вызвать
                } опрокидывание как минимум на {NATURALFIXED($time, 3)} сек. без накопления
        [remove]{ $chance ->
                    [1] Убирает
                    *[other] убрать
                } {NATURALFIXED($time, 3)} сек. опрокидывания
    }

entity-effect-guidebook-set-solution-temperature-effect =
    { $chance ->
        [1] Устанавливает
        *[other] установить
    } температуру раствора ровно на {NATURALFIXED($temperature, 2)}K

entity-effect-guidebook-adjust-solution-temperature-effect =
    { $chance ->
        [1] { $deltasign ->
                [1] Добавляет
                *[-1] Отводит
            }
        *[other]
            { $deltasign ->
                [1] добавить
                *[-1] отвести
            }
    } тепло из раствора, пока температура не достигнет { $deltasign ->
                [1] не более {NATURALFIXED($maxtemp, 2)}K
                *[-1] не менее {NATURALFIXED($mintemp, 2)}K
            }

entity-effect-guidebook-adjust-reagent-reagent =
    { $chance ->
        [1] { $deltasign ->
                [1] Добавляет
                *[-1] Удаляет
            }
        *[other]
            { $deltasign ->
                [1] добавить
                *[-1] удалить
            }
    } {NATURALFIXED($amount, 2)}u реагента {$reagent} { $deltasign ->
        [1] в
        *[-1] из
    } раствор

entity-effect-guidebook-adjust-reagent-group =
    { $chance ->
        [1] { $deltasign ->
                [1] Добавляет
                *[-1] Удаляет
            }
        *[other]
            { $deltasign ->
                [1] добавить
                *[-1] удалить
            }
    } {NATURALFIXED($amount, 2)}u реагентов из группы {$group} { $deltasign ->
            [1] в
            *[-1] из
        } раствор

entity-effect-guidebook-adjust-temperature =
    { $chance ->
        [1] { $deltasign ->
                [1] Добавляет
                *[-1] Удаляет
            }
        *[other]
            { $deltasign ->
                [1] добавить
                *[-1] удалить
            }
    } {POWERJOULES($amount)} тепла { $deltasign ->
            [1] телу
            *[-1] из тела
        }

entity-effect-guidebook-chem-cause-disease =
    { $chance ->
        [1] Вызывает
        *[other] вызвать
    } болезнь { $disease }

entity-effect-guidebook-chem-cause-random-disease =
    { $chance ->
        [1] Вызывает
        *[other] вызвать
    } болезни { $diseases }

entity-effect-guidebook-jittering =
    { $chance ->
        [1] Вызывает
        *[other] вызвать
    } дрожь

entity-effect-guidebook-clean-bloodstream =
    { $chance ->
        [1] Очищает
        *[other] очистить
    } кровоток от других химикатов

entity-effect-guidebook-cure-disease =
    { $chance ->
        [1] Лечит
        *[other] вылечить
    } болезни

entity-effect-guidebook-eye-damage =
    { $chance ->
        [1] { $deltasign ->
                [1] Наносит
                *[-1] Лечит
            }
        *[other]
            { $deltasign ->
                [1] нанести
                *[-1] вылечить
            }
    } повреждение глаз

entity-effect-guidebook-vomit =
    { $chance ->
        [1] Вызывает
        *[other] вызвать
    } рвоту

entity-effect-guidebook-create-gas =
    { $chance ->
        [1] Создаёт
        *[other] создать
    } { $moles } { $moles ->
        [1] моль
        *[other] моль
    } газа { $gas }

entity-effect-guidebook-drunk =
    { $chance ->
        [1] Вызывает
        *[other] вызвать
    } опьянение

entity-effect-guidebook-electrocute =
    { $chance ->
        [1] { $stuns ->
            [true] Электрошокирует
            *[false] Бьёт током
            }
        *[other] { $stuns ->
            [true] электрошокировать
            *[false] ударить током
            }
    } метаболизирующего в течение {NATURALFIXED($time, 3)} сек.

entity-effect-guidebook-emote =
    { $chance ->
        [1] Принудительно заставляет
        *[other] принудительно заставить
    } метаболизирующего [bold][color=white]{$emote}[/color][/bold]

entity-effect-guidebook-extinguish-reaction =
    { $chance ->
        [1] Тушит
        *[other] потушить
    } огонь

entity-effect-guidebook-flammable-reaction =
    { $chance ->
        [1] Повышает
        *[other] повысить
    } воспламеняемость

entity-effect-guidebook-ignite =
    { $chance ->
        [1] Поджигает
        *[other] поджечь
    } метаболизирующего

entity-effect-guidebook-make-sentient =
    { $chance ->
        [1] Делает
        *[other] сделать
    } метаболизирующего разумным

entity-effect-guidebook-make-polymorph =
    { $chance ->
        [1] Превращает
        *[other] превратить
    } метаболизирующего в { $entityname }

entity-effect-guidebook-modify-bleed-amount =
    { $chance ->
        [1] { $deltasign ->
                [1] Вызывает
                *[-1] Уменьшает
            }
        *[other] { $deltasign ->
                    [1] вызвать
                    *[-1] уменьшить
                 }
    } кровотечение

entity-effect-guidebook-modify-blood-level =
    { $chance ->
        [1] { $deltasign ->
                [1] Повышает
                *[-1] Понижает
            }
        *[other] { $deltasign ->
                    [1] повысить
                    *[-1] понизить
                 }
    } уровень крови

entity-effect-guidebook-paralyze =
    { $chance ->
        [1] Парализует
        *[other] парализовать
    } метаболизирующего как минимум на {NATURALFIXED($time, 3)} сек.

entity-effect-guidebook-movespeed-modifier =
    { $chance ->
        [1] Изменяет
        *[other] изменить
    } скорость передвижения в {NATURALFIXED($sprintspeed, 3)}x как минимум на {NATURALFIXED($time, 3)} сек.

entity-effect-guidebook-reset-narcolepsy =
    { $chance ->
        [1] Временно подавляет
        *[other] временно подавить
    } нарколепсию

entity-effect-guidebook-wash-cream-pie-reaction =
    { $chance ->
        [1] Смывает
        *[other] смыть
    } кремовый пирог с лица

entity-effect-guidebook-cure-zombie-infection =
    { $chance ->
        [1] Лечит
        *[other] вылечить
    } текущую зомби-инфекцию

entity-effect-guidebook-cause-zombie-infection =
    { $chance ->
        [1] Даёт
        *[other] дать
    } особи зомби-инфекцию

entity-effect-guidebook-innoculate-zombie-infection =
    { $chance ->
        [1] Лечит
        *[other] вылечить
    } текущую зомби-инфекцию и даёт иммунитет к будущим заражениям

entity-effect-guidebook-reduce-rotting =
    { $chance ->
        [1] Регенерирует
        *[other] регенерировать
    } {NATURALFIXED($time, 3)} сек. гниения

entity-effect-guidebook-area-reaction =
    { $chance ->
        [1] Вызывает
        *[other] вызвать
    } дымовую или пенную реакцию на {NATURALFIXED($duration, 3)} сек.

entity-effect-guidebook-add-to-solution-reaction =
    { $chance ->
        [1] Заставляет
        *[other] заставить
    } {$reagent} добавляться во внутренний контейнер раствора

entity-effect-guidebook-artifact-unlock =
    { $chance ->
        [1] Помогает
        *[other] помочь
        } разблокировать инопланетный артефакт.

entity-effect-guidebook-artifact-durability-restore =
    Восстанавливает {$restored} прочности активным узлам инопланетного артефакта.

entity-effect-guidebook-plant-attribute =
    { $chance ->
        [1] Изменяет
        *[other] изменить
    } {$attribute} на {$positive ->
    [true] [color=red]{$amount}[/color]
    *[false] [color=green]{$amount}[/color]
    }

entity-effect-guidebook-plant-cryoxadone =
    { $chance ->
        [1] Омолаживает
        *[other] омолодить
    } растение в зависимости от его возраста и времени роста

entity-effect-guidebook-plant-phalanximine =
    { $chance ->
        [1] Возвращает
        *[other] вернуть
    } жизнеспособность растению, утратившему её из-за мутации

entity-effect-guidebook-plant-diethylamine =
    { $chance ->
        [1] Увеличивает
        *[other] увеличить
    } срок жизни и/или базовое здоровье растения с шансом 10% для каждого

entity-effect-guidebook-plant-robust-harvest =
    { $chance ->
        [1] Повышает
        *[other] повысить
    } потенциал растения на {$increase} до максимума {$limit}. Растение теряет семена, когда потенциал достигает {$seedlesstreshold}. Попытка превысить {$limit} может с шансом 10% уменьшить урожайность

entity-effect-guidebook-plant-seeds-add =
    { $chance ->
        [1] Возвращает
        *[other] вернуть
    } растению семена

entity-effect-guidebook-plant-seeds-remove =
    { $chance ->
        [1] Удаляет
        *[other] удалить
    } семена растения

entity-effect-guidebook-plant-mutate-chemicals =
    { $chance ->
        [1] Мутирует
        *[other] мутировать
    } растение так, чтобы оно производило {$name}
