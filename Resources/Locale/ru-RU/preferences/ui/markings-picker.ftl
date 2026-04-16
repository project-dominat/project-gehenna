markings-search = Поиск
-markings-selection = { $selectable ->
    [one] Можно выбрать еще одну отметину.
    [few] Можно выбрать еще { $selectable } отметины.
   *[other] Можно выбрать еще { $selectable } отметин.
}
markings-limits = { $required ->
    [true] { $count ->
        [-1] Выберите хотя бы одну отметину.
        [0] Нельзя выбрать ни одной отметины, но почему-то это требуется. Это баг.
        [one] Выберите одну отметину.
       *[other] Выберите хотя бы одну отметину и не более {$count}. { -markings-selection(selectable: $selectable) }
    }
   *[false] { $count ->
        [-1] Выберите любое число отметин.
        [0] Нельзя выбрать ни одной отметины.
        [one] Выберите не более одной отметины.
       *[other] Выберите не более {$count} отметин. { -markings-selection(selectable: $selectable) }
    }
}
markings-reorder = Изменить порядок отметин

humanoid-marking-modifier-respect-limits = Учитывать лимиты
humanoid-marking-modifier-respect-group-sex = Учитывать ограничения группы и пола
humanoid-marking-modifier-base-layers = Базовые слои
humanoid-marking-modifier-enable = Включить
humanoid-marking-modifier-prototype-id = ID прототипа:

# Categories

markings-organ-Torso = Торс
markings-organ-Head = Голова
markings-organ-ArmLeft = Левая рука
markings-organ-ArmRight = Правая рука
markings-organ-HandRight = Правая кисть
markings-organ-HandLeft = Левая кисть
markings-organ-LegLeft = Левая нога
markings-organ-LegRight = Правая нога
markings-organ-FootLeft = Левая ступня
markings-organ-FootRight = Правая ступня
markings-organ-Eyes = Глаза

markings-layer-Special = Особое
markings-layer-Tail = Хвост
markings-layer-Tail-Moth = Крылья
markings-layer-Hair = Волосы
markings-layer-FacialHair = Растительность на лице
markings-layer-UndergarmentTop = Нижняя рубашка
markings-layer-UndergarmentBottom = Нижнее белье
markings-layer-Chest = Грудь
markings-layer-Head = Голова
markings-layer-Snout = Морда
markings-layer-SnoutCover = Морда (покров)
markings-layer-HeadSide = Голова (сбоку)
markings-layer-HeadTop = Голова (сверху)
markings-layer-Eyes = Глаза
markings-layer-RArm = Правая рука
markings-layer-LArm = Левая рука
markings-layer-RHand = Правая кисть
markings-layer-LHand = Левая кисть
markings-layer-RLeg = Правая нога
markings-layer-LLeg = Левая нога
markings-layer-RFoot = Правая ступня
markings-layer-LFoot = Левая ступня
markings-layer-Overlay = Оверлей
markings-layer-TailOverlay = Оверлей

