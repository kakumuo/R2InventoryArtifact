
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

        public bool IsVisible = true;


        public event Action<InventoryIndex, int> OnInventoryItemDropped; 
        public event Action<bool> OnUIVisibilityChanged; 

        public void Initialize(IntRect rect, List<InventoryLock> locks)
        {
            if(Instance) Destroy(Instance); 
            Instance = this;
            InventoryModel.Initialize(rect, locks);

            _canvasGroup = GetComponent<CanvasGroup>();

            _inventoryHoldList      = ComponentBuilder.BuildInventoryHoldComponent(transform);  
            _inventoryDropArea      = ComponentBuilder.BuildInventoryDropComponent(transform); 
            _inventoryGrid          = ComponentBuilder.BuildInventoryGridComponent(transform); 
            _inventoryNonEquipList  = ComponentBuilder.BuildInventoryNonEquipComponent(transform); 
            // _inventoryEffectList    = ComponentBuilder.BuildInventoryEffectComponent(transform);

            _inventoryGrid.Initialize(rect, locks);
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
            IsVisible = show;


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

        public bool AddToInventory(EquipmentIndex equipmentIndex, bool toNonEquip)
        {
            Debug.Log($"InventoryUI | AddToInventory | :{equipmentIndex}");
            return AddToInventory(new(equipmentIndex), toNonEquip); 
        }

        public bool AddToInventory(ItemIndex itemIndex, bool toNonEquip)
        {
            Debug.Log($"InventoryUI | AddToInventory | :{itemIndex}");
            return AddToInventory(new(itemIndex), toNonEquip); 
        }

        private bool AddToInventory(InventoryIndex inventoryIndex, bool toNonEquip)
        {
            InventoryUpdateResult result = InventoryModel.AddToInventory(inventoryIndex, toNonEquip); 
            if(result.ResultCode == InventoryResultCode.FAILED)
                return false; 
                
            switch (result.ResultCode)
            {
                case InventoryResultCode.NONEQUIP_INSERT: 
                    _inventoryNonEquipList.AddItem(result.InventoryItem); 
                break; 
                case InventoryResultCode.GRID_INSERT:
                    _inventoryGrid.InsertItemAt(result.InventoryItem, result.Pos);
                break; 
                case InventoryResultCode.HOLD_INSERT:
                    _inventoryHoldList.AddToHold(result.InventoryItem);
                break; 
            }

            return true; 
        }

        // public void AddToHold(R2ItemCode itemCode){}
        public void AddToHold(InventoryItem item)
        {
            InventoryModel.AddToHold(item); 
            _inventoryHoldList.AddToHold(item); 
        }

        public void RemoveFromInventory(ItemIndex itemIndex, bool isTemp)
        {
            Debug.Log($"InventoryUI | RemoveFromInventory | :{itemIndex}");
            RemoveFromInventory(new InventoryIndex(itemIndex), isTemp); 
        }

        public void RemoveFromInventory(EquipmentIndex equipmentIndex)
        {
            Debug.Log($"InventoryUI | RemoveFromInventory | :{equipmentIndex}");
            RemoveFromInventory(new InventoryIndex(equipmentIndex)); 
        }

        public void RemoveFromInventory(InventoryIndex inventoryIndex, bool isTemp)
        {
            List<InventoryUpdateResult> removeResult = InventoryModel.RemoveItems(inventoryIndex, isTemp); 
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
            List<InventoryLock> unlocks = InventoryModel.SetUnlocksAtLevel(PlayerLevel); 
            _inventoryGrid.UpdateGridLocks(unlocks); 
            _inventoryGrid.RepaintGrid(); 
        }

        public void ResetInventory()
        {
            // MAYBE: reset InventoryModel
            InventoryModel.Reset(); 

            _inventoryHoldList.Clear(); 
            _inventoryNonEquipList.Clear(); 
            _inventoryGrid.Clear(); 
            _inventoryGrid.UpdateGridLocks(InventoryModel.InventoryLocks); 

            _inventoryGrid.RepaintGrid(); 
        }

        // void Update()
        // {
        //     if (Input.GetKeyUp(KeyCode.Q))
        //     {
        //         SetUIVisibility(!IsVisible); 
        //     }

        //     if (Input.GetKeyUp(KeyCode.R) && CursorElement != null)
        //     {
        //         CursorElement.Rotate();
        //         _inventoryGrid.RepaintGrid();
        //     }
        // }

        public void RotateCursorItem()
        {
            if(CursorElement != null)
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