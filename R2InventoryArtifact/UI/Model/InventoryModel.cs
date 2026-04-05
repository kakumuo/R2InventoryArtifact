
using System.Collections.Generic;
using R2InventoryArtifact.Util.Math;
using R2InventoryArtifact.Util.R2API;
using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace R2InventoryArtifact.Model
{
    public enum InventoryAddCode
    {
        NONE, GRID_INSERT, GRID_STACK, HOLD_STACK, HOLD_INSERT, NONEQUIP_INSERT,
        NONEQUIP_STACK
    }

    // unlocks upon achieving specific level
    [Serializable]public class InventoryLock
    {
        public GridPosition Root; 
        public List<GridPosition> Nodes; 
        public int UnlockLevel; 
        public bool IsLocked = true; 
    }

    public static class InventoryModel
    {
        private static IntRect _grid;

        private static List<InventoryItem> _holdList;
        private static List<InventoryItem> _nonEquipList;
        private static InventoryItem[,] _inventory;
        private static List<InventoryLock> _inventoryLocks; 
        private static Dictionary<InventoryItem, GridPosition> _itemRoot;

        private static Dictionary<GridPosition, List<InventoryItem>> _activeMap;
        private static InventoryEffectObserver _effectObserver;


        public static event Action<InventoryEffectCode, int> OnEffectAdd
        {
            add => _effectObserver.OnEffectAdd += value;
            remove => _effectObserver.OnEffectAdd -= value;
        }

        public static event Action<InventoryEffectCode, int> OnEffectRemove
        {
            add => _effectObserver.OnEffectRemove += value;
            remove => _effectObserver.OnEffectRemove -= value;
        }

        public static void Initialize(IntRect grid, List<InventoryLock> locks)
        {
            _grid = grid;

            _inventory = new InventoryItem[grid.Height, grid.Width];
            _activeMap = new Dictionary<GridPosition, List<InventoryItem>>();
            _holdList = new List<InventoryItem>();
            _nonEquipList = new List<InventoryItem>();
            _itemRoot = new Dictionary<InventoryItem, GridPosition>();
            _inventoryLocks = locks;             

            _effectObserver = new InventoryEffectObserver();

        }

        public static bool SetItemAt(InventoryItem item, GridPosition pos)
        {
            if (!IsValidItemPosition(item, pos))
                return false;

            foreach (GridPosition node in item.Nodes)
            {
                GridPosition next = node + pos;
                _inventory[next.Row, next.Col] = item;

                // check adj for current item
                if (_activeMap.ContainsKey(next))
                    foreach (InventoryItem adjItem in _activeMap[next])
                        if (item != adjItem)
                            _effectObserver.AddAdjacency(item, adjItem);
            }

            foreach (GridPosition node in item.ActiveNodes)
            {
                GridPosition next = node + pos;
                if (!_grid.Contains(next)) continue;

                // check adj for other items
                if (_inventory[next.Row, next.Col] != null)
                    _effectObserver.AddAdjacency(item, _inventory[next.Row, next.Col]);

                List<InventoryItem> adjList = _activeMap.GetValueOrDefault(next, new List<InventoryItem>());
                adjList.Add(item);
                _activeMap[next] = adjList;
            }

            _itemRoot[item] = pos;

            return true;
        }

        public static void UnsetItemAt(InventoryItem item, GridPosition pos)
        {
            if (item != null)
            {
                foreach (GridPosition node in item.Nodes)
                {
                    GridPosition next = node + pos;
                    _inventory[next.Row, next.Col] = null;

                    // remove adj for cur item
                    if (_activeMap.ContainsKey(next))
                        foreach (InventoryItem adjItem in _activeMap[next])
                            if (item != adjItem)
                                _effectObserver.RemoveAdjacency(item, adjItem);
                }

                foreach (GridPosition node in item.ActiveNodes)
                {
                    GridPosition next = node + pos;
                    if (!_grid.Contains(next)) continue;

                    // check adj for other items
                    if (_inventory[next.Row, next.Col] != null)
                        _effectObserver.RemoveAdjacency(item, _inventory[next.Row, next.Col]);

                    _activeMap[next].Remove(item);
                    if (_activeMap[next].Count == 0)
                        _activeMap.Remove(next);
                }

                _itemRoot.Remove(item);
            }
        }

        public static List<InventoryEffectCode> GetEffectList()
        {
            return _effectObserver.EffectFreqs.Keys.ToList();
        }

        public static InventoryItem GetItemAt(GridPosition pos)
        {
            return GetItemAt(pos.Row, pos.Col);
        }

        public static InventoryItem GetItemAt(int row, int col)
        {
            if (!_grid.Contains(row, col)) return null;
            return _inventory[row, col];
        }

        public static GridPosition GetItemRoot(InventoryItem item)
        {
            return _itemRoot[item];
        }

        /// <summary>
        /// Tries to add an item to the inventory, searches for the next valid position
        /// </summary>
        /// <value><c>updatedItem</c>: if add => updated item, if insert => new item, otherwise <c>null</c>.</value>
        /// <value><c>pos</c>: if add/insert => position of item, otherwise <c>null</c>.</value>
        /// <param name="itemCode"></param>
        /// <param name="updatedItem"></param>
        /// <returns>
        /// InventoryAddCode - INSERT, STACK, NONE
        /// </returns>
        public static InventoryAddCode AddToInventory(R2ItemCode baseCode, out InventoryItem updatedItem, out GridPosition itemPos)
        {
            R2ItemCode itemCode = TryGetCorruptItemCodeInInventory(baseCode);             

            InventoryItem next = InventoryService.GetInventoryItem(itemCode);
            if(!next.IsEquippable)
            {
                foreach(InventoryItem nonEquip in _nonEquipList)
                {
                    if(nonEquip.ItemCode == itemCode)
                    {
                        nonEquip.StackCount += 1; 
                        updatedItem = next; 
                        itemPos = new(); 
                        return InventoryAddCode.NONEQUIP_STACK; 
                    }
                }

                _nonEquipList.Add(next); 
                updatedItem = next; 
                itemPos = new(); 
                return InventoryAddCode.NONEQUIP_INSERT; 
            }

            // get next item
            for (int r = 0; r < _grid.Height; r++)
            {
                for (int c = 0; c < _grid.Width; c++)
                {
                    if (_inventory[r, c] != null && _inventory[r, c].ItemCode == itemCode && _inventory[r, c].StackCount < _inventory[r, c].MaxStackCount)
                    {
                        updatedItem = _inventory[r, c];
                        updatedItem.StackCount += 1;
                        itemPos = new GridPosition(c, r);
                        return InventoryAddCode.GRID_STACK;
                    }
                }
            }

            // try increase item in item hold 
            foreach (InventoryItem holdItem in _holdList)
            {
                if (holdItem.ItemCode == itemCode && holdItem.StackCount < holdItem.MaxStackCount)
                {
                    holdItem.StackCount += 1;
                    updatedItem = holdItem;
                    itemPos = new(0, 0);
                    return InventoryAddCode.HOLD_STACK;
                }
            }

            // try insert, rotate
            for (int r = 0; r < _grid.Height; r++)
            {
                for (int c = 0; c < _grid.Width; c++)
                {
                    for (int rot = 0; rot < 4; rot++)
                    {
                        GridPosition pos = new(c, r);
                        if (IsValidItemPosition(next, pos))
                        {
                            SetItemAt(next, pos);
                            updatedItem = next;
                            itemPos = pos;
                            return InventoryAddCode.GRID_INSERT;
                        }

                        next.Rotate();
                    }
                }
            }

            // try to add to hold if can be held
            updatedItem = next;
            itemPos = new(0, 0);
            return next.IsDroppable ? InventoryAddCode.HOLD_INSERT : InventoryAddCode.NONE;
        }

        public static R2ItemCode TryGetCorruptItemCodeInInventory(R2ItemCode code)
        {
            // check to see if corrupted item is already in inventory
            if(R2ItemService.Normal2Corrupt.ContainsKey(code))
            {
                R2ItemCode corruptCode = R2ItemService.Normal2Corrupt[code]; 
                foreach(InventoryItem item in _itemRoot.Keys) if(item.ItemCode == corruptCode) return item.ItemCode; 
                foreach(InventoryItem item in _holdList) if(item.ItemCode == corruptCode) return item.ItemCode; 
                foreach(InventoryItem item in _nonEquipList) if(item.ItemCode == corruptCode) return item.ItemCode; 
            }

            return code; 
        }

        public static bool TryCorruptInventory(R2ItemCode corruptedItemCode)
        {
            if (!R2ItemService.Corrupt2Normal.ContainsKey(corruptedItemCode))
                return false;

            R2Item corruptedItem = R2ItemService.GetR2Item(corruptedItemCode);
            List<R2ItemCode> baseItems = R2ItemService.Corrupt2Normal[corruptedItemCode];

            // Corrupted item conversion
            // corrupted items maintain the original items max stack count & shape, new items have their own stack count and shape
            // corrupted items cannot be dropped
            List<InventoryItem> keys = new List<InventoryItem>(_itemRoot.Keys);

            foreach (InventoryItem invItem in keys)
            {
                // InventoryItem invItem = _inventory[r,c];  
                if (baseItems.Contains(invItem.ItemCode))
                {
                    invItem.CorruptItem(corruptedItem);

                    // handle item relationship changes 
                    GridPosition rootPos = _itemRoot[invItem];
                    UnsetItemAt(invItem, rootPos);
                    SetItemAt(invItem, rootPos);
                }
            }


            foreach (InventoryItem holdItem in _holdList)
            {
                if (baseItems.Contains(holdItem.ItemCode)) holdItem.CorruptItem(corruptedItem);
            }

            return true;
        }

        public static void DropItem(InventoryItem item) { }

        public static bool IsPositionLocked(GridPosition pos)
        {
            foreach(InventoryLock invLock in _inventoryLocks)
            {
                foreach(GridPosition node in invLock.Nodes)
                {
                    if(pos == node + invLock.Root) return invLock.IsLocked; 
                }
            }

            return false; 
        } 

        public static void SetUnlocksAtLevel(int level)
        {
            for(int i = 0; i < _inventoryLocks.Count; i++)
            {
                if(_inventoryLocks[i].IsLocked && _inventoryLocks[i].UnlockLevel <= level) 
                    _inventoryLocks[i].IsLocked = false; 
            }
        }

        public static bool IsValidItemPosition(InventoryItem item, GridPosition pos)
        {
            foreach (GridPosition node in item.Nodes)
            {
                GridPosition next = pos + node;
                if (!_grid.Contains(next) || _inventory[next.Row, next.Col] != null || IsPositionLocked(next))
                    return false;
            };

            return true;
        }

        public static void AddToHold(InventoryItem item)
        {
            _holdList.Add(item);
        }

        public static void RemoveFromHold(InventoryItem item)
        {
            _holdList.Remove(item);
        }

        public new static string ToString()
        {
            List<string> lines = new List<string>();

            for (int r = 0; r < _inventory.GetLength(0); r++)
            {
                List<string> line = new List<string>();
                for (int c = 0; c < _inventory.GetLength(1); c++)
                {
                    line.Add(_inventory[r, c] != null ? "[x]" : "[ ]");
                }
                lines.Add(string.Join("\t", line));
            }

            return string.Join("\n", lines);
        }
    }
}