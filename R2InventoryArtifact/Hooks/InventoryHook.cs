

using System;
using System.Collections.Generic;
using System.Linq;
using R2InventoryArtifact.Artifact;
using R2InventoryArtifact.Model;
using RoR2;
using UnityEngine;
namespace R2InventoryArtifact.Hooks
{
    /// <summary>
    /// Hooks for managing inventory
    /// </summary>
    public class InventoryHook : MonoBehaviour
    {
        // 
        private Dictionary<ItemIndex, int> tempItemDict; 
        private List<ItemIndex> _FORCE_ITEMS_TO_NONEQUIP = new(); 
        private List<ItemIndex> _ITEM_BLACKLIST = new(); 
        private bool _isInRun = false; 
        
        public void InitializeInventoryHook() {
            tempItemDict = new Dictionary<ItemIndex, int>(); 
            _FORCE_ITEMS_TO_NONEQUIP = new ()
            {
                DLC2Content.Items.LowerPricedChests.itemIndex, 
                DLC2Content.Items.LowerPricedChestsConsumed.itemIndex, 
                DLC1Content.Items.RegeneratingScrap.itemIndex, 
                DLC1Content.Items.RegeneratingScrapConsumed.itemIndex, 
                RoR2Content.Items.ExtraLifeConsumed.itemIndex, 
                DLC1Content.Items.ExtraLifeVoidConsumed.itemIndex, 
                DLC1Content.Items.HealingPotionConsumed.itemIndex, 
                DLC1Content.Items.FragileDamageBonusConsumed.itemIndex, 
                DLC2Content.Items.TeleportOnLowHealthConsumed.itemIndex, 
            }; 
        }

        private bool IsPlayerInventory(Inventory inventory)
        {
            CharacterMaster master = inventory.GetComponentInParent<CharacterMaster>(); 
            if(master == null) return false; 

            return UIHook.PlayerBody != null && master.GetBody() == UIHook.PlayerBody; 
        }

        /*************************** INVENTORY REMOVE ***************************/
        private void HandleInventoryItemDropped(UniquePickup pickup, int stackCount)
        {
            // CharacterBody PlayerBody = LocalUserManager.GetFirstLocalUser().cachedBody; 
            Vector3 launchDir = (UIHook.PlayerBody.transform.forward + UIHook.PlayerBody.transform.up) * 10f; //MAYBE: launch in direction of the camera

            for(int i = 0; i < stackCount; i++)
            {
                PickupDropletController.CreatePickupDroplet(pickup, UIHook.PlayerBody.transform.position, launchDir, false);
            }
            PickupDef pickupDef = pickup.pickupIndex.pickupDef; 
            // _lastInventoryItemDropped = pickup; 

            if(pickupDef.itemIndex != ItemIndex.None)
            {
                UIHook.PlayerBody.inventory.RemoveItemPermanent(pickupDef.itemIndex, stackCount);
            }
            else if (pickupDef.equipmentIndex != EquipmentIndex.None)
            {
                UIHook.PlayerBody.inventory.RemoveEquipment(pickupDef.equipmentIndex);
            }
        }

