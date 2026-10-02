# High-End 2D Visual Pipeline Guide for SUBSISTENCE

Этот гайд описывает, как в рантайм-сцене Backrooms сейчас настроены многослойность, свет, пост-процессинг и плавное движение.

## 1. Слои и параллакс

Геймплей остаётся строго боковым 2D; глубина создаётся не перспективой, а отдельными параллакс-слоями.

| Слой | Что на слое | Sorting order | Параллакс X / Y |
| --- | --- | --- | --- |
| Far haze | Дальняя дымка и самые дальние перегородки | -38 | 0.06 / 0.02 |
| Midground wallpaper | Более читаемые стены, ниши и обои | -24 | 0.20 / 0.05 |
| Gameplay | Плитка, пол, коробки, игрок, противники, коллизии, свет, тени | от -20 до +24 | 0 |
| Foreground frame | Мягкая тёмная рамка по краям кадра | +24 | 0.55 / 0.12 |

Скрипты: `ParallaxLayer2D.cs` (движение одного слоя за камерой) и `ParallaxSystem2D.cs` (сборка слоёв из `Assets/Resources/Parallax/`). Геймплейные координаты и коллайдеры не меняются.

## 1.5. Спрайты и пиксельная сетка

- Игровой масштаб: `96 pixels per world unit`.
- Обои/бетон — 384×384 px; пол/ковёр — 384×96 px; потолок — 384×48 px. Каждый материал растягивается tiled-рендерером на ширину уровня, а не запускается заново на границах секций.
- Персонажи и props загружаются из `Assets/Resources/Art/PixelArt/characters.png` и `props.png`; lookup-таблица создаётся `Tools/art2d/build_assets.py`.
- Источники находятся в `ArtSource/raw/`, вне `Assets/Resources`. Пересборка и список неготовых источников — в `ART_PIPELINE.md`.
- Все tile edges sealing-ятся до одинаковых крайних пикселей; так point-filtered повторы не дают тонких швов.
- `CameraFollow2D` сглаживает движение, затем округляет позицию камеры по сетке 1/96 world unit.

## 2. URP 2D свет и normal maps

### Пакет и Renderer
- В `Packages/manifest.json` подключены:
  - `com.unity.render-pipelines.core: 14.0.11`
  - `com.unity.render-pipelines.universal: 14.0.11`
- `Assets/Editor/UrpFieldTestSetup.cs` **не** запускается автоматически при старте редактора (автозапуск вызывал предупреждение Unity про изменение иммутабельных пакетных ассетов). После первого открытия проекта выберите в меню **Tools → Subsistence → Setup URP 2D Pipeline** — скрипт:
  - создаёт `Assets/Settings/Subsistence2DRenderer.asset`
  - создаёт `Assets/Settings/SubsistenceURP.asset`
  - включает HDR, Render Scale 1, MSAA 2x
  - после подтверждения в диалоге назначает URP asset в Graphics/Quality (от назначения можно отказаться и настроить вручную в Edit → Project Settings → Graphics)

### Runtime-свет
- `GraphicsBootstrap2D.cs`
  - включает `Camera.allowHDR`
  - включает `UniversalAdditionalCameraData.renderPostProcessing`
  - создаёт глобальный приглушённый 2D-амбиент
  - создаёт Global Volume
- `WorldLighting2D.cs`
  - расставляет URP `Light2D.Point` источники под потолком
  - добавляет мерцание через `LightFlicker2D.cs`
  - добавляет `ShadowCaster2D` для не-trigger коллайдеров: пол, границы/стены, коробки, верстаки, лифт
- `PlayerController.cs`
  - прикрепляет URP точку-фонарь к руке
  - включает её только если фонарь есть в инвентаре и нажат `F`
  - синхронизирует направление с facing персонажа

### Normal maps
- `RendererLibrary2D.cs` назначает спрайтам материал `Universal Render Pipeline/2D/Sprite-Lit-Default` и включает `_NORMALMAP`.
- `PixelArtFactory.cs` и `RendererLibrary2D.cs` генерируют простые runtime normal maps из luminance/alpha спрайтов, чтобы свет реагировал на края и объём.
- Это не hand-painted PBR-нормали; задача — дать читаемую реакцию на свет в 2D-сцене.

## 3. Пост-процессинг (cinematic volume)

`GraphicsBootstrap2D.cs` создаёт runtime Volume-профиль с такими базовыми настройками:
- **Tonemapping:** ACES
- **Bloom:** threshold 1.18, intensity 0.22, scatter 0.55, dirt intensity 0.12
- **Lens Dirt:** процедурная текстура из `TextureFactory.LensDirt()` (если нет кастомного ассета)
- **Color Adjustments:** post exposure -0.03, contrast +6, saturation -4
- **Vignette:** intensity 0.16, smoothness 0.48, rounded on
- **Chromatic Aberration:** intensity 0.012

Настройки специально сдержанные: они должны добавлять «дороговизну» кадра, а не превращать Backrooms в неоновый экшен.

## 4. Плавность движения и камеры

### PlayerController.cs
- `Rigidbody2D.interpolation = Interpolate`
- `CollisionDetectionMode2D.Continuous`
- горизонтальное движение через acceleration/deceleration вместо мгновенной установки скорости
- `AirControl` для ощущения тяжести
- `CoyoteTime` и `JumpBuffer` для чистого прыжка
- в лифте движение ограничено кабиной, как требовалось ранее

### CameraFollow2D.cs
- `Vector3.SmoothDamp` вместо мгновенного следования
- отдельное время сглаживания для меню и геймплея
- `maxFollowSpeed`
- `Time.unscaledDeltaTime`
- pixel snapping **включён по умолчанию** на 96 PPU после SmoothDamp; это даёт стабильную сетку пикселей для спрайтов. Проверить на целевом разрешении, что скорость сглаживания не создаёт заметного ступенчатого движения.

## 5. Что нужно проверить в Unity Editor

Unity Editor здесь недоступен, поэтому после импорта обязательно проверь в Unity 2022.3.62f2:
1. URP пакеты импортировались без ошибок.
2. В `Project Settings → Graphics` и `Quality` выбран `SubsistenceURP.asset`.
3. Сцена запускается, экран не чёрный и не полностью неосвещённый.
4. Параллакс двигается с разной скоростью и не ломает коллизии.
5. Фонарь реально освещает/затеняет окружение, а не только рисует спрайт-конус.
6. Нет micro-freeze или субпиксельного дрожания при беге и прыжках.
7. Bloom, vignette и ACES заметны, но не перебивают читаемость HUD и иконок.
