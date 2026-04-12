
using System; 
using System.Collections.Generic; 
using R2InventoryArtifact.Util.R2API; 
using R2InventoryArtifact.Util;
using RoR2;

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

        // TODO: implement
        public string GetItemName()
        {
            return "NameNotFound"; 
        }

        // TODO: implement
        public string GetDescription()
        {
            return "GetDescriptionNotImplemented"; 
        }
    }
        
}