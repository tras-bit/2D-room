(() => {
  'use strict';

  const STORAGE_KEY = 'subsistence-brief-v2-50';
  const LEGACY_KEY = 'subsistence-brief-v1';
  const sections = [
    { id: 'section-core', title: 'Основа игры', kicker: 'ЗАДАЧА И ИГРОК', first: 1, last: 5 },
    { id: 'section-survival', title: 'Выживание', kicker: 'ЦИКЛ И ПРАВИЛА', first: 6, last: 10 },
    { id: 'section-world', title: 'Мир и история', kicker: 'МЕСТО И ТАЙНЫ', first: 11, last: 15 },
    { id: 'section-growth', title: 'Развитие и первая версия', kicker: 'ПРОГРЕСС И ПРИОРИТЕТ', first: 16, last: 20 },
    { id: 'section-rules', title: 'Детали мира', kicker: 'СРЕДА И ИССЛЕДОВАНИЕ', first: 21, last: 26 },
    { id: 'section-threats', title: 'Опасности и добыча', kicker: 'ВРАГИ · СНАРЯЖЕНИЕ · РЕСУРСЫ', first: 27, last: 32 },
    { id: 'section-people', title: 'Персонажи и взаимодействие', kicker: 'NPC · ИГРОКИ · СОБЫТИЯ', first: 33, last: 38 },
    { id: 'section-presentation', title: 'Подача и удобство', kicker: 'ВИЗУАЛ · ЗВУК · ИНТЕРФЕЙС', first: 39, last: 44 },
    { id: 'section-production', title: 'Производство', kicker: 'ОБЪЁМ · ПЛАН · РЕШЕНИЯ', first: 45, last: 50 }
  ];

  // The first twenty prompts preserve the archived brief's order and topic names,
  // which lets a saved 20-question draft migrate without moving its answers.
  const questions = [
    { n: 1, s: 0, area: 'Название и короткий питч', title: 'Как называется игра и как описать её одной-двумя фразами?', hint: 'Представь, что рассказываешь о ней человеку, который никогда не видел проект.', prompt: 'Название и короткий питч', chips: ['SUBSISTENCE', 'выживание в Backrooms', 'одинокий путь домой'] },
    { n: 2, s: 0, area: 'Платформы', title: 'На каких устройствах и платформах должна запускаться игра?', hint: 'Выбери главную платформу первой. Остальные можно добавить позже.', prompt: 'Например: Windows PC, Steam Deck, контроллер.', chips: ['Windows PC', 'Steam', 'Steam Deck', 'геймпад'] },
    { n: 3, s: 0, area: 'Вид и камера', title: 'Какой ракурс и ощущение камеры нужны в игре?', hint: 'Уже принято: игра остаётся в 2D, боковой вид. Опиши масштаб персонажа и видимость коридора.', prompt: 'Например: фиксированный боковой ракурс, персонаж целиком в кадре.', chips: ['боковой 2D-вид', 'камера следует за игроком', 'пиксель-перфект'] },
    { n: 4, s: 0, area: 'Одиночная игра / мультиплеер', title: 'Это одиночное приключение, кооператив или несколько режимов?', hint: 'Можно решить позже, но важно понять, рассчитываем ли мы на других игроков.', prompt: 'Опиши основной режим и желаемое число игроков.', chips: ['одиночная игра', 'кооператив', 'сначала одиночная'] },
    { n: 5, s: 0, area: 'Роль игрока и завязка', title: 'Кем является игрок и что заставляет его идти вперёд?', hint: 'Короткая завязка может объяснить цель, не раскрывая все тайны.', prompt: 'Кто герой? Чего он хочет добиться?', chips: ['выживший', 'ищет выход', 'ищет пропавшего', 'ищет убежище'] },

    { n: 6, s: 1, area: 'Ресурсы и показатели выживания', title: 'Какие ресурсы и потребности должен отслеживать игрок?', hint: 'Необязательно добавлять голод и жажду только потому, что это survival-игра.', prompt: 'Назови обязательные ресурсы, показатели и то, что можно убрать.', chips: ['здоровье', 'вода', 'еда', 'выносливость', 'патроны'] },
    { n: 7, s: 1, area: 'Основной игровой цикл', title: 'Как выглядит обычный игровой цикл от минуты к минуте?', hint: 'Например: исследовать → найти ресурсы → пережить угрозу → вернуться в безопасное место.', prompt: 'Составь последовательность из 3–6 действий.', chips: ['исследование', 'сбор ресурсов', 'опасность', 'возвращение', 'подготовка'] },
    { n: 8, s: 1, area: 'Крафт и строительство', title: 'Что игрок сможет создавать и насколько свободно строить?', hint: 'Раздели вещи на простые рецепты, станки и постройки, если это важно.', prompt: 'Что крафтится из инвентаря, а что требует верстака или места?', chips: ['инструменты', 'лечение', 'оружие', 'укрытие', 'ограниченное строительство'] },
    { n: 9, s: 1, area: 'Бой и опасности', title: 'Как игрок должен справляться с прямыми угрозами?', hint: 'Бой, скрытность, бегство и отвлечение могут сосуществовать, но задают разный темп.', prompt: 'Что чаще всего разумно: драться, прятаться или убегать?', chips: ['бой редкий', 'скрытность', 'бегство', 'ресурсы ограничены'] },
    { n: 10, s: 1, area: 'Смерть и сохранения', title: 'Что происходит при смерти и как работает сохранение?', hint: 'Реши, теряется ли добыча, откуда игрок продолжает и насколько сурово наказание.', prompt: 'Опиши контрольные точки, потерю инвентаря и повторный вход.', chips: ['сохранение в убежище', 'частичная потеря вещей', 'контрольные точки'] },

    { n: 11, s: 2, area: 'Место действия', title: 'Где происходит игра и как связаны локации?', hint: 'В проекте уже есть Level 0: The Lobby, редкая Manila Room и путь на Level 1.', prompt: 'Какие места игрок увидит и как между ними перемещается?', chips: ['Level 0: The Lobby', 'Manila Room', 'Level 1', 'лифты'] },
    { n: 12, s: 2, area: 'Визуальный стиль', title: 'Как должен выглядеть мир, кроме выбранного пиксель-арта?', hint: 'Персонаж из референса задаёт качество детализации; его фон не является стилевым ориентиром.', prompt: 'Опиши палитру, контраст, материалы и настроение окружения.', chips: ['детальный 2D пиксель-арт', 'тёплая грязная охра', 'холодный индустриальный серый', 'сильные контуры'] },
    { n: 13, s: 2, area: 'Атмосфера и эмоции', title: 'Какие эмоции должны чаще всего испытывать игроки?', hint: 'Одиночество, напряжение, любопытство, краткое облегчение в безопасном месте?', prompt: 'Назови 2–4 основных ощущения и когда игрок может выдохнуть.', chips: ['тревога', 'исследовательское любопытство', 'одиночество', 'редкое облегчение'] },
    { n: 14, s: 2, area: 'Противники и существа', title: 'Какие существа или противники обитают в мире?', hint: 'Важно не только как они выглядят, но и как игрок распознаёт их поведение.', prompt: 'Опиши внешний вид, цель, поведение и отличие угроз.', chips: ['Наблюдатель', 'другие выжившие', 'разные паттерны поведения'] },
    { n: 15, s: 2, area: 'Тайны и правила мира', title: 'Есть ли у мира необычные правила или тайны?', hint: 'Ложные проходы, меняющаяся геометрия, сигналы, опасные зоны, нестыковки пространства.', prompt: 'Какая особенность отличает этот мир от обычного постапокалипсиса?', chips: ['ложные проходы', 'странные переходы', 'скрытые комнаты', 'непонятные сигналы'] },

    { n: 16, s: 3, area: 'Прогресс и развитие', title: 'Как игрок продвигается и становится сильнее?', hint: 'Новые инструменты, рецепты, безопасные зоны, знания или улучшения снаряжения.', prompt: 'Что открывается со временем и как игрок понимает свой прогресс?', chips: ['новые рецепты', 'лучшее снаряжение', 'доступ к уровням', 'знание мира'] },
    { n: 17, s: 3, area: 'Структура мира', title: 'Мир будет открытым, уровневым или процедурно генерируемым?', hint: 'Можно сочетать вручную сделанные важные места с повторяемыми маршрутами.', prompt: 'Как устроены карта, переходы и повторные прохождения?', chips: ['связанные уровни', 'ручные локации', 'вариативные маршруты'] },
    { n: 18, s: 3, area: 'Звук и музыка', title: 'Какими должны быть звук и музыка?', hint: 'Гул ламп, далёкие шаги, радио с помехами и тишина могут быть важнее постоянной музыки.', prompt: 'Что слышно постоянно, а что должно настораживать или пугать?', chips: ['гул флуоресцентных ламп', 'редкая музыка', 'дальние шаги', 'радиопомехи'] },
    { n: 19, s: 3, area: 'Управление и интерфейс', title: 'Насколько сложное управление и интерфейс ты хочешь?', hint: 'Учитывай клавиатуру, геймпад, быстрые слоты, инвентарь и объём экранных подсказок.', prompt: 'Что всегда видно на экране, а что открывается по запросу?', chips: ['минимальный HUD', 'инвентарь по Tab', 'быстрые слоты', 'поддержка геймпада'] },
    { n: 20, s: 3, area: 'Первая версия / приоритет', title: 'Что обязательно должно войти в первую играбельную версию?', hint: 'Выбери самый маленький, но уже интересный прототип.', prompt: 'Назови несколько обязательных систем и главный критерий готовности.', chips: ['одна законченная локация', 'выживание', 'фонарь', 'один противник', 'сохранение'] },

    { n: 21, s: 4, area: 'Маршрут и масштаб', title: 'Какой длины должно быть первое прохождение и сколько оно длится?', hint: 'Оцени отдельно короткий тестовый маршрут и желаемую полную игру.', prompt: 'Например: 20 минут на прототип, 3–5 часов на сюжет.', chips: ['короткая сессия', 'несколько часов', 'длинное исследование'] },
    { n: 22, s: 4, area: 'Зоны и контраст локаций', title: 'Чем уровни и зоны должны отличаться друг от друга?', hint: 'Меняй не только цвет: звук, ширину коридора, опасности и материалы.', prompt: 'Опиши 2–4 зоны и их главное отличие.', chips: ['жёлтые офисные коридоры', 'технические тоннели', 'тихая безопасная комната'] },
    { n: 23, s: 4, area: 'Время и освещение', title: 'Есть ли смена времени, света или состояния локации?', hint: 'Постоянная ночь и мерцающие лампы могут быть выразительнее обычного дня и ночи.', prompt: 'Что меняется со временем, а что остаётся неизменным?', chips: ['постоянный искусственный свет', 'мерцание', 'зоны полной темноты', 'без смены суток'] },
    { n: 24, s: 4, area: 'Безопасность и отдых', title: 'Где можно передохнуть и что делает место безопасным?', hint: 'Укрытие может восстанавливать ресурсы, хранить предметы и давать ощущение передышки.', prompt: 'Можно ли обустроить убежище? Как угрозы влияют на него?', chips: ['торговец в Manila Room', 'безопасное убежище', 'ограниченный запас ресурсов'] },
    { n: 25, s: 4, area: 'Исследование и ориентирование', title: 'Как игрок понимает, куда идти, не получая слишком много подсказок?', hint: 'Навигация может опираться на окружение, заметки, звук, карту или запоминающиеся детали.', prompt: 'Какие ориентиры, карты и подсказки доступны?', chips: ['визуальные ориентиры', 'заметки', 'карта вручную', 'редкие указатели'] },
    { n: 26, s: 4, area: 'Награды за риск', title: 'Что игрок получает за опасное исследование?', hint: 'Редкий лут, короткий путь, сведения о мире или доступ к новой зоне.', prompt: 'Как игра показывает, что риск стоил того?', chips: ['редкие материалы', 'новая история', 'короткий путь', 'доступ к тайнику'] },

    { n: 27, s: 5, area: 'Поведение врагов', title: 'Как противники обнаруживают и преследуют игрока?', hint: 'Зрение, шум, свет фонаря, запах, следы или заранее заданные патрули.', prompt: 'Что игрок может понять и использовать против каждого типа врага?', chips: ['реагируют на шум', 'видят свет фонаря', 'патрулируют', 'теряют след'] },
    { n: 28, s: 5, area: 'Уровень угрозы', title: 'Как меняется опасность от зоны к зоне?', hint: 'Можно усиливать давление, не просто повышая здоровье и урон врагов.', prompt: 'Какие новые угрозы появляются дальше и как игрок к ним готовится?', chips: ['меньше света', 'больше звуковых ловушек', 'новые враги', 'ресурсы реже'] },
    { n: 29, s: 5, area: 'Оружие и самооборона', title: 'Какое оружие и инструменты самообороны подходят миру?', hint: 'Оружие может быть шумным, редким и ненадёжным; уклонение тоже вариант.', prompt: 'Перечисли желаемые средства защиты и ограничения.', chips: ['самодельное оружие', 'огнестрельное редкое', 'фонарь', 'отвлечение'] },
    { n: 30, s: 5, area: 'Ранения и восстановление', title: 'Какие травмы и способы восстановления нужны?', hint: 'Выбери между простой полоской здоровья и последствиями, которые меняют тактику.', prompt: 'Что можно вылечить, а что влияет на движения или обзор?', chips: ['бинты', 'аптечки', 'временная слабость', 'простое здоровье'] },
    { n: 31, s: 5, area: 'Инвентарь и добыча', title: 'Как устроены переноска вещей, вес и редкость добычи?', hint: 'Сетка, слоты и вес дают разные решения по управлению ресурсами.', prompt: 'Что ограничивает инвентарь и какие предметы должны быть редкими?', chips: ['слоты', 'ограничение веса', 'быстрый пояс', 'редкие предметы'] },
    { n: 32, s: 5, area: 'Экипировка', title: 'Какие части экипировки можно менять и что они дают?', hint: 'Одежда может влиять на защиту, шум, тепло, вместимость и внешний вид.', prompt: 'Назови слоты и эффекты экипировки.', chips: ['куртка', 'штаны', 'ботинки', 'шлем', 'рюкзак', 'бронежилет'] },

    { n: 33, s: 6, area: 'Персонажи и торговец', title: 'Какие важные персонажи встречаются игроку?', hint: 'Уже есть торговец в Manila Room; он может быть полезным, подозрительным или нейтральным.', prompt: 'Кто помогает, чего хочет и как игрок с ним взаимодействует?', chips: ['торговец', 'проводник', 'другие выжившие', 'без постоянных NPC'] },
    { n: 34, s: 6, area: 'Сюжетная подача', title: 'Как рассказывать историю — напрямую или через окружение?', hint: 'Записки, предметы, короткие диалоги и перемены мира могут поддерживать исследование.', prompt: 'Какие формы повествования тебе ближе и сколько текста допустимо?', chips: ['история через окружение', 'короткие записки', 'диалоги', 'почти без текста'] },
    { n: 35, s: 6, area: 'Совместная игра', title: 'Если будет кооператив, что игрокам можно делать вместе?', hint: 'Этот вопрос можно оставить на будущее, если сначала строим одиночную игру.', prompt: 'Как делятся добыча, задачи, риск и прогресс между игроками?', chips: ['помощь в бою', 'совместный лут', 'общая база', 'пока без кооператива'] },
    { n: 36, s: 6, area: 'Коммуникация и доверие', title: 'Как игроки общаются и есть ли место недоверию?', hint: 'Голосовой чат может влиять на атмосферу; альтернативой будут пинги и жесты.', prompt: 'Какие инструменты общения нужны и могут ли игроки мешать друг другу?', chips: ['голосовой чат', 'пинги', 'без PvP', 'доверие важно'] },
    { n: 37, s: 6, area: 'Случайные события', title: 'Нужны ли случайные события и насколько часто они происходят?', hint: 'Событие должно менять решение игрока, а не быть случайным шумом.', prompt: 'Опиши несколько событий и допустимую частоту.', chips: ['отключение света', 'шум из соседней комнаты', 'редкое событие', 'без случайных событий'] },
    { n: 38, s: 6, area: 'Повторное прохождение', title: 'Что должно быть разным при новом запуске игры?', hint: 'Можно варьировать добычу и патрули, сохраняя ключевую структуру локации.', prompt: 'Какие элементы фиксированы, а какие меняются?', chips: ['положение ресурсов', 'маршруты врагов', 'фиксированная история', 'несколько концовок'] },

    { n: 39, s: 7, area: 'Анимация и детализация', title: 'Какой уровень анимации нужен персонажам и существам?', hint: 'Лучше определить читаемые ключевые движения, чем сразу обещать сотни кадров.', prompt: 'Какие действия должны иметь отдельные кадры или состояния?', chips: ['стойка', 'ходьба', 'прыжок', 'атака', 'реакция на урон'] },
    { n: 40, s: 7, area: 'Свет и эффекты', title: 'Как использовать свет, тени, пыль и другие эффекты?', hint: 'Оставляем 2D-пайплайн: эффекты поддерживают пиксельную сцену и читаемость.', prompt: 'Какие источники света и визуальные эффекты важны?', chips: ['фонарь игрока', 'мерцающие лампы', 'пыль', 'длинные тени', 'дождь/протечки'] },
    { n: 41, s: 7, area: 'Палитра и материалы', title: 'Какие цвета и материалы должны чаще всего встречаться?', hint: 'Палитру можно строить вокруг жёлтых обоев Level 0 и холодного бетона Level 1.', prompt: 'Назови основные цвета, акценты и нежелательные оттенки.', chips: ['пыльная охра', 'оливковый', 'бетонно-серый', 'ржавчина', 'ограниченная палитра'] },
    { n: 42, s: 7, area: 'Звуковая перспектива', title: 'Как звук должен помогать ориентироваться и замечать угрозу?', hint: 'Направление, расстояние и внезапная тишина могут давать полезные сигналы.', prompt: 'Какие звуковые ориентиры должны быть понятны игроку?', chips: ['пространственный звук', 'гул лампы', 'шаги за стеной', 'тишина как сигнал'] },
    { n: 43, s: 7, area: 'Доступность', title: 'Какие настройки доступности и комфорта нужны?', hint: 'Учитывай размер текста, переназначение клавиш, вспышки и громкость отдельных каналов.', prompt: 'Какие опции стоит иметь с первой версии?', chips: ['переназначение клавиш', 'размер текста', 'уменьшение вспышек', 'громкость по каналам'] },
    { n: 44, s: 7, area: 'Язык и локализация', title: 'На каких языках должен быть интерфейс и текст?', hint: 'Можно начать с одного языка и заложить поддержку локализации позже.', prompt: 'Укажи язык интерфейса, озвучки и субтитров.', chips: ['русский интерфейс', 'английский интерфейс', 'субтитры', 'без озвучки'] },

    { n: 45, s: 8, area: 'Объём контента', title: 'Сколько уровней, врагов, предметов и рецептов реально нужно?', hint: 'Прикинь минимум для прототипа и желаемое количество для полноценного релиза.', prompt: 'Напиши реалистичный минимум и мечту отдельно.', chips: ['2 уровня для прототипа', '1 враг для прототипа', 'расширять постепенно'] },
    { n: 46, s: 8, area: 'Технические ограничения', title: 'Есть ли ограничения по устройствам, производительности или размеру сборки?', hint: 'Влияет ли что-то на рендер, сохранения, разрешение и количество эффектов?', prompt: 'Укажи минимальные устройства и важные ограничения.', chips: ['обычный ПК', 'низкие системные требования', 'Steam Deck', 'без онлайна'] },
    { n: 47, s: 8, area: 'Команда и инструменты', title: 'Кто будет делать игру и какие инструменты доступны?', hint: 'Даже если работаешь один, это помогает выбрать реалистичный объём.', prompt: 'Перечисли роли, навыки, редакторы и доступное время.', chips: ['один разработчик', 'Unity 2022 LTS', 'пиксель-арт', 'звук отдельно'] },
    { n: 48, s: 8, area: 'Этапы и обратная связь', title: 'Как проверять, что прототип работает и становится лучше?', hint: 'Плейтесты, короткие сборки и список рисков помогают раньше находить проблемы.', prompt: 'Как часто показывать сборку и кто будет тестировать?', chips: ['играть самому каждую неделю', 'малые тесты', 'собирать отзывы', 'сначала вертикальный срез'] },
    { n: 49, s: 8, area: 'Приоритеты и компромиссы', title: 'Какие три вещи важнее всего, а от чего можно отказаться?', hint: 'Это поможет удержать проект в рамках, если времени или ресурсов не хватит.', prompt: 'Раздели ответ на «обязательно» и «можно убрать».', chips: ['атмосфера', 'выживание', 'исследование', 'контент можно наращивать'] },
    { n: 50, s: 8, area: 'Свободные заметки', title: 'Что ещё важно знать о твоём видении игры?', hint: 'Добавь референсы, запреты, конкретные примеры или идеи, которые не подошли ни к одному вопросу.', prompt: 'Любые дополнительные мысли и пожелания.', chips: ['не менять 2D', 'не менять основную идею', 'нужны варианты'] }
  ];

  const byNumber = new Map(questions.map(question => [question.n, question]));
  const fields = new Map();
  const sectionNodes = new Map();
  const list = document.getElementById('questions');
  const sectionNav = document.getElementById('sectionNav');
  const toast = document.getElementById('toast');
  const manualDialog = document.getElementById('manualCopyDialog');
  const manualTextarea = document.getElementById('manualCopyText');
  const feedback = document.getElementById('feedback');
  let activeSection = sections[0].id;
  let filterMode = 'all';
  let searchText = '';
  let focusMode = false;
  let toastTimer;

  function create(tag, className, text) {
    const node = document.createElement(tag);
    if (className) node.className = className;
    if (text !== undefined) node.textContent = text;
    return node;
  }

  function render() {
    sections.forEach((section, index) => {
      const button = create('button', 'section-link');
      button.type = 'button';
      button.dataset.section = section.id;
      button.setAttribute('aria-pressed', index === 0 ? 'true' : 'false');
      const number = create('b', '', String(index + 1).padStart(2, '0'));
      button.append(number, document.createTextNode(section.title));
      button.addEventListener('click', () => {
        activeSection = section.id;
        document.getElementById(section.id).scrollIntoView({ behavior: 'smooth', block: 'start' });
        setActiveSection(section.id);
      });
      sectionNav.append(button);

      const sectionNode = create('section', 'question-section');
      sectionNode.id = section.id;
      sectionNode.dataset.section = section.id;
      sectionNodes.set(section.id, sectionNode);
      const heading = create('div', 'section-heading');
      const numberBox = create('span', 'section-number', String(index + 1).padStart(2, '0'));
      const titleWrap = create('div');
      titleWrap.append(create('div', 'kicker', section.kicker), create('h3', '', section.title));
      const range = create('span', 'section-count', `ВОПРОСЫ ${section.first}—${section.last}`);
      heading.append(numberBox, titleWrap, range);
      sectionNode.append(heading);

      questions.filter(question => question.s === index).forEach(question => sectionNode.append(makeQuestion(question)));
      list.append(sectionNode);
    });
  }

  function makeQuestion(question) {
    const card = create('article', 'question-card');
    card.dataset.number = String(question.n);
    card.dataset.question = question.area.toLocaleLowerCase('ru');
    const top = create('div', 'question-top');
    top.append(create('span', 'q-number', String(question.n).padStart(2, '0')));
    const title = create('span', 'question-title', question.title);
    top.append(title);
    card.append(top);

    const hint = create('p', 'question-hint', question.hint);
    card.append(hint);

    if (question.chips && question.chips.length) {
      const chips = create('div', 'suggestions');
      chips.setAttribute('aria-label', 'Быстрые варианты, нажми чтобы добавить в ответ');
      question.chips.forEach(value => {
        const chip = create('button', 'suggestion', value);
        chip.type = 'button';
        chip.dataset.value = value;
        chip.addEventListener('click', () => toggleSuggestion(question.n, value));
        chips.append(chip);
      });
      card.append(chips);
    }

    const wrap = create('div', 'answer-wrap');
    const label = create('label', 'q-label', `Ответ на вопрос ${question.n}: ${question.title}`);
    const textarea = document.createElement('textarea');
    textarea.id = `answer-${question.n}`;
    textarea.className = 'answer-input';
    textarea.rows = 2;
    textarea.placeholder = question.prompt;
    textarea.setAttribute('aria-label', `Ответ на вопрос ${question.n}: ${question.title}`);
    textarea.dataset.area = question.area;
    textarea.dataset.question = question.title;
    textarea.addEventListener('input', () => {
      card.classList.toggle('is-answered', Boolean(textarea.value.trim()));
      updateChipStates(question.n);
      saveDraft();
      applyFilters();
    });
    fields.set(question.n, textarea);
    label.htmlFor = textarea.id;
    const copy = create('button', 'copy-one', '↗');
    copy.type = 'button';
    copy.title = `Скопировать ответ ${question.n}`;
    copy.setAttribute('aria-label', `Скопировать ответ на вопрос ${question.n}`);
    copy.addEventListener('click', () => copyQuestion(question.n));
    wrap.append(label, textarea, copy);
    card.append(wrap);
    return card;
  }

  function tokenizeSuggestions(value) {
    return value.split(/\s*;\s*/).map(part => part.trim()).filter(Boolean);
  }

  function toggleSuggestion(number, value) {
    const field = fields.get(number);
    const tokens = tokenizeSuggestions(field.value);
    const found = tokens.findIndex(token => token.toLocaleLowerCase('ru') === value.toLocaleLowerCase('ru'));
    if (found >= 0) tokens.splice(found, 1);
    else tokens.push(value);
    field.value = tokens.join('; ');
    field.dispatchEvent(new Event('input', { bubbles: true }));
    field.focus();
  }

  function updateChipStates(number) {
    const field = fields.get(number);
    const card = document.querySelector(`.question-card[data-number="${number}"]`);
    const tokens = new Set(tokenizeSuggestions(field.value).map(token => token.toLocaleLowerCase('ru')));
    card.querySelectorAll('.suggestion').forEach(chip => {
      const selected = tokens.has(chip.dataset.value.toLocaleLowerCase('ru'));
      chip.classList.toggle('selected', selected);
      chip.setAttribute('aria-pressed', String(selected));
    });
  }

  function setActiveSection(id) {
    activeSection = id;
    document.querySelectorAll('.section-link').forEach(button => {
      const active = button.dataset.section === id;
      button.classList.toggle('active', active);
      button.setAttribute('aria-pressed', String(active));
    });
  }

  function updateProgress() {
    const filled = questions.filter(question => fields.get(question.n).value.trim()).length;
    const percent = Math.round((filled / questions.length) * 100);
    document.getElementById('answeredCount').textContent = String(filled).padStart(2, '0');
    document.getElementById('progressFill').style.width = `${percent}%`;
    document.getElementById('progressPercent').textContent = `${percent}%`;
    document.getElementById('resultCount').textContent = `${questions.length} вопросов`;
    const rows = document.getElementById('sectionProgress');
    rows.replaceChildren();
    sections.forEach((section, index) => {
      const sectionQuestions = questions.filter(question => question.s === index);
      const count = sectionQuestions.filter(question => fields.get(question.n).value.trim()).length;
      const row = create('div', 'section-progress-row');
      row.append(create('b', '', String(index + 1).padStart(2, '0')));
      const title = create('span', '', section.title);
      const track = create('span', 'section-progress-track');
      const fill = document.createElement('span');
      fill.style.width = `${sectionQuestions.length ? count / sectionQuestions.length * 100 : 0}%`;
      track.append(fill);
      row.append(title, track, create('em', '', `${count}/${sectionQuestions.length}`));
      rows.append(row);
    });
  }

  function saveDraft() {
    try {
      const answers = Object.fromEntries(questions.map(question => [question.n, fields.get(question.n).value]));
      localStorage.setItem(STORAGE_KEY, JSON.stringify({ version: 2, savedAt: new Date().toISOString(), answers }));
      const status = document.querySelector('#saveStatus span:last-child');
      if (status) status.textContent = 'Сохранено локально';
    } catch (error) {
      showToast('Не удалось сохранить черновик в браузере');
    }
    updateProgress();
  }

  function loadDraft() {
    let loaded = false;
    try {
      const current = JSON.parse(localStorage.getItem(STORAGE_KEY) || 'null');
      if (current && current.answers) {
        const answers = current.answers;
        questions.forEach(question => {
          const value = Array.isArray(answers)
            ? answers.find(item => Number(item.number) === question.n)?.answer
            : answers[question.n] ?? answers[String(question.n)];
          if (typeof value === 'string') fields.get(question.n).value = value;
        });
        loaded = true;
      }
      if (!loaded) migrateLegacyDraft();
    } catch (error) {
      try { localStorage.removeItem(STORAGE_KEY); } catch (_) { /* storage can be disabled */ }
    }
    fields.forEach((field, number) => {
      const card = document.querySelector(`.question-card[data-number="${number}"]`);
      if (card) card.classList.toggle('is-answered', Boolean(field.value.trim()));
      updateChipStates(number);
    });
    updateProgress();
  }

  function migrateLegacyDraft() {
    const old = JSON.parse(localStorage.getItem(LEGACY_KEY) || 'null');
    if (!old || !Array.isArray(old.answers)) return;
    old.answers.forEach(item => {
      const number = Number(item.number);
      if (!Number.isInteger(number) || number < 1 || number > 20 || typeof item.answer !== 'string') return;
      // The earlier draft used this placeholder before the user confirmed the 2D side view.
      if (number === 3 && item.answer.trim() === 'Игра будет 2D. Точный ракурс ещё нужно выбрать.') return;
      fields.get(number).value = item.answer;
    });
    saveDraft();
  }

  function chosenQuestions(section = null) {
    const onlyFilled = document.getElementById('onlyFilled').checked;
    return questions.filter(question => (section === null || question.s === section) && (!onlyFilled || fields.get(question.n).value.trim()));
  }

  function makeText(section = null, markdown = false) {
    const selected = chosenQuestions(section);
    const output = ['SUBSISTENCE — ПРОЕКТНЫЙ БРИФ', '2D survival-хоррор · боковой вид · Unity 2022.3.62f2', 'Ответы из локального черновика. Ничего не отправляется автоматически.', ''];
    const groups = section === null ? sections : [sections[section]];
    groups.forEach(group => {
      output.push(markdown ? `## ${group.title}` : `=== ${group.title.toLocaleUpperCase('ru')} ===`);
      const groupQuestions = selected.filter(question => question.s === sections.indexOf(group));
      if (!groupQuestions.length && document.getElementById('onlyFilled').checked) {
        output.push('— В этом разделе пока нет ответов.', '');
        return;
      }
      groupQuestions.forEach(question => {
        const answer = fields.get(question.n).value.trim() || '— Пока без ответа —';
        output.push(markdown ? `### ${question.n}. ${question.area}` : `${String(question.n).padStart(2, '0')}. ${question.area}`);
        output.push(question.title);
        output.push(answer, '');
      });
    });
    return output.join('\n').trim() + '\n';
  }

  async function copyText(text, message) {
    try {
      if (!navigator.clipboard || !navigator.clipboard.writeText) throw new Error('clipboard unavailable');
      await navigator.clipboard.writeText(text);
      showToast(message);
      feedback.textContent = message;
    } catch (error) {
      manualTextarea.value = text;
      document.getElementById('dialogTitle').textContent = message;
      manualDialog.showModal();
      requestAnimationFrame(() => {
        manualTextarea.focus();
        manualTextarea.select();
      });
      feedback.textContent = 'Текст открыт для ручного копирования';
    }
  }

  function copyQuestion(number) {
    const question = byNumber.get(number);
    const answer = fields.get(number).value.trim() || '— Пока без ответа —';
    copyText(`${String(number).padStart(2, '0')}. ${question.area}\n${question.title}\n${answer}`, `Ответ ${number} готов к копированию`);
  }

  function showToast(message) {
    toast.textContent = message;
    toast.classList.add('show');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove('show'), 2500);
  }

  function download(name, content, mime) {
    const blob = new Blob([content], { type: mime });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = name;
    document.body.append(anchor);
    anchor.click();
    anchor.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }

  function applyFilters() {
    const query = searchText.trim().toLocaleLowerCase('ru');
    let visibleCount = 0;
    document.querySelectorAll('.question-card').forEach(card => {
      const number = Number(card.dataset.number);
      const field = fields.get(number);
      const text = `${card.dataset.question} ${card.querySelector('.question-title').textContent} ${card.querySelector('.question-hint').textContent} ${field.value}`.toLocaleLowerCase('ru');
      const isFilled = Boolean(field.value.trim());
      const matchesFilter = filterMode === 'all' || (filterMode === 'empty' && !isFilled) || (filterMode === 'answered' && isFilled);
      const matchesQuery = !query || text.includes(query);
      const visible = matchesFilter && matchesQuery;
      card.hidden = !visible;
      if (visible) visibleCount += 1;
    });
    document.querySelectorAll('.question-section').forEach(section => {
      section.hidden = ![...section.querySelectorAll('.question-card')].some(card => !card.hidden);
    });
    document.getElementById('resultCount').textContent = `${visibleCount} из ${questions.length} вопросов`;
    updateFocusCard();
  }

  function visibleCards() {
    return [...document.querySelectorAll('.question-card')].filter(card => !card.hidden && !card.closest('.question-section').hidden);
  }

  function updateFocusCard() {
    const cards = visibleCards();
    document.querySelectorAll('.question-card').forEach(card => card.classList.remove('focus-card'));
    document.querySelectorAll('.question-section').forEach(section => section.classList.remove('focus-active'));
    if (!focusMode || !cards.length) return;
    const current = cards[Math.max(0, Math.min(focusIndex, cards.length - 1))];
    focusIndex = cards.indexOf(current);
    current.classList.add('focus-card');
    current.closest('.question-section').classList.add('focus-active');
    document.getElementById('focusPosition').textContent = `Вопрос ${current.dataset.number} · ${focusIndex + 1} из ${cards.length}`;
    setActiveSection(current.closest('.question-section').id);
  }

  let focusIndex = 0;
  function stepFocus(delta) {
    const cards = visibleCards();
    if (!cards.length) return;
    focusIndex = Math.max(0, Math.min(cards.length - 1, focusIndex + delta));
    updateFocusCard();
    const current = cards[focusIndex];
    current.scrollIntoView({ behavior: 'smooth', block: 'center' });
    const field = current.querySelector('textarea');
    if (field) field.focus({ preventScroll: true });
  }

  function setFocus(enabled) {
    focusMode = enabled;
    document.body.classList.toggle('focus-mode', enabled);
    document.getElementById('focusBar').hidden = !enabled;
    document.getElementById('focusToggle').hidden = enabled;
    focusIndex = 0;
    updateFocusCard();
    if (enabled) stepFocus(0);
  }

  async function importBackup(file) {
    try {
      const parsed = JSON.parse(await file.text());
      if (!parsed || (!Array.isArray(parsed.answers) && typeof parsed.answers !== 'object')) throw new Error('invalid backup');
      questions.forEach(question => {
        const item = Array.isArray(parsed.answers)
          ? parsed.answers.find(entry => Number(entry.number) === question.n)
          : null;
        const value = item ? item.answer : parsed.answers[question.n] ?? parsed.answers[String(question.n)];
        if (typeof value === 'string') fields.get(question.n).value = value;
      });
      fields.forEach((field, number) => {
        document.querySelector(`.question-card[data-number="${number}"]`).classList.toggle('is-answered', Boolean(field.value.trim()));
        updateChipStates(number);
      });
      saveDraft();
      applyFilters();
      showToast('Резервная копия загружена в этот браузер');
    } catch (error) {
      showToast('Не удалось прочитать этот JSON-файл');
    }
  }

  render();
  loadDraft();
  applyFilters();

  document.querySelectorAll('.filter-button').forEach(button => button.addEventListener('click', () => {
    filterMode = button.dataset.filter;
    document.querySelectorAll('.filter-button').forEach(other => {
      const active = other === button;
      other.classList.toggle('active', active);
      other.setAttribute('aria-pressed', String(active));
    });
    applyFilters();
  }));
  document.getElementById('searchQuestions').addEventListener('input', event => {
    searchText = event.target.value;
    applyFilters();
  });
  document.getElementById('onlyFilled').addEventListener('change', () => saveDraft());
  document.getElementById('copyAll').addEventListener('click', () => copyText(makeText(), 'Весь бриф готов к копированию'));
  document.getElementById('copySection').addEventListener('click', () => {
    const sectionIndex = sections.findIndex(section => section.id === activeSection);
    copyText(makeText(sectionIndex), `Раздел «${sections[sectionIndex].title}» готов`);
  });
  document.getElementById('downloadTxt').addEventListener('click', () => {
    download('subsistence-brief.txt', makeText(), 'text/plain;charset=utf-8');
    showToast('Скачан файл .txt');
  });
  document.getElementById('downloadMd').addEventListener('click', () => {
    download('subsistence-brief.md', makeText(null, true), 'text/markdown;charset=utf-8');
    showToast('Скачан файл .md');
  });
  document.getElementById('downloadJson').addEventListener('click', () => {
    const answers = Object.fromEntries(questions.map(question => [question.n, fields.get(question.n).value]));
    download('subsistence-brief-backup.json', JSON.stringify({ version: 2, savedAt: new Date().toISOString(), answers }, null, 2), 'application/json;charset=utf-8');
    showToast('Резервная копия .json скачана');
  });
  document.getElementById('importJson').addEventListener('change', event => {
    const file = event.target.files && event.target.files[0];
    if (file) importBackup(file);
    event.target.value = '';
  });
  document.getElementById('resetForm').addEventListener('click', () => {
    if (!window.confirm('Очистить все 50 ответов? Это действие нельзя отменить.')) return;
    fields.forEach((field, number) => {
      field.value = '';
      document.querySelector(`.question-card[data-number="${number}"]`).classList.remove('is-answered');
      updateChipStates(number);
    });
    saveDraft();
    applyFilters();
    feedback.textContent = '';
    showToast('Ответы очищены');
  });
  document.getElementById('focusToggle').addEventListener('click', () => setFocus(true));
  document.getElementById('exitFocus').addEventListener('click', () => setFocus(false));
  document.getElementById('focusPrev').addEventListener('click', () => stepFocus(-1));
  document.getElementById('focusNext').addEventListener('click', () => stepFocus(1));
  document.getElementById('selectCopyText').addEventListener('click', () => {
    manualTextarea.focus();
    manualTextarea.select();
  });

  document.addEventListener('keydown', event => {
    const tag = event.target && event.target.tagName;
    const typing = tag === 'INPUT' || tag === 'TEXTAREA' || event.target.isContentEditable;
    if (event.key === '/' && !typing && !manualDialog.open) {
      event.preventDefault();
      document.getElementById('searchQuestions').focus();
    }
    if ((event.ctrlKey || event.metaKey) && event.key === 'Enter' && focusMode && !manualDialog.open) {
      const card = visibleCards()[focusIndex];
      if (card) {
        event.preventDefault();
        copyQuestion(Number(card.dataset.number));
        stepFocus(1);
      }
    }
  });

  if ('IntersectionObserver' in window) {
    const observer = new IntersectionObserver(entries => {
      const visible = entries.filter(entry => entry.isIntersecting).sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];
      if (visible && !focusMode) setActiveSection(visible.target.id);
    }, { rootMargin: '-15% 0px -68% 0px', threshold: [0, .1, .35] });
    sectionNodes.forEach(section => observer.observe(section));
  }
})();
