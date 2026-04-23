suspicion-ally-count-display =
    { $allyCount ->
       *[zero] Вы сами по себе. Удачи!
        [one] Ваш союзник: { $allyNames }.
        [other] Ваши союзники: { $allyNames }.
    }