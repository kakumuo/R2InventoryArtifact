


using System;
using R2InventoryArtifact.Model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace R2InventoryArtifact.UI.Components
{
    public class InventoryDropComponent : MonoBehaviour, IDropHandler
    {

        public event Action<InventoryItem> OnInventoryItemDropped; 

        public void OnDrop(PointerEventData eventData)
        {
            InventoryItemElement element = eventData.pointerDrag.GetComponent<InventoryItemElement>(); 
            if(element == null || !element.Item.IsDroppable) 
                return; 

            OnInventoryItemDropped.Invoke(element.Item); 
            Destroy(element.gameObject); 
        }
    }
}
