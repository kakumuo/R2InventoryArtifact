using System.Collections;
using IL.RoR2;
using R2InventoryArtifact.Model;
using R2InventoryArtifact.Util.R2API;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

namespace R2InventoryArtifact.UI.Services
{
    public static class UIAssetService
    {
        public static void Initialize(){}

        public static Sprite GetSprite(InventoryIndex inventoryIndex)
        {
            Texture targetTexture;
            if (inventoryIndex.ItemIndex != ItemIndex.None)
                targetTexture = ContentManager.itemDefs[(int)inventoryIndex.ItemIndex].pickupIconTexture; 
            else if(inventoryIndex.EquipmentIndex != EquipmentIndex.None)
                targetTexture = ContentManager.equipmentDefs[(int)inventoryIndex.ItemIndex].pickupIconTexture;  
            else return null; 
            
            // TODO: find how to make sprite not transparent
            return Sprite.Create(targetTexture as Texture2D, new(0, 0, 128, 128), Vector2.zero); 
        }
    }
}