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

                // TODO: Підписатися на події GameDatabase, щоб фільтрувати ConfigNode
                // GameEvents.OnGameDatabaseLoaded.Add(OnDatabaseLoaded);
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

        // Приклад гака: фільтрувати ноди для вимкнених паків (псевдологіка)
        // private void OnDatabaseLoaded()
        // {
        //     var disabledPacks = ...;
        //     foreach (var node in GameDatabase.Instance.root.AllConfigs)
        //     {
        //         // Якщо node належить до каталогу паку X і X вимкнений — виключити його
        //     }
        // }
    }
}