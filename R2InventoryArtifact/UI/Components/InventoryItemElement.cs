


using IL.RoR2;
using R2InventoryArtifact.Model;
using R2InventoryArtifact.UI.Services;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

namespace R2InventoryArtifact.UI.Components
{
    public enum DragSource
    {
        HOLD, GRID,
        NONEQUIP
    }

    public class InventoryItemElement :  MonoBehaviour , IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        const int MAX_ROTATION = 4; 

        private Image _icon; 
        private TextMeshProUGUI _label; 
        public InventoryItem Item; 
        private CanvasGroup _canvasGroup; 
        private Transform dropTarget; 
        private int _orientationTarget; 
        public DragSource DragSource; 
        private TooltipProvider _tooltip; 

        public void Initialize(InventoryItem item, DragSource dragSource)
        {
            Item = item; 
            _icon = GetComponentInChildren<Image>(); 
            _label = GetComponentInChildren<TextMeshProUGUI>(); 
            _canvasGroup = GetComponent<CanvasGroup>(); 

            // ItemIcon icon = this.GetComponent<ItemIcon>(); 
            // icon.SetItemIndex(item.Pickup.pickupIndex.pickupDef.itemIndex, 1, 1); 

            if(!item.IsEquippable) _canvasGroup.blocksRaycasts = false; 

            _icon.sprite = UIAssetService.GetSprite(Item.Pickup); 
            _label.text = Item.StackCount.ToString();
            
            item.OnStackCountChanged += UpdateStackCountLabel; 

            DragSource = dragSource; 

            _tooltip = gameObject.AddComponent<TooltipProvider>(); 
            _tooltip.SetContent(Item.GetTooltipContent()); 
        }

        // public void HandleItemCorruption(R2Item targetItem)
        // {
        //     Debug.Log("corrupting item"); 
        //     _icon.sprite = UIAssetService.GetSprite(targetItem.ItemCode); 

        //     _tooltip.Initialize(new(){
        //         Title = Item.GetItemName(), 
        //         Body = Item.GetDescription(), 
        //         HeaderBkg = UIConstants.GetItemTeirColor(Item.ItemTier).Item1
        //     }); 
        // }

        public void UpdateStackCountLabel()
        {
            _label.text = Item.StackCount.ToString(); 
        }

        void OnDestroy()
        {
            Item.OnStackCountChanged -= UpdateStackCountLabel; 
            // Item.OnItemCorrupted -= HandleItemCorruption; 
        }

        public void Rotate()
        {
            Item.Rotate(); 
            _orientationTarget = (_orientationTarget + 1) % MAX_ROTATION; 
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            SetDropTarget(transform.parent); 
            transform.SetParent(transform.root); 
            transform.SetAsLastSibling(); 

            _orientationTarget = 0; 

            // InventoryItemElement targetElement = eventData.pointerDrag.GetComponent<InventoryItemElement>(); 
            // if(targetElement == null) targetElement = this; 

            InventoryUI.Instance.SetCursorElement(this); 
            _canvasGroup.alpha = .5f; 
            _canvasGroup.blocksRaycasts = false; 
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position; 
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // reset rotaiton or confirm rotaion
            for(int i = 0; i < MAX_ROTATION && _orientationTarget != 0; i++)
            {
                Rotate(); 
            }
            
            transform.SetParent(dropTarget);
            _canvasGroup.alpha = 1f; 
            _canvasGroup.blocksRaycasts = true; 
            InventoryUI.Instance.SetCursorElement(null);    
        }

        public void SetDropTarget(Transform target)
        {
            dropTarget = target; 
            _orientationTarget = 0; // confirm rotation
        }
    }
}