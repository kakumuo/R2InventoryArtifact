


using BepInEx.Configuration;
using R2InventoryArtifact.Util;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;

namespace R2InventoryArtifact
{
    // TODO: add configs for item properties
    public static class PluginConfig
    {

        public static ConfigEntry<KeyboardShortcut> ShowInventoryKey {get; private set; }
        public static ConfigEntry<bool> ToggleShowInventory {get; private set;}
        public static ConfigEntry<int> InventoryHeight {get; private set;}
        public static ConfigEntry<int> InventoryWidth {get; private set;}
        public static ConfigEntry<float> UIScale { get; private set; }

        public static void Initialize(ConfigFile config, BepInEx.PluginInfo pluginInfo)
        {
            ShowInventoryKey = config.Bind(
                section:        "Inventory", 
                key:            "Show Inventory Keybind", 
                description:    "Keybind for Showing and hiding the inventory",
                defaultValue:   new KeyboardShortcut(UnityEngine.KeyCode.Q)
            ); 

            ToggleShowInventory  = config.Bind(
                section:        "Inventory", 
                key:            "Toggle Inventory", 
                description:    "Toggle or Show inventory",
                defaultValue:   false
            ); 

            InventoryWidth  = config.Bind(
                section:            "Inventory", 
                key:                "Inventory Width", 
                defaultValue:       5,
                configDescription:  new ConfigDescription( "Number of cells for the width of the inventory", new AcceptableValueRange<int>(5, 10)) 
            ); 

            InventoryHeight  = config.Bind(
                section:            "Inventory", 
                key:                "Inventory Height", 
                defaultValue:       30,
                configDescription:  new ConfigDescription( "Number of cells for the height of the inventory", new AcceptableValueRange<int>(20, 50)) 
            ); 

            UIScale = config.Bind(
                section:        "Inventory", 
                key:            "UI Scale", 
                defaultValue:   1.0f,
                description:    "Scale multiplier for the level-up selection UI (0.5 = half size, 2.0 = double)."
            );

            // REGISTER PROPS
            try
            {
                string basePath = System.IO.Path.GetDirectoryName(pluginInfo.Location);
                string iconPath = System.IO.Path.Combine(basePath, "Assets", "icon.png");
                Sprite icon = R2InventoryArtifactUtil.LoadSprite(iconPath);
                ModSettingsManager.SetModIcon(icon);
            }
            catch (System.Exception e)
            {
                Log.Warning($"Failed to load mod icon: {e.Message}");
            }
           

            ModSettingsManager.SetModDescription("Artifact of Inventory - Manage your inventory");

            ModSettingsManager.AddOption(new KeyBindOption(ShowInventoryKey)); 
            ModSettingsManager.AddOption(new CheckBoxOption(ToggleShowInventory)); 
            ModSettingsManager.AddOption(new IntSliderOption(InventoryWidth)); 
            ModSettingsManager.AddOption(new IntSliderOption(InventoryHeight)); 
            ModSettingsManager.AddOption(new StepSliderOption(UIScale, new StepSliderConfig { min = 0.5f, max = 2.0f, increment = 0.1f }));
        }
    }
}