# Технические решения и источники

## Backrooms — Level 0 / The Lobby

Перед переделкой стартового пространства сверил описание Level 0 и визуальные референсы. В распространённой версии лора это входной уровень Backrooms: старые комнаты/служебные помещения розничного здания, выцветшие жёлтые обои с повторяющимся рисунком, сырой ковёр, разбросанные розетки и непостоянно расположенные флуоресцентные лампы. Геометрия нелинейна, комнаты похожи, но не идентичны; первая зона ценна именно ощущением пустоты, дезориентации, гулом света и нехваткой ресурсов, а не промышленными трубами, ящиками повсюду и противниками.

- [Level 0: “The Lobby” — Backrooms Wiki](https://backrooms.fandom.com/wiki/Level_0) — визуальные признаки, нелинейная планировка и отдельная аномалия Manila Room.
- [Level 0 — Backrooms General Public Database](https://bgpd.wikidot.com/level-0) — описывает жёлтые стены, ковёр, флуоресцентный гул и отсутствие подтверждённых сущностей; версии лора Backrooms различаются, поэтому это художественный ориентир, а не единый официальный канон.
- [Фото-референс уровня](https://unionstreetjournal.com/2022/04/scents-of-wet-carpet-and-terrifying-entities-welcome-to-the-backrooms/) и [вариант с потолочными панелями и длинной перспективой](https://backrooms-redacted.fandom.com/wiki/Level_0) — использованы только для анализа обоев, ковра, освещения и планировки; игровая сцена не заменена и не перерисована поверх этих изображений.

В проекте Level 0 сделан визуально повторяющимся и почти пустым: узорные выцветшие обои, влажные пятна на ковре, подвесные панели, флуоресцентные лампы, редкие розетки и ложные проходы. По требованиям игрового цикла одиночный торговец вынесен в редкую Manila Room; враги появляются только после перехода в промышленный Level 1. Промежуток к лифтам — игровая аномалия выхода, а не заявление об универсальном каноне.

Перед расширением проекта также сверил требования с документацией Unity. Версию проекта закрепил ровно на **2022.3.62f2**; Unity указывает её в официальных релизах от 3 октября 2025 года.

- [Unity 2022.3.62f2 — официальный релиз и установщики](https://unity.com/releases/editor/whats-new/2022.3.62f2) — используем запрошенную версию без подмены на другую.
- [2D Sorting](https://docs.unity3d.com/2022.3/Documentation/Manual/2DSorting.html) — мир, персонажи и предметы визуализируются `SpriteRenderer`-спрайтами; порядок слоёв задаётся сортировкой по `sortingOrder`.
- [Orthographic camera](https://docs.unity3d.com/2022.3/Documentation/Manual/class-Camera.html) — игровая камера работает в ортографическом режиме для устойчивого бокового 2D-вида.
- [Built-in packages](https://docs.unity3d.com/Manual/pack-build.html) — игровой мир использует встроенный модуль `com.unity.modules.physics2d`; графика создаётся из point-filtered пиксельных спрайтов.
- [Screen.SetResolution](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Screen.SetResolution.html) и [Screen.resolutions](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Screen-resolutions.html) — окно настроек перебирает разрешения, поддерживаемые монитором, и применяет выбранное через публичный API Unity.
- [PlayerPrefs](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PlayerPrefs.html) и [QualitySettings.vSyncCount](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/QualitySettings-vSyncCount.html) — локально сохраняются параметры звука/экрана, а VSync переключается штатной настройкой Unity.
- [Animator Controller в Unity 2022.3](https://docs.unity3d.com/2022.3/Documentation/Manual/class-AnimatorController.html) — штатный Animator строится из Clips и Controller. Для этого прототипа маленькие point-filtered спрайты собираются и проигрываются по кадрам в `PixelFrameAnimator`, чтобы не зависеть от набора импортированных кадров.
- [AudioSource в Unity 2022.3](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioSource.html) — звуковой слой использует обычные 2D AudioSource: зацикленный эмбиент и локальные оригинальные WAV-эффекты.
- [Play Mode](https://docs.unity3d.com/2022.3/Documentation/Manual/ConfigurableEnterPlayMode.html) и [Build Settings](https://docs.unity3d.com/2022.3/Documentation/Manual/BuildSettings.html) — запуск внутри Editor делается кнопкой Play, а отдельная сборка требует выбранной платформы и сцены в списке сборки.
- [Unity Graphics: соответствие версий SRP](https://github.com/Unity-Technologies/Graphics) — 2022.2/2022.3 соответствует семейству SRP 14.x.

Основной мир и большая часть окружения собираются в `WorldBuilder2D` и `PixelArtFactory`. Игровые персонажи и объекты упакованы в `Assets/Resources/Art/PixelArt/characters.png` и `props.png`, а источники лежат вне runtime-ресурсов в `ArtSource/raw/`; процедура описана в `ART_PIPELINE.md`. Устаревшие Piskel-пути и старые ссылки на `Assets/Resources/PixelArt/individual_models.png` больше не используются. В игровом коде нет 3D-мешей, 3D-коллайдеров, перспективной камеры или сетевого игрового соединения; проект остаётся 2D. В `Assets/Resources/Audio` находятся музыка и WAV-эффекты. UI включает 30 ячеек рюкзака, пояс на шесть слотов, быстрый перенос и меню крафта с проверкой материалов, верстаков и изученных чертежей.
