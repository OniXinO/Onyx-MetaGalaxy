using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace OMG
{
    // Запускається рано (у головному меню), читає профіль
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class OMGProfileManager : MonoBehaviour
    {
        private string GameDataPath;
        private string OMGRoot;
        private string SettingsPath;

        private void Awake()
        {
            try
            {
                GameDataPath = Path.Combine(KSPUtil.ApplicationRootPath, "GameData");
                OMGRoot = Path.Combine(GameDataPath, "OniXinO", "OMG");
                SettingsPath = Path.Combine(OMGRoot, "OMGSettings.cfg");

                var activeProfile = ReadActiveProfileName(SettingsPath) ?? "default";
                var profilePath = Path.Combine(OMGRoot, "Profiles", activeProfile + ".cfg");
                var packs = ReadPacks(profilePath);
                Log($"OMG active profile: {activeProfile} ({packs.Count} packs)");

                // Підписка: після завантаження БД логувати (і згодом фільтрувати) ноди вимкнених паків
                GameEvents.OnGameDatabaseLoaded.Add(() => OnDatabaseLoaded(packs));
            }
            catch (Exception ex)
            {
                Log("OMG init failed: " + ex);
            }
        }

        private void Log(string msg)
        {
            Debug.Log("[OMG] " + msg);
        }

        private string ReadActiveProfileName(string settingsPath)
        {
            if (!File.Exists(settingsPath)) return null;
            foreach (var line in File.ReadAllLines(settingsPath))
            {
                var t = line.Trim();
                if (t.StartsWith("activeProfile"))
                {
                    var parts = t.Split('=');
                    if (parts.Length >= 2) return parts[1].Trim();
                }
            }
            return null;
        }

        private List<(string id, bool enabled)> ReadPacks(string profilePath)
        {
            var result = new List<(string id, bool enabled)>();
            if (!File.Exists(profilePath)) return result;
            bool inPack = false; string id = null; bool enabled = false;
            foreach (var raw in File.ReadAllLines(profilePath))
            {
                var line = raw.Trim();
                if (!inPack && (line == "Pack" || line.StartsWith("Pack{")))
                {
                    inPack = true; id = null; enabled = false; continue;
                }
                if (inPack && line == "{") continue;
                if (inPack && line.StartsWith("id")) { id = line.Split('=')[1].Trim(); continue; }
                if (inPack && line.StartsWith("enabled")) { enabled = ParseBool(line.Split('=')[1]); continue; }
                if (inPack && line == "}") { if (!string.IsNullOrEmpty(id)) result.Add((id, enabled)); inPack = false; }
            }
            return result;
        }

        private bool ParseBool(string s)
        {
            s = (s ?? "").Trim().ToLowerInvariant();
            return s == "true" || s == "1" || s == "yes";
        }

        // Логування нод для вимкнених паків (м'яка перевірка)
        private void OnDatabaseLoaded(List<(string id, bool enabled)> packs)
        {
            try
            {
                var disabled = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var p in packs) if (!p.enabled) disabled.Add(p.id ?? string.Empty);
                Log($"OMG DB loaded. Disabled packs: {string.Join(",", disabled)}");

                // GameDatabase зберігає відносний шлях у властивості url (тип UrlConfig/ConfigNode)
                // Тут ми лише логуватимемо кандидатів, щоб уникнути небажаного видалення.
                var configs = GameDatabase.Instance.root; // кореневий вузол
                if (configs != null)
                {
                    foreach (var node in configs.nodes)
                    {
                        // Багато нод не мають прив'язки до файлового шляху, це лише демо-логіка
                        var src = node.name; // для демонстрації, реальний шлях потребує UrlDir/UrlConfig
                        foreach (var id in disabled)
                        {
                            if (src != null && src.StartsWith(id, StringComparison.InvariantCultureIgnoreCase))
                            {
                                Log($"Candidate node from disabled pack '{id}': {src}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("OMG OnDatabaseLoaded failed: " + ex);
            }
        }
    }
}