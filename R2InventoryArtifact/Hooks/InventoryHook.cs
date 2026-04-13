

using System;
using System.Collections.Generic;
using System.Linq;
using R2InventoryArtifact.Model;
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
        private List<UniquePickup> _FORCE_PICKUPS_TO_NONEQUIP = new(); 
        private List<ItemDef.Pair> _BASE2CONSUME = new(); 
        
        public void InitializeInventoryHook() {
            _FORCE_PICKUPS_TO_NONEQUIP = new ()
            {
                new UniquePickup(PickupCatalog.FindPickupIndex(DLC2Content.Items.LowerPricedChests.itemIndex)),
                new UniquePickup(PickupCatalog.FindPickupIndex(DLC1Content.Items.RegeneratingScrapConsumed.itemIndex))
            }; 

            _BASE2CONSUME = new ()
            {
                new(){itemDef1 = RoR2Content.Items.ExtraLife,           itemDef2 = RoR2Content.Items.ExtraLifeConsumed},
                new(){itemDef1 = DLC1Content.Items.ExtraLifeVoid,       itemDef2 = DLC1Content.Items.ExtraLifeVoidConsumed},
                new(){itemDef1 = DLC1Content.Items.HealingPotion,       itemDef2 = DLC1Content.Items.HealingPotionConsumed},
                new(){itemDef1 = DLC1Content.Items.RegeneratingScrap,   itemDef2 = DLC1Content.Items.RegeneratingScrapConsumed},
                new(){itemDef1 = DLC1Content.Items.FragileDamageBonus,  itemDef2 = DLC1Content.Items.FragileDamageBonusConsumed},
                new(){itemDef1 = DLC2Content.Items.LowerPricedChests,   itemDef2 = DLC2Content.Items.LowerPricedChestsConsumed},
                new(){itemDef1 = DLC2Content.Items.TeleportOnLowHealth, itemDef2 = DLC2Content.Items.TeleportOnLowHealthConsumed},
            }; 
        }

        /*************************** INVENTORY REMOVE ***************************/
        private void HandleInventoryItemDropped(UniquePickup pickup, int stackCount)
        {
            CharacterBody playerBody = LocalUserManager.GetFirstLocalUser().cachedBody; 
            Vector3 launchDir = (playerBody.transform.forward + playerBody.transform.up) * 10f; //MAYBE: launch in direction of the camera

            for(int i = 0; i < stackCount; i++)
            {
                PickupDropletController.CreatePickupDroplet(pickup, playerBody.transform.position, launchDir, false);
            }
            PickupDef pickupDef = pickup.pickupIndex.pickupDef; 
            // _lastInventoryItemDropped = pickup; 

            if(pickupDef.itemIndex != ItemIndex.None)
            {
                playerBody.inventory.RemoveItemPermanent(pickupDef.itemIndex, stackCount);
            }
            else if (pickupDef.equipmentIndex != EquipmentIndex.None)
            {
                playerBody.inventory.RemoveEquipment(pickupDef.equipmentIndex);
            }
        }

        private void HandleItemRemoved(UniquePickup pickup, int count)
        {
            if(!UIHook.InventoryUI) return; 
            Log.Info($"Removing: {pickup.pickupIndex}"); 

            // do remove if not removed from dropping through inventory            
            bool inventoryDroppedCalledInStack = new System.Diagnostics.StackTrace(1, false)
                .GetFrames()
                .Select(f => f.GetMethod().Name)
                .Contains("HandleInventoryItemDropped"); 

            

            if(!inventoryDroppedCalledInStack)  
                UIHook.InventoryUI.RemoveFromInventory(pickup, count); 
            // _lastInventoryItemDropped = UniquePickup.none; 
        }

        private void Inventory_RemoveItemPermanent_ItemIndex_int(On.RoR2.Inventory.orig_RemoveItemPermanent_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemIndex)); 
            HandleItemRemoved(pickup, count); 
            orig(self, itemIndex, count); 
        }

        private void Inventory_RemoveItemPermanent_ItemDef_int(On.RoR2.Inventory.orig_RemoveItemPermanent_ItemDef_int orig, Inventory self, ItemDef itemDef, int count)
        {
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemDef.itemIndex)); 
            HandleItemRemoved(pickup, count); 
            orig(self, itemDef, count); 
        }

        private void Inventory_RemoveItem_ItemIndex_int(On.RoR2.Inventory.orig_RemoveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemIndex)); 
            HandleItemRemoved(pickup, count); 
            orig(self, itemIndex, count); 
        }

        private void Inventory_RemoveItem_ItemDef_int(On.RoR2.Inventory.orig_RemoveItem_ItemDef_int orig, Inventory self, ItemDef itemDef, int count)
        {
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemDef.itemIndex)); 
            HandleItemRemoved(pickup, count); 
            orig(self, itemDef, count); 
        }

        private void Inventory_RemoveItemTemp(On.RoR2.Inventory.orig_RemoveItemTemp orig, Inventory self, ItemIndex itemIndex, float count)
        {
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemIndex)); 
            HandleItemRemoved(pickup, (int) count);
            orig(self, itemIndex, count);
        }

        private void Inventory_RemoveItemChanneled(On.RoR2.Inventory.orig_RemoveItemChanneled orig, Inventory self, ItemIndex itemIndex, int count)
        {
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemIndex)); 
            HandleItemRemoved(pickup, count);
            orig(self, itemIndex, count);
        }
        
        private void Inventory_RemoveEquipment(On.RoR2.Inventory.orig_RemoveEquipment orig, Inventory self, EquipmentIndex equipmentIndex)
        {
            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(equipmentIndex)); 
            HandleItemRemoved(pickup, 1);
            orig(self, equipmentIndex);
        }
        
        // private void Inventory_RemoveEquipmentSet(On.RoR2.Inventory.orig_RemoveEquipmentSet orig, Inventory self)
        // {
        //     // UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(equipmentIndex)); 
        //     // HandleItemRemoved(pickup, 1);
        //     orig(self);
        // }

        /*************************** INVENTORY ADD ***************************/
        private bool HandleInventoryAdd(UniquePickup pickup, bool addToNonEquip, int count)
        {
            // check to see if already called by other means (ex: GrantItem)
            var methodHandle = System.Reflection.MethodBase.GetCurrentMethod(); 
            bool methodAlreadyCalled = new System.Diagnostics.StackTrace(1, false)
                .GetFrames()
                .Select(f => f.GetMethod().Name)
                .Contains(methodHandle.Name); 
            if(methodAlreadyCalled) return false;


            UniquePickup targetPickup = pickup; 
            PickupDef pickupDef = pickup.pickupIndex.pickupDef; 

            if (pickupDef.itemIndex == ItemIndex.None && pickupDef.equipmentIndex == EquipmentIndex.None) // only allow items and equiptment in inventory
                return false; 

            Log.Info($"Adding... {pickup.pickupIndex}");
            
            // if adding consumed item, try remove base from player TODO: see if there is a better way to do this
            foreach(var pair in _BASE2CONSUME)
            {
                if(pair.itemDef2.itemIndex == pickupDef.itemIndex)
                {
                    UIHook.InventoryUI.RemoveFromInventory(new UniquePickup(PickupCatalog.FindPickupIndex(pair.itemDef1.itemIndex)), count);
                }
            }

            // check void items, TODO: see if there is a better way to do this CONTINUE: move logic to On.RoR2.Items.ContagiousItemManager.TryForceReplacement
            var pairs = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem]; 
            foreach(var pair in pairs)
            {
                if(pair.itemDef1.itemIndex == pickupDef.itemIndex)
                {
                    targetPickup = new UniquePickup(PickupCatalog.FindPickupIndex(pair.itemDef2.itemIndex)); 
                    break; 
                }
            }

                        
            InventoryResultCode resultCode = UIHook.InventoryUI.AddToInventory(targetPickup, addToNonEquip); 

            if(resultCode == InventoryResultCode.FAILED)
                return false; 
            else if (resultCode == InventoryResultCode.HOLD_INSERT || resultCode == InventoryResultCode.HOLD_UPDATE)
                UIHook.InventoryUI.SetUIVisibility(true);  //Force UI open when items are added to hold

            return true; 
        }

        private void GenericPickupController_AttemptGrant(On.RoR2.GenericPickupController.orig_AttemptGrant orig, RoR2.GenericPickupController self, RoR2.CharacterBody body)
        {
            if (!InventoryArtifactProvider.IsEnabled() || !body.isPlayerControlled || self.pickup == null)
            {
                orig(self, body);
                return;
            }    
            bool addToNonEquip = self.pickup.isTempItem  || _FORCE_PICKUPS_TO_NONEQUIP.Contains(self.pickup); 
            HandleInventoryAdd(self.pickup, addToNonEquip, 1);  
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

        // only for item consumptions, other items handled through grant
        private void Inventory_GiveItemPermanent_ItemIndex_int(On.RoR2.Inventory.orig_GiveItemPermanent_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int countToAdd)
        {
            // Log.Info($"Adding... {itemIndex}"); 
            bool isConsumed = false; 
            foreach(var pair in _BASE2CONSUME)
            {
                if(pair.itemDef2.itemIndex == itemIndex)
                {
                    isConsumed = true; 
                    break; 
                }
            }

            if(isConsumed)
                HandleInventoryAdd(new UniquePickup(PickupCatalog.FindPickupIndex(itemIndex)), true, countToAdd); 
            orig(self, itemIndex, countToAdd); 
        }
        
        // only for item consumptions, other items handled through grant
        private void Inventory_GiveItemPermanent_ItemDef_int(On.RoR2.Inventory.orig_GiveItemPermanent_ItemDef_int orig, Inventory self, ItemDef itemDef, int count)
        {
            // Log.Info($"Adding... {itemDef}"); 
            bool isConsumed = false; 
            foreach(var pair in _BASE2CONSUME)
            {
                if(pair.itemDef2.itemIndex == itemDef.itemIndex)
                {
                    isConsumed = true; 
                    break; 
                }
            }
            
            if(isConsumed)
                HandleInventoryAdd(new UniquePickup(PickupCatalog.FindPickupIndex(itemDef.itemIndex)), true, count); 
            orig(self, itemDef, count); 
        }

        public void Start(){
            UIHook.OnInventoryItemDropped += HandleInventoryItemDropped; 
            UIHook.OnInitializeUI += InitializeInventoryHook; 

            On.RoR2.GenericPickupController.AttemptGrant        += GenericPickupController_AttemptGrant;
            On.RoR2.GenericPickupController.OnTriggerStay       += GenericPickupController_OnTriggerStay;
            // On.RoR2.GenericPickupController.OnInteractionBegin += GenericPickupController_OnInteractionBegin;

            On.RoR2.Inventory.RemoveItemChanneled               += Inventory_RemoveItemChanneled; 
            On.RoR2.Inventory.RemoveItemPermanent_ItemDef_int   += Inventory_RemoveItemPermanent_ItemDef_int;
            On.RoR2.Inventory.RemoveItemPermanent_ItemIndex_int += Inventory_RemoveItemPermanent_ItemIndex_int; 
            On.RoR2.Inventory.RemoveItem_ItemDef_int            += Inventory_RemoveItem_ItemDef_int;  
            On.RoR2.Inventory.RemoveItem_ItemIndex_int          += Inventory_RemoveItem_ItemIndex_int; 

            On.RoR2.Inventory.RemoveEquipment                   += Inventory_RemoveEquipment; 
            // On.RoR2.Inventory.RemoveEquipmentSet                += Inventory_RemoveEquipmentSet; 

            On.RoR2.Inventory.GiveItemPermanent_ItemDef_int     += Inventory_GiveItemPermanent_ItemDef_int; 
            On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int   += Inventory_GiveItemPermanent_ItemIndex_int; 
        }

        public void OnDestroy() {
            On.RoR2.GenericPickupController.AttemptGrant    -= GenericPickupController_AttemptGrant;
            On.RoR2.GenericPickupController.OnTriggerStay   -= GenericPickupController_OnTriggerStay;
            // On.RoR2.GenericPickupController.OnInteractionBegin -= GenericPickupController_OnInteractionBegin;

            On.RoR2.Inventory.RemoveItemChanneled               -= Inventory_RemoveItemChanneled; 
            On.RoR2.Inventory.RemoveItemPermanent_ItemDef_int   -= Inventory_RemoveItemPermanent_ItemDef_int;
            On.RoR2.Inventory.RemoveItemPermanent_ItemIndex_int -= Inventory_RemoveItemPermanent_ItemIndex_int;
            On.RoR2.Inventory.RemoveItem_ItemDef_int            -= Inventory_RemoveItem_ItemDef_int; 
            On.RoR2.Inventory.RemoveItem_ItemIndex_int          -= Inventory_RemoveItem_ItemIndex_int;

            On.RoR2.Inventory.RemoveEquipment                   -= Inventory_RemoveEquipment; 
            // On.RoR2.Inventory.RemoveEquipmentSet                -= Inventory_RemoveEquipmentSet; 

            On.RoR2.Inventory.GiveItemPermanent_ItemDef_int     -= Inventory_GiveItemPermanent_ItemDef_int; 
            On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int   -= Inventory_GiveItemPermanent_ItemIndex_int; 
        }
    }
}