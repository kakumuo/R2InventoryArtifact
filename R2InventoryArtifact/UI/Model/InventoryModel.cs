
using System.Collections.Generic;
using R2InventoryArtifact.Util;
using R2InventoryArtifact.Util.R2API;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using RoR2;
using EntityStates.VultureHunter.Weapon;
using R2InventoryArtifact.UI.Services;

namespace R2InventoryArtifact.Model
{
   public enum InventoryResultCode
    {
        FAILED, 
        GRID_INSERT, GRID_UPDATE, GRID_REMOVE, 
        HOLD_INSERT, HOLD_UPDATE, HOLD_REMOVE,
        NONEQUIP_INSERT, NONEQUIP_UPDATE, NONEQUIP_REMOVE
    }

    public struct InventoryUpdateResult
    {
        public InventoryResultCode ResultCode;
        public InventoryItem InventoryItem; 
        public GridPosition Pos; 

        public InventoryUpdateResult(InventoryResultCode ResultCode, InventoryItem InventoryItem, GridPosition Pos=new())
        {
            this.ResultCode = ResultCode; 
            this.InventoryItem = InventoryItem; 
            this.Pos = Pos; 
        }
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
        public static List<InventoryLock> InventoryLocks; 
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
            InventoryLocks = locks;             

            _effectObserver = new InventoryEffectObserver();

