
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2InventoryArtifact.Util;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

namespace R2InventoryArtifact.Artifact
{
    public class InventoryArtifactProvider : IContentPackProvider
    {
        private static string _basePath; 
        public ContentPack ContentPack = new(); 
        public static ArtifactDef ArtifactDef; 

        public string identifier => "R2InventoryArtifact.R2InventoryArtifact";

        internal static void Initialize(BepInEx.PluginInfo pluginInfo)
        {
            _basePath = System.IO.Path.GetDirectoryName(pluginInfo.Location); 

            LanguageAPI.Add("ARTIFACT_FOXFEN64_INVENTORY_NAME", "Artifact of Inventory");
            LanguageAPI.Add("ARTIFACT_FOXFEN64_INVENTORY_DESC", "Manage your inventory.");

            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private static void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new InventoryArtifactProvider());
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(ContentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            ArtifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
            ArtifactDef.cachedName = "ARTIFACT_FOXFEN64_INVENTORY";
            ArtifactDef.nameToken = "ARTIFACT_FOXFEN64_INVENTORY_NAME";
            ArtifactDef.descriptionToken = "ARTIFACT_FOXFEN64_INVENTORY_DESC";

            if (!string.IsNullOrEmpty(_basePath))
            {
                string onPath = System.IO.Path.Combine(_basePath, "Assets", "aoi-on.png");
                string offPath = System.IO.Path.Combine(_basePath, "Assets", "aoi-off.png");

                ArtifactDef.smallIconSelectedSprite = R2InventoryArtifactUtil.LoadSprite(onPath); 
                ArtifactDef.smallIconDeselectedSprite = R2InventoryArtifactUtil.LoadSprite(offPath);
            }

            ContentPack.artifactDefs.Add([ArtifactDef]); 
            yield break; 
        }

        public static bool IsEnabled()
        {
            return RunArtifactManager.instance && ArtifactDef && RunArtifactManager.instance.IsArtifactEnabled(ArtifactDef); 
        }
    }
}