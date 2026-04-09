
using R2InventoryArtifact.UI.Builders;
using R2InventoryArtifact.Model;
using R2InventoryArtifact.Util;
using UnityEngine;
using R2InventoryArtifact.Util.R2API;
using UnityEngine.EventSystems;
using R2InventoryArtifact.UI.Components;
using RoR2;
using System.Collections.Generic;
using System;

namespace R2InventoryArtifact.UI
{
    public class InventoryUI : MonoBehaviour, IDropHandler
    {
        public static InventoryUI Instance;

        private CanvasGroup _canvasGroup;

        private InventoryGridComponent _inventoryGrid;
        private InventoryHoldComponent _inventoryHoldList;
        private InventoryNonEquipComponent _inventoryNonEquipList; 
        private InventoryDropComponent _inventoryDropArea; 
        // private InventoryEffectComponent _inventoryEffectList;

        public InventoryItemElement CursorElement;
        public int PlayerLevel; 

        private bool _isVisible = true;


        public event Action<InventoryIndex, int> OnInventoryItemDropped; 
        public event Action<bool> OnUIVisibilityChanged; 

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
            _inventoryDropArea.OnInventoryItemDropped += HandleItemDrop; 
        }

        public void Destroy()
        {
            _inventoryDropArea.OnInventoryItemDropped -= HandleItemDrop;
        }

        public void SetUIVisibility(bool show)
        {
            _canvasGroup.alpha = show ? 1 : 0;
            _canvasGroup.blocksRaycasts = show;
            _isVisible = show;


            if(!show)
            {
                List<InventoryUpdateResult> results = InventoryModel.DiscardHold(); 
                results.ForEach(res =>
                {
                    _inventoryHoldList.RemoveFromHold(res.InventoryItem);  
                    HandleItemDrop(res.InventoryItem); 
                });  
            }
            OnUIVisibilityChanged?.Invoke(show); 
        }

        public void HandleItemDrop(InventoryItem item)
        {
            OnInventoryItemDropped.Invoke(item.InventoryIndex, item.StackCount); 
        }

        public bool AddToInventory(EquipmentIndex equipmentIndex)
        {
            Debug.Log($"InventoryUI | AddToInventory | :{equipmentIndex}");
            return AddToInventoryHelper(new(equipmentIndex)); 
        }

        public bool AddToInventory(ItemIndex itemIndex)
        {
            Debug.Log($"InventoryUI | AddToInventory | :{itemIndex}");
            return AddToInventoryHelper(new(itemIndex)); 
        }

        private bool AddToInventoryHelper(InventoryIndex inventoryIndex)
        {
            InventoryUpdateResult result = InventoryModel.AddToInventory(inventoryIndex); 
            switch (result.ResultCode)
            {
                case InventoryResultCode.NONEQUIP_INSERT: 
                    _inventoryNonEquipList.AddItem(result.InventoryItem); 
                    return true; 
                case InventoryResultCode.GRID_INSERT:
                    _inventoryGrid.InsertItemAt(result.InventoryItem, result.Pos);
                    return true;
                case InventoryResultCode.HOLD_INSERT:
                    _inventoryHoldList.AddToHold(result.InventoryItem);
                    return true; 
                default: break;
            }

            return false; 
        }

        // public void AddToHold(R2ItemCode itemCode){}
        public void AddToHold(InventoryItem item)
        {
            InventoryModel.AddToHold(item); 
            _inventoryHoldList.AddToHold(item); 
        }

        public void RemoveItem(InventoryIndex inventoryIndex)
        {
            List<InventoryUpdateResult> removeResult = InventoryModel.RemoveItems(inventoryIndex); 
            removeResult.ForEach(result =>
            {
                switch(result.ResultCode)
                {
                    case InventoryResultCode.GRID_REMOVE:
                        _inventoryGrid.RemoveAt(result.Pos); 
                    break; 
                    case InventoryResultCode.HOLD_REMOVE: 
                        _inventoryHoldList.RemoveFromHold(result.InventoryItem); 
                    break; 
                    case InventoryResultCode.NONEQUIP_REMOVE: 
                        _inventoryNonEquipList.RemoveFromNonEquip(result.InventoryItem); 
                    break; 
                }
            });
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

        public void ResetInventory()
        {
            // MAYBE: reset InventoryModel
            _inventoryHoldList.Clear(); 
            _inventoryNonEquipList.Clear(); 
            _inventoryGrid.Clear(); 

            InventoryModel.Reset(); 
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Q))
            {
                SetUIVisibility(!_isVisible); 
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
                AddToHold(element.Item);
                Destroy(element.gameObject);
            }
        }
    }
}