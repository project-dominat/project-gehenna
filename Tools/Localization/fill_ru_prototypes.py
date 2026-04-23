from __future__ import annotations

import json
import pathlib
import re


ROOT = pathlib.Path(r"D:\CodeWork\project-gehenna")
REPORT = ROOT / "remaining_manual_entities_current.json"
PROTO_ROOT = ROOT / "Resources" / "Prototypes"
RU_ROOT = ROOT / "Resources" / "Locale" / "ru-RU" / "_prototypes"

ENTITY_START = re.compile(r"^- type: entity\s*$")
KEY_PAT = re.compile(r"^ent-(.+?)\s*=")
ATTR_PAT = re.compile(r"^\s+\.(\w+)\s*=")
LOC_KEY_PAT = re.compile(r"^[a-z0-9][a-z0-9-]*(?:\.[a-z0-9-]+)?$")

NAME_MAP: dict[str, str] = {
    "Alexander Spawner": "спавнер Александра",
    "Bandito Spawner": "спавнер Бандито",
    "Battle Map Spawner": "спавнер боевых карт",
    "Bingus Spawner": "спавнер Бингуса",
    "Board Game Spawner": "спавнер настольных игр",
    "Boxing Kangaroo Spawner": "спавнер боевого кенгуру",
    "C-20r ROW": "C-20r ROW",
    "Cap Gun Spawner": "спавнер пистонов",
    "Corgi Spawner": "спавнер корги",
    "Crayon Spawner": "спавнер мелков",
    "DNA Store": "Хранилище ДНК",
    "EMP grenade": "ЭМИ-граната",
    "Exception Spawner": "спавнер Эксепшена",
    "Floppa Spawner": "спавнер Шлёпы",
    "Fox Renault Spawner": "спавнер Рено",
    "Hamster Hamlet Spawner": "спавнер Гамлета",
    "Hijack the Automated Trade Station.": "Захватить автоматизированную торговую станцию.",
    "Jump boost": "Усиленный прыжок",
    "Leap": "Прыжок",
    "MRE flask": "фляга ИРП",
    "McGriff Spawner": "спавнер МакГриффа",
    "Mech Figurine Spawner": "спавнер фигурок мехов",
    "Miscellaneous Toy Spawner": "спавнер разных игрушек",
    "Plushie Spawner": "спавнер плюшек",
    "Possum Morty Spawner": "спавнер Морти",
    "Pun Pun Spawner": "спавнер Пун Пуна",
    "Raccoon Morticia Spawner": "спавнер Мортиши",
    "Random Cat Spawner": "спавнер случайного кота",
    "Runtime Spawner": "спавнер Рантайма",
    "Salvage Living Light Spawner": "спавнер живого света утилизации",
    "Shiva Spawner": "спавнер Шивы",
    "SlimePerson appearance": "внешность слаймолюда",
    "Sloth Paperwork Spawner": "спавнер Пэйперворка",
    "Smile Spawner": "спавнер Смайла",
    "Spacemen Minifigure Spawner": "спавнер минифигурок космонавтов",
    "Spray water!": "Брызнуть водой!",
    "Store": "Магазин",
    "Toy Sound Maker Spawner": "спавнер шумовых игрушек",
    "Toy Spawner": "спавнер игрушек",
    "Toy Weapon Spawner": "спавнер игрушечного оружия",
    "Tropico Spawner": "спавнер Тропико",
    "Walter Spawner": "спавнер Уолтера",
    "Willow Spawner": "спавнер Уиллоу",
    "XL8": "XL8",
    "Xeno Spawner": "спавнер ксено",
    "Xenoborgs Camera Monitor": "монитор камер ксеноборгов",
    "Xenoborgs Control Console": "консоль управления ксеноборгами",
    "admin anomaly scanner": "админский сканер аномалий",
    "advanced first aid cyborg module": "продвинутый медицинский модуль киборга",
    "ammunition box (.20 rifle)": "коробка патронов (.20 винтовочные)",
    "ammunition box (.30 rifle)": "коробка патронов (.30 винтовочные)",
    "arachnid appearance": "внешность арахнида",
    "base repairable shield": "базовый ремонтируемый щит",
    "baton grenade": "граната-дубинка",
    "black checker crown": "чёрная дамка",
    "black checker piece": "чёрная шашка",
    "blast grenade": "фугасная граната",
    "blood-red locker": "кроваво-красный шкафчик",
    "blood-red wall locker": "кроваво-красный настенный шкафчик",
    "bonfire with stake": "костёр с колом",
    "borderless astro-ironsand": "астро-железный песок без границ",
    "cafe latte": "латте",
    "chameleon projector": "маскировочный проектор",
    "cleanade grenade round": "снаряд гранаты Cleanade",
    "crayon box": "коробка мелков",
    "dark steel bordered horizontal slat tile": "тёмная стальная горизонтальная реечная плитка с окантовкой",
    "dark steel bordered vertical slat tile": "тёмная стальная вертикальная реечная плитка с окантовкой",
    "dark steel continuous slat tile": "тёмная стальная сплошная реечная плитка",
    "desert astro-sand": "пустынный астро-песок",
    "detonator cap": "капсюль-детонатор",
    "diona appearance": "внешность дионы",
    "dwarf appearance": "внешность дворфа",
    "electric crayon": "электрический мелок",
    "electrical wire brush": "электрическая проволочная щётка",
    "encrusted ironstone door": "инкрустированная железокаменная дверь",
    "energy bolt": "энергетический болт",
    "grass battlemap": "травяная боевая карта",
    "green glowstick": "зелёная светящаяся палочка",
    "green headset": "зелёная гарнитура",
    "handheld juicer": "ручная соковыжималка",
    "head of security weapon spawner": "спавнер оружия главы службы безопасности",
    "hijack beacon": "маяк захвата",
    "honkmother mitre": "митра Хонкоматери",
    "identity mask implant": "имплант маски личности",
    "incomplete handheld juicer": "незавершённая ручная соковыжималка",
    "incomplete mortar and pestle": "незавершённая ступка с пестиком",
    "infinite crayon": "бесконечный мелок",
    "iron sand concrete mono tile": "моно бетонная плитка из железного песка",
    "iron sand concrete smooth": "гладкая бетонная плитка из железного песка",
    "iron sand concrete tile": "бетонная плитка из железного песка",
    "ironsand brick wall": "стена из железопесчаного кирпича",
    "ironsand small statue": "маленькая статуя из железного песка",
    "ironsand step": "ступень из железного песка",
    "ironsand step concave corner": "вогнутый угол ступени из железного песка",
    "ironsand step convex corner": "выпуклый угол ступени из железного песка",
    "ironsand tall statue": "высокая статуя из железного песка",
    "ironstone door": "железокаменная дверь",
    "magic 9 ball": "магический шар 9",
    "magnum laser bolt": "лазерный болт магнум",
    "magnum window-piercing bolt": "пробивающий окна болт магнум",
    "mail cart": "почтовая тележка",
    "makeshift juicer": "самодельная соковыжималка",
    "makeshift mortar and pestle": "самодельная ступка с пестиком",
    "mini vial": "мини-виала",
    "mirth of the honkmother": "веселье Хонкоматери",
    "mortar and pestle": "ступка с пестиком",
    "mystery fried chicken": "загадочная жареная курица",
    "nuke ops ammo spawner": "спавнер боеприпасов ядерных оперативников",
    "nuke ops brute medkit spawner": "спавнер брут-медкита ядерных оперативников",
    "nuke ops general medkit spawner": "спавнер универсального медкита ядерных оперативников",
    "nuke ops grenade spawner": "спавнер гранат ядерных оперативников",
    "nuke ops loot spawner": "спавнер лута ядерных оперативников",
    "nuke ops weapon spawner": "спавнер оружия ядерных оперативников",
    "nutri-bâtard": "нутри-батард",
    "paper centrifuge": "бумажная центрифуга",
    "parchís": "парчис",
    "pie cannon": "пирогомёт",
    "post light": "фонарный столб",
    "press fedora": "федора прессы",
    "prison bedside table": "тюремная прикроватная тумбочка",
    "prison botanist apron": "фартук тюремного ботаника",
    "prison cook apron": "фартук тюремного повара",
    "prison doctor PDA": "КПК тюремного врача",
    "prison engineer PDA": "КПК тюремного инженера",
    "prison engineer uniform": "форма тюремного инженера",
    "prison heavy officer PDA": "КПК тяжёлого тюремного офицера",
    "prison medic uniform": "форма тюремного медика",
    "prison officer cap": "фуражка тюремного офицера",
    "prison officer PDA": "КПК тюремного офицера",
    "prison scientist PDA": "КПК тюремного учёного",
    "prison scientist uniform": "форма тюремного учёного",
    "prison senior worker uniform": "форма старшего тюремного рабочего",
    "prison trainee PDA": "КПК тюремного стажёра",
    "prison warden cap": "фуражка начальника тюрьмы",
    "prison warden PDA": "КПК начальника тюрьмы",
    "prison warden suit": "костюм начальника тюрьмы",
    "prison warden uniform": "форма начальника тюрьмы",
    "prison worker PDA": "КПК тюремного рабочего",
    "prison worker uniform": "форма тюремного рабочего",
    "prisoner PDA": "КПК заключённого",
    "prisoner uniform": "форма заключённого",
    "prying module": "модуль взлома",
    "random banana peel spawner": "спавнер случайной банановой кожуры",
    "random gate": "случайный логический элемент",
    "random rigged boxing glove spawner": "спавнер случайной подстроенной боксёрской перчатки",
    "random slip spawner": "спавнер случайной скользкой ловушки",
    "sand battlemap": "песчаная боевая карта",
    "secure weapon case": "защищённый оружейный кейс",
    "sentient slime core": "разумное ядро слайма",
    "ship battlemap": "корабельная боевая карта",
    "small cardboard box": "маленькая картонная коробка",
    "snow battlemap": "снежная боевая карта",
    "stamp box": "коробка печатей",
    "steel continuous slat tile": "стальная сплошная реечная плитка",
    "steel horizontal bordered slat tile": "стальная горизонтальная реечная плитка с окантовкой",
    "steel vertical bordered slat tile": "стальная вертикальная реечная плитка с окантовкой",
    "sticky hand": "липкая рука",
    "sticky hand palm": "ладонь липкой руки",
    "super synthesizer": "суперсинтезатор",
    "syndicate medicine duffel bag": "медицинский вещмешок Синдиката",
    "syndimov circuit kit": "набор схем Синдимова",
    "tablet of ratvar": "табличка Ратвара",
    "target station map": "карта станции-цели",
    "tennis ball": "теннисный мяч",
    "tile gun": "плиткомёт",
    "tile gun xenoborg module": "модуль ксеноборга с плиткомётом",
    "tome of nar'sie": "том Нар'Си",
    "travel camera": "походная камера",
    "utility knife": "универсальный нож",
    "vox appearance": "внешность вокса",
    "vulpkanin plushie": "плюшевый вульпканин",
    "warden weapon spawner": "спавнер оружия смотрителя",
    "white checker crown": "белая дамка",
    "white checker piece": "белая шашка",
    "white steel continuous slat tile": "белая стальная сплошная реечная плитка",
    "white steel horizontal bordered slat tile": "белая стальная горизонтальная реечная плитка с окантовкой",
    "white steel vertical bordered slat tile": "белая стальная вертикальная реечная плитка с окантовкой",
    "wrapped parcel": "упакованная посылка",
    "xenoborg jump module": "модуль прыжка ксеноборга",
}

