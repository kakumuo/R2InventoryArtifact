
using System.Collections.Generic;
using RoR2;
using R2InventoryArtifact.Util;
using System.IO;
using Newtonsoft.Json;
using System;
using R2InventoryArtifact.Model;

namespace R2InventoryArtifact.UI.Services
{
    public class InventoryItemProps
    {
        // public ItemIndex ItemIndex; 
        // public EquipmentIndex EquipmentIndex; 
        public List<GridPosition> NodeOrigin;
        public List<GridPosition> ActiveOrigin; 
        public int MaxStackCount; 
    }

    public partial class InventoryService
    {
        private static string _ITEM_DATA_FILENAME = "item_data.json";
        private static Dictionary<string, InventoryItemProps> _inventoryDict; //key => name token
        // private static Dictionary<EquipmentIndex, InventoryItemProps> _equipDict;

        // called at the start of the game
        public static void Initialize(BepInEx.PluginInfo pluginInfo)
        {
            _inventoryDict = new Dictionary<string, InventoryItemProps>(); 
            string _basePath = System.IO.Path.GetDirectoryName(pluginInfo.Location);
            if (!string.IsNullOrEmpty(_basePath))
            {
                string filePath = System.IO.Path.Combine(_basePath, "Assets", _ITEM_DATA_FILENAME);
                Log.Info($"try load file: {filePath}"); 
                try { using(StreamReader reader = new StreamReader(filePath))
                {
                    string json = reader.ReadToEnd(); 
                    _inventoryDict = JsonConvert.DeserializeObject<Dictionary<string, InventoryItemProps>>(json); 
                }} 
                catch(Exception e)
                {
                    Log.Info($"Unable to read file ({filePath}): {e}"); 
                }
            }

            // _itemDict = _inventoryItemList.Where(x => x.ItemIndex != ItemIndex.None).ToDictionary(x => x.ItemIndex, x => x); 
            // _equipDict = _inventoryItemList.Where(x => x.EquipmentIndex != EquipmentIndex.None).ToDictionary(x => x.EquipmentIndex, x => x); 
        }

        public static InventoryItem GetInventoryItem(UniquePickup pickup)
        {
            // ItemIndex itemI = pickup.pickupIndex.pickupDef.itemIndex;
            // EquipmentIndex equipI = pickup.pickupIndex.pickupDef.equipmentIndex; 
            InventoryItemProps target = _inventoryDict.GetValueOrDefault(pickup.pickupIndex.pickupDef.nameToken, null);

            if(target != null)
            {
                return new InventoryItem(
                    pickup, 
                    nodeOrigin: target.NodeOrigin, 
                    activeOrigin: target.ActiveOrigin
                ) {MaxStackCount=target.MaxStackCount}; 
            } 
            
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