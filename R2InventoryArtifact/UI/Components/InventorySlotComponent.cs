

using RoR2.UI;
using R2InventoryArtifact.Model;
using R2InventoryArtifact.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using R2InventoryArtifact.UI.Services;

namespace R2InventoryArtifact.UI.Components
{
    public struct SlotPaintParams
    {
        public Color baseColor; 
        public Color outlineColor;
        public bool AdjT, AdjB, AdjL, AdjR; 
    }

    public class InventorySlotComponent : MonoBehaviour, IDropHandler, IPointerEnterHandler
    {
        private const float OUTLINE_THICKNESS = .05f; //.25f max 

        private GridPosition _pos; 
        private InventoryGridComponent _parentGrid; 
        private InventoryItem _item; 
        private InventoryItemElement _itemElement; 
        private InventoryLock _slotLock; 
        private TooltipProvider _tooltipProvider; 

        private Image _img; 

        public void Initialize(InventoryGridComponent parentGrid, GridPosition pos)
        {
            _img = GetComponent<Image>(); 
            _tooltipProvider = GetComponent<TooltipProvider>(); 

            // Paint(UIConstants.COLOR_ITEM_SLOT_NEUTRAL);
            _parentGrid = parentGrid; 
            _pos = pos; 

            _tooltipProvider.enabled = false; 
        }

        public void Occupy(InventoryItem item)
        {
            _item = item; 
            _tooltipProvider.enabled = true; 
            _tooltipProvider.SetContent(_item.GetTooltipContent()); 
        }

        public void UnOccupy()
        {
            _item = null; 
            _tooltipProvider.enabled = false; 
        }   

        public void Paint(Color baseColor)
        {
            _img.color = baseColor; 
            _img.sprite = UIAssetService.GetTileSprite(UIAssetService.SpriteTileType.TILE); 
        }

        public void Paint(Color baseColor, bool isLocked, bool AdjT, bool AdjB, bool AdjL, bool AdjR)
        {
            _img.color = baseColor; 
            _img.sprite = UIAssetService.GetTileSprite(isLocked, !AdjT, !AdjB, !AdjL, !AdjR); 
        }

        /*
            doesnt work with item transformations where more than one of the same item is transformed to another
            best guess: 
                frame 0: void_item1 is added
                frame 1: item1 and item2 are removed
                frame 2a: void_item1 stacks are added & void_item2 is added
                frame 2b: OnTransformChildrenChanged called with transform state from frame1: (child count == 0) w/ updated children, void_item, thus void_item1 is removed
        */
        // public void OnTransformChildrenChanged()
        // {
        //     if(transform.childCount == 0)
        //     {
        //         _parentGrid.UnsetItemAt(_item, _pos); 
        //     } 
        //     else
        //     {
        //         InventoryItemElement element = transform.GetChild(0).GetComponent<InventoryItemElement>(); 
        //         _parentGrid.SetItemAt(element.Item, _pos);
        //     }
        // }

        public void SlotItem(InventoryItemElement element)
        {
            _parentGrid.SetItemAt(element.Item, _pos); 
            _itemElement = element; 
            element.transform.SetParent(transform); 
        }

        public InventoryItemElement UnslotItem()
        {
            _parentGrid.UnsetItemAt(_item, _pos); 
            InventoryItemElement res = _itemElement; 
            _itemElement = null; 

            return res;
        }

        public void OnDrop(PointerEventData eventData)
        {
            InventoryItemElement element = eventData.pointerDrag.GetComponent<InventoryItemElement>(); 
            if(element == null) return; 

            
            if(element.DragSource == DragSource.NONEQUIP) //nonequip items can only from non-equip to drop area
            {
                return; 
            }
            else if(InventoryModel.IsValidItemPosition(element.Item, _pos))
            {
                element.SetDropTarget(transform);
                element.DragSource = DragSource.GRID;  
            } else if (element.DragSource == DragSource.HOLD)
            {
                InventoryHoldComponent.Instance.AddToHold(element.Item); 
                Destroy(element.gameObject); 
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _parentGrid.UpdateCursorPosition(_pos); 
        }

        public void LockSlot(InventoryLock slotLock)
        {
            _slotLock = slotLock; 
            _tooltipProvider.enabled = true; 
            _tooltipProvider.SetContent(new()
            {
                titleColor  = UIConstants.COLOR_TOOLTIP_TITLE_SLOT_LOCKED, 
                bodyColor   = UIConstants.COLOR_TOOLTIP_BODY_SLOT_LOCKED, 
                titleToken  = "Slot Locked", 
                bodyToken   = $"Unlocks at Level <style=cIsHealth>{slotLock.UnlockLevel}</style>."
            });
            // _img.sprite = UIAssetService.GetTileSprite(UIAssetService.SpriteTileType.DISABLED_TILE); 
        }

        public void UnlockSlot()
        {
            _slotLock = null; 
            _tooltipProvider.enabled = false;  
            // _img.sprite = UIAssetService.GetTileSprite(UIAssetService.SpriteTileType.TILE); 
        }
    }
}