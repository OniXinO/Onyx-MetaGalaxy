# OMG Plugin (KSP1)

Мета: Плагін читає `OMGSettings.cfg` і активний профіль, та на ранньому етапі завантаження БД може фільтрувати `ConfigNode`/патчі для вимкнених паків (розширювано).

Збірка:
- Цільова платформа: .NET Framework 4.7.2 (`net472`).
- Потрібні посилання на збірки KSP (встановлення KSP1):
  - `Assembly-CSharp.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `KSPUtil.dll` із каталогу `KSP_x64_Data/Managed`.

Компіляція:
1. Встановіть змінну оточення `KSP_DIR` до кореня інсталяції KSP.
2. Відкрийте `.csproj` та побудуйте (MSBuild / Visual Studio).
3. Скомпільовану DLL покладіть у `GameData/OniXinO/OMG/Plugins/`.

Розширення логіки:
- Використайте `KSPAddon` зі `Startup.Instantly` або `MainMenu` щоб ініціалізувати та підписатися на події БД.
- Для реальної фільтрації `ConfigNode` потрібно працювати із `GameDatabase.Instance` та подіями після завантаження.