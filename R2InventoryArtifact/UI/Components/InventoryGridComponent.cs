
using R2InventoryArtifact.Util;
using UnityEngine;
using R2InventoryArtifact.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using R2InventoryArtifact.UI.Builders;
using R2InventoryArtifact.UI.Layouts;
using UnityEngine.UIElements;
using UnityEngine.UI;
using IL.RoR2;

namespace R2InventoryArtifact.UI.Components
{
    /// <summary>
    /// Note: 
    /// <list type="bullet">
    /// <item>
    /// All methods set/unsert without validating position; validation is performed previously called functions    
    /// </item>
    /// </list>
    /// </summary>
    public class InventoryGridComponent : MonoBehaviour, IPointerExitHandler
    {
        private IntRect _gridRect;
        private InventorySlotComponent[,] _slots;
        private GridPosition? _cursorPosition;

        private static List<(int, int)> _DIRS = new(){(-1, 0), (1, 0), (0, -1), (0, 1)}; 

        public void Initialize(IntRect gridSize, List<InventoryLock> locks)
        {
            _gridRect = gridSize;
            _slots = new InventorySlotComponent[_gridRect.Height, _gridRect.Width];

            // init layout
            InventoryGridLayoutGroup inventoryLayout = gameObject.AddComponent<InventoryGridLayoutGroup>();
            inventoryLayout.Initialize(_gridRect.Width, 4);
            
            // update scroll speed based off ui size
            
            ScrollRect scrollRect; 
            if(scrollRect = gameObject.transform.parent.parent.GetComponent<ScrollRect>())
            {
                void HandleCellSizeChanged(float size) => scrollRect.scrollSensitivity = size / 2; // each scroll step = 1/2 grid size
                inventoryLayout.OnCellSizeChanged += HandleCellSizeChanged; 
            }

            // TODO: see if there is a way do this with only the component bulider
            // ScrollRect scrollView = GetComponentInChildren<ScrollRect>(); 
            // scrollView.scrollSensitivity = _gridRect.Height * 2; 

            // create slots
            for (int r = 0; r < gridSize.Height; r++)
            {
                for (int c = 0; c < gridSize.Width; c++)
                {
                    GridPosition pos = new(c, r); 
                    InventorySlotComponent slot = ComponentBuilder.BuildGridSlot(transform, $"{r}-{c}");
                    slot.Initialize(this, pos);
                    // slot.transform.SetParent(transform);
                    _slots[r, c] = slot;
                }
            }

            UpdateGridLocks(locks); 
        }

        public void InsertItemAt(InventoryItem item, GridPosition pos)
        {
            InventoryItemElement element = ComponentBuilder.BuildItemElement($"{item.Pickup.ToString()}");
            element.Initialize(item, DragSource.GRID);
            element.transform.SetParent(_slots[pos.Row, pos.Col].transform);
        }

        internal void RemoveAt(GridPosition pos)
        {
            if(_slots[pos.Row, pos.Col].transform.childCount == 0) return; //slots should only have one child 

            Transform elementTrans = _slots[pos.Row, pos.Col].transform.GetChild(0); 
            InventoryItemElement element; 
            if(elementTrans && (element = elementTrans.GetComponent<InventoryItemElement>()))
            {
                Destroy(element.gameObject); // triggers slot's transform children updated listener
            }
        }

        public void UnsetItemAt(InventoryItem item, GridPosition pos)
        {
            InventoryModel.UnsetItemAt(item, pos);
            SetHelper(item, pos, set: false);
        }

        public void SetItemAt(InventoryItem item, GridPosition pos)
        {
            InventoryModel.SetItemAt(item, pos);
            SetHelper(item, pos, set: true);
        }

        private void SetHelper(InventoryItem item, GridPosition pos, bool set)
        {
            foreach (GridPosition node in item.Nodes)
            {
                GridPosition next = node + pos;
                InventorySlotComponent curSlot = _slots[next.Row, next.Col];
                if (set) curSlot.Occupy(item);
                else curSlot.UnOccupy();
            }
            RepaintGrid();
        }

        public void UpdateCursorPosition(GridPosition pos)
        {
            _cursorPosition = pos;
            RepaintGrid();
        }

