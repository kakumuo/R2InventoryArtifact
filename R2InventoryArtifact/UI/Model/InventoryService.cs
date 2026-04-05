
using System.Collections.Generic;
using R2InventoryArtifact.Util.R2API;


namespace R2InventoryArtifact.Model
{
    public class InventoryService
    {
        public static InventoryItem GetInventoryItem(R2ItemCode itemCode)
        {
            R2Item targetItem = R2ItemService.GetR2Item(itemCode); 
            switch(itemCode)
            {
                case R2ItemCode.BundleOfFireworks: 
                    return new InventoryItem(
                        item: targetItem, 
                        nodeOrigin: new(){new(0, 0), new(0, 1)}, 
                        activeOrigin: new(){new(-1, 0), new(1, 0)}
                    ){MaxStackCount = 1}; 
                case R2ItemCode.BisonSteak: 
                    return new InventoryItem(
                        item: targetItem, 
                        nodeOrigin: new(){new(0, 0), new(0, 1), new(0, 2)}, 
                        activeOrigin: new(){new(-1, 0), new(1, 0)}
                    ){MaxStackCount = 1}; 
                case R2ItemCode.IgnitionTank: 
                    return new InventoryItem(
                        item: targetItem, 
                        nodeOrigin: new(){new(0, 0), new(0, 1), new(0, 2)}, 
                        activeOrigin: new(){new(-1, 0), new(1, 0)}
                    ){MaxStackCount = 1}; 
                case R2ItemCode.RunaldsBand:
                    return new InventoryItem(
                        item: targetItem, 
                        nodeOrigin: new(){new(0, 0), new(0, 1), new(0, 2)}, 
                        activeOrigin: new(){new(-1, 0), new(1, 0)}
                    ); 
                case R2ItemCode.SingularityBand: 
                    return new InventoryItem(
                        item: targetItem, 
                        nodeOrigin: new(){new(0, 0), new(0, 1), new(0, 2)}, 
                        activeOrigin: new(){new(-1, 0), new(1, 0)}
                    ){IsDroppable = false}; 
                case R2ItemCode.EmptyBottle: 
                    return new InventoryItem(
                        item: targetItem, 
                        nodeOrigin: new(){new(0, 0)}, 
                        activeOrigin: new(){new(-1, 0)}
                    ){IsEquippable = false}; 
                default: return null; 
            }
        }

        public static InventoryEffectCode GetInventoryEffectCode(R2ItemCode parent, HashSet<R2ItemCode> adjacent)
        {
            InventoryEffectCode resCode = InventoryEffectCode.None; 
            if(parent == R2ItemCode.BundleOfFireworks && adjacent.Contains(R2ItemCode.BisonSteak) && adjacent.Contains(R2ItemCode.IgnitionTank)) 
                resCode = InventoryEffectCode.BundleOfFireworks_BisonSteak_IgnitionTank; 

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