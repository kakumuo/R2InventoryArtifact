

using R2InventoryArtifact.Model;
using R2InventoryArtifact.Util;
using Rewired.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        private Image img; 

        public void Initialize(InventoryGridComponent parentGrid, GridPosition pos)
        {
            img = GetComponent<Image>(); 
            Paint(UIConstants.COLOR_ITEM_SLOT_NEUTRAL);
            _parentGrid = parentGrid; 
            _pos = pos; 
        }

        public void Occupy(InventoryItem item)
        {
            _item = item; 
            // img.color = UIConstants.COLOR_ITEM_SLOT_OCCUPY; 
        }

        public void UnOccupy()
        {
            _item = null; 
            // img.color = UIConstants.COLOR_ITEM_SLOT_NEUTRAL;
        }   

        public void Paint(Color baseColor)
        {
            Paint(baseColor, baseColor, false, false, false, false);
        }

        /**FIXME: 
        1. set item in inventory
        2. scroll down
        3. set item lower in inventory
        => item internal borders show when it shouldn't
        */
        public void Paint(
            Color baseColor, Color outlineColor, 
            bool AdjT, bool AdjB, bool AdjL, bool AdjR
        )
        {
            if(!img.materialForRendering)
            {
                img.color = baseColor; 
                return; 
            }
            img.materialForRendering.SetColor("_Color", baseColor);
            img.materialForRendering.SetColor("_OutlineColor", outlineColor);
            img.materialForRendering.SetFloat("_Thickness", OUTLINE_THICKNESS);
            img.materialForRendering.SetFloat("_AdjT", AdjT ? 1 : 0);
            img.materialForRendering.SetFloat("_AdjB", AdjB ? 1 : 0);
            img.materialForRendering.SetFloat("_AdjL", AdjL ? 1 : 0);
            img.materialForRendering.SetFloat("_AdjR", AdjR ? 1 : 0);
        }

        public void OnTransformChildrenChanged()
        {
            if(transform.childCount == 0)
            {
                _parentGrid.UnsetItemAt(_item, _pos); 
            } else
            {
                InventoryItemElement element = transform.GetChild(0).GetComponent<InventoryItemElement>(); 
                _parentGrid.SetItemAt(element.Item, _pos);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            InventoryItemElement element = eventData.pointerDrag.GetComponent<InventoryItemElement>(); 
            if(element == null) return; 

            
            if(InventoryModel.IsValidItemPosition(element.Item, _pos))
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
    }
}