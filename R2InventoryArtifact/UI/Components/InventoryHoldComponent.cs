using UnityEngine;
using UnityEngine.EventSystems;
using R2InventoryArtifact.UI.Builders;
using R2InventoryArtifact.Model;
using System.Collections.Generic;

namespace R2InventoryArtifact.UI.Components
{
    public class InventoryHoldComponent : MonoBehaviour, IDropHandler
    {
        public static InventoryHoldComponent Instance;
        private static Transform _listTarget;
        private static List<InventoryHoldElement> _elements;

        void Awake()
        {
            Instance = this;
            if (transform.childCount > 0)
            {
                _listTarget = transform.GetChild(0);
            }
            else
            {
                _listTarget = transform;
            }
            _elements = new();
        }

        public void AddToHold(InventoryItem item)
        {
            InventoryHoldElement holdElement = ComponentBuilder.BuildHoldElement(item.GetItemName());
            holdElement.Initialize(item);
            holdElement.transform.SetParent(_listTarget);
            _elements.Add(holdElement);
        }

        public void RemoveFromHold(InventoryItem item)
        {
            InventoryHoldElement target = _elements.Find(element => element.Item == item);
            _elements.Remove(target);
            if (target) Destroy(target.gameObject);
        }

        public void OnDrop(PointerEventData eventData)
        {
            InventoryItemElement element = eventData.pointerDrag.GetComponent<InventoryItemElement>();
            if (element == null || (!element.Item.IsDroppable && element.DragSource == DragSource.GRID)) return;

            InventoryItem item = element.Item;
            Destroy(element.gameObject);

            InventoryUI.Instance.AddToHold(item);
        }
    }
}