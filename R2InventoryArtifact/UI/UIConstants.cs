using UnityEngine; 
using R2InventoryArtifact.Util.R2API;
using System;

namespace R2InventoryArtifact.UI
{
    public static class UIConstants {
        public const int SPACING_SX = 2; 
        public const int SPACING_SM = 4; 
        public const int SPACING_MD = 8; 
        public const int SPACING_LG = 12; 

        public const int FONT_SM = 8; 
        public const int FONT_MD = 12; 
        public const int FONT_LG = 16; 

        private const float ALPHA = .5f; 
        public static Color COLOR_ITEM_SLOT_NEUTRAL         = ColorFromHex("#ffffff", ALPHA); 
        public static Color COLOR_ITEM_SLOT_ACTIVE          = ColorFromHex("#daa520", ALPHA); 
        public static Color COLOR_ITEM_SLOT_ACTIVE_OUTLINE  = ColorFromHex("#b8860b", ALPHA);
        public static Color COLOR_ITEM_SLOT_HOVER_VALID     = ColorFromHex("#008000", ALPHA);
        public static Color COLOR_ITEM_SLOT_HOVER_INVALID   = ColorFromHex("#FF0000", ALPHA);
        public static Color COLOR_ITEM_SLOT_LOCKED          = ColorFromHex("#6b6b6b", ALPHA);

        // (Base Color, Border Color)
        public static (Color, Color) GetItemTeirColor(R2ItemTier tier)
        {
            switch(tier)
            {  
                case R2ItemTier.T1:     return (ColorFromHex("#6b6b6b", ALPHA), ColorFromHex("#6b6b6b", ALPHA));
                case R2ItemTier.T2:     return (ColorFromHex("#a0ff9a", ALPHA), ColorFromHex("#2f7d32", ALPHA));
                case R2ItemTier.T3:     return (ColorFromHex("#ff6b6b", ALPHA), ColorFromHex("#8b1a1a", ALPHA));
                case R2ItemTier.BOSS:   return (ColorFromHex("#ffd966", ALPHA), ColorFromHex("#b38f00", ALPHA));
                case R2ItemTier.LUNAR:  return (ColorFromHex("#7fbfff", ALPHA), ColorFromHex("#1f4f99", ALPHA));
                case R2ItemTier.VT1:    return (ColorFromHex("#d98cff", ALPHA), ColorFromHex("#6b2f8f", ALPHA));
                case R2ItemTier.VT2:    return (ColorFromHex("#d98cff", ALPHA), ColorFromHex("#6b2f8f", ALPHA));
                case R2ItemTier.VT3:    return (ColorFromHex("#d98cff", ALPHA), ColorFromHex("#6b2f8f", ALPHA));
                case R2ItemTier.VBOSS:  return (ColorFromHex("#d98cff", ALPHA), ColorFromHex("#6b2f8f", ALPHA));
                default:                return (ColorFromHex("#6b6b6b", ALPHA), ColorFromHex("#6b6b6b", ALPHA));
            }
        }

        public static Color ColorFromHex(string hex, float alpha = 1)
        {
            float r = 0, g = 0, b = 0; 
            hex = hex.Trim().Replace("#", "");
            r = Convert.ToInt32(hex.Substring(0, 2), 16) / 256.0f; 
            g = Convert.ToInt32(hex.Substring(2, 4), 16) / 256.0f; 
            b = Convert.ToInt32(hex.Substring(4, 6), 16) / 256.0f; 
            return new Color(r, g, b, alpha); 
        }
    }
}
