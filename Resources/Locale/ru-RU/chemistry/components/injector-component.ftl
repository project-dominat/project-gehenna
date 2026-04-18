## Интерфейс

injector-volume-transfer-label = Объём: [color=white]{ $currentVolume }/{ $totalVolume }ед.[/color]
    Режим: [color=white]{ $modeString }[/color] ([color=white]{ $transferVolume }ед.[/color])
injector-volume-label = Объём: [color=white]{ $currentVolume }/{ $totalVolume }ед.[/color]
    Режим: [color=white]{ $modeString }[/color]
injector-toggle-verb-text = Переключить режим инъектора

## Сущность

injector-component-inject-mode-name = ввод
injector-component-draw-mode-name = забор
injector-component-dynamic-mode-name = динамический
injector-component-mode-changed-text = Теперь { $mode }
injector-component-transfer-success-message = Вы переносите { $amount }ед. в { THE($target) }.
injector-component-transfer-success-message-self = Вы переносите { $amount }ед. в себя.
injector-component-inject-success-message = Вы вводите { $amount }ед. в { THE($target) }!
injector-component-inject-success-message-self = Вы вводите { $amount }ед. в себя!
injector-component-draw-success-message = Вы набираете { $amount }ед. из { THE($target) }.
injector-component-draw-success-message-self = Вы набираете { $amount }ед. из себя.

## Сообщения об ошибках

injector-component-target-already-full-message = { CAPITALIZE(THE($target)) } уже полон!
injector-component-target-already-full-message-self = Вы уже заполнены!
injector-component-target-is-empty-message = { CAPITALIZE(THE($target)) } пуст!
injector-component-target-is-empty-message-self = Вы пусты!
injector-component-cannot-toggle-draw-message = Слишком полон для забора!
injector-component-cannot-toggle-inject-message = Нечего вводить!
injector-component-cannot-toggle-dynamic-message = Нельзя переключить динамический режим!
injector-component-empty-message = { CAPITALIZE(THE($injector)) } пуст!
injector-component-blocked-user = Защитное снаряжение заблокировало вашу инъекцию!
injector-component-blocked-other = Броня { CAPITALIZE(THE(POSS-ADJ($target))) } заблокировала инъекцию { THE($user) }!
injector-component-cannot-transfer-message = Вы не можете перелить в { THE($target) }!
injector-component-cannot-transfer-message-self = Вы не можете перелить в себя!
injector-component-cannot-inject-message = Вы не можете ввести в { THE($target) }!
injector-component-cannot-inject-message-self = Вы не можете ввести в себя!
injector-component-cannot-draw-message = Вы не можете набрать из { THE($target) }!
injector-component-cannot-draw-message-self = Вы не можете набрать из себя!
injector-component-ignore-mobs = Этот инъектор может взаимодействовать только с контейнерами!

## Сообщения во время действия

injector-component-needle-injecting-user = Вы начинаете вводить иглу.
injector-component-needle-injecting-target = { CAPITALIZE(THE($user)) } пытается ввести вам иглу!
injector-component-needle-drawing-user = Вы начинаете набирать иглой.
injector-component-needle-drawing-target = { CAPITALIZE(THE($user)) } пытается набрать иглой из вас!
injector-component-spray-injecting-user = Вы начинаете готовить распылитель.
injector-component-spray-injecting-target = { CAPITALIZE(THE($user)) } пытается приложить распылитель к вам!

## Всплывающие сообщения об успехе
injector-component-feel-prick-message = Вы чувствуете лёгкий укол!
