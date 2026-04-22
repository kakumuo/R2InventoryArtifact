

using System;
using System.Collections.Generic;
using System.Linq;
using EntityStates.VoidSurvivor.Weapon;
using R2InventoryArtifact.Artifact;
using R2InventoryArtifact.Model;
using R2InventoryArtifact.UI;
using RoR2;
using RoR2.Items;
using UnityEngine;
namespace R2InventoryArtifact.Hooks
{
    /// <summary>
    /// Hooks for all lifecycle methods (awake, destroy, etc...)
    /// </summary>
    public class InventoryHook : MonoBehaviour
    {
        // 
        private List<UniquePickup> _FORCE_PICKUPS_TO_NONEQUIP = new(); 
        private bool _isInRun = false; 
        
        public void InitializeInventoryHook() {
            _FORCE_PICKUPS_TO_NONEQUIP = new ()
            {
                new UniquePickup(PickupCatalog.FindPickupIndex(DLC2Content.Items.LowerPricedChests.itemIndex)),
                new UniquePickup(PickupCatalog.FindPickupIndex(DLC1Content.Items.RegeneratingScrapConsumed.itemIndex)),
                new UniquePickup(PickupCatalog.FindPickupIndex(RoR2Content.Items.ExtraLifeConsumed.itemIndex)),
                new UniquePickup(PickupCatalog.FindPickupIndex(DLC1Content.Items.ExtraLifeVoidConsumed.itemIndex)),
                new UniquePickup(PickupCatalog.FindPickupIndex(DLC1Content.Items.HealingPotionConsumed.itemIndex)),
                new UniquePickup(PickupCatalog.FindPickupIndex(DLC1Content.Items.RegeneratingScrapConsumed.itemIndex)),
                new UniquePickup(PickupCatalog.FindPickupIndex(DLC1Content.Items.FragileDamageBonusConsumed.itemIndex)),
                new UniquePickup(PickupCatalog.FindPickupIndex(DLC2Content.Items.LowerPricedChestsConsumed.itemIndex)),
                new UniquePickup(PickupCatalog.FindPickupIndex(DLC2Content.Items.TeleportOnLowHealthConsumed.itemIndex)),
            }; 
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

        private void HandleItemRemoved(UniquePickup pickup, bool removeFromNonEquip, int count)
        {
            if(!UIHook.InventoryUI) return; 
            Log.Info($"Removing: {pickup.pickupIndex}"); 

            // do remove if not removed from dropping through inventory
            // since item dropped is invoked firsst, will need to check twice          
            bool inventoryDroppedCalledInStack = new System.Diagnostics.StackTrace(1, false)
                .GetFrames()
                .Select(f => f.GetMethod().Name)
                .Count(name => name == "HandleInventoryItemDropped") > 1; 

            bool isInNonEquip = _FORCE_PICKUPS_TO_NONEQUIP.FindIndex(p => p.pickupIndex == pickup.pickupIndex) != -1; 
            if(!inventoryDroppedCalledInStack)  
                UIHook.InventoryUI.RemoveFromInventory(pickup, removeFromNonEquip || isInNonEquip, count); 
            // _lastInventoryItemDropped = UniquePickup.none; 
        }

        private void Inventory_RemoveItemPermanent_ItemIndex_int(On.RoR2.Inventory.orig_RemoveItemPermanent_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemIndex)); 
            HandleItemRemoved(pickup, false, count); 
            orig(self, itemIndex, count); 
        }