SUFFIX_MAP: dict[str, str] = {
    '"Prison"': "Тюрьма",
    "Prison": "Тюрьма",
    "[Prison, Cracked]": "Тюрьма, Треснутый",
    "Chef Pet": "Питомец шеф-повара",
    "RD Pet": "Питомец научного руководителя",
    "CMO Pet": "Питомец главного врача",
    "HoP Pet": "Питомец главы персонала",
    "Atmos Pet": "Питомец атмосферного отдела",
    "Captain Pet": "Питомец капитана",
    "Bridge Pet (crate)": "Питомец мостика (ящик)",
    "Boxer Pet": "Питомец боксёра",
    "Warden Pet": "Питомец смотрителя",
    "Bartender Pet": "Питомец бармена",
    "Morgue Pet": "Питомец морга",
    "QM Pet": "Питомец квартирмейстера",
    "Security Pet": "Питомец службы безопасности",
    "Librarian Pet": "Питомец библиотекаря",
    "Science Pet": "Питомец научного отдела",
    "Chemistry Pet": "Питомец химика",
    "100": "100",
    "90": "90",
    "50": "50",
    "Diona": "диона",
    '"Diona, Nymphing"': "Диона, Нимфование",
    "Filled": "Заполнено",
    "Full": "Полный",
    "DO NOT MAP; 3 papers": "НЕ МАППИТЬ; 3 бумаги",
    "3 papers": "3 бумаги",
    "armed": "взведена",
    "Animal": "животное",
    "bloodsucker": "кровосос",
    "Ruminant": "жвачное",
    "slime person": "слаймолюд",
    "Arachnid": "арахнид",
    "Dwarf": "дворф",
    "gingerbread": "пряничный",
    "SkeletonPerson": "скелет",
    "Vox": "вокс",
    "Salvage Ruleset": "Правила утилизации",
    "Military O2": "Военный O2",
    "Military N2": "Военный N2",
    "Rigged": "Подстроенная",
    "Big": "Большая",
    "Battery": "Батарея",
    "Handheld, NukeOps": "Ручная, Ядерные оперативники",
    "Handheld, Works Off-Station": "Ручная, Работает вне станции",
    "Mirrored": "Отражённый",
    "EVIL/ADMEME": "ЗЛО/АДМЕМ",
    "Scrap": "Лом",
    "Gun, Empty": "Оружие, Пусто",
    "Gun, Small, Empty": "Оружие, Малое, Пусто",
    "charcoal": "уголь",
    "laughter": "смех",
    "Space Cleaner": "Космический очиститель",
    "empty": "пусто",
    "beaker": "бикер",
    "large beaker": "большой бикер",
    "no ore yield": "без руды",
    "higher ore yield": "повышенный выход руды",
    "Admin": "Админ",
    "Machine Board": "Машинная плата",
    "AI, Silicon": "ИИ, Силикон",
    "Hostile": "Враждебный",
    "Praetorian": "Преторианец",
    "borg": "борг",
    "explosive": "взрывной",
    "VODA": "ВОДА",
    '"Locked"': "Закрыто",
    "Fill, Chameleon, Syndie": "Наполнение, Хамелеон, Синди",
    "Dirty Water, Random Cistern Loot": "Грязная вода, случайный лут бачка",
    "Empty, Random Cistern Loot": "Пусто, случайный лут бачка",
    "frag": "осколочный",
    "Empty": "Пусто",
    "Syndie": "Синди",
}

