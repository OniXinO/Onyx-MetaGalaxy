# Onyx MetaGalaxy (OMG) — Release Notes v0.0.1

Дата: 2025‑11‑17
Сумісність: KSP 1.12.5, Kopernicus, ModuleManager

## Огляд
- Перший тестовий реліз з фокусом на Stock + Outer Planets Mod (OPM) через OMG-обгортку та профілі продуктивності.
- Виправлено версію на `0.0.1` у всій документації та файлах.
- Додано ліцензії: `LICENSE` (MIT) для коду, `LICENSE-CC-BY-NC-SA-4.0.txt` для конфігів/ресурсів за потреби.

## У першому релізі - v0.0.1
- Профілі OPM-only:
  - `opm_productivity` — для слабших систем, пріоритет FPS і стабільність.
  - `opm_balance` — збалансовані налаштування графіки/ресурсів.
  - `opm_quality` — вища якість для потужних систем.
  - `opm_ultra` — максимум якості/ефектів для топ систем.
- Оновлено `README.md` (профілі, реліз, ліцензії) та створено `DESIGN.md` (цілі, архітектура, процес релізу).
- Оновлено `OMG.version` до `0.0.1` (KSP-AVC сумісність).
- Виправлено назву профілю `ksrss_gpp` (було `ksrrs_gpp`).

## Інсталяція
- Потрібні: KSP 1.12.5, `Kopernicus`, `ModuleManager`, встановлений `OuterPlanetsMod` у `GameData`.
- Скопіюйте `GameData/OniXinO/OMG` у вашу інсталяцію KSP.
- Активуйте профіль:
  1. Встановіть активний профіль в `GameData/OniXinO/OMG/OMGSettings.cfg` (`activeProfile = opm_balance` або інший).
  2. Запустіть `tools/ActivateProfile.ps1` (опційно: `-ActiveProfile <name>`).
  3. Перевірте маркери `GameData/OniXinO/OMG_Enable_<PackId>`.
- Рекомендовані налаштування `OMGSettings.cfg`: `strictMarkers = true`, `filterSafeMode = true` на старті.

## Сумісність
- Перевірено комбінацію Stock + OPM. Інші пакети поки не підтримуються офіційно у v0.0.1.
- Патчі для дерева досліджень/біомів/частин відкладено до наступних версій.

## Відомі питання
- Увімкнення складних графічних модів у `opm_quality/opm_ultra` може вимагати тонкого тюнінгу (налаштування PostProcessing/Scatterer/EVE), що не входить до цього релізу.
- Якщо `filterDisabledPacks = true` і `filterSafeMode = false`, можливе агресивне видалення `ConfigNode` від вимкнених паків — спершу перевіряйте логи `[OMG]`.

## Перевірка інсталяції
- `GameData/OniXinO/OMG/OMG.version` показує `0.0.1`.
- Маркери `OMG_Enable_<id>` створені лише для реально встановлених паків.
- У логах KSP є повідомлення `[OMG]` про фільтрацію/кандидатів.

## Завантаження
- Дистрибуція: GitHub Releases (v0.0.1). З версії `0.1` — план SpaceDock/CKAN.

## Ліцензія
- Код: MIT (`LICENSE`).
- Конфіги/ресурси: MIT або CC BY-NC-SA 4.0 (`LICENSE-CC-BY-NC-SA-4.0.txt`) — залежно від походження та вимог.

## Додатково
- Повний список змін див. `CHANGELOG.md`.