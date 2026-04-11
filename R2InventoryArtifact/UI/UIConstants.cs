using UnityEngine; 
using R2InventoryArtifact.Util.R2API;
using System;
using RoR2;

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
        public readonly static Color COLOR_ITEM_SLOT_NEUTRAL         = ColorFromHex("#ffffff", ALPHA); 
        public readonly static Color COLOR_ITEM_SLOT_ACTIVE          = ColorFromHex("#daa520", ALPHA); 
        public readonly static Color COLOR_ITEM_SLOT_ACTIVE_OUTLINE  = ColorFromHex("#b8860b", ALPHA);
        public readonly static Color COLOR_ITEM_SLOT_HOVER_VALID     = ColorFromHex("#008000", ALPHA);
        public readonly static Color COLOR_ITEM_SLOT_HOVER_INVALID   = ColorFromHex("#FF0000", ALPHA);
        public readonly static Color COLOR_ITEM_SLOT_LOCKED          = ColorFromHex("#6b6b6b", ALPHA);

        // tooltip
        public readonly static Color COLOR_TOOLTIP_TITLE_SLOT_LOCKED = ColorFromHex("#414141", ALPHA);
        public readonly static Color COLOR_TOOLTIP_BODY_SLOT_LOCKED  = ColorFromHex("#d6d6d6", ALPHA);

        // (Base Color, Border Color)
        public static (Color, Color) GetItemTeirColor(ItemTier tier)
        {
            switch(tier)
            {  
                case ItemTier.Tier1:        return (ColorFromHex("#6b6b6b", ALPHA), ColorFromHex("#6b6b6b", ALPHA));
                case ItemTier.Tier2:        return (ColorFromHex("#a0ff9a", ALPHA), ColorFromHex("#2f7d32", ALPHA));
                case ItemTier.Tier3:        return (ColorFromHex("#ff6b6b", ALPHA), ColorFromHex("#8b1a1a", ALPHA));
                case ItemTier.Boss:         return (ColorFromHex("#ffd966", ALPHA), ColorFromHex("#b38f00", ALPHA));
                case ItemTier.Lunar:        return (ColorFromHex("#7fbfff", ALPHA), ColorFromHex("#1f4f99", ALPHA));
                case ItemTier.VoidTier1:    return (ColorFromHex("#d98cff", ALPHA), ColorFromHex("#6b2f8f", ALPHA));
                case ItemTier.VoidTier2:    return (ColorFromHex("#d98cff", ALPHA), ColorFromHex("#6b2f8f", ALPHA));
                case ItemTier.VoidTier3:    return (ColorFromHex("#d98cff", ALPHA), ColorFromHex("#6b2f8f", ALPHA));
                case ItemTier.VoidBoss:     return (ColorFromHex("#d98cff", ALPHA), ColorFromHex("#6b2f8f", ALPHA));
                default:                    return (ColorFromHex("#6b6b6b", ALPHA), ColorFromHex("#6b6b6b", ALPHA));
            }
        }

        public static Color ColorFromHex(string hex, float alpha = 1)
        {
            float r = 0, g = 0, b = 0; 
            hex = hex.Trim().Replace("#", "");
            r = Convert.ToInt32(hex.Substring(0, 2), 16) / 256.0f; 
            g = Convert.ToInt32(hex.Substring(2, 2), 16) / 256.0f; 
            b = Convert.ToInt32(hex.Substring(4, 2), 16) / 256.0f; 
            return new Color(r, g, b, alpha); 
        }
    }
}
