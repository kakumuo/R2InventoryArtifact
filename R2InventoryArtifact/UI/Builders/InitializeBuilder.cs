

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; 
using UnityEngine.AddressableAssets;
using RoR2BepInExPack.GameAssetPaths;
using System.Linq;
using UnityEngine.UIElements.Collections;


namespace R2InventoryArtifact.UI.Builders
{


    public static partial class ComponentBuilder
    {
        public enum SpritePanelType
        {
            HEADER, TILE, PANEL,
            DISABLED_TILE,
            HANDLE, 
            BACKDROP
        }

        private static Dictionary<SpritePanelType, Sprite> _spriteDict; 

        public static void Initialize()
        {
            _spriteDict = new Dictionary<SpritePanelType, Sprite>
            {
                { SpritePanelType.HEADER, Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHighlightHeader.png").WaitForCompletion() },
                { SpritePanelType.TILE, Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHighlightBoxOutline.png").WaitForCompletion() },
                { SpritePanelType.DISABLED_TILE, Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHatchingTile.png").WaitForCompletion() },
                { SpritePanelType.PANEL, Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUICleanPanel.png").WaitForCompletion() },
                { SpritePanelType.HANDLE, Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHandle.png").WaitForCompletion() },
                { SpritePanelType.BACKDROP, Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIBackdrop.png").WaitForCompletion() }
            }; 
        }

        public static Sprite GetSprite(SpritePanelType type)
        {
            return _spriteDict[type]; 
        }
    }
}
