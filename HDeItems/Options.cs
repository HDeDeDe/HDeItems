using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using HarmonyLib;
using RiskOfOptions;
using RiskOfOptions.Components.Options;
using RiskOfOptions.Components.Panel;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;
using UnityEngine.Events;

namespace HDeMods.HDeItems.Options {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal static class RoO {
        public static bool Enabled =>
            BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");

        public delegate void LogFunc(object data);

        private static string modGUID;
        private static string modNAME;
        private static LogFunc logError;
        private static bool initialized;
#if DEBUG
        private static LogFunc logDebug;
#endif

        public static void Init(string modGuid, string modName, LogFunc errorFunc, LogFunc debugFunc = null) {
#if DEBUG
            logDebug = debugFunc;
#endif
            logError = errorFunc;
            modGUID = modGuid;
            modNAME = modName;
            if (!Enabled) {
                logError("Risk of Options is not present, the author of " + modNAME +
                         " did not check for this! Mod GUID: " + modGUID);
                return;
            }
            initialized = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddCheck(ConfigEntry<bool> option, bool requireRestart = false) {
            if (!initialized) return;
            LocalizedCheckBoxOption boxOption = new LocalizedCheckBoxOption(option, requireRestart);
            ModSettingsManager.AddOption(boxOption, modGUID,
                modNAME);
#if DEBUG
            if (logDebug == null) return;
            logDebug(boxOption.GetNameToken());
            logDebug(boxOption.GetDescriptionToken());
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddInt(ConfigEntry<int> option, int minimum, int maximum) {
            if (!initialized) return;
            LocalizedIntSliderOption sliderOption =
                new LocalizedIntSliderOption(option, new IntSliderConfig() { min = minimum, max = maximum });
            ModSettingsManager.AddOption(sliderOption, modGUID,
                modNAME);
#if DEBUG
            if (logDebug == null) return;
            logDebug(sliderOption.GetNameToken());
            logDebug(sliderOption.GetDescriptionToken());
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddFloat(ConfigEntry<float> option, float minimum, float maximum,
            string format = "{0:0}%") {
            if (!initialized) return;
            LocalizedSliderOption sliderOption = new LocalizedSliderOption(option,
                new SliderConfig() { min = minimum, max = maximum, FormatString = format });
            ModSettingsManager.AddOption(sliderOption, modGUID,
                modNAME);
#if DEBUG
            if (logDebug == null) return;
            logDebug(sliderOption.GetNameToken());
            logDebug(sliderOption.GetDescriptionToken());
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddFloatStep(ConfigEntry<float> option, float minimum, float maximum, float step,
            string format = "{0:0}%") {
            if (!initialized) return;
            LocalizedSliderStepOption stepSliderOption = new LocalizedSliderStepOption(option, new StepSliderConfig()
                { min = minimum, max = maximum, FormatString = format, increment = step });
            ModSettingsManager.AddOption(stepSliderOption, modGUID,
                modNAME);
#if DEBUG
            if (logDebug == null) return;
            logDebug(stepSliderOption.GetNameToken());
            logDebug(stepSliderOption.GetDescriptionToken());
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddButton(string name, string category, UnityAction onButtonPressed) {
            if (!initialized) return;
            LocalizedButtonOption buttonOption = new LocalizedButtonOption(name, category, "", "", onButtonPressed);
            ModSettingsManager.AddOption(buttonOption, modGUID,
                modNAME);
#if DEBUG
            if (logDebug == null) return;
            logDebug(buttonOption.GetNameToken());
            logDebug(buttonOption.GetDescriptionToken());
            logDebug(buttonOption.GetButtonLabelToken());
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SetSprite(Sprite sprite) => ModSettingsManager.SetModIcon(sprite);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SetDescriptionToken(string description) =>
            ModSettingsManager.SetModDescriptionToken(description);

        // Thanks to Bubbet for the suggestion and .score for helping me get it working
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void ResetToDefault() {
            if (!initialized) return;
            // I'm too lazy to find a proper way of doing this
            GameObject panel = GameObject.Find("SettingsPanelTitle(Clone)");
            if (panel == null) panel = GameObject.Find("SettingsPanel(Clone)");

            ModOptionPanelController options = panel.GetComponent<ModOptionPanelController>();

            foreach (ModSetting setting in options._modSettings) {
                if (setting.GetType() == typeof(GenericButtonController)) continue;
                AccessTools.Method(setting.GetType(), "ResetToDefault")?.Invoke(setting, null);
            }
        }
    }
    
    // Thanks to Bubbet for the suggestion to do this
    internal class LocalizedCheckBoxOption : CheckBoxOption {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public LocalizedCheckBoxOption(ConfigEntry<bool> configEntry, bool restart) : base(configEntry, restart) {
            RoR2.Language.onCurrentLanguageChanged += ResetDescription;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public override void RegisterTokens() {
            Description = RoR2.Language.GetString(GetDescriptionToken());
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void ResetDescription() {
            Description = RoR2.Language.GetString(GetDescriptionToken());
        }
    }

    internal class LocalizedIntSliderOption : IntSliderOption {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public LocalizedIntSliderOption(ConfigEntry<int> configEntry, IntSliderConfig config) : base(configEntry, config) {
            RoR2.Language.onCurrentLanguageChanged += ResetDescription;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public override void RegisterTokens() {
            Description = RoR2.Language.GetString(GetDescriptionToken());
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void ResetDescription() {
            Description = RoR2.Language.GetString(GetDescriptionToken());
        }
    }

    internal class LocalizedSliderOption : SliderOption {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public LocalizedSliderOption(ConfigEntry<float> configEntry, SliderConfig config) : base(configEntry, config) {
            RoR2.Language.onCurrentLanguageChanged += ResetDescription;
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public override void RegisterTokens() {
            Description = RoR2.Language.GetString(GetDescriptionToken());
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void ResetDescription() {
            Description = RoR2.Language.GetString(GetDescriptionToken());
        }
    }

    internal class LocalizedSliderStepOption : StepSliderOption {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public LocalizedSliderStepOption(ConfigEntry<float> configEntry, StepSliderConfig config) : base(configEntry,
            config) {
            RoR2.Language.onCurrentLanguageChanged += ResetDescription;
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public override void RegisterTokens() {
            Description = RoR2.Language.GetString(GetDescriptionToken());
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void ResetDescription() {
            Description = RoR2.Language.GetString(GetDescriptionToken());
        }
    }
    internal class LocalizedButtonOption : GenericButtonOption {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public LocalizedButtonOption(string name, string category, string description, string buttonText, UnityAction onButtonPressed) 
            : base(name, category, description, buttonText, onButtonPressed) {
            RoR2.Language.onCurrentLanguageChanged += ResetDescription;
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public override void RegisterTokens() {
            Description = RoR2.Language.GetString(GetDescriptionToken());
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void ResetDescription() {
            Description = RoR2.Language.GetString(GetDescriptionToken());
        }
    }
    
}
