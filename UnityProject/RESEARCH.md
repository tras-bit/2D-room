# Технические решения и источники

Перед расширением проекта сверил требования с документацией Unity. Версию проекта закрепил ровно на **2022.3.62f2**; Unity указывает её в официальных релизах от 3 октября 2025 года.

- [Unity 2022.3.62f2 — официальный релиз и установщики](https://unity.com/releases/editor/whats-new/2022.3.62f2) — используем запрошенную версию без подмены на другую.
- [Сочетание 2D- и 3D-настроек](https://docs.unity3d.com/2022.3/Documentation/Manual/2DAnd3DModeSettings.html) — боковое управление и 2D-коллизии оставлены, а комнаты, модели персонажа и освещение сделаны объёмными; Unity-проект может сочетать оба типа графики.
- [Built-in packages](https://docs.unity3d.com/Manual/pack-build.html) — для 2D- и 3D-физики подключены встроенные модули `com.unity.modules.physics2d` и `com.unity.modules.physics`; рендеринг сделан встроенными средствами без принудительного перехода на URP.
- [Text Mesh component](https://docs.unity3d.com/2022.3/Documentation/Manual/class-TextMesh.html) — небольшие таблички секторов и ящиков создаются как 3D-текст прямо в игровом мире.
- [Screen.SetResolution](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Screen.SetResolution.html) и [Screen.resolutions](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Screen-resolutions.html) — окно настроек перебирает разрешения, поддерживаемые монитором, и применяет выбранное через публичный API Unity.
- [PlayerPrefs](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PlayerPrefs.html) и [QualitySettings.vSyncCount](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/QualitySettings-vSyncCount.html) — локально сохраняются параметры звука/экрана, а VSync переключается штатной настройкой Unity.
- [Animator Controller в Unity 2022.3](https://docs.unity3d.com/2022.3/Documentation/Manual/class-AnimatorController.html) — штатный Animator строится из Clips и Controller. Для этого прототипа маленькие point-filtered спрайты собираются и проигрываются по кадрам в `PixelFrameAnimator`, чтобы не зависеть от набора импортированных кадров.
- [AudioSource в Unity 2022.3](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioSource.html) — звуковой слой использует обычные 2D AudioSource: зацикленный эмбиент и локальные оригинальные WAV-эффекты.
- [Play Mode](https://docs.unity3d.com/2022.3/Documentation/Manual/ConfigurableEnterPlayMode.html) и [Build Settings](https://docs.unity3d.com/2022.3/Documentation/Manual/BuildSettings.html) — запуск внутри Editor делается кнопкой Play, а отдельная сборка требует выбранной платформы и сцены в списке сборки.
- [Unity Graphics: соответствие версий SRP](https://github.com/Unity-Technologies/Graphics) — 2022.2/2022.3 соответствует семейству SRP 14.x.

В `Assets/Resources/Art` хранятся бесшовные текстуры обоев и ковра; в `Assets/Resources/Audio` — созданные для проекта WAV-эмбиент, музыка и эффекты. Внешние ассет-паки не подключены. Арт комнат, контейнеров и персонажей собирается в сцене из объёмных мешей и этих текстур, а передвижение по-прежнему использует 2D-физику. UI поддерживает перетаскивание, быстрый перенос и разделение стопок; сетевой код в этом прототипе отсутствует.
