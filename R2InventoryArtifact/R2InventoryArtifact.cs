using BepInEx;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace R2InventoryArtifact
{
    
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class R2InventoryArtifact : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "foxfen64";
        public const string PluginName = "R2InventoryArtifact";
        public const string PluginVersion = "0.0.1";

        public void Awake()
        {
            
        }
    }
}