DESC_MAP: dict[str, str] = {
    "Fake red sand. Imported from fake Mars.": "Поддельный красный песок. Импортирован с поддельного Марса.",
    "Fake sand, designed to be fine.": "Поддельный песок, специально сделанный мелким.",
    "A high-authority PDA belonging to the warden of Gehenna prison.": "КПК высокого допуска, принадлежащий начальнику тюрьмы Гехенна.",
    "A medical PDA for the doctor stationed at Gehenna prison.": "Медицинский КПК для врача, служащего в тюрьме Гехенна.",
    "A technical PDA for the engineer working at Gehenna prison.": "Технический КПК для инженера, работающего в тюрьме Гехенна.",
    "A reinforced PDA issued to senior Gehenna prison officers.": "Усиленный КПК, выдаваемый старшим офицерам тюрьмы Гехенна.",
    "A basic PDA given to new recruits of Gehenna prison.": "Базовый КПК, выдаваемый новобранцам тюрьмы Гехенна.",
    "A standard-issue PDA for Gehenna prison officers.": "Стандартный КПК офицера тюрьмы Гехенна.",
    "A battered, cracked PDA that's clearly seen better days inside Gehenna prison.": "Потрёпанный, треснутый КПК, явно переживший свои лучшие дни в тюрьме Гехенна.",
    "A locked-down PDA issued to inmates at Gehenna prison.": "Заблокированный КПК, выдаваемый заключённым тюрьмы Гехенна.",
    "A research PDA for the scientist assigned to Gehenna prison.": "Исследовательский КПК для учёного, прикреплённого к тюрьме Гехенна.",
    "A rugged PDA issued to Gehenna prison staff.": "Прочный КПК, выдаваемый персоналу тюрьмы Гехенна.",
    "A reinforced uniform worn by senior staff of Gehenna prison.": "Усиленная форма, которую носит старший персонал тюрьмы Гехенна.",
    "A formal uniform worn by the warden of Gehenna prison.": "Парадная форма, которую носит начальник тюрьмы Гехенна.",
    "A business suit tailored for the warden of Gehenna prison.": "Деловой костюм, пошитый для начальника тюрьмы Гехенна.",
    "A durable uniform for Gehenna prison workers. It smells of industrial cleaner.": "Прочная форма для работников тюрьмы Гехенна. Пахнет промышленным очистителем.",
    "An alternate durable uniform for Gehenna prison workers.": "Альтернативная прочная форма для работников тюрьмы Гехенна.",
    "A worn uniform issued to inmates of Gehenna prison.": "Поношенная форма, выдаваемая заключённым тюрьмы Гехенна.",
    "Used for juicing small amounts of objects.": "Используется для выжимания сока из небольших предметов.",
    "Used for juicing small amounts of objects. Inferior version made out of wood.": "Используется для выжимания сока из небольших предметов. Упрощённая версия, сделанная из дерева.",
    "A some wood and plastic stuck together.": "Немного дерева и пластика, кое-как скреплённых вместе.",
    "A few planks of wood stuck together.": "Несколько досок, кое-как скреплённых вместе.",
    "Used for grinding small amounts of objects.": "Используется для измельчения небольших предметов.",
    "Used for grinding small amounts of objects. Inferior version made out of wood.": "Используется для измельчения небольших предметов. Упрощённая версия, сделанная из дерева.",
    "A mysterious statue found in a desert of iron sand.": "Таинственная статуя, найденная в пустыне железного песка.",
    "Opens the store": "Открывает магазин.",
    "Spray water towards your enemies.": "Брызгает водой в сторону ваших врагов.",
    "Use your agile legs to leap a short distance. Be careful not to bump into anything!": "Используйте свои ловкие ноги, чтобы прыгнуть на короткое расстояние. Смотрите, не врежьтесь ни во что!",
    "Advanced medical module containing the cyborg adaptation of the highly coveted hypospray. Now your cyborgs can inject crew-harmers with chloral hydrate even faster!": "Продвинутый медицинский модуль, содержащий киборг-адаптацию крайне желанного гипоспрея. Теперь ваши киборги смогут ещё быстрее колоть вредителям команды хлоралгидрат!",
    "A universal cyborg module which allows the unit to pry open doors.": "Универсальный модуль киборга, позволяющий взламывать двери.",
    "Module that allows a xenoborg to jump forward.": "Модуль, позволяющий ксеноборгу прыгать вперёд.",
    "Module with a tile gun. wait, a what?": "Модуль с плиткомётом. Подождите, с чем?",
}

