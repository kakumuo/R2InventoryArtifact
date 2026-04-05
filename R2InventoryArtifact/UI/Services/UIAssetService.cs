using R2InventoryArtifact.Util.R2API;
using UnityEngine;

namespace R2InventoryArtifact.UI.Services
{
    public static class UIAssetService
    {
        public static Sprite GetSprite(R2ItemCode itemCode)
        {
            string spritePath = ""; 
            switch(itemCode)
            {
                case R2ItemCode.BisonSteak:         spritePath = "Sprites/Bison_Steak"; break; 
                case R2ItemCode.BundleOfFireworks:  spritePath = "Sprites/Bundle_Of_Fireworks"; break; 
                case R2ItemCode.IgnitionTank:       spritePath = "Sprites/Ignition_Tank"; break; 
                case R2ItemCode.RunaldsBand:        spritePath = "Sprites/Runalds_Band"; break; 
                case R2ItemCode.SingularityBand:    spritePath = "Sprites/Singularity_Band"; break; 
                case R2ItemCode.EmptyBottle:        spritePath = "Sprites/Empty_Bottle"; break; 
                default: break; 
            }

            return Resources.Load<Sprite>(spritePath); 
        }
    }
}