ingestion-you-need-to-hold-utensil = Чтобы съесть это, вам нужно иметь {INDEFINITE($utensil)} {$utensil}!

ingestion-try-use-is-empty = {CAPITALIZE(THE($entity))} пуст!

ingestion-try-use-wrong-utensil = Вы не можете {$verb} {THE($food)} с {INDEFINITE($utensil)} {$utensil}.

ingestion-remove-mask = Сначала вам нужно снять {$entity}.

## Failed Ingestion

ingestion-you-cannot-ingest-any-more = Ты больше не можешь {$verb}!

ingestion-other-cannot-ingest-any-more = {CAPITALIZE(SUBJECT($target))} больше не может {$verb}!

ingestion-cant-digest = Ты не можешь переварить {THE($entity)}!

ingestion-cant-digest-other = {CAPITALIZE(SUBJECT($target))} не может переварить {THE($entity)}!

## Action Verbs, not to be confused with Verbs

ingestion-verb-food = есть

ingestion-verb-drink = Напиток

# Edible Component

-edible-satiated = { $satiated ->
    [true] {" "}Ты больше не чувствуешь, что сможешь { $verb }.
  *[false] {""}
}

edible-nom = Ном. {$flavors}{ -edible-satiated(satiated: $satiated, verb: "eat") }

edible-nom-other = Ном.

edible-slurp = Хлебать. {$flavors}{ -edible-satiated(satiated: $satiated, verb: "drink") }

edible-slurp-other = Хлебать.

edible-swallow = Вы проглатываете { THE($food) }.{ -edible-satiated(satiated: $satiated, verb: "swallow") }

edible-gulp = Глоток. {$flavors}

edible-gulp-other = Глоток.

edible-has-used-storage = Вы не можете {$verb} { THE($food) } с предметом, хранящимся внутри.

## Nouns

edible-noun-edible = съедобный

edible-noun-food = еда

edible-noun-drink = пить

edible-noun-pill = таблетка

## Verbs

edible-verb-edible = проглотить

edible-verb-food = есть

edible-verb-drink = пить

edible-verb-pill = глотать

## Force feeding

edible-force-feed = {CAPITALIZE(THE($user))} пытается сделать что-то для вас, {$verb}!

edible-force-feed-success = {CAPITALIZE(THE($user))} заставил вас {$verb} что-то сделать! {$flavors}{ -edible-satiated(satiated: $satiated, verb: $verb) }

edible-force-feed-success-user = Вы успешно кормите {THE($target)}