ID_NAME_MAP: dict[str, str] = {
    "*BackgammonBoard": "нарды",
    "*ChessBoard": "шахматная доска",
    "*checkerboard": "шашечная доска",
    "AppearanceGingerbread": "внешность пряничного человечка",
    "AppearanceSkeletonPerson": "внешность скелета",
    "BarSignEmped": "глючная барная вывеска",
    "BasePhotograph": "фотография",
    "BaseSoap": "мыло",
    "BloodSmoke": "дым",
    "ChemistryEmptyVial": "виала",
    "ComputerNukieDelivery": "компьютер доставки Синдиката",
    "GeneratorWallmountAPULV": "шаттловый ВСПА LV",
    "MagazineLightRifleBox": "коробка с лентой L6 SAW (.30 винтовочные)",
    "MindBase": "разум",
    "NinjaPDA": "КПК ниндзя",
    "OrganSlimePersonLungs": "газовые мешки",
    "PosterLegitBotanyFear": "Страх гидропоники",
    "PrisonBossPDA": "КПК начальника тюрьмы",
    "PrisonDoctorPDA": "КПК тюремного врача",
    "PrisonEngineerPDA": "КПК тюремного инженера",
    "PrisonHeavyOfficerPDA": "КПК тяжёлого тюремного офицера",
    "PrisonNewbiePDA": "КПК тюремного стажёра",
    "PrisonOfficerPDA": "КПК тюремного офицера",
    "PrisonPrisoneerCrackedPDA": "КПК заключённого",
    "PrisonPrisoneerPDA": "КПК заключённого",
    "PrisonScientistPDA": "КПК тюремного учёного",
    "PrisonWorkerPDA": "КПК тюремного рабочего",
}

