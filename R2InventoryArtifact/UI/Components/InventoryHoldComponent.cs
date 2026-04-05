using UnityEngine; 
using UnityEngine.EventSystems;
using R2InventoryArtifact.UI.Builders;
using R2InventoryArtifact.Model;

namespace R2InventoryArtifact.UI.Components
    {
    public class InventoryHoldComponent : MonoBehaviour, IDropHandler
    {
        public static InventoryHoldComponent Instance; 
        public static Transform _listTarget; 

        void Awake()
        {
            Instance = this; 
            if(transform.childCount > 0)
            {
                _listTarget = transform.GetChild(0); 
            } else
            {
                _listTarget = transform; 
            }
        }

        public void AddToHold(InventoryItem item)
        {
            InventoryModel.AddToHold(item); 
            InventoryHoldElement holdElement = ComponentBuilder.BuildHoldElement(item.GetItemName());
            holdElement.Initialize(item); 
            holdElement.transform.SetParent(_listTarget);  
        }

        public void OnDrop(PointerEventData eventData)
        {
            InventoryItemElement element = eventData.pointerDrag.GetComponent<InventoryItemElement>();
            if(element == null || (!element.Item.IsDroppable && element.DragSource == DragSource.GRID)) return; 

            InventoryItem item = element.Item; 
            Destroy(element.gameObject);  
            
            AddToHold(item); 
        }
    }
}