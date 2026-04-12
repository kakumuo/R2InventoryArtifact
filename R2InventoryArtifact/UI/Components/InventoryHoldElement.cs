using R2InventoryArtifact.Model;
using R2InventoryArtifact.UI;
using R2InventoryArtifact.UI.Builders;
using R2InventoryArtifact.UI.Services;
using R2InventoryArtifact.Util.R2API;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace R2InventoryArtifact.UI.Components
{
    public class InventoryHoldElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public InventoryItem Item;
        private TextMeshProUGUI _titleLbl;
        private TextMeshProUGUI _stackLbl;
        private Image _icon;
        private TooltipProvider _tooltip;

        public void Initialize(InventoryItem item)
        {
            Item = item;
            _icon = transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI[] labels = GetComponentsInChildren<TextMeshProUGUI>();
            _titleLbl = labels[0];
            _stackLbl = labels[1];

            _icon.sprite = UIAssetService.GetSprite(item.Pickup);
            Item.OnStackCountChanged += UpdateLabels;
            // Item.OnItemCorrupted += HandleItemCorruption;

            _tooltip = gameObject.AddComponent<TooltipProvider>();
            _tooltip.SetContent(new()
            {
                titleToken = Item.GetItemName(), 
                bodyToken = Item.GetDescription(), 
                titleColor = UIConstants.GetItemTeirColor(Item.ItemTier).Item1, 
            }); 

            UpdateLabels();
        }

        void OnDestroy()
        {
            Item.OnStackCountChanged -= UpdateLabels;
            // Item.OnItemCorrupted -= HandleItemCorruption;
            // _tooltip.OnDeselect();
        }

        // public void HandleItemCorruption(R2Item targetItem)
        // {
        //     _icon.sprite = UIAssetService.GetSprite(targetItem.);
        // }

        public void UpdateLabels()
        {
            if (_titleLbl != null) _titleLbl.text = Item.GetItemName();
            if (_stackLbl != null) _stackLbl.text = $"Stack: {Item.StackCount}";
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            InventoryItemElement element = ComponentBuilder.BuildItemElement(Item.GetItemName());
            element.Initialize(Item, DragSource.HOLD);
            element.transform.SetParent(InventoryUI.Instance.transform); // need to add to canvas, otheriwse elements don't appear
            eventData.pointerDrag = element.gameObject;
            element.OnBeginDrag(eventData);

            InventoryModel.RemoveFromHold(Item);
            Destroy(gameObject);
        }

        public void OnDrag(PointerEventData eventData) { }
        public void OnEndDrag(PointerEventData eventData) { }
    }
}