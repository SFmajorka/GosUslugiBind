using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GosUslugiBind.Models;

namespace GosUslugiBind.Services
{
    public class ProfileManager
    {
        private readonly string _appDataPath;
        private Dictionary<string, List<BinderItem>> _profiles;
        public string CurrentProfile { get; private set; } = "default";

        public event Action? OnProfilesChanged;
        public event Action? OnCurrentProfileChanged;

        public ProfileManager()
        {
            _appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BinderApp"
            );
            Directory.CreateDirectory(_appDataPath);
            LoadProfiles();
        }

        private void LoadProfiles()
        {
            var filePath = Path.Combine(_appDataPath, "profiles.json");
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    _profiles = JsonSerializer.Deserialize<Dictionary<string, List<BinderItem>>>(json)
                                ?? new Dictionary<string, List<BinderItem>>();
                }
                catch
                {
                    _profiles = new Dictionary<string, List<BinderItem>>();
                }
            }
            else
            {
                _profiles = new Dictionary<string, List<BinderItem>>();
            }

            if (!_profiles.ContainsKey("default"))
            {
                _profiles["default"] = new List<BinderItem>
                {
                    new BinderItem { Key = "Ctrl+1", Action = "/e dance", Condition = "all" },
                    new BinderItem { Key = "Ctrl+2", Action = "/e wave", Condition = "all" },
                    new BinderItem { Key = "Alt+Z", Action = "Привет!", Condition = "roblox" },
                    new BinderItem { Key = "Shift+F", Action = "/fly", Condition = "roblox" },
                };
            }
            if (!_profiles.ContainsKey("roblox"))
            {
                _profiles["roblox"] = new List<BinderItem>
                {
                    new BinderItem { Key = "Ctrl+1", Action = "/e dance", Condition = "roblox" },
                    new BinderItem { Key = "Ctrl+2", Action = "/e wave", Condition = "roblox" },
                    new BinderItem { Key = "Ctrl+3", Action = "/e laugh", Condition = "roblox" },
                };
            }
            if (!_profiles.ContainsKey("work"))
            {
                _profiles["work"] = new List<BinderItem>
                {
                    new BinderItem { Key = "Ctrl+C", Action = "Копировать", Condition = "all" },
                    new BinderItem { Key = "Ctrl+V", Action = "Вставить", Condition = "all" },
                };
            }

            SaveProfiles();
        }

        public void SaveProfiles()
        {
            try
            {
                var json = JsonSerializer.Serialize(_profiles, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Path.Combine(_appDataPath, "profiles.json"), json);
                OnProfilesChanged?.Invoke();
            }
            catch { }
        }

        public List<BinderItem> GetCurrentBinds()
        {
            return _profiles.ContainsKey(CurrentProfile) ? _profiles[CurrentProfile] : new List<BinderItem>();
        }

        public void SetCurrentBinds(List<BinderItem> binds)
        {
            if (_profiles.ContainsKey(CurrentProfile))
            {
                _profiles[CurrentProfile] = binds;
                SaveProfiles();
            }
        }

        public List<string> GetProfileNames()
        {
            return _profiles.Keys.ToList();
        }

        public void SwitchProfile(string name)
        {
            if (_profiles.ContainsKey(name) && name != CurrentProfile)
            {
                CurrentProfile = name;
                OnCurrentProfileChanged?.Invoke();
            }
        }

        public void AddProfile(string name)
        {
            if (!_profiles.ContainsKey(name))
            {
                _profiles[name] = new List<BinderItem>();
                SaveProfiles();
            }
        }

        public void DeleteProfile(string name)
        {
            if (name != "default" && _profiles.ContainsKey(name))
            {
                _profiles.Remove(name);
                SaveProfiles();
                if (CurrentProfile == name) SwitchProfile("default");
            }
        }

        public void IncrementUse(string key)
        {
            var binds = GetCurrentBinds();
            var bind = binds.FirstOrDefault(b => b.Key == key);
            if (bind != null)
            {
                bind.Uses++;
                bind.LastUsed = DateTime.Now;
                SetCurrentBinds(binds);
            }
        }

        public void ExportProfile(string filePath)
        {
            var data = new
            {
                profile = CurrentProfile,
                binds = GetCurrentBinds(),
                exported = DateTime.Now
            };
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public void ImportProfile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<dynamic>(json);
            string name = data?.profile?.ToString() ?? "imported";
            var binds = JsonSerializer.Deserialize<List<BinderItem>>(data?.binds?.ToString() ?? "[]");
            if (binds != null)
            {
                if (!_profiles.ContainsKey(name))
                {
                    _profiles[name] = binds;
                }
                else
                {
                    _profiles[name + "_imported"] = binds;
                }
                SaveProfiles();
            }
        }
    }
}