
using System; 
using System.Collections.Generic; 
using R2InventoryArtifact.Util.R2API; 
using R2InventoryArtifact.Util;
using RoR2;
using RoR2.UI;
using UnityEngine;
using R2InventoryArtifact.UI.Services;
using R2InventoryArtifact.UI;

namespace R2InventoryArtifact.Model
{   
    public enum PickupType
    {
        None, Item, Equipment
    }
    
    [Serializable] public class InventoryItem
    {
        // TODO: MAYBE: use unique pickup isntead of pickup index
        public UniquePickup Pickup; 
        public PickupType PickupType = PickupType.None;   

        private List<GridPosition> _nodeOrigin;
        private List<GridPosition> _activeOrigin;  
        private int _orientation = 0; 
        public List<GridPosition> Nodes; 
        public List<GridPosition> ActiveNodes; 

        private int _stackCount = 1; 
        public int StackCount
        {
            get => _stackCount; 
            set { _stackCount = value; OnStackCountChanged.Invoke(); }
        }
        public int MaxStackCount = 4; 
        public event Action OnStackCountChanged;  

        public ItemTier ItemTier
        {
            get => Pickup.pickupIndex.pickupDef.itemTier; 
        }

        // public event Action<R2Item> OnItemCorrupted; 
        public bool IsDroppable = true; 
        public bool IsEquippable = true; 

        public InventoryItem(UniquePickup pickup, List<GridPosition> nodeOrigin, List<GridPosition> activeOrigin)
        {
            Pickup = pickup; 
            if(pickup.pickupIndex.pickupDef.itemIndex != ItemIndex.None)
            {
                PickupType = PickupType.Item; 
            } 
            else if (pickup.pickupIndex.pickupDef.equipmentIndex != EquipmentIndex.None)
            {
                PickupType = PickupType.Equipment; 
            }

            ItemTier teir = pickup.pickupIndex.pickupDef.itemTier; 
            switch(teir)
            {
                case ItemTier.VoidBoss: 
                case ItemTier.VoidTier1:
                case ItemTier.VoidTier2:
                case ItemTier.VoidTier3:
                case ItemTier.NoTier:
                case ItemTier.Lunar: 
                    IsDroppable = false; 
                break; 
            }

            _nodeOrigin = nodeOrigin; 
            _activeOrigin = activeOrigin; 

            Nodes = new List<GridPosition>(_nodeOrigin);
            ActiveNodes = new List<GridPosition>(_activeOrigin);
        }

        public void Rotate()
        {
            // Debug.Log("rotating"); 
            _orientation = (_orientation + 1) % 3; 
            for(int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i] = new GridPosition(-Nodes[i].Row, Nodes[i].Col); 
            }

            for(int i = 0; i < ActiveNodes.Count; i++)
            {
                ActiveNodes[i] = new GridPosition(-ActiveNodes[i].Row, ActiveNodes[i].Col); 
            }
        }

        // public void CorruptItem(R2Item corruptedItem)
        // {
        //     OnItemCorrupted?.Invoke(corruptedItem); 
        //     _item = corruptedItem; 
        //     IsDroppable = false; 
        // }

        public TooltipContent GetTooltipContent()
        {
            TooltipContent content = new TooltipContent();
            switch(PickupType)
            {
                case PickupType.Item: 
                    // ItemIcon icon = new ItemIcon(); 
                    // icon.SetItemIndex(Pickup.pickupIndex.pickupDef.itemIndex, 1, 1);
                    // Log.Info($"{icon.tooltipProvider.bodyText}");  
                    ItemDef itemDef = ItemCatalog.GetItemDef(Pickup.pickupIndex.pickupDef.itemIndex); //TODO: move to constructor
                    content.titleToken = itemDef.nameToken; 
                    content.bodyToken = itemDef.descriptionToken;
                    content.bodyColor = Pickup.pickupIndex.pickupDef.baseColor; 
                    content.titleColor = Pickup.pickupIndex.pickupDef.darkColor; 
                break; 
                case PickupType.Equipment: 
                    EquipmentDef equipDef = EquipmentCatalog.GetEquipmentDef(Pickup.pickupIndex.pickupDef.equipmentIndex); 
                    content.titleToken = equipDef.nameToken; 
                    content.bodyToken = equipDef.descriptionToken;
                    content.bodyColor = Pickup.pickupIndex.pickupDef.baseColor; 
                    content.titleColor = Pickup.pickupIndex.pickupDef.darkColor; 
                break; 
            }

            return content; 
        }
    }
        
}