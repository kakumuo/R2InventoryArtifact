using R2InventoryArtifact.Model;
using R2InventoryArtifact.UI;
using R2InventoryArtifact.UI.Builders;
using R2InventoryArtifact.UI.Services;
using R2InventoryArtifact.Util.R2API;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

namespace R2InventoryArtifact.UI.Components
    {
    public class InventoryHoldElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private InventoryItem _item; 
        private TextMeshProUGUI _titleLbl;
        private TextMeshProUGUI _stackLbl;
        private Image _icon; 
        private R2TooltipProvider _tooltip; 

        public void Initialize(InventoryItem item)
        {
            _item = item; 
            _icon = transform.GetChild(0).GetComponent<Image>(); 
            TextMeshProUGUI[] labels = GetComponentsInChildren<TextMeshProUGUI>();         
            _titleLbl = labels[0]; 
            _stackLbl = labels[1]; 

            _icon.sprite = UIAssetService.GetSprite(item.ItemCode); 
            _item.OnStackCountChanged += UpdateLabels; 
            _item.OnItemCorrupted += HandleItemCorruption;

            _tooltip = gameObject.AddComponent<R2TooltipProvider>(); 
            _tooltip.Initialize(new(){
                Title = _item.GetItemName(), 
                Body = _item.GetDescription(), 
                HeaderBkg = UIConstants.GetItemTeirColor(_item.ItemTier).Item1
            }); 

            UpdateLabels(); 
        }

        void OnDestroy()
        {
            _item.OnStackCountChanged -= UpdateLabels; 
            _item.OnItemCorrupted -= HandleItemCorruption; 
            _tooltip.ForceHide(); 
        }

        public void HandleItemCorruption(R2Item targetItem)
        {
            _icon.sprite = UIAssetService.GetSprite(targetItem.ItemCode); 
        }

        public void UpdateLabels()
        {
            if(_titleLbl != null)_titleLbl.text = _item.GetItemName(); 
            if(_stackLbl != null) _stackLbl.text = $"Stack: {_item.StackCount}"; 
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            InventoryItemElement element = ComponentBuilder.BuildItemElement(_item.GetItemName()); 
            element.Initialize(_item, DragSource.HOLD); 
            element.transform.SetParent(InventoryUI.Instance.transform); // need to add to canvas, otheriwse elements don't appear
            eventData.pointerDrag = element.gameObject;         
            element.OnBeginDrag(eventData); 
            
            InventoryModel.RemoveFromHold(_item); 
            Destroy(gameObject); 
        }

        public void OnDrag(PointerEventData eventData){}
        public void OnEndDrag(PointerEventData eventData){}
    }
}