


using R2InventoryArtifact.Model;
using R2InventoryArtifact.UI.Builders;
using UnityEngine;
using UnityEngine.EventSystems;

namespace R2InventoryArtifact.UI.Components
{
    public class InventoryDropComponent : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            InventoryItemElement element = eventData.pointerDrag.GetComponent<InventoryItemElement>(); 
            if(element == null || !element.Item.IsDroppable) 
                return; 

            InventoryModel.DropItem(element.Item); 
            Destroy(element.gameObject); 
        }
    }
}
