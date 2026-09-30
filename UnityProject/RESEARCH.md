# Технические решения и источники

Перед расширением проекта сверил требования с документацией Unity. Версию проекта закрепил ровно на **2022.3.62f2**; Unity указывает её в официальных релизах от 3 октября 2025 года.

- [Unity 2022.3.62f2 — официальный релиз и установщики](https://unity.com/releases/editor/whats-new/2022.3.62f2) — используем запрошенную версию без подмены на другую.
- [Unity 2022.3: создание 2D-игры](https://docs.unity3d.com/2022.3/Documentation/Manual/Quickstart2DCreate.html) — Unity описывает 2D Renderer и 2D Lights в URP; этот стартовый field test пока остаётся на встроенном рендерере, чтобы проект открывался без ручной настройки pipeline. Фонарный конус и световая атмосфера делаются спрайтовыми слоями.
- [URP 14.0: совместимость](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/requirements.html) — ветка URP 14 совместима с Unity 2022.2–2022.x; переход на неё можно сделать отдельным графическим этапом, когда будет готов импортируемый арт.
- [Animator Controller в Unity 2022.3](https://docs.unity3d.com/2022.3/Documentation/Manual/class-AnimatorController.html) — штатный Animator строится из Clips и Controller. Для этого прототипа маленькие point-filtered спрайты собираются и проигрываются по кадрам в `PixelFrameAnimator`, чтобы не зависеть от набора импортированных кадров.
- [AudioSource в Unity 2022.3](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioSource.html) — звуковой слой использует обычные 2D AudioSource: зацикленный эмбиент и локальные оригинальные WAV-эффекты.
- [Unity Graphics: соответствие версий SRP](https://github.com/Unity-Technologies/Graphics) — 2022.2/2022.3 соответствует семейству SRP 14.x.

В папке `Assets/Resources/Audio` лежат сгенерированные специально для проекта WAV: 24-секундная музыкальная тема, гул технического коридора, металл, вода, ткань, шаг, взмах, удар существу и UI. Внешние звуковые библиотеки не подключены; файлы можно пересоздать Python-скриптом `Tools/generate_audio.py`. 2D-арт создаётся процедурно и точечно фильтруется; присланная картинка задаёт язык спрайтов и силуэты, а не фон уровня.
