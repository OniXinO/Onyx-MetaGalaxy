# OMG Plugin (KSP1)

Мета: Плагін читає `OMGSettings.cfg` і активний профіль, та на ранньому етапі завантаження БД може фільтрувати `ConfigNode`/патчі для вимкнених паків (розширювано).

Збірка:
- Цільова платформа: .NET Framework 4.7.2 (`net472`).
- Потрібні посилання на збірки KSP (встановлення KSP1):
  - `Assembly-CSharp.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `KSPUtil.dll` із каталогу `KSP_x64_Data/Managed`.

Компіляція:
1. Встановіть змінну оточення `KSP_DIR` до кореня інсталяції KSP.
2. Побудуйте проект (MSBuild / dotnet / Visual Studio).
   - `dotnet build -c Release` (потребує встановлених Reference Assemblies для .NET Framework 4.7.2).
   - або `msbuild OMG.csproj /p:Configuration=Release`.
3. DLL автоматично зʼявиться у `GameData/OniXinO/OMG/Plugins/` (налаштований OutputPath).

Примітка:
- Встановіть `KSP_DIR`, наприклад: `setx KSP_DIR "D:\Games\KSP1"` або тимчасово у поточній сесії PowerShell: `$env:KSP_DIR = 'D:\Games\KSP1'`.

Розширення логіки:
- Використайте `KSPAddon` зі `Startup.Instantly` або `MainMenu` щоб ініціалізувати та підписатися на події БД.
- Для реальної фільтрації `ConfigNode` потрібно працювати із `GameDatabase.Instance` та подіями після завантаження.
 - Налаштування:
   - `filterDisabledPacks` — увімкнути фільтрацію нод із файлів вимкнених паків.
   - `filterSafeMode` — безпечний режим (лише лог): спершу true, потім за потреби false.

Журнал:
- Плагін логуватиме статистику: `Candidates`, `Removed`, `SafeMode` та шлях файлу (`url`) для кандидатів.