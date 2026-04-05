
using R2InventoryArtifact.UI.Builders;
using R2InventoryArtifact.Model;
using R2InventoryArtifact.Util.Math;
using UnityEngine;
using R2InventoryArtifact.Util.R2API;
using UnityEngine.EventSystems;
using R2InventoryArtifact.UI.Components;

namespace R2InventoryArtifact.UI
{
    public class InventoryUI : MonoBehaviour, IDropHandler
    {
        public static InventoryUI Instance;

        private CanvasGroup _canvasGroup;

        private InventoryGridComponent _inventoryGrid;
        private InventoryHoldComponent _inventoryHoldList;
        private InventoryEffectComponent _inventoryEffectList;
        private InventoryNonEquipComponent _inventoryNonEquipList; 
        private InventoryDropComponent _inventoryDropArea; 

        public InventoryItemElement CursorElement;
        public int PlayerLevel; 

        private bool _isVisible = true;

        public void Initialize(IntRect rect)
        {
            Instance = this;

            _canvasGroup = GetComponent<CanvasGroup>();

            _inventoryHoldList      = ComponentBuilder.BuildInventoryHoldComponent(transform);  
            _inventoryDropArea      = ComponentBuilder.BuildInventoryDropComponent(transform); 
            _inventoryGrid          = ComponentBuilder.BuildInventoryGridComponent(transform); 
            _inventoryNonEquipList  = ComponentBuilder.BuildInventoryNonEquipComponent(transform); 
            // _inventoryEffectList    = ComponentBuilder.BuildInventoryEffectComponent(transform);

            _inventoryGrid.Initialize(rect);
            // _inventoryEffectList.Initialize();
        }

        public void ShowUI()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.blocksRaycasts = true;
            _isVisible = true;
        }

        public void HideUI()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
            _isVisible = false;
        }


        //TODO: move to InventoryGrid
        public void AddToInventory(R2ItemCode itemCode)
        {
            InventoryItem item;
            GridPosition itemPos;

            if(InventoryModel.TryCorruptInventory(itemCode))
            {
                _inventoryGrid.RepaintGrid(); 
            } 

            switch (InventoryModel.AddToInventory(itemCode, out item, out itemPos))
            {
                case InventoryAddCode.NONEQUIP_INSERT: 
                    _inventoryNonEquipList.AddItem(item); 
                    break; 
                case InventoryAddCode.GRID_INSERT:
                    _inventoryGrid.InsertItemAt(item, itemPos);
                    break;
                case InventoryAddCode.HOLD_INSERT:
                    _inventoryHoldList.AddToHold(item);
                    break;
                default: break;
            }
        }

        public void SetCursorElement(InventoryItemElement element)
        {
            // Debug.Log($"Selecting {element?.Item.ItemCode}"); 
            CursorElement = element;
            _inventoryGrid.RepaintGrid();
        }

        public void IncreasePlayerLevel()
        {
            PlayerLevel += 1; 
            Debug.Log($"Increasing player level to: {PlayerLevel}"); 
            InventoryModel.SetUnlocksAtLevel(PlayerLevel); 
            _inventoryGrid.RepaintGrid(); 
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Q))
            {
                if (_isVisible) HideUI(); else ShowUI();
            }

            if (Input.GetKeyUp(KeyCode.R) && CursorElement != null)
            {
                CursorElement.Rotate();
                _inventoryGrid.RepaintGrid();
            }
        }

        // handle when item element is dropped outside of drop areas
        public void OnDrop(PointerEventData eventData)
        {
            InventoryItemElement element = eventData.pointerDrag.GetComponent<InventoryItemElement>();
            if (element != null && element.DragSource == DragSource.HOLD)
            {
                _inventoryHoldList.AddToHold(element.Item);
                Destroy(element.gameObject);
            }
        }
    }
}