ID_SUFFIX_MAP: dict[str, str] = {
    "BoxFolderFill": "Заполнено",
    "BoxFolderFillThreePapers": "3 бумаги",
    "ChameleonAgentPDA": "Хамелеон, ID агента",
    "MobLuminousEntitySalvage": "Правила утилизации",
    "MobLuminousObjectSalvage": "Правила утилизации",
    "MobLuminousPersonSalvage": "Правила утилизации",
    "OrganBloodsucker": "кровосос",
    "PrisonPrisoneerCrackedPDA": "Тюрьма, Треснутый",
}

ID_DESC_MAP: dict[str, str] = {
    "ActionChangelingStore": "Открывает магазин способностей.",
    "ActionXenoborgCameraMonitor": "Открывает монитор камер ксеноборгов.",
    "ActionXenoborgControlMonitor": "Открывает консоль управления ксеноборгами.",
    "AnomalyScannerAdmin": "Ручной сканер, созданный для сбора информации о различных аномальных объектах. У этого, похоже, есть несколько дополнительных функций.",
    "BarSignEmped": "Кажется, хороший удар мог бы это исправить.",
    "BaseRepairableShield": "Ремонтируемый щит!",
    "BibleHonk": "\"О великая и славная Мать, владычица веселья, покровительница масок и развлечений, благословенна ты среди нас, шутов.\"",
    "BibleNarsie": "\"Что вообще может пойти не так с книгой, покрытой кровью?\"",
    "BibleRatvar": "\"Священная реликвия Часового культа, благословлённая Часовым Правосудием, Ратваром.\"",
    "BonfireStake": "Зловещий костёр с колом для... церемониальных целей. Лучше не спрашивать.",
    "BoxCardboardSmall": "Небольшая картонная коробка для хранения вещей.",
    "BoxStamps": "Небольшая коробка с печатями.",
    "BoxSurvivalMilitaryDouble": "Это коробка с базовым набором для дыхания. На этой указано, что внутри двойной баллон увеличенной ёмкости.",
    "ChameleonProjectorNoBattery": "Технология, похожая на голопаразитическую, которая позволяет создать из твёрдого света копию любого объекта рядом с вами. Маскировка разрушается, если объект поднять или отключить.",
    "ChemistryEmptyVial": "Небольшая виала.",
    "ChemistryEmptyVialSmall": "Совсем маленькая виала.",
    "ChestDrawerPrisonGehenna": "Небольшая прикроватная тумбочка, встречающаяся в камерах тюрьмы Гехенна.",
    "ClothingBackpackDuffelSyndicateFilledMedicine": "Большой вещмешок, содержащий необходимые медицинские реагенты.",
    "ClothingHeadHatFedoraPress": "На ленте приклеена маленькая записка с надписью \"PRESS\". Практически пропуск всюду!",
    "ClothingHeadHatMitreClown": "Прихожанам трудно заметить банановую кожуру на полу, когда они смотрят на ваш славный головной убор.",
    "ClothingHeadHatPrisonBoss": "Отличительная фуражка, которую носит начальник тюрьмы Гехенна.",
    "ClothingHeadHatPrisonWorker": "Прочная фуражка, которую носят офицеры тюрьмы Гехенна.",
    "ClothingHeadsetNinja": "Кто смог бы отказаться от такой стильной чёрно-зелёной гарнитуры?",
    "ClothingOuterApronPrisonBotanist": "Прочный фартук, который носит ботаник, работающий в теплице тюрьмы Гехенна.",
    "ClothingOuterApronPrisonChief": "Фартук, который носит повар, работающий на кухне тюрьмы Гехенна.",
    "ClothingOuterApronPrisonFactory": "Тяжёлый фартук, который носят заключённые, работающие на фабрике тюрьмы Гехенна.",
    "ClothingOuterCoatPrisonBoss": "Отличительное бронированное пальто, которое носит начальник тюрьмы Гехенна. Достаточно прочное, чтобы пережить любые проблемы со стороны заключённых.",
    "ClothingUniformJumpsuitPrisonCitizenEngineer": "Рабочая форма для инженера, служащего в тюрьме Гехенна.",
    "ClothingUniformJumpsuitPrisonCitizenMedic": "Медицинская форма для врача, прикреплённого к тюрьме Гехенна.",
    "ClothingUniformJumpsuitPrisonCitizenScientist": "Исследовательская форма для учёного, прикреплённого к тюрьме Гехенна.",
    "ComputerNukieDelivery": "Компьютер, способный блюспейс-доставкой получать определённое снаряжение для ядерных операций. Плата интегрирована в корпус и не может быть извлечена при разборке.",
    "CrayonBorg": "Предположительно, самый вкусный вид мелка во всех вселенных; к сожалению, вы не можете его съесть.",
    "CrayonBoxEmpty": "Это коробка мелков.",
    "CrayonInedible": "Цветной мелок. Запретный вкус.",
    "EnergyCrossbowBolt": "Будет больно.",
    "EvilBeachBall": "Кто-то несмываемыми чернилами нарисовал на боку этого пляжного мяча \">:3c\".",
    "FoodBreadNutriBatard": "бон аппетит!",
    "FoodMeatChickenFriedVox": "\"Одиннадцать секретных трав и... о нет. Это не курица.\"",
    "GeneratorWallmountAPULV": "Продвинутый вспомогательный силовой агрегат для шаттла.",
    "HandheldMixerPaperCentrifuge": "Небольшая портативная самодельная центрифуга. Работает, вращая бумажные листы, когда тянут за шнуры.",
    "HandheldStationMapNukeops": "Показывает данные о целевой станции.",
    "HijackBeacon": "Устройство, обходящее межсетевой экран автоматизированных торговых станций бренда NanoTrasen.",
    "HijackTradeStationObjective": "Ваш аплинк получил авторизацию на один маяк захвата. Разместите его на автоматизированной торговой станции и защищайте, пока он захватывает её.",
    "HoloprojectorBorg": "Модифицированный проектор голографических знаков для уборочных киборгов. Автоматически перезаряжается.",
    "IronsandStep": "Поднимает ваш железный песок на уровень выше.",
    "IronstoneDoor": "Таинственная дверь из камня, покрытого рунами.",
    "EncrustedIronstoneDoor": "Каменная дверь, покрытая перламутровыми сгустками неизвестного вещества.",
    "KillStationAiObjective": "NanoTrasen с гордостью хвастается своим передовым искусственным интеллектом. Напомните им, что это всего лишь ещё одна игрушка, которую можно сломать.",
    "LockerPrisonDoctor": "Медицинский шкафчик, закреплённый за врачом, служащим в тюрьме Гехенна.",
    "LockerPrisonEngineer": "Технический шкафчик, закреплённый за инженером, работающим в тюрьме Гехенна.",
    "LockerPrisonGehenna": "Надёжный шкафчик для хранения личных вещей заключённых.",
    "LockerPrisonHoS": "Личный шкафчик начальника тюрьмы Гехенна.",
    "MagazineLightRifleBox": "Коробка, содержащая ленту из 100 связанных патронов .30 калибра, используемую лёгкими пулемётами вроде L6. Предназначена для обычных кинетических боеприпасов.",
    "Magic9Ball": "Бесконечный источник мудрости... Теперь со встроенным динамиком!",
    "MailCart": "Доставляйте посылки стильно и эффективно.",
    "NinjaPDA": "Ну и скрытный же ублюдок!",
    "PlushieVulp": "Очаровательная мягкая игрушка, напоминающая вульпканина. Ип! Яп!",
    "PosterLegitBotanyFear": "Трижды подумайте, прежде чем открывать шлюз в гидропонику: там может скрываться красная угроза.",
    "RandomGate": "Логический элемент, выдающий случайный сигнал при изменении входа.",
    "SupercritAnomaliesObjective": "NanoTrasen очень интересуют аномалии с потенциально катастрофическими последствиями. Познакомьте их с огнём, с которым они играют.",
    "SyndimovCircuitKit": "Набор, содержащий электронную плату с набором законов Синдимова и удостоверение Синдиката.",
    "TennisBall": "Пушистый шар бесконечного предательства.",
    "TravelCamera": "Снимок говорит больше тысячи слов. Оснащена сверхяркой вспышкой и внутренним перезаряжаемым запасом фотобумаги.",
    "UtilityKnife": "Нож с маленьким выдвижным лезвием. Полезен как резак для коробок, вскрыватель писем и не только.",
    "VoiceMaskImplant": "Этот имплант позволяет менять личность по своему желанию.",
    "WallIronsandCobblebrick": "Бледные округлые формы, составляющие эту стену, выглядят разительно иначе, чем железные пески, из которых она якобы сделана.",
    "WeaponRifleLecterXL8": "Экспериментальный Lecter 8. Неоправданно дорогая штурмовая винтовка военного класса со встроенной оптикой. Использует патроны .20 винтовочные.",
    "WeaponStickyHand": "Они говорят, что ты слишком далеко тянешься. Говорят, что это уже перебор. Ты всем им докажешь и хорошенько к ним прилипнешь.",
    "WeaponSubMachineGunC20rROW": "Пистолет-пулемёт C-20r с очередями для использования киборгами. Создаёт патроны .35 калибра на лету из внутреннего фабрикатора боеприпасов, который медленно самозаряжается.",
    "WeaponTileGun": "Странное оружие, стреляющее плитками. Застрели их полом!",
    "WireBrushElectrical": "Жёсткая стальная проволочная щётка с подвижной головкой, заметно упрощающая чистку.",
    "WiredDetonator": "Капсюль-детонатор.",
    "WrappedParcelHumanoid": "Что-то, завёрнутое в бумагу. Подозрительно похоже на человека.",
    "XenoArtifactHostileFaunaSpawn": "Создаёт враждебную фауну",
}


