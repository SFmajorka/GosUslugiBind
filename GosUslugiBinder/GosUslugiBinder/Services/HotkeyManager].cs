using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GosUslugiBind.Models;

namespace GosUslugiBind.Services
{
    public class HotkeyManager : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly IntPtr _windowHandle;
        private readonly Dictionary<int, string> _hotkeyActions = new();
        private int _nextId = 1;

        public event Action<string>? OnHotkeyPressed;

        public HotkeyManager(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        public void RegisterHotkey(string keyCombination, string action)
        {
            var (modifiers, key) = ParseKeyCombination(keyCombination);
            if (key == Keys.None) return;

            var id = _nextId++;
            if (RegisterHotKey(_windowHandle, id, modifiers, (uint)key))
            {
                _hotkeyActions[id] = action;
                Debug.WriteLine($"Hotkey registered: {keyCombination} (ID: {id})");
            }
            else
            {
                Debug.WriteLine($"Failed to register hotkey: {keyCombination}");
            }
        }

        public void UnregisterAll()
        {
            foreach (var id in _hotkeyActions.Keys)
            {
                UnregisterHotKey(_windowHandle, id);
            }
            _hotkeyActions.Clear();
            _nextId = 1;
            Debug.WriteLine("All hotkeys unregistered");
        }

        public void ReloadHotkeys(List<BinderItem> binds)
        {
            UnregisterAll();
            foreach (var bind in binds)
            {
                RegisterHotkey(bind.Key, bind.Action);
            }
        }

        private (uint modifiers, Keys key) ParseKeyCombination(string combo)
        {
            uint modifiers = 0;
            Keys key = Keys.None;

            var parts = combo.Split('+');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                switch (trimmed.ToLower())
                {
                    case "ctrl":
                        modifiers |= 2;
                        break;
                    case "shift":
                        modifiers |= 4;
                        break;
                    case "alt":
                        modifiers |= 1;
                        break;
                    default:
                        if (Enum.TryParse<Keys>(trimmed, true, out var parsedKey))
                        {
                            key = parsedKey;
                        }
                        break;
                }
            }

            return (modifiers, key);
        }

        public void ProcessHotkey(int id)
        {
            if (_hotkeyActions.TryGetValue(id, out var action))
            {
                OnHotkeyPressed?.Invoke(action);
            }
        }

        public void Dispose()
        {
            UnregisterAll();
        }
    }
}