        private void HandleItemRemoved(ItemIndex itemIndex, bool isTemp, int count)
        {
            // do remove if not removed from dropping through inventory
            // since item dropped is invoked firsst, will need to check twice          
            bool inventoryDroppedCalledInStack = new System.Diagnostics.StackTrace(1, false)
                .GetFrames()
                .Select(f => f.GetMethod().Name)
                .Count(name => name == "HandleInventoryItemDropped") > 1; 

            // ignore blacklisted items
            if(_ITEM_BLACKLIST.Contains(itemIndex)) return; 

            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemIndex)); 
            ItemDef itemDef = ItemCatalog.allItemDefs[(int) itemIndex]; 
            bool toNonEquip = isTemp || _FORCE_ITEMS_TO_NONEQUIP.Contains(itemIndex) || itemDef.isConsumed; 
            RemoveFromInventory(pickup, toNonEquip, count); 
        }

        private void HandleEquipmentRemoved(EquipmentIndex equipIndex, int count)
        {
            // do remove if not removed from dropping through inventory
            // since item dropped is invoked firsst, will need to check twice          
            bool inventoryDroppedCalledInStack = new System.Diagnostics.StackTrace(1, false)
                .GetFrames()
                .Select(f => f.GetMethod().Name)
                .Count(name => name == "HandleInventoryItemDropped") > 1; 

            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(equipIndex)); 
            RemoveFromInventory(pickup, false, count); 
        }

        private void RemoveFromInventory(UniquePickup pickup, bool removeFromNonEquip, int count)
        {
            if(!UIHook.InventoryUI) return; 
            Log.Debug($"Removing: {pickup.pickupIndex}"); 
            
            UIHook.InventoryUI.RemoveFromInventory(pickup, removeFromNonEquip, count); 
        }

        private void Inventory_RemoveItemPermanent_ItemIndex_int(On.RoR2.Inventory.orig_RemoveItemPermanent_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            if(IsPlayerInventory(self))
                HandleItemRemoved(itemIndex, false, count); 
            orig(self, itemIndex, count); 
        }

        private void Inventory_RemoveItemPermanent_ItemDef_int(On.RoR2.Inventory.orig_RemoveItemPermanent_ItemDef_int orig, Inventory self, ItemDef itemDef, int count)
        {
            if(IsPlayerInventory(self))
                HandleItemRemoved(itemDef.itemIndex, false, count); 
            orig(self, itemDef, count); 
        }

        private void Inventory_RemoveItem_ItemIndex_int(On.RoR2.Inventory.orig_RemoveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            if(IsPlayerInventory(self))
                HandleItemRemoved(itemIndex, false, count); 
            orig(self, itemIndex, count); 
        }

        private void Inventory_RemoveItem_ItemDef_int(On.RoR2.Inventory.orig_RemoveItem_ItemDef_int orig, Inventory self, ItemDef itemDef, int count)
        {
            if(IsPlayerInventory(self))
                HandleItemRemoved(itemDef.itemIndex, false, count); 
            orig(self, itemDef, count); 
        }

        // //CONTINUE:FIXME: remove items when temp item expires  
        // private void Inventory_RemoveItemTemp(On.RoR2.Inventory.orig_RemoveItemTemp orig, Inventory self, ItemIndex itemIndex, float count)
        // {
        //     if(IsPlayerInventory(self))
        //         HandleItemRemoved(itemIndex, true, (int) count); 
        //     orig(self, itemIndex, count);
        // }
        
        // TODO: find better way to handle this
        private void Inventory_TempItemsStorage_SyncStackToDecay(On.RoR2.Inventory.TempItemsStorage.orig_SyncStackToDecay orig, ref Inventory.TempItemsStorage self, ItemIndex itemIndex)
        {
            orig(ref self, itemIndex); 
            if(!IsPlayerInventory(self.inventory)) return; 

            int prevStacks = tempItemDict.GetValueOrDefault(itemIndex, 0); 
            int curStacks = self.GetItemStacks(itemIndex); 
            // on temp item lost
            if(curStacks < prevStacks)
            {
                HandleItemRemoved(itemIndex, true, prevStacks - curStacks); 
            }

            if(curStacks == 0 && tempItemDict.ContainsKey(itemIndex)) tempItemDict.Remove(itemIndex); 
            else tempItemDict[itemIndex] = curStacks; 
        }

        private void CharacterBody_OnEquipmentLost(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if(UIHook.PlayerBody == self)
                HandleEquipmentRemoved(equipmentDef.equipmentIndex, 1); 
            orig(self, equipmentDef); 
        }
                
        /*************************** INVENTORY ADD ***************************/
        private void HandleItemAdd(ItemIndex itemIndex, bool isTemp, int count)
        {
            // ignore blacklisted items
            if(_ITEM_BLACKLIST.Contains(itemIndex)) return; 

            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemIndex)); 
            ItemDef itemDef = ItemCatalog.allItemDefs[(int) itemIndex]; 
            bool toNonEquip = isTemp || _FORCE_ITEMS_TO_NONEQUIP.Contains(itemIndex) || itemDef.isConsumed; 
            AddToInventory(pickup, toNonEquip, count); 
        }

        private void HandleEquipmentAdd(EquipmentIndex equipIndex, int count)
        {
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(equipIndex)); 
            AddToInventory(pickup, false, count); 
        }

        private bool AddToInventory(UniquePickup pickup, bool toNonEquip, int count)
        {
            // // check to see if already called by other means (ex: GrantItem)
            // var methodHandle = System.Reflection.MethodBase.GetCurrentMethod(); 
            // bool methodAlreadyCalled = new System.Diagnostics.StackTrace(1, false)
            //     .GetFrames()
            //     .Select(f => f.GetMethod().Name)
            //     .Contains(methodHandle.Name); 
            // if(methodAlreadyCalled) return false;

            PickupDef pickupDef = pickup.pickupIndex.pickupDef; 

            if (pickupDef.itemIndex == ItemIndex.None && pickupDef.equipmentIndex == EquipmentIndex.None) // only allow items and equiptment in inventory
                return false; 

            Log.Debug($"{pickup.pickupIndex}");

            bool res = true; 
            for(int i = 0; i < count; i++)
            {
                InventoryResultCode resultCode = UIHook.InventoryUI.AddToInventory(pickup, toNonEquip); 

                if(resultCode == InventoryResultCode.FAILED)
                    res = false; 
                else if (resultCode == InventoryResultCode.HOLD_INSERT || resultCode == InventoryResultCode.HOLD_UPDATE)
                    UIHook.InventoryUI.SetUIVisibility(true);  //Force UI open when items are added to hold
            }

            return res; 
        }

        private void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            if(IsPlayerInventory(self))
                HandleItemAdd(itemIndex, false, count); 
            orig(self, itemIndex, count);
        }

        // private void Inventory_GiveItem_ItemDef_int(On.RoR2.Inventory.orig_GiveItem_ItemDef_int orig, Inventory self, ItemDef itemDef, int count)
        // {
        //     if(IsPlayerInventory(self))
        //         HandleItemAdd(itemDef.itemIndex, false, count); 
        //     orig(self, itemDef, count);
        // }

        private void Inventory_GiveItemPermanent_ItemIndex_int(On.RoR2.Inventory.orig_GiveItemPermanent_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            
            if(IsPlayerInventory(self))
                HandleItemAdd(itemIndex, false, count); 
            orig(self, itemIndex, count);
        }

        // private void Inventory_GiveItemPermanent_ItemDef_int(On.RoR2.Inventory.orig_GiveItemPermanent_ItemDef_int orig, Inventory self, ItemDef itemDef, int count)
        // {
        //     if(IsPlayerInventory(self))
        //         HandleItemAdd(itemDef.itemIndex, false, count); 
        //     orig(self, itemDef, count);
        // }

        private void Inventory_GiveItemTemp(On.RoR2.Inventory.orig_GiveItemTemp orig, Inventory self, ItemIndex itemIndex, float count)
        {
           if(IsPlayerInventory(self))
            {
                if(count > 0) HandleItemAdd(itemIndex, true, (int)count); 
                else HandleItemRemoved(itemIndex, true, (int) count); 
            }
            orig(self, itemIndex, count);
        }

        private void CharacterBody_OnEquipmentGained(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if(UIHook.PlayerBody == self)
                HandleEquipmentAdd(equipmentDef.equipmentIndex, 1); 
            orig(self, equipmentDef);
        }

        /************************ TRANSFORMATIONS ************************/
        // private void GenericPickupController_OnTriggerStay(On.RoR2.GenericPickupController.orig_OnTriggerStay orig, GenericPickupController self, Collider other)
        // {
        //     if(UIHook.PlayerBody == other?.GetComponent<CharacterMaster>()?.GetBody() && PluginConfig.DisableAutoPickup.Value)
        //         return; 
        //     orig(self, other); 
        // }

        private bool Inventory_ItemTransformation_TryTransform(On.RoR2.Inventory.ItemTransformation.orig_TryTransform orig, ref Inventory.ItemTransformation self, Inventory inventory, out Inventory.ItemTransformation.TryTransformResult result)
        { 
            
            bool didTransform = orig(ref self, inventory, out result); 
            if(UIHook.PlayerBody != null && UIHook.PlayerBody.netId == inventory.netId /* && PluginConfig.DisableAutoPickup.Value */)
                if(didTransform && result.totalTransformed > 0) {
                    UniquePickup takenPickup = new UniquePickup(PickupCatalog.FindPickupIndex(result.takenItem.itemIndex)); 
                    UniquePickup givenPickup = new UniquePickup(PickupCatalog.FindPickupIndex(result.givenItem.itemIndex)); 
                    
                    if(result.takenItem.stackValues.permanentStacks > 0) {
                        RemoveFromInventory(takenPickup, false, result.takenItem.stackValues.permanentStacks);
                    }

                    if(result.takenItem.stackValues.temporaryStacksValue > 0) {
                        RemoveFromInventory(takenPickup, true, (int) result.takenItem.stackValues.temporaryStacksValue);
                    }

                    if(result.givenItem.stackValues.permanentStacks > 0) {
                        AddToInventory(givenPickup, givenPickup.isTempItem, result.givenItem.stackValues.permanentStacks);
                    }

                    if(result.givenItem.stackValues.temporaryStacksValue > 0) {
                        AddToInventory(givenPickup, givenPickup.isTempItem, (int) result.givenItem.stackValues.temporaryStacksValue);
                    }
                }
            return didTransform; 
        }

        private void Awake()
        {
            Run.onRunStartGlobal += HandleRunStart; 
            Run.onRunDestroyGlobal += HandleRunEnd; 
        }

        private void HandleRunStart(Run run) {
            _isInRun = false; 
            if(!InventoryArtifactProvider.IsEnabled()) return; 
            _isInRun = true; 

            UIHook.OnInventoryItemDropped   += HandleInventoryItemDropped; 
            UIHook.OnInitializeUI           += InitializeInventoryHook; 

            // On.RoR2.Inventory.GiveItemPermanent_ItemDef_int     += Inventory_GiveItemPermanent_ItemDef_int; 
            On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int   += Inventory_GiveItemPermanent_ItemIndex_int; 
            // On.RoR2.Inventory.GiveItemString_string             += Inventory_GiveItemString_string; 
            // On.RoR2.Inventory.GiveItemString_string_int         += Inventory_GiveItemString_string_int; 
            On.RoR2.Inventory.GiveItemTemp                      += Inventory_GiveItemTemp; 
            // On.RoR2.Inventory.GiveItem_ItemDef_int              += Inventory_GiveItem_ItemDef_int; 
            On.RoR2.Inventory.GiveItem_ItemIndex_int            += Inventory_GiveItem_ItemIndex_int; 
            On.RoR2.CharacterBody.OnEquipmentGained             += CharacterBody_OnEquipmentGained;
        
            // On.RoR2.Inventory.RemoveItemChanneled               += Inventory_RemoveItemChanneled; 
            On.RoR2.Inventory.RemoveItemPermanent_ItemDef_int   += Inventory_RemoveItemPermanent_ItemDef_int;
            On.RoR2.Inventory.RemoveItemPermanent_ItemIndex_int += Inventory_RemoveItemPermanent_ItemIndex_int; 
            On.RoR2.Inventory.RemoveItem_ItemDef_int            += Inventory_RemoveItem_ItemDef_int;  
            On.RoR2.Inventory.RemoveItem_ItemIndex_int          += Inventory_RemoveItem_ItemIndex_int; 
            // On.RoR2.Inventory.RemoveItemTemp                    += Inventory_RemoveItemTemp;
            On.RoR2.CharacterBody.OnEquipmentLost               += CharacterBody_OnEquipmentLost; 
            On.RoR2.Inventory.TempItemsStorage.SyncStackToDecay += Inventory_TempItemsStorage_SyncStackToDecay;

            // On.RoR2.GenericPickupController.OnTriggerStay       += GenericPickupController_OnTriggerStay;
            On.RoR2.Inventory.ItemTransformation.TryTransform   += Inventory_ItemTransformation_TryTransform;

            /*
            GIVE ITEM: 
                List`1 R2InventoryArtifact.Hooks.InventoryDeltaHook:FindInventoryDelta (ItemCollection, ItemCollection) (c:\Users\foxfe\Documents\Projects\R2InventoryArtifact\R2InventoryArtifact\Hooks\InventoryDeltaHook.cs:114)
                Void R2InventoryArtifact.Hooks.InventoryDeltaHook:Inventory_onInventoryChangedGlobal (Inventory) (c:\Users\foxfe\Documents\Projects\R2InventoryArtifact\R2InventoryArtifact\Hooks\InventoryDeltaHook.cs:128)
                Void RoR2.Inventory:HandleInventoryChanged () (Unknown Source:0)
                Void InventoryChangeScope:Dispose () (Unknown Source:0)
                Void RoR2.Inventory:ChangeItemStacksCount (GiveItemPermanentImpl, ItemIndex, Int32) (Unknown Source:0)
                Void RoR2.Inventory:GiveItemPermanent (ItemIndex, Int32) (Unknown Source:0)
                Void R2InventoryArtifact.DebugManager:Update () (c:\Users\foxfe\Documents\Projects\R2InventoryArtifact\R2InventoryArtifact\DebugManager.cs:50)
            */  

            /*
            ON TEMP ITEM DECAY: 
                List`1 R2InventoryArtifact.Hooks.InventoryDeltaHook:FindInventoryDelta (ItemCollection, ItemCollection) (c:\Users\foxfe\Documents\Projects\R2InventoryArtifact\R2InventoryArtifact\Hooks\InventoryDeltaHook.cs:114)
                Void R2InventoryArtifact.Hooks.InventoryDeltaHook:Inventory_onInventoryChangedGlobal (Inventory) (c:\Users\foxfe\Documents\Projects\R2InventoryArtifact\R2InventoryArtifact\Hooks\InventoryDeltaHook.cs:128)
                Void RoR2.Inventory:HandleInventoryChanged () (Unknown Source:0)
                Void InventoryChangeScope:Dispose () (Unknown Source:0)
                Void RoR2.Inventory:ChangeItemStacksCount (GiveItemTempImpl, ItemIndex, Int32) (Unknown Source:0)
                Void TempItemsStorage:SyncStackToDecay (ItemIndex) (Unknown Source:0)
                Void TempItemsStorage:SyncStacksToDecay () (Unknown Source:0)
                Void RoR2.Inventory:MyFixedUpdate (Single) (Unknown Source:0)
                Void RoR2.Inventory:FixedUpdate () (Unknown Source:0)
            */
        }

        private void HandleRunEnd(Run run)
        {
            if(!_isInRun) return; 
            _isInRun = false; 

            UIHook.OnInventoryItemDropped   -= HandleInventoryItemDropped; 
            UIHook.OnInitializeUI           -= InitializeInventoryHook; 

            // On.RoR2.Inventory.GiveItemPermanent_ItemDef_int     -= Inventory_GiveItemPermanent_ItemDef_int; 
            On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int   -= Inventory_GiveItemPermanent_ItemIndex_int; 
            // On.RoR2.Inventory.GiveItemString_string             -= Inventory_GiveItemString_string; 
            // On.RoR2.Inventory.GiveItemString_string_int         -= Inventory_GiveItemString_string_int; 
            On.RoR2.Inventory.GiveItemTemp                      -= Inventory_GiveItemTemp; 
            // On.RoR2.Inventory.GiveItem_ItemDef_int              -= Inventory_GiveItem_ItemDef_int; 
            On.RoR2.Inventory.GiveItem_ItemIndex_int            -= Inventory_GiveItem_ItemIndex_int; 
            On.RoR2.CharacterBody.OnEquipmentGained             -= CharacterBody_OnEquipmentGained;
        
            // On.RoR2.Inventory.RemoveItemChanneled               -= Inventory_RemoveItemChanneled; 
            On.RoR2.Inventory.RemoveItemPermanent_ItemDef_int   -= Inventory_RemoveItemPermanent_ItemDef_int;
            On.RoR2.Inventory.RemoveItemPermanent_ItemIndex_int -= Inventory_RemoveItemPermanent_ItemIndex_int; 
            On.RoR2.Inventory.RemoveItem_ItemDef_int            -= Inventory_RemoveItem_ItemDef_int;  
            On.RoR2.Inventory.RemoveItem_ItemIndex_int          -= Inventory_RemoveItem_ItemIndex_int; 
            // On.RoR2.Inventory.RemoveItemTemp                    -= Inventory_RemoveItemTemp;
            On.RoR2.CharacterBody.OnEquipmentLost               -= CharacterBody_OnEquipmentLost;
            On.RoR2.Inventory.TempItemsStorage.SyncStackToDecay -= Inventory_TempItemsStorage_SyncStackToDecay; 
            
            // On.RoR2.GenericPickupController.OnTriggerStay       -= GenericPickupController_OnTriggerStay;
            On.RoR2.Inventory.ItemTransformation.TryTransform   -= Inventory_ItemTransformation_TryTransform;  
        }
    }
}