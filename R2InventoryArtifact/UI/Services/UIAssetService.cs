using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

namespace R2InventoryArtifact.UI.Services
{
    public static class UIAssetService
    {
        public static void Initialize(){}

        public static Sprite GetSprite(UniquePickup pickup)
        {
            // return Sprite.Create(pickup.pickupIndex.pickupDef.iconTexture as Texture2D, new(0, 0, 128, 128), Vector2.zero); 
            return pickup.pickupIndex.pickupDef.iconSprite; 
        }
    }
}