        private void Inventory_RemoveItemPermanent_ItemDef_int(On.RoR2.Inventory.orig_RemoveItemPermanent_ItemDef_int orig, Inventory self, ItemDef itemDef, int count)
        {
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemDef.itemIndex)); 
            HandleItemRemoved(pickup, false, count); 
            orig(self, itemDef, count); 
        }

        private void Inventory_RemoveItem_ItemIndex_int(On.RoR2.Inventory.orig_RemoveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemIndex)); 
            HandleItemRemoved(pickup, false, count); 
            orig(self, itemIndex, count); 
        }

        private void Inventory_RemoveItem_ItemDef_int(On.RoR2.Inventory.orig_RemoveItem_ItemDef_int orig, Inventory self, ItemDef itemDef, int count)
        {
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemDef.itemIndex)); 
            HandleItemRemoved(pickup, false, count); 
            orig(self, itemDef, count); 
        }

        private void Inventory_RemoveItemTemp(On.RoR2.Inventory.orig_RemoveItemTemp orig, Inventory self, ItemIndex itemIndex, float count)
        {
            Log.Info(""); 
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemIndex)); 
            HandleItemRemoved(pickup, true, (int) count);
            orig(self, itemIndex, count);
        }

        private void Inventory_RemoveItemChanneled(On.RoR2.Inventory.orig_RemoveItemChanneled orig, Inventory self, ItemIndex itemIndex, int count)
        {
            Log.Info(""); 
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemIndex)); 
            HandleItemRemoved(pickup, true, count);
            orig(self, itemIndex, count);
        }
        
        private void Inventory_RemoveEquipment(On.RoR2.Inventory.orig_RemoveEquipment orig, Inventory self, EquipmentIndex equipmentIndex)
        {
            Log.Info(""); 
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(equipmentIndex)); 
            HandleItemRemoved(pickup, false, 1);
            orig(self, equipmentIndex);
        }

        private void Inventory_RemoveEquipmentSet(On.RoR2.Inventory.orig_RemoveEquipmentSet orig, Inventory self)
        {
            Log.Info("");
            orig(self); 
        }
        
        /*************************** INVENTORY ADD ***************************/
        private bool HandleItemAdd(UniquePickup pickup, int count)
        {
            // check to see if already called by other means (ex: GrantItem)
            var methodHandle = System.Reflection.MethodBase.GetCurrentMethod(); 
            bool methodAlreadyCalled = new System.Diagnostics.StackTrace(1, false)
                .GetFrames()
                .Select(f => f.GetMethod().Name)
                .Contains(methodHandle.Name); 
            if(methodAlreadyCalled) return false;

            PickupDef pickupDef = pickup.pickupIndex.pickupDef; 

            if (pickupDef.itemIndex == ItemIndex.None && pickupDef.equipmentIndex == EquipmentIndex.None) // only allow items and equiptment in inventory
                return false; 

            Log.Info($"{pickup.pickupIndex}");

            bool addToNonEquip = pickup.isTempItem  || _FORCE_PICKUPS_TO_NONEQUIP.Contains(pickup); 
            bool res = true; 
            for(int i = 0; i < count; i++)
            {
                InventoryResultCode resultCode = UIHook.InventoryUI.AddToInventory(pickup, addToNonEquip); 

                if(resultCode == InventoryResultCode.FAILED)
                    res = false; 
                else if (resultCode == InventoryResultCode.HOLD_INSERT || resultCode == InventoryResultCode.HOLD_UPDATE)
                    UIHook.InventoryUI.SetUIVisibility(true);  //Force UI open when items are added to hold
            }

            return res; 
        }

        private void GenericPickupController_AttemptGrant(On.RoR2.GenericPickupController.orig_AttemptGrant orig, RoR2.GenericPickupController self, RoR2.CharacterBody body)
        {
            if (!InventoryArtifactProvider.IsEnabled() || !body.isPlayerControlled || self.pickup == null)
            {
                orig(self, body);
                return;
            }    
            HandleItemAdd(self.pickup, 1);  
            // _nextItemToPickUp = UniquePickup.none; 
            orig(self, body);
        }

        private void GenericPickupController_OnTriggerStay(On.RoR2.GenericPickupController.orig_OnTriggerStay orig, GenericPickupController self, Collider other)
        {
            if(!PluginConfig.DisableAutoPickup.Value)
            {
                orig(self, other); 
            }
            else if (self.pickup != null && self.pickup.isTempItem)
            {
                orig(self, other);
            }
        }

        /************************ TRANSFORMATIONS ************************/
        private bool Inventory_ItemTransformation_TryTransform(On.RoR2.Inventory.ItemTransformation.orig_TryTransform orig, ref Inventory.ItemTransformation self, Inventory inventory, out Inventory.ItemTransformation.TryTransformResult result)
        { 
            bool didTransform = orig(ref self, inventory, out result); 
            if(didTransform && result.totalTransformed > 0) {
                UniquePickup takenPickup = new UniquePickup(PickupCatalog.FindPickupIndex(result.takenItem.itemIndex)); 
                UniquePickup givenPickup = new UniquePickup(PickupCatalog.FindPickupIndex(result.givenItem.itemIndex)); 
                if(result.takenItem.stackValues.permanentStacks > 0) {
                    HandleItemRemoved(takenPickup, false, result.takenItem.stackValues.permanentStacks);
                }

                if(result.takenItem.stackValues.temporaryStacksValue > 0) {
                    HandleItemRemoved(takenPickup, true, (int) result.takenItem.stackValues.temporaryStacksValue);
                }

                if(result.givenItem.stackValues.permanentStacks > 0) {
                    HandleItemAdd(givenPickup, result.givenItem.stackValues.permanentStacks);
                }

                if(result.givenItem.stackValues.temporaryStacksValue > 0) {
                    HandleItemAdd(givenPickup, (int) result.givenItem.stackValues.temporaryStacksValue);
                }
            }
            return didTransform; 
        }

        
        private void CharacterBody_OnEquipmentLost(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(equipmentDef.equipmentIndex)); 
            HandleItemRemoved(pickup, false, 1); 
            orig(self, equipmentDef); 
        }

        private void HandleRunStart(Run run) {
            _isInRun = false; 
            if(!InventoryArtifactProvider.IsEnabled()) return; 
            _isInRun = true; 

            UIHook.OnInventoryItemDropped   += HandleInventoryItemDropped; 
            UIHook.OnInitializeUI           += InitializeInventoryHook; 

            On.RoR2.GenericPickupController.AttemptGrant        += GenericPickupController_AttemptGrant;
            On.RoR2.GenericPickupController.OnTriggerStay       += GenericPickupController_OnTriggerStay;
        
            On.RoR2.Inventory.RemoveItemChanneled               += Inventory_RemoveItemChanneled; 
            On.RoR2.Inventory.RemoveItemPermanent_ItemDef_int   += Inventory_RemoveItemPermanent_ItemDef_int;
            On.RoR2.Inventory.RemoveItemPermanent_ItemIndex_int += Inventory_RemoveItemPermanent_ItemIndex_int; 
            On.RoR2.Inventory.RemoveItem_ItemDef_int            += Inventory_RemoveItem_ItemDef_int;  
            On.RoR2.Inventory.RemoveItem_ItemIndex_int          += Inventory_RemoveItem_ItemIndex_int; 
            On.RoR2.Inventory.RemoveEquipment                   += Inventory_RemoveEquipment;
            On.RoR2.Inventory.RemoveEquipmentSet                += Inventory_RemoveEquipmentSet;
            On.RoR2.Inventory.RemoveItemTemp                    += Inventory_RemoveItemTemp;
            On.RoR2.CharacterBody.OnEquipmentLost               += CharacterBody_OnEquipmentLost;
            
            On.RoR2.Inventory.ItemTransformation.TryTransform += Inventory_ItemTransformation_TryTransform;  
        }

        private void HandleRunEnd(Run run)
        {
            if(!_isInRun) return; 
            _isInRun = false; 

            UIHook.OnInventoryItemDropped   -= HandleInventoryItemDropped; 
            UIHook.OnInitializeUI           -= InitializeInventoryHook; 

            On.RoR2.GenericPickupController.AttemptGrant    -= GenericPickupController_AttemptGrant;
            On.RoR2.GenericPickupController.OnTriggerStay   -= GenericPickupController_OnTriggerStay;

            On.RoR2.Inventory.RemoveItemChanneled               -= Inventory_RemoveItemChanneled; 
            On.RoR2.Inventory.RemoveItemPermanent_ItemDef_int   -= Inventory_RemoveItemPermanent_ItemDef_int;
            On.RoR2.Inventory.RemoveItemPermanent_ItemIndex_int -= Inventory_RemoveItemPermanent_ItemIndex_int;
            On.RoR2.Inventory.RemoveItem_ItemDef_int            -= Inventory_RemoveItem_ItemDef_int; 
            On.RoR2.Inventory.RemoveItem_ItemIndex_int          -= Inventory_RemoveItem_ItemIndex_int;
            On.RoR2.Inventory.RemoveEquipment                   -= Inventory_RemoveEquipment; 
            On.RoR2.Inventory.RemoveEquipmentSet                -= Inventory_RemoveEquipmentSet;
            On.RoR2.Inventory.RemoveItemTemp                    -= Inventory_RemoveItemTemp;
            On.RoR2.CharacterBody.OnEquipmentLost               -= CharacterBody_OnEquipmentLost;

            On.RoR2.Inventory.ItemTransformation.TryTransform   -= Inventory_ItemTransformation_TryTransform; 
        }

        private void Awake()
        {
            Run.onRunStartGlobal += HandleRunStart; 
            Run.onRunDestroyGlobal += HandleRunEnd; 
        }
    }
}