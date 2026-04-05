
using System; 
using System.Collections.Generic; 
using R2InventoryArtifact.Util.R2API; 
using R2InventoryArtifact.Util.Math; 

namespace R2InventoryArtifact.Model
{   [Serializable] public class InventoryItem
    {
        private R2Item _item; 

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

        public R2ItemTier ItemTier
        {
            get => _item.ItemTier; 
        }
        public R2ItemCode ItemCode
        {
            get => _item.ItemCode; 
        }
        public event Action<R2Item> OnItemCorrupted; 
        public bool IsDroppable = true; 
        public bool IsEquippable = true; 

        public InventoryItem(R2Item item, List<GridPosition> nodeOrigin, List<GridPosition> activeOrigin)
        {
            _item = item; 
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

        public void CorruptItem(R2Item corruptedItem)
        {
            OnItemCorrupted?.Invoke(corruptedItem); 
            _item = corruptedItem; 
            IsDroppable = false; 
        }

        public string GetItemName()
        {
            return _item.Name; 
        }

        public string GetDescription()
        {
            return _item.Description; 
        }
    }
        
}