def strip_comment(value: str) -> str:
    out = []
    in_single = False
    in_double = False
    i = 0
    while i < len(value):
        ch = value[i]
        if ch == "'" and not in_double:
            in_single = not in_single
        elif ch == '"' and not in_single:
            in_double = not in_double
        elif ch == "#" and not in_single and not in_double:
            prev = value[i - 1] if i > 0 else ""
            if prev.isspace() or i == 0:
                break
        out.append(ch)
        i += 1
    return "".join(out).rstrip()


def norm_scalar(value: str | None) -> str | None:
    if value is None:
        return None
    stripped = value.strip()
    if stripped in ("", '""', "''", "false", "False", "null", "Null", "~"):
        return None
    return stripped


def is_loc_key(value: str) -> bool:
    return bool(LOC_KEY_PAT.match(value)) and (value.count("-") >= 2 or "." in value)


class Block:
    def __init__(self, ent_id: str, sections: list[tuple[str, list[str]]]):
        self.id = ent_id
        self.sections = sections

    @classmethod
    def from_lines(cls, ent_id: str, lines: list[str]) -> "Block":
        sections: list[tuple[str, list[str]]] = []
        current = "__name__"
        current_lines = [lines[0]]
        for line in lines[1:]:
            match = ATTR_PAT.match(line)
            if match:
                sections.append((current, current_lines))
                current = match.group(1)
                current_lines = [line]
            else:
                current_lines.append(line)
        sections.append((current, current_lines))
        return cls(ent_id, sections)

    def set_name(self, value: str) -> None:
        if self.sections and self.sections[0][0] == "__name__":
            self.sections[0] = ("__name__", [f"ent-{self.id} = {value}"])
        else:
            self.sections.insert(0, ("__name__", [f"ent-{self.id} = {value}"]))

    def set_attr(self, key: str, value: str) -> None:
        for idx, (section_key, _) in enumerate(self.sections):
            if section_key == key:
                self.sections[idx] = (key, [f"    .{key} = {value}"])
                return
        self.sections.append((key, [f"    .{key} = {value}"]))

    def render(self) -> str:
        output: list[str] = []
        for _, lines in self.sections:
            output.extend(lines)
        return "\n".join(output)


