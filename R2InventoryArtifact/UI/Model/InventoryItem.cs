
using System; 
using System.Collections.Generic; 
using R2InventoryArtifact.Util.R2API; 
using R2InventoryArtifact.Util;
using RoR2;

namespace R2InventoryArtifact.Model
{   
    public enum InventoryIndexType
    {
        None, Item, Equipment
    }

    public class InventoryIndex
    {
        public ItemIndex ItemIndex = ItemIndex.None; 
        public EquipmentIndex EquipmentIndex = EquipmentIndex.None;
        public InventoryIndexType IndexType = InventoryIndexType.None; 

        public InventoryIndex(ItemIndex itemIndex)
        {
            Reset(); 
            ItemIndex = itemIndex; 
            IndexType = InventoryIndexType.Item; 
        }

        public InventoryIndex(EquipmentIndex equipmentIndex)
        {
            Reset(); 
            EquipmentIndex = equipmentIndex; 
            IndexType = InventoryIndexType.Equipment; 
        }

        private void Reset()
        {
            IndexType = InventoryIndexType.None; 
            ItemIndex = ItemIndex.None; 
            EquipmentIndex = EquipmentIndex.None; 
        }

        public static bool operator ==(InventoryIndex a, InventoryIndex b)
        {
            return a.ItemIndex == b.ItemIndex && a.EquipmentIndex == b.EquipmentIndex; 
        }

        public static bool operator !=(InventoryIndex a, InventoryIndex b)
        {
            return !(a == b); 
        }

        public static implicit operator ItemIndex(InventoryIndex x)
        {
            return x.ItemIndex; 
        }

        public static implicit operator EquipmentIndex(InventoryIndex x)
        {
            return x.EquipmentIndex; 
        }
    }

    [Serializable] public class InventoryItem
    {
        public InventoryIndex InventoryIndex; 

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
            get => ItemTier.Tier1; 
        }

        // public event Action<R2Item> OnItemCorrupted; 
        public bool IsDroppable = true; 
        public bool IsEquippable = true; 

        public InventoryItem(InventoryIndex inventoryIndex, List<GridPosition> nodeOrigin, List<GridPosition> activeOrigin)
        {
            InventoryIndex = inventoryIndex; 
            InitHelper(nodeOrigin, activeOrigin); 
        }

        private void InitHelper(List<GridPosition> nodeOrigin, List<GridPosition> activeOrigin)
        {
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

        public string GetItemName()
        {
            if(InventoryIndex.ItemIndex != ItemIndex.None) return ItemCatalog.itemDefs[(int)InventoryIndex.ItemIndex].name; 
            if(InventoryIndex.EquipmentIndex != EquipmentIndex.None) return EquipmentCatalog.equipmentDefs[(int)InventoryIndex.EquipmentIndex].name; 
            return "NameNotFound"; 
        }

        public string GetDescription()
        {
            return "GetDescriptionNotImplemented"; 
        }
    }
        
}