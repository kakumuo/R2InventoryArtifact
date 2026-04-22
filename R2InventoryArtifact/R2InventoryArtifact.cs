using BepInEx;
using R2API;
using R2API.ContentManagement;
using R2InventoryArtifact.Hooks;
using R2InventoryArtifact.UI.Services;
using UnityEngine;
using R2InventoryArtifact.Artifact;
using R2InventoryArtifact.UI.Builders;


namespace R2InventoryArtifact
{
    
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(R2APIContentManager.PluginGUID)]
    [BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class R2InventoryArtifact : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "foxfen64";
        public const string PluginName = "R2InventoryArtifact";
        public const string PluginVersion = "0.0.1";
        public static R2InventoryArtifact Instance; 

        public void Awake()
        {
            if(Instance) Destroy(Instance); 
            Instance = this; 
            Log.Initialize(Logger); 

            PluginConfig.Initialize(Config, Info); 
            InventoryArtifactProvider.Initialize(Info); 
            InventoryService.Initialize(Info); 
            ComponentBuilder.Initialize(); 

            GameObject pluginObj = new GameObject("R2InventoryArtifactPlugin"); 
            DontDestroyOnLoad(pluginObj); 
            pluginObj.AddComponent<UIHook>(); 
            pluginObj.AddComponent<LevelUpHook>(); 
            pluginObj.AddComponent<InventoryHook>(); 
            pluginObj.AddComponent<DebugManager>(); 

            Log.Info("R2InventoryArtifactPlugin Initialized..."); 
        }
    }
}
