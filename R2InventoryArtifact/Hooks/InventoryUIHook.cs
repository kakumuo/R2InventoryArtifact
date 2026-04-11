



using System;
using System.Collections.Generic;
using R2InventoryArtifact.Model;
using R2InventoryArtifact.UI;
using R2InventoryArtifact.UI.Builders;
using R2InventoryArtifact.Util;
using Rewired.Utils;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace R2InventoryArtifact.Hooks
{
    /// <summary>
    /// Hooks for all lifecycle methods (awake, destroy, etc...)
    /// </summary>
    public partial class InventoryUIHook : MonoBehaviour
    {
        private bool IsArtifactEnabled
        {
            get => InventoryArtifactProvider.IsEnabled() && _inventoryUI != null; 
        }

        private InventoryUI _inventoryUI;
        private RoR2.CharacterBody _playerBody; 

        private InventoryIndex _lastInventoryItemDropped = new(); 
        private InventoryIndex _nextItemToPickUp = new (); 
        private List<InventoryLock> _locks = new() //testing only
        {
            new(){UnlockLevel=10, Root=new(0, 0), Nodes=new(){new(0, 0), new(0, 1), new(0, 2)}},
            new(){UnlockLevel=20, Root=new(1, 0), Nodes=new(){new(1, 0), new(1, 1), new(1, 2)}}
        }; 

        private void HUD_OnAwake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);

            if (!InventoryArtifactProvider.IsEnabled())
            {
                Log.Debug("Inventory Artifact not enabled...");
                return;
            }

            Log.Debug("Init InventoryUI...");

            IntRect rect = new IntRect(
                PluginConfig.InventoryWidth.IsNullOrDestroyed() ? 8 : PluginConfig.InventoryWidth.Value,
                PluginConfig.InventoryHeight.IsNullOrDestroyed() ? 10 : PluginConfig.InventoryHeight.Value
            );

            // _playerBody = LocalUserManager.GetFirstLocalUser().cachedBody; 

            //MAYBE: embed ui into the base game ui
            if (_inventoryUI)
            {
                _inventoryUI.ResetInventory();
            }
            else
            {
                _inventoryUI = ComponentBuilder.BuildInventoryUI(null);
                _inventoryUI.Initialize(rect, _locks);
                _inventoryUI.SetUIVisibility(show: false);
                _inventoryUI.OnUIVisibilityChanged += HandleMouseVisibility;
                _inventoryUI.OnInventoryItemDropped += HandleInventoryItemDropped; 
            }

            Log.Debug("InventoryUI Init Complete...");
        }

        private void HandleInventoryItemDropped(InventoryIndex inventoryIndex, int stackCount)
        {
            UniquePickup pickup; 
            CharacterBody playerBody = LocalUserManager.GetFirstLocalUser().cachedBody; 
            Vector3 launchDir = (playerBody.transform.forward + playerBody.transform.up) * 10f; //MAYBE: launch in direction of the camera
            switch(inventoryIndex.IndexType)
            {
                case InventoryIndexType.Item: 
                    pickup = new UniquePickup(PickupCatalog.FindPickupIndex(inventoryIndex.ItemIndex)); 
                    PickupDropletController.CreatePickupDroplet(pickup, playerBody.transform.position, launchDir, false);
                    playerBody.inventory.RemoveItemPermanent(inventoryIndex.ItemIndex);
                break; 
                case InventoryIndexType.Equipment: 
                    pickup = new UniquePickup(PickupCatalog.FindPickupIndex(inventoryIndex.EquipmentIndex)); 
                    PickupDropletController.CreatePickupDroplet(pickup, playerBody.transform.position, launchDir, false);
                    playerBody.inventory.RemoveEquipment(inventoryIndex.EquipmentIndex); 
                break; 
            }
            
            _lastInventoryItemDropped = inventoryIndex; 
        }

        private void HandleItemRemoved(ItemIndex itemIndex, int count, bool isTemp)
        {
            // do remove if not removed from dropping through inventory
            if(!isTemp && _inventoryUI && _lastInventoryItemDropped.IsNull())
                _inventoryUI.RemoveFromInventory(itemIndex, isTemp); 
            else 
                _inventoryUI.RemoveFromInventory(itemIndex, isTemp); 
            _lastInventoryItemDropped.Reset(); 
        }

        private void Inventory_RemoveItemPermanent_ItemIndex_int(On.RoR2.Inventory.orig_RemoveItemPermanent_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            HandleItemRemoved(itemIndex, count, false); 
            orig(self, itemIndex, count); 
        }

        private void Inventory_RemoveItemPermanent_ItemDef_int(On.RoR2.Inventory.orig_RemoveItemPermanent_ItemDef_int orig, Inventory self, ItemDef itemDef, int count)
        {
            HandleItemRemoved(itemDef.itemIndex, count, false); 
            orig(self, itemDef, count); 
        }

        // private void GenericPickupController_OnInteractionBegin(On.RoR2.GenericPickupController.orig_OnInteractionBegin orig, GenericPickupController self, Interactor activator)
        // {
        //     if(!IsArtifactEnabled) orig(self, activator); 
        //     if(self.pickup.pickupIndex == PickupIndex.none) orig(self, activator); 

        //     PickupDef pickup = PickupCatalog.GetPickupDef(self.pickup.pickupIndex); 
        //     if(pickup.itemIndex != ItemIndex.None) _nextItemToPickUp = new(pickup.itemIndex);
        //     if(pickup.equipmentIndex != EquipmentIndex.None) _nextItemToPickUp = new(pickup.equipmentIndex);

        //     orig(self, activator); 
        // }

        // FIXME: when picking up multiple of the same item, adds max stacks under a new slot
        private void GenericPickupController_AttemptGrant(On.RoR2.GenericPickupController.orig_AttemptGrant orig, RoR2.GenericPickupController self, RoR2.CharacterBody body)
        {
            if (!InventoryArtifactProvider.IsEnabled() || !body.isPlayerControlled || self.pickup == null)
            {
                orig(self, body);
                return;
            }

            PickupDef pickupDef = PickupCatalog.GetPickupDef(self.pickup.pickupIndex);

            if (pickupDef.itemIndex == ItemIndex.None && pickupDef.equipmentIndex == EquipmentIndex.None) // only allow items and equiptment in inventory
                orig(self, body);

            Debug.Log($"GenericPickupController_AttemptGrant: {body.name} {self.pickup.pickupIndex}");
            bool addToNonEquip = self.pickup.isTempItem; 
            if (
                (_nextItemToPickUp == pickupDef.itemIndex || _nextItemToPickUp == pickupDef.equipmentIndex ) &&
                _inventoryUI.AddToInventory(pickupDef.itemIndex, addToNonEquip)
            ){
                _nextItemToPickUp.Reset(); 
                orig(self, body);
            }
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

        private void Inventory_RemoveItemTemp(On.RoR2.Inventory.orig_RemoveItemTemp orig, Inventory self, ItemIndex itemIndex, float count)
        {
            HandleItemRemoved(itemIndex, (int) count, true);
            orig(self, itemIndex, count);
        }

        private void HandleMouseVisibility(bool show)
        {
            var pes = MPEventSystemManager.primaryEventSystem;
            pes.cursorOpenerCount = show ? 1 : 0;
            // pes.SetCursorIndicatorEnabled(show); a
        }

        private void Run_BeginStage(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            if(_inventoryUI) _inventoryUI.gameObject.SetActive(true);
            orig(self);
        }

        private void Run_EndStage(On.RoR2.Run.orig_EndStage orig, Run self)
        {
           if(_inventoryUI) _inventoryUI.gameObject.SetActive(false);
            orig(self);
        }

        private void PauseScreenController_OnPauseEnd(On.RoR2.UI.PauseScreenController.orig_OnPauseEnd orig, PauseScreenController self)
        {
            if(_inventoryUI) _inventoryUI.gameObject.SetActive(true);
            orig(self);
        }

        private void PauseScreenController_OnPauseStart(On.RoR2.UI.PauseScreenController.orig_OnPauseStart orig, PauseScreenController self)
        {
            if(_inventoryUI) _inventoryUI.gameObject.SetActive(false);
            orig(self);
        }

        private GameEndReportPanelController GameOverController_GenerateReportScreen(On.RoR2.GameOverController.orig_GenerateReportScreen orig, GameOverController self, HUD hud)
        {
            if(_inventoryUI) _inventoryUI.gameObject.SetActive(false);
            return orig(self, hud);
        }


        private void Start()
        {
            On.RoR2.UI.HUD.Awake += HUD_OnAwake;
            On.RoR2.Run.BeginStage += Run_BeginStage;
            On.RoR2.Run.EndStage += Run_EndStage;
            On.RoR2.GameOverController.GenerateReportScreen += GameOverController_GenerateReportScreen;

            On.RoR2.GenericPickupController.AttemptGrant += GenericPickupController_AttemptGrant;
            On.RoR2.GenericPickupController.OnTriggerStay += GenericPickupController_OnTriggerStay;
            // On.RoR2.GenericPickupController.OnInteractionBegin += GenericPickupController_OnInteractionBegin;

            On.RoR2.Inventory.RemoveItemPermanent_ItemDef_int += Inventory_RemoveItemPermanent_ItemDef_int;
            On.RoR2.Inventory.RemoveItemPermanent_ItemIndex_int += Inventory_RemoveItemPermanent_ItemIndex_int;
        }

        private void OnDestroy()
        {
            On.RoR2.UI.HUD.Awake -= HUD_OnAwake;
            On.RoR2.UI.PauseScreenController.OnPauseStart -= PauseScreenController_OnPauseStart; 
            On.RoR2.UI.PauseScreenController.OnPauseEnd -= PauseScreenController_OnPauseEnd; 
            On.RoR2.Run.BeginStage -= Run_BeginStage;
            On.RoR2.Run.EndStage -= Run_EndStage;
            On.RoR2.GameOverController.GenerateReportScreen -= GameOverController_GenerateReportScreen;

            On.RoR2.GenericPickupController.AttemptGrant -= GenericPickupController_AttemptGrant;
            // On.RoR2.GenericPickupController.OnInteractionBegin -= GenericPickupController_OnInteractionBegin;
            On.RoR2.GenericPickupController.OnTriggerStay -= GenericPickupController_OnTriggerStay;

            On.RoR2.Inventory.RemoveItemPermanent_ItemDef_int -= Inventory_RemoveItemPermanent_ItemDef_int;
            On.RoR2.Inventory.RemoveItemPermanent_ItemIndex_int -= Inventory_RemoveItemPermanent_ItemIndex_int;

            On.RoR2.Inventory.RemoveItemTemp -= Inventory_RemoveItemTemp; 
        }


        void Update()
        {
            if(IsArtifactEnabled)
            {
                switch(PluginConfig.InventoryShowType.Value)
                {
                    case InventoryShowType.ToggleShow: 
                        if(PluginConfig.ShowInventoryKey.Value.IsUp()) _inventoryUI.SetUIVisibility(!_inventoryUI.IsVisible); 
                    break;
                    case InventoryShowType.HoldToShow: 
                        if(PluginConfig.ShowInventoryKey.Value.IsDown()) _inventoryUI.SetUIVisibility(true); 
                        else if (PluginConfig.ShowInventoryKey.Value.IsUp()) _inventoryUI.SetUIVisibility(false); 
                    break; 
                }

                if(PluginConfig.RotateInventoryItemKey.Value.IsUp()) _inventoryUI.RotateCursorItem(); 
            }
        }
    }
}