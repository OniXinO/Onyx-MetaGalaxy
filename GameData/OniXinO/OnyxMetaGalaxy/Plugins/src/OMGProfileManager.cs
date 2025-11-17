using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
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
        private bool filterDisabledPacks = false;
        private bool filterSafeMode = true;

        private void Awake()
        {
            try
            {
                GameDataPath = Path.Combine(KSPUtil.ApplicationRootPath, "GameData");
                OMGRoot = Path.Combine(GameDataPath, "OnyxMetaGalaxy");
                SettingsPath = Path.Combine(OMGRoot, "OMGSettings.cfg");

                var activeProfile = ReadActiveProfileName(SettingsPath) ?? "default";
                var profilePath = Path.Combine(OMGRoot, "Profiles", activeProfile + ".cfg");
                var packs = ReadPacks(profilePath);
                Log($"OMG active profile: {activeProfile} ({packs.Count} packs)");

                // Налаштування фільтрації
                filterDisabledPacks = ReadBoolSetting(SettingsPath, "filterDisabledPacks", false);
                filterSafeMode = ReadBoolSetting(SettingsPath, "filterSafeMode", true);
                Log($"Settings: filterDisabledPacks={filterDisabledPacks}, filterSafeMode={filterSafeMode}");

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

        private bool ReadBoolSetting(string settingsPath, string key, bool @default)
        {
            try
            {
                if (!File.Exists(settingsPath)) return @default;
                foreach (var raw in File.ReadAllLines(settingsPath))
                {
                    var t = raw.Trim();
                    if (t.StartsWith(key))
                    {
                        var parts = t.Split('=');
                        if (parts.Length >= 2)
                        {
                            var v = parts[1].Trim().ToLowerInvariant();
                            return v == "true" || v == "1" || v == "yes";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"ReadBoolSetting error for '{key}': {ex}");
            }
            return @default;
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

        // Фільтрація ConfigNode за шляхами файлів для вимкнених паків
        private void OnDatabaseLoaded(List<(string id, bool enabled)> packs)
        {
            try
            {
                var disabled = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var p in packs) if (!p.enabled) disabled.Add(p.id ?? string.Empty);
                Log($"DB loaded. Disabled packs: {string.Join(",", disabled)}");

                if (!filterDisabledPacks || disabled.Count == 0)
                {
                    Log("Filtering disabled or no disabled packs.");
                    return;
                }

                var root = GameDatabase.Instance.root; // UrlDir
                if (root == null)
                {
                    Log("GameDatabase root is null; skipping filtering.");
                    return;
                }

                // Reflection to access UrlDir.AllFiles and UrlFile.configs (List<UrlConfig>)
                var allFilesProp = root.GetType().GetProperty("AllFiles");
                IEnumerable<object> files = null;
                if (allFilesProp != null)
                {
                    files = allFilesProp.GetValue(root, null) as IEnumerable<object>;
                }

                if (files == null)
                {
                    Log("UrlDir.AllFiles not available; logging only.");
                    return;
                }

                int totalCandidates = 0;
                int totalRemoved = 0;

                foreach (var file in files)
                {
                    string url = SafeGetStringProp(file, "url"); // e.g. "OuterPlanetsMod/Config/OPM.cfg"
                    if (string.IsNullOrEmpty(url)) continue;
                    var parts = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0) continue;
                    string top = parts[0];
                    if (!disabled.Contains(top)) continue;

                    int cfgCount = SafeGetListCount(file, "configs");
                    if (cfgCount <= 0) continue;
                    totalCandidates += cfgCount;

                    if (filterSafeMode)
                    {
                        Log($"SafeMode: would remove {cfgCount} config(s) from '{url}'");
                    }
                    else
                    {
                        int removed = ClearList(file, "configs");
                        totalRemoved += removed;
                        Log($"Removed {removed} config(s) from '{url}'");
                    }
                }

                Log($"Filtering complete. Candidates={totalCandidates}, Removed={totalRemoved}, SafeMode={filterSafeMode}");
            }
            catch (Exception ex)
            {
                Log("OMG OnDatabaseLoaded failed: " + ex);
            }
        }

        private string SafeGetStringProp(object obj, string propName)
        {
            try
            {
                var p = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (p == null) return null;
                var v = p.GetValue(obj, null);
                return v as string ?? v?.ToString();
            }
            catch { return null; }
        }

        private int SafeGetListCount(object obj, string fieldName)
        {
            try
            {
                var f = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f == null) return 0;
                var list = f.GetValue(obj) as System.Collections.ICollection;
                return list?.Count ?? 0;
            }
            catch { return 0; }
        }

        private int ClearList(object obj, string fieldName)
        {
            try
            {
                var f = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f == null) return 0;
                var list = f.GetValue(obj) as System.Collections.IList;
                int count = list?.Count ?? 0;
                list?.Clear();
                return count;
            }
            catch (Exception ex)
            {
                Log($"ClearList error for field '{fieldName}': {ex}");
                return 0;
            }
        }
    }
}