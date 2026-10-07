using BepInEx.Configuration;
using UnityEngine;

namespace SPT_silent_speed
{
    internal static class Settings
    {
        public static ConfigEntry<bool> Enabled;
        public static ConfigEntry<KeyboardShortcut> MaxSilentKey;
        public static ConfigEntry<float> SafetyMargin;

        public static void Init(ConfigFile config)
        {
            Enabled = config.Bind(
                "General",
                "Enabled",
                true,
                "When disabled the mod does nothing and vanilla walk stays.");

            MaxSilentKey = config.Bind(
                "Keybinds",
                "Max Silent Key",
                new KeyboardShortcut(KeyCode.Keypad0),
                "Sets movement speed to the maximum silent limit.");

            SafetyMargin = config.Bind(
                "General",
                "Safety Margin",
                0.02f,
                new ConfigDescription(
                    "Subtracted from computed silent speed to stay reliably silent.",
                    new AcceptableValueRange<float>(0f, 0.1f)));
        }
    }
}
