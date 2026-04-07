
using System.Collections.Generic;
using IL.RoR2;
using R2InventoryArtifact.Util.R2API;
using RoR2;


namespace R2InventoryArtifact.Model
{
    public partial class InventoryService
    {
        public static InventoryItem GetInventoryItem(InventoryIndex inventoryIndex)
        {
            return new InventoryItem(
                inventoryIndex, 
                new(){new(0, 0), new(0, 1)}, 
                new(){new(-1, 0), new(1, 0)}
            ); 
        }

        public static InventoryEffectCode GetInventoryEffectCode(InventoryIndex inventoryIndex, HashSet<InventoryIndex> adjacent)
        {
            InventoryEffectCode resCode = InventoryEffectCode.None; 
            // if(parent == R2ItemCode.BundleofFireworks && adjacent.Contains(R2ItemCode.BisonSteak) && adjacent.Contains(R2ItemCode.IgnitionTank)) 
            //     resCode = InventoryEffectCode.BundleOfFireworks_BisonSteak_IgnitionTank; 

            return resCode; 
        }

        public static InventoryEffect GetInventoryEffectInfo(InventoryEffectCode effectCode)
        {
            switch (effectCode)
            {
                case InventoryEffectCode.BundleOfFireworks_BisonSteak_IgnitionTank: 
                    return new (){ Name="Cook-out", Description="Bundle of Fireworks now ignite on it" };
                default: 
                    return null; 
            }
        }
    }
}