using System;
using R2InventoryArtifact.Model;
using R2InventoryArtifact.UI.Builders;
using UnityEngine;
using System.Collections.Generic;

namespace R2InventoryArtifact.UI.Components
{
    public class InventoryNonEquipComponent : MonoBehaviour
    {
        List<InventoryItemElement> _elements;
        public static Transform _listTarget;

        public void Awake()
        {
            _elements = new List<InventoryItemElement>();
            if (transform.childCount > 0)
            {
                _listTarget = transform.GetChild(0);
            }
            else
            {
                _listTarget = transform;
            }
        }

        public void AddItem(InventoryItem item)
        {
            InventoryItemElement element = ComponentBuilder.BuildItemElement(item.GetItemName());
            element.Initialize(item, DragSource.NONEQUIP);
            element.transform.SetParent(_listTarget);
            _elements.Add(element);
        }

        internal void RemoveFromNonEquip(InventoryItem inventoryItem)
        {
            InventoryItemElement element = _elements.Find(item => item.Item == inventoryItem); 
            _elements.Remove(element); 
            Destroy(element.gameObject); 
        }
    }
}