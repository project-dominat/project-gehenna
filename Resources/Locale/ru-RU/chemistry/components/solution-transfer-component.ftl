### Компонент переноса раствора

comp-solution-transfer-fill-normal = Вы наполняете { THE($target) } на { $amount }ед. из { THE($owner) }.
comp-solution-transfer-fill-fully = Вы наполняете { THE($target) } до краёв — { $amount }ед. из { THE($owner) }.
comp-solution-transfer-transfer-solution = Вы переливаете { $amount }ед. в { THE($target) }.

## При попытке перелить, когда источник пуст или цель полна
comp-solution-transfer-is-empty = { CAPITALIZE(THE($target)) } пуст!
comp-solution-transfer-is-full = { CAPITALIZE(THE($target)) } полон!

## Название действия изменения объёма переноса
comp-solution-transfer-verb-custom-amount = Произвольно
comp-solution-transfer-verb-amount = { $amount }ед.
comp-solution-transfer-verb-toggle = Переключить на { $amount }ед.

## После успешного изменения объёма через интерфейс
comp-solution-transfer-set-amount = Объём переноса установлен на { $amount }ед.
comp-solution-transfer-set-amount-max = Макс.: { $amount }ед.
comp-solution-transfer-set-amount-min = Мин.: { $amount }ед.
