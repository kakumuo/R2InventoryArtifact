


using BepInEx.Configuration;
using R2InventoryArtifact.Util;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;

namespace R2InventoryArtifact
{
    public enum InventoryShowType
    {
        ToggleShow,
        HoldToShow, 
    }

    // TODO: add configs for item properties
    public static class PluginConfig
    {

        public static ConfigEntry<KeyboardShortcut> ShowInventoryKey {get; private set; }
        public static ConfigEntry<KeyboardShortcut> RotateInventoryItemKey {get; private set; }
        public static ConfigEntry<InventoryShowType> InventoryShowType {get; private set;}
        public static ConfigEntry<bool> DisableAutoPickup {get; private set; }
        public static ConfigEntry<int> InventoryHeight {get; private set;}
        public static ConfigEntry<int> InventoryWidth {get; private set;}
        public static ConfigEntry<float> UIScale { get; private set; }

        private const int INVENTORY_WIDTH_MIN_VALUE = 7; 
        private const int INVENTORY_WIDTH_MAX_VALUE = 12; 
        private const int INVENTORY_HEIGHT_MIN_VALUE = 20; 
        private const int INVENTORY_HEIGHT_MAX_VALUE = 50; 

        public static void Initialize(ConfigFile config, BepInEx.PluginInfo pluginInfo)
        {
            /***************************** INVENTORY *****************************/
            InventoryShowType = config.Bind(
                section:        "Inventory", 
                key:            "Toggle Inventory", 
                description:    "Toggle or Show inventory",
                defaultValue:   (InventoryShowType)0
            ); 

            DisableAutoPickup = config.Bind(
                section:        "Inventory", 
                key:            "Disable Autopickup", 
                description:    "Disables automatically picking up items by approaching them",
                defaultValue:   false
            ); 

            InventoryWidth  = config.Bind(
                section:            "Inventory", 
                key:                "Inventory Width", 
                defaultValue:       (INVENTORY_WIDTH_MIN_VALUE + INVENTORY_WIDTH_MAX_VALUE) / 2,
                configDescription:  new ConfigDescription( "Number of cells for the width of the inventory", new AcceptableValueRange<int>(INVENTORY_WIDTH_MIN_VALUE, INVENTORY_WIDTH_MAX_VALUE)) 
            ); 

            InventoryHeight  = config.Bind(
                section:            "Inventory", 
                key:                "Inventory Height", 
                defaultValue:       (INVENTORY_HEIGHT_MIN_VALUE + INVENTORY_HEIGHT_MAX_VALUE) / 2,
                configDescription:  new ConfigDescription( "Number of cells for the height of the inventory", new AcceptableValueRange<int>(INVENTORY_HEIGHT_MIN_VALUE, INVENTORY_HEIGHT_MAX_VALUE)) 
            ); 

            /***************************** KEYBINDS *****************************/
            ShowInventoryKey = config.Bind(
                section:        "Keybinds", 
                key:            "Show Inventory Keybind", 
                description:    "Keybind for Showing and hiding the inventory",
                defaultValue:   new KeyboardShortcut(UnityEngine.KeyCode.Q)
            ); 

            RotateInventoryItemKey  = config.Bind(
                section:        "Keybinds", 
                key:            "Rotate Inventory Item", 
                description:    "Keybind for rotating the inventory",
                defaultValue:   new KeyboardShortcut(UnityEngine.KeyCode.Mouse1)
            ); 


            /***************************** UI *****************************/
            UIScale = config.Bind(
                section:        "UI", 
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
            ModSettingsManager.AddOption(new KeyBindOption(RotateInventoryItemKey)); 
            ModSettingsManager.AddOption(new ChoiceOption(InventoryShowType)); 
            ModSettingsManager.AddOption(new IntSliderOption(InventoryWidth, new IntSliderConfig{ min = INVENTORY_WIDTH_MIN_VALUE, max=INVENTORY_WIDTH_MAX_VALUE })); 
            ModSettingsManager.AddOption(new IntSliderOption(InventoryHeight, new IntSliderConfig{ min = INVENTORY_HEIGHT_MIN_VALUE, max=INVENTORY_HEIGHT_MAX_VALUE })); 
            ModSettingsManager.AddOption(new StepSliderOption(UIScale, new StepSliderConfig { min = 0.5f, max = 2.0f, increment = 0.1f }));
        }
    }
}