def load_parents() -> dict[str, str | None]:
    parents: dict[str, str | None] = {}
    for path in sorted(PROTO_ROOT.rglob("*.yml")):
        lines = path.read_text(encoding="utf-8").splitlines()
        i = 0
        while i < len(lines):
            if not ENTITY_START.match(lines[i].strip()):
                i += 1
                continue
            block: list[str] = []
            i += 1
            while i < len(lines) and not lines[i].startswith("- type: "):
                block.append(lines[i])
                i += 1
            ent_id = None
            parent = None
            j = 0
            while j < len(block):
                line = block[j]
                match = re.match(r"^  id:\s*(.+?)\s*$", line)
                if match:
                    ent_id = norm_scalar(strip_comment(match.group(1)))
                    j += 1
                    continue
                match = re.match(r"^  parent:\s*(.*?)\s*$", line)
                if match:
                    value = strip_comment(match.group(1))
                    if value:
                        if value.startswith("["):
                            values = [norm_scalar(strip_comment(part.strip())) for part in value.strip("[]").split(",") if part.strip()]
                            parent = values[0] if values else None
                        else:
                            parent = norm_scalar(value)
                    else:
                        values = []
                        k = j + 1
                        while k < len(block):
                            child = re.match(r"^  -\s*(.+?)\s*$", block[k])
                            if not child:
                                break
                            values.append(norm_scalar(strip_comment(child.group(1))))
                            k += 1
                        parent = values[0] if values else None
                j += 1
            if ent_id:
                parents[ent_id] = parent
    return parents


def load_locale_presence() -> dict[str, dict[str, bool]]:
    presence: dict[str, dict[str, bool]] = {}
    locale_root = ROOT / "Resources" / "Locale" / "ru-RU"
    for path in locale_root.rglob("*.ftl"):
        current = None
        for raw in path.read_text(encoding="utf-8").splitlines():
            line = raw.lstrip("\ufeff")
            match = KEY_PAT.match(line)
            if match:
                current = match.group(1)
                presence.setdefault(current, {})["name"] = True
                continue
            match = ATTR_PAT.match(line)
            if match and current:
                presence.setdefault(current, {})[match.group(1)] = True
    return presence


def get_file_state(cache: dict[pathlib.Path, dict], path: pathlib.Path) -> dict:
    state = cache.get(path)
    if state is not None:
        return state
    blocks: list[Block] = []
    block_map: dict[str, Block] = {}
    if path.exists():
        lines = path.read_text(encoding="utf-8").splitlines()
        i = 0
        while i < len(lines):
            match = KEY_PAT.match(lines[i].lstrip("\ufeff"))
            if not match:
                i += 1
                continue
            ent_id = match.group(1)
            block_lines = [lines[i].lstrip("\ufeff")]
            i += 1
            while i < len(lines) and not KEY_PAT.match(lines[i].lstrip("\ufeff")):
                block_lines.append(lines[i].lstrip("\ufeff"))
                i += 1
            while block_lines and block_lines[-1] == "":
                block_lines.pop()
            block = Block.from_lines(ent_id, block_lines)
            blocks.append(block)
            block_map[ent_id] = block
    state = {"blocks": blocks, "block_map": block_map}
    cache[path] = state
    return state


def main() -> None:
    report = json.loads(REPORT.read_text(encoding="utf-8"))["remaining_ru"]
    parents = load_parents()
    presence = load_locale_presence()
    file_cache: dict[pathlib.Path, dict] = {}
    resolved_fields = 0

    for item in report:
        ent_id = item["id"]
        if ent_id.startswith("*"):
            continue
        have = presence.get(ent_id, {})
        if item["file"].startswith("_Gehenna/Prison/"):
            target = ROOT / "Resources" / "Locale" / "ru-RU" / "_Gehenna" / "Prison" / "entities.ftl"
        else:
            target = RU_ROOT / item["file"].lower().replace(".yml", ".ftl")
        state = get_file_state(file_cache, target)
        block = state["block_map"].get(ent_id)

        name_value = None
        desc_value = None
        suffix_value = None

        if item["name"] and not have.get("name"):
            name_value = ID_NAME_MAP.get(ent_id) or NAME_MAP.get(item["name"])
            if name_value is None and is_loc_key(item["name"]):
                name_value = f"{{ {item['name']} }}"
        elif not item["name"] and not have.get("name"):
            parent = parents.get(ent_id)
            if parent:
                name_value = f"{{ ent-{parent} }}"

        if item["suffix"] and not have.get("suffix"):
            suffix_value = ID_SUFFIX_MAP.get(ent_id) or SUFFIX_MAP.get(item["suffix"])
            if suffix_value is None and item["suffix"].isdigit():
                suffix_value = item["suffix"]

        if item["desc"] and not have.get("desc"):
            desc_value = ID_DESC_MAP.get(ent_id) or DESC_MAP.get(item["desc"])
            if desc_value is None and is_loc_key(item["desc"]):
                desc_value = f"{{ {item['desc']} }}"

        if block is None and name_value is None and (suffix_value or desc_value):
            parent = parents.get(ent_id)
            if parent:
                name_value = f"{{ ent-{parent} }}"
            else:
                name_value = '{ "" }'

        if block is None and name_value is not None:
            block = Block(ent_id, [("__name__", [f"ent-{ent_id} = {name_value}"])])
            state["blocks"].append(block)
            state["block_map"][ent_id] = block
            presence.setdefault(ent_id, {})["name"] = True
            resolved_fields += 1

        if block is None:
            continue

        if name_value is not None and not have.get("name"):
            block.set_name(name_value)
            presence.setdefault(ent_id, {})["name"] = True
            resolved_fields += 1

        if desc_value is not None and not have.get("desc"):
            block.set_attr("desc", desc_value)
            presence.setdefault(ent_id, {})["desc"] = True
            resolved_fields += 1

        if suffix_value is not None and not have.get("suffix"):
            block.set_attr("suffix", suffix_value)
            presence.setdefault(ent_id, {})["suffix"] = True
            resolved_fields += 1

    for path, state in file_cache.items():
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text("\n\n".join(block.render() for block in state["blocks"]).rstrip() + "\n", encoding="utf-8")

    print(f"updated_files={len(file_cache)}")
    print(f"resolved_fields={resolved_fields}")


if __name__ == "__main__":
    main()
