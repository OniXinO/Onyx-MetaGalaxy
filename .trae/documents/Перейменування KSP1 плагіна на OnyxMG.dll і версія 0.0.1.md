## Мета
- Перейменувати збірку плагіна KSP1 з `OMG` на `OnyxMG` (вихідний файл `OnyxMG.dll`).
- Встановити версію збірки плагіна на `0.0.1` (лише для плагіна, не всього проєкту).

## Зміни у проєкті
- Файл `GameData/OniXinO/OMG/Plugins/src/OMG.csproj`:
  - Оновити `AssemblyName` з `OMG` на `OnyxMG` (`GameData/OniXinO/OMG/Plugins/src/OMG.csproj:4`).
  - Додати властивості версій у `<PropertyGroup>`:
    - `<Version>0.0.1</Version>`
    - `<AssemblyVersion>0.0.1.0</AssemblyVersion>`
    - `<FileVersion>0.0.1.0</FileVersion>`
    - (за бажанням) `<InformationalVersion>0.0.1</InformationalVersion>`
- Файл KSP-AVC версії `GameData/OniXinO/OMG/OMG.version` вже має `0.0.1` і змін не потребує (`GameData/OniXinO/OMG/OMG.version:3`).

## Реалізація
1. Внести зміни до `OMG.csproj` у секції `<PropertyGroup>` без створення `AssemblyInfo.cs` (SDK генерує його автоматично).
2. Перезібрати проєкт плагіна KSP1 (`net472`).
3. Перевірити, що артефакт зібрано як `GameData/OniXinO/OMG/Plugins/OnyxMG.dll`.
4. Переконатися, що у згенерованому `obj/.../AssemblyInfo.cs` версії стали `0.0.1.0`.

## Перевірка
- Запуск KSP1 та підтвердження завантаження плагіна (класи з `[KSPAddon]` у просторі імен `OMG` працюють незалежно від імені збірки; код у `GameData/OniXinO/OMG/Plugins/src/OMGProfileManager.cs:7-12`).
- Логи міститимуть префікс `[OMG]` (залишаємо як є, бо змінюється лише ім’я DLL).
- Пошук посилань на старі назви не виявив `OMG-KSP1.dll`; конфіги не прив’язані до імені DLL (підтверджено пошуком по репозиторію).

## Примітки
- Зміни стосуються тільки плагіна KSP1; версії у інших частинах проєкту не змінюються.
- Якщо існують зовнішні скрипти складання/релізу, що очікують `OMG.dll`, їх потрібно оновити, але у репозиторії посилань не знайдено.