        public void RepaintGrid()
        {
            List<GridPosition> highlightPos = new List<GridPosition>();
            List<GridPosition> activePos = new List<GridPosition>();
            bool isValidPosition = true;

            if (_cursorPosition != null)
            {

                if (InventoryUI.Instance.CursorElement != null)
                {
                    isValidPosition = InventoryModel.IsValidItemPosition(InventoryUI.Instance.CursorElement.Item, (GridPosition)_cursorPosition);
                    highlightPos = InventoryUI.Instance.CursorElement.Item.Nodes.Select(n => n + (GridPosition)_cursorPosition).ToList();
                    activePos = InventoryUI.Instance.CursorElement.Item.ActiveNodes.Select(n => n + (GridPosition)_cursorPosition).Where(n => _gridRect.Contains(n)).ToList();
                }
                else
                {
                    InventoryItem hoveredItem = InventoryModel.GetItemAt((GridPosition)_cursorPosition);
                    if (hoveredItem != null)
                    {
                        GridPosition itemRoot = InventoryModel.GetItemRoot(hoveredItem);
                        activePos = hoveredItem.ActiveNodes.Select(n => n + itemRoot).Where(n => _gridRect.Contains(n)).ToList();
                    }
                }
            }

            for (int r = 0; r < _gridRect.Height; r++)
            {
                for (int c = 0; c < _gridRect.Width; c++)
                {
                    InventoryItem item = InventoryModel.GetItemAt(r, c);
                    InventorySlotComponent slot = _slots[r, c];

                    // check adjacent elements for painting
                    bool[] adjList = new bool[4]; 
                    if(item != null)
                    {
                        for (int i = 0; i < _DIRS.Count; i++)
                        {
                            int dR = r + _DIRS[i].Item1;
                            int dC = c + _DIRS[i].Item2;

                            adjList[i] = _gridRect.Contains(dC, dR) && InventoryModel.GetItemAt(dR, dC) == item;
                        }
                    }

                    GridPosition curPos = new(c,r); 
                    if(InventoryModel.IsPositionLocked(curPos))
                    {
                        slot.Paint(UIConstants.COLOR_ITEM_SLOT_LOCKED); 
                    }
                    // TODO: re-enable when adding item buffs
                    // else if (activePos.Contains(curPos))
                    // {
                    //     slot.Paint(
                    //         baseColor: UIConstants.COLOR_ITEM_SLOT_ACTIVE, outlineColor: UIConstants.COLOR_ITEM_SLOT_ACTIVE_OUTLINE
                    //         , AdjT: adjList[0], AdjB: adjList[1], AdjL: adjList[2], AdjR: adjList[3]
                    //     );
                    // }
                    else if (item != null)
                    {
                        // var (baseColor, outlineColor) = UIConstants.GetItemTeirColor(item.ItemTier); 
                        Color baseColor = item.GetTooltipContent().titleColor; 
                        slot.Paint(
                            baseColor: baseColor, outlineColor: baseColor
                            , AdjT: adjList[0], AdjB: adjList[1], AdjL: adjList[2], AdjR: adjList[3]
                        );
                    }
                    else if (InventoryUI.Instance.CursorElement != null && _cursorPosition != null && highlightPos.Contains(curPos))
                    {
                        slot.Paint(isValidPosition ? UIConstants.COLOR_ITEM_SLOT_HOVER_VALID : UIConstants.COLOR_ITEM_SLOT_HOVER_INVALID);
                    }
                    else
                    {
                        slot.Paint(UIConstants.COLOR_ITEM_SLOT_NEUTRAL);
                    }
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _cursorPosition = null;
            RepaintGrid();
        }

        public void UpdateGridLocks(List<InventoryLock> locks)
        {
            foreach(InventoryLock invLock in locks)
            {
                invLock.Nodes.ForEach(node =>
                {
                    GridPosition target = node + invLock.Root; 
                    if(_gridRect.Contains(target))
                    {
                        if(invLock.IsLocked) _slots[target.Row, target.Col].LockSlot(invLock); 
                        else _slots[target.Row, target.Col].UnlockSlot(); 
                    }
                }); 
            }
        }

        internal void Clear()
        {
            for(int r = 0; r < _slots.GetLength(0); r++)
            {
                for(int c = 0; c < _slots.GetLength(1); c++)
                {
                    if(_slots[r,c].transform.childCount > 0)
                        Destroy(_slots[r,c].transform.GetChild(0).gameObject); 
                }
            }
        }
    }
}