            Reset(); 
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
            if (item != null && _inventory[pos.Row, pos.Col] == item)
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
        /// <param name="itemIndex"></param>
        /// <param name="updatedItem"></param>
        /// <returns>
        /// InventoryAddCode - INSERT, STACK, NONE
        /// </returns>
        public static InventoryUpdateResult AddToInventory(UniquePickup pickup, bool toNonEquip)
        {
            InventoryItem next = InventoryService.GetInventoryItem(pickup);
            if(toNonEquip)
            {
                foreach(InventoryItem nonEquip in _nonEquipList)
                {
                    if(nonEquip.Pickup == next.Pickup)
                    {
                        nonEquip.StackCount += 1; 
                        return new (InventoryResultCode.NONEQUIP_UPDATE, nonEquip); 
                    }
                }

                _nonEquipList.Add(next); 
                return new (InventoryResultCode.NONEQUIP_INSERT, next); 
            }

            // increase stack if already in inventory
            for (int r = 0; r < _grid.Height; r++)
            {
                for (int c = 0; c < _grid.Width; c++)
                {
                    if (_inventory[r, c] != null && _inventory[r, c].Pickup == next.Pickup && _inventory[r, c].StackCount < _inventory[r, c].MaxStackCount)
                    {
                        _inventory[r, c].StackCount += 1;
                        return new (InventoryResultCode.GRID_UPDATE, _inventory[r, c], new GridPosition(c, r)) ;
                    }
                }
            }

            // try increase item in item hold 
            foreach (InventoryItem holdItem in _holdList)
            {
                if (holdItem.Pickup == next.Pickup && holdItem.StackCount < holdItem.MaxStackCount)
                {
                    holdItem.StackCount += 1;
                    return new (InventoryResultCode.HOLD_UPDATE, holdItem);
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
                            return new (InventoryResultCode.GRID_INSERT, next, pos);
                        }

                        next.Rotate();
                    }
                }
            }

            
            AddToHold(next); 
            return new (InventoryResultCode.HOLD_INSERT, next); 
        }

        private static List<InventoryUpdateResult> RemoveFromNonEquip(UniquePickup pickup, int count)
        {
            List<InventoryUpdateResult> removedItems = new(); 
            
            InventoryItem targetItem = _nonEquipList.Find(ne => ne.Pickup == pickup);
            if(targetItem == null) return removedItems; 

            InventoryUpdateResult result = new(){InventoryItem=targetItem, Pos=new(), ResultCode=InventoryResultCode.NONEQUIP_UPDATE}; 
            targetItem.StackCount = Math.Max(targetItem.StackCount - count, 0); 

            if(targetItem.StackCount == 0) result.ResultCode = InventoryResultCode.NONEQUIP_REMOVE; 

            removedItems.Add(result); 
            return removedItems;
        }

        public static List<InventoryUpdateResult> RemoveItems(UniquePickup pickup, bool fromNonEquip, int count=1)
        {            
            // all temp items go to non-equip, so remove from there
            if(fromNonEquip || pickup.isTempItem) return RemoveFromNonEquip(pickup, count); 

            List<InventoryUpdateResult> removedItems = new(); 
            List<InventoryUpdateResult> tmp = new(); 

            _nonEquipList.ForEach(item =>
            {
                if(item.Pickup == pickup)
                {
                    tmp.Add(new(){ResultCode=InventoryResultCode.NONEQUIP_UPDATE, InventoryItem=item}); 
                }
            });

            _holdList.ForEach(item =>
            {
                if(item.Pickup == pickup)
                {
                    tmp.Add(new(){ResultCode=InventoryResultCode.HOLD_UPDATE, InventoryItem=item}); 
                }
            }); 

            foreach(InventoryItem item in _itemRoot.Keys)
            {
                if(item.Pickup == pickup)
                {
                    tmp.Add(new(){ResultCode=InventoryResultCode.GRID_UPDATE, InventoryItem=item, Pos=_itemRoot[item]}); 
                }
            }

            tmp.Sort((item1, item2) =>
            {
                if(item1.ResultCode == item2.ResultCode)
                    return item1.InventoryItem.StackCount - item2.InventoryItem.StackCount; 
                return item1.ResultCode - item2.ResultCode; 
            }); 

            int removeCount = 0; 

            for(int i = 0; i < tmp.Count && removeCount < count; i++)
            {
                InventoryUpdateResult result = tmp[i]; 
                int remDiff = count - removeCount;
                if(result.InventoryItem.StackCount > remDiff)
                {
                    result.InventoryItem.StackCount -= remDiff; 
                    removedItems.Add(result); 
                    break; 
                }

                // delete items from inventory
                switch(result.ResultCode)
                {
                    case InventoryResultCode.GRID_UPDATE: 
                        UnsetItemAt(result.InventoryItem, result.Pos); 
                        result.ResultCode = InventoryResultCode.GRID_REMOVE;  
                    break; 
                    case InventoryResultCode.HOLD_UPDATE: 
                        RemoveFromHold(result.InventoryItem);  
                        result.ResultCode = InventoryResultCode.HOLD_REMOVE; 
                    break; 
                    case InventoryResultCode.NONEQUIP_UPDATE: 
                        RemoveFromNonEquip(result.InventoryItem); 
                        result.ResultCode = InventoryResultCode.NONEQUIP_REMOVE; 
                    break; 
                }

                removedItems.Add(result); 
                removeCount += result.InventoryItem.StackCount;   
            }; 

            return removedItems; 
        }

        public static void RemoveFromNonEquip(InventoryItem item)
        {
            _nonEquipList.Remove(item); 
        }

        // private static R2ItemCode TryGetCorruptItemCodeInInventory(R2ItemCode code)
        // {
        //     // check to see if corrupted item is already in inventory
        //     if(R2ItemService.Normal2Corrupt.ContainsKey(code))
        //     {
        //         R2ItemCode corruptCode = R2ItemService.Normal2Corrupt[code]; 
        //         foreach(InventoryItem item in _itemRoot.Keys) if(item.ItemCode == corruptCode) return item.ItemCode; 
        //         foreach(InventoryItem item in _holdList) if(item.ItemCode == corruptCode) return item.ItemCode; 
        //         foreach(InventoryItem item in _nonEquipList) if(item.ItemCode == corruptCode) return item.ItemCode; 
        //     }

        //     return code; 
        // }

        // public static bool TryCorruptInventory(R2ItemCode corruptedItemCode)
        // {
        //     if (!R2ItemService.Corrupt2Normal.ContainsKey(corruptedItemCode))
        //         return false;

        //     R2Item corruptedItem = R2ItemService.GetR2Item(corruptedItemCode);
        //     List<R2ItemCode> baseItems = R2ItemService.Corrupt2Normal[corruptedItemCode];

        //     // Corrupted item conversion
        //     // corrupted items maintain the original items max stack count & shape, new items have their own stack count and shape
        //     // corrupted items cannot be dropped
        //     List<InventoryItem> keys = new List<InventoryItem>(_itemRoot.Keys);

        //     foreach (InventoryItem invItem in keys)
        //     {
        //         // InventoryItem invItem = _inventory[r,c];  
        //         if (baseItems.Contains(invItem.ItemCode))
        //         {
        //             invItem.CorruptItem(corruptedItem);

        //             // handle item relationship changes 
        //             GridPosition rootPos = _itemRoot[invItem];
        //             UnsetItemAt(invItem, rootPos);
        //             SetItemAt(invItem, rootPos);
        //         }
        //     }


        //     foreach (InventoryItem holdItem in _holdList)
        //     {
        //         if (baseItems.Contains(holdItem.ItemCode)) holdItem.CorruptItem(corruptedItem);
        //     }

        //     return true;
        // }

        public static bool IsPositionLocked(GridPosition pos)
        {
            foreach(InventoryLock invLock in InventoryLocks)
            {
                foreach(GridPosition node in invLock.Nodes)
                {
                    if(pos == node + invLock.Root) return invLock.IsLocked; 
                }
            }

            return false; 
        } 

        public static List<InventoryLock> SetUnlocksAtLevel(int level)
        {
            List<InventoryLock> unlocks = new(); 
            for(int i = 0; i < InventoryLocks.Count; i++)
            {
                if(InventoryLocks[i].IsLocked && InventoryLocks[i].UnlockLevel <= level)
                {
                    InventoryLocks[i].IsLocked = false; 
                    unlocks.Add(InventoryLocks[i]); 
                }
            }

            return unlocks; 
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

        internal static List<InventoryUpdateResult> DiscardHold()
        {
            if(_holdList.Count == 0) return new(); 

            List<InventoryUpdateResult> res = _holdList
                .Select(item => new InventoryUpdateResult(InventoryResultCode.HOLD_REMOVE, item))
                .ToList(); 

            res.ForEach(res => RemoveFromHold(res.InventoryItem)); 
            return res; 
        }

        internal static void Reset()
        {
            _inventory = new InventoryItem[_grid.Height, _grid.Width];
            _activeMap = new Dictionary<GridPosition, List<InventoryItem>>();
            _holdList = new List<InventoryItem>();
            _nonEquipList = new List<InventoryItem>();
            _itemRoot = new Dictionary<InventoryItem, GridPosition>();

            for(int i = 0; i < InventoryLocks.Count; i++)
            {
                InventoryLocks[i].IsLocked = true; 
            }
        }
    }
}