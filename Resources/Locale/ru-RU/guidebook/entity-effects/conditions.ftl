entity-condition-guidebook-total-damage =
    { $max ->
        [2147483648] у цели не меньше {NATURALFIXED($min, 2)} общего урона
        *[other] { $min ->
                    [0] у цели не больше {NATURALFIXED($max, 2)} общего урона
                    *[other] у цели от {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)} общего урона
                 }
    }

entity-condition-guidebook-type-damage =
    { $max ->
        [2147483648] у цели не меньше {NATURALFIXED($min, 2)} урона типа {$type}
        *[other] { $min ->
                    [0] у цели не больше {NATURALFIXED($max, 2)} урона типа {$type}
                    *[other] у цели от {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)} урона типа {$type}
                 }
    }

entity-condition-guidebook-group-damage =
    { $max ->
        [2147483648] у цели не меньше {NATURALFIXED($min, 2)} урона группы {$type}
        *[other] { $min ->
                    [0] у цели не больше {NATURALFIXED($max, 2)} урона группы {$type}
                    *[other] у цели от {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)} урона группы {$type}
                 }
    }

entity-condition-guidebook-total-hunger =
    { $max ->
        [2147483648] у цели не меньше {NATURALFIXED($min, 2)} голода
        *[other] { $min ->
                    [0] у цели не больше {NATURALFIXED($max, 2)} голода
                    *[other] у цели от {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)} голода
                 }
    }

entity-condition-guidebook-reagent-threshold =
    { $max ->
        [2147483648] содержится не меньше {NATURALFIXED($min, 2)}u {$reagent}
        *[other] { $min ->
                    [0] содержится не больше {NATURALFIXED($max, 2)}u {$reagent}
                    *[other] содержится от {NATURALFIXED($min, 2)}u до {NATURALFIXED($max, 2)}u {$reagent}
                 }
    }

entity-condition-guidebook-mob-state-condition =
    моб находится в состоянии { $state }

entity-condition-guidebook-job-condition =
    должность цели — { $job }

entity-condition-guidebook-solution-temperature =
    температура раствора { $max ->
            [2147483648] не ниже {NATURALFIXED($min, 2)}K
            *[other] { $min ->
                        [0] не выше {NATURALFIXED($max, 2)}K
                        *[other] от {NATURALFIXED($min, 2)}K до {NATURALFIXED($max, 2)}K
                     }
    }

entity-condition-guidebook-body-temperature =
    температура тела { $max ->
            [2147483648] не ниже {NATURALFIXED($min, 2)}K
            *[other] { $min ->
                        [0] не выше {NATURALFIXED($max, 2)}K
                        *[other] от {NATURALFIXED($min, 2)}K до {NATURALFIXED($max, 2)}K
                     }
    }

entity-condition-guidebook-organ-type =
    метаболизирующий орган { $shouldhave ->
                                [true] является
                                *[false] не является
                           } органом {$name}

entity-condition-guidebook-has-tag =
    цель { $invert ->
                 [true] не имеет
                 *[false] имеет
                } тег {$tag}

entity-condition-guidebook-this-reagent = этот реагент

entity-condition-guidebook-breathing =
    метаболизирующий { $isBreathing ->
                [true] дышит нормально
                *[false] задыхается
               }

entity-condition-guidebook-internals =
    метаболизирующий { $usingInternals ->
                [true] использует внутреннее дыхание
                *[false] дышит атмосферным воздухом
               }
