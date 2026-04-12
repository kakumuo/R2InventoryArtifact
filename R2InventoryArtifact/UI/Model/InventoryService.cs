
using System.Collections.Generic;
using R2InventoryArtifact.Util.R2API;
using RoR2;
using R2InventoryArtifact.Util; 


namespace R2InventoryArtifact.Model
{
    public partial class InventoryService
    {
        public static InventoryItem GetInventoryItem(UniquePickup pickup)
        {
            return new InventoryItem(
                pickup, 
                new List<GridPosition>(){new(0, 0), new(0, 1)}, 
                new List<GridPosition>(){new(0, 0), new(0, 1)}
            ); 
        }

        public static InventoryEffectCode GetInventoryEffectCode(UniquePickup pickup, HashSet<UniquePickup> adjacent)
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