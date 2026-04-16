# Displayed as initiator of vote when no user creates the vote
ui-vote-initiator-server = Сервер

## Default.Votes

ui-vote-restart-title = Перезапуск раунда
ui-vote-restart-succeeded = Голосование за перезапуск прошло успешно.
ui-vote-restart-failed = Голосование за перезапуск провалилось (нужно { TOSTRING($ratio, "P0") }).
ui-vote-restart-fail-not-enough-ghost-players = Голосование за перезапуск провалилось: для его запуска требуется минимум { $ghostPlayerRequirement }% игроков-призраков. Сейчас игроков-призраков недостаточно.
ui-vote-restart-yes = Да
ui-vote-restart-no = Нет
ui-vote-restart-abstain = Воздержаться

ui-vote-gamemode-title = Следующий режим
ui-vote-gamemode-tie = Ничья в голосовании за режим! Выбирается... { $picked }
ui-vote-gamemode-win = { $winner } победил в голосовании за режим!

ui-vote-map-title = Следующая карта
ui-vote-map-tie = Ничья в голосовании за карту! Выбирается... { $picked }
ui-vote-map-win = { $winner } победила в голосовании за карту!
ui-vote-map-notlobby = Голосование за карты доступно только в предраундовом лобби!
ui-vote-map-notlobby-time = Голосование за карты доступно только в предраундовом лобби, когда осталось { $time }!
ui-vote-map-invalid = { $winner } стала недоступна после голосования за карту! Она не будет выбрана!

# Votekick votes
ui-vote-votekick-unknown-initiator = Игрок
ui-vote-votekick-unknown-target = Неизвестный игрок
ui-vote-votekick-title = { $initiator } создал голосование за кик игрока: { $targetEntity }. Причина: { $reason }
ui-vote-votekick-yes = Да
ui-vote-votekick-no = Нет
ui-vote-votekick-abstain = Воздержаться
ui-vote-votekick-success = Голосование за кик { $target } прошло успешно. Причина: { $reason }
ui-vote-votekick-failure = Голосование за кик { $target } провалилось. Причина: { $reason }
ui-vote-votekick-not-enough-eligible = Недостаточно подходящих голосующих онлайн для запуска кика: { $voters }/{ $requirement }
ui-vote-votekick-server-cancelled = Голосование за кик { $target } отменено сервером.

