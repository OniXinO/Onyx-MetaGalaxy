# Onyx MetaGalaxy (OMG) для KSP 1.12.5

Мета: агрегатор профілів для інсталяції/комбінації планетпаків KSP1 без включення їхньої ліцензійної частини у збірку. Профілі у `.cfg`, активація через скрипт, який створює маркерні моди для ModuleManager.

Структура:
- `GameData/OniXinO/OMG/OMGSettings.cfg` — вибір активного профілю.
- `GameData/OniXinO/OMG/Profiles/*.cfg` — профілі зі списком `Pack { id; enabled }`.
- Маркери: `GameData/OMG_Enable_<PackId>` — створюються скриптом.

Залежності:
- Встановлені планетпаки у `GameData` (наприклад `OuterPlanetsMod`, `JNSQ` тощо).
- `Kopernicus`, `ModuleManager` для KSP 1.12.5.

Використання:
1. Встановіть потрібні планетпаки у `GameData`.
2. Відредагуйте `Profiles/default.cfg`, увімкніть/вимкніть `enabled`.
3. За потреби змініть активний профіль у `OMGSettings.cfg` (`activeProfile = default`).
4. Запустіть `tools/ActivateProfile.ps1` (опційно `-ActiveProfile <name>`).
5. Скрипт створить/прибере маркери `GameData/OMG_Enable_<id>`, їх можна перевірити у файловій системі.
6. Запускайте KSP.

Приклад ModuleManager патча для інтеграції:

```
// Спрацює лише якщо пак OPM увімкнений у профілі (маркер існує)
@Kopernicus:NEEDS[OuterPlanetsMod,OMG_Enable_OuterPlanetsMod]
{
  // Інтеграційний конфіг (ресурси/баланс/налаштування)
}
```

Зауваження:
- `id` має збігатися з назвою папки паку у `GameData`.
- Деякі пакети конфліктують, тестуйте комбінації.
- Скрипт не видаляє чужі файли, лише керує маркерами.