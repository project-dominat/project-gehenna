shared-solution-container-component-on-examine-main-text = Содержит { INDEFINITE($desc) } [color={ $color }]{ $desc }[/color] { $chemCount ->
    [1] химикат.
   *[other] смесь химикатов.
    }

examinable-solution-has-recognizable-chemicals = В растворе можно распознать { $recognizedString }.
examinable-solution-recognized = [color={ $color }]{ $chemical }[/color]

examinable-solution-on-examine-volume = Содержащийся раствор { $fillLevel ->
    [exact] вмещает [color=white]{ $current }/{ $max }ед.[/color].
   *[other] [bold]{ -solution-vague-fill-level(fillLevel: $fillLevel) }[/bold].
}

examinable-solution-on-examine-volume-no-max = Содержащийся раствор { $fillLevel ->
    [exact] вмещает [color=white]{ $current }ед.[/color].
   *[other] [bold]{ -solution-vague-fill-level(fillLevel: $fillLevel) }[/bold].
}

examinable-solution-on-examine-volume-puddle = Лужа { $fillLevel ->
    [exact] [color=white]{ $current }ед.[/color].
    [full] огромная и переливается через край!
    [mostlyfull] огромная и переливается через край!
    [halffull] глубокая и растекается.
    [halfempty] очень глубокая.
   *[mostlyempty] собирается в лужу.
    [empty] образует несколько маленьких луж.
}

-solution-vague-fill-level =
    { $fillLevel ->
        [full] [color=white]Полон[/color]
        [mostlyfull] [color=#DFDFDF]Почти полон[/color]
        [halffull] [color=#C8C8C8]Наполовину полон[/color]
        [halfempty] [color=#C8C8C8]Наполовину пуст[/color]
        [mostlyempty] [color=#A4A4A4]Почти пуст[/color]
       *[empty] [color=gray]Пуст[/color]
    }
