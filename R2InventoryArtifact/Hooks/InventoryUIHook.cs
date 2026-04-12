



using System;
using System.Collections.Generic;
using System.Linq;
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
        // private RoR2.CharacterBody _playerBody; 

        private UniquePickup _lastInventoryItemDropped = UniquePickup.none; //TODO: see if there is a way to differentiate item dropped vs item removed, MAYBE: use stacktrace
        private UniquePickup _nextItemToPickUp = UniquePickup.none; 
        private List<InventoryLock> _locks = new() //testing only
        {
            new(){UnlockLevel=10, Root=new(0, 0), Nodes=new(){new(0, 0), new(0, 1), new(0, 2)}},
            new(){UnlockLevel=20, Root=new(1, 0), Nodes=new(){new(1, 0), new(1, 1), new(1, 2)}}
        }; 

        private List<UniquePickup> _FORCE_PICKUPS_TO_NONEQUIP = new(); 
        private List<ItemDef.Pair> _BASE2CONSUME = new(); 
        private bool _isInRun = false; 

        private void InitializeConstants()
        {
            _FORCE_PICKUPS_TO_NONEQUIP = new ()
            {
                new UniquePickup(PickupCatalog.FindPickupIndex(DLC2Content.Items.LowerPricedChests.itemIndex))
            }; 

            _BASE2CONSUME = new ()
            {
                new(){itemDef1 = RoR2Content.Items.ExtraLife, itemDef2 = RoR2Content.Items.ExtraLifeConsumed},
                new(){itemDef1 = DLC1Content.Items.ExtraLifeVoid, itemDef2 = DLC1Content.Items.ExtraLifeVoidConsumed},
                new(){itemDef1 = DLC1Content.Items.HealingPotion, itemDef2 = DLC1Content.Items.HealingPotionConsumed},
                new(){itemDef1 = DLC1Content.Items.RegeneratingScrap, itemDef2 = DLC1Content.Items.RegeneratingScrapConsumed},
                new(){itemDef1 = DLC1Content.Items.FragileDamageBonus, itemDef2 = DLC1Content.Items.FragileDamageBonusConsumed},
                new(){itemDef1 = DLC2Content.Items.LowerPricedChests, itemDef2 = DLC2Content.Items.LowerPricedChestsConsumed},
                new(){itemDef1 = DLC2Content.Items.TeleportOnLowHealth, itemDef2 = DLC2Content.Items.TeleportOnLowHealthConsumed},
            }; 
        }

        private void HUD_OnAwake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);

            InitializeConstants(); 

            if (!InventoryArtifactProvider.IsEnabled())
            {
                Log.Info("Inventory Artifact not enabled...");
                return;
            }

            Log.Info("Init InventoryUI...");

            IntRect rect = new IntRect(
                PluginConfig.InventoryWidth.IsNullOrDestroyed() ? 8 : PluginConfig.InventoryWidth.Value,
                PluginConfig.InventoryHeight.IsNullOrDestroyed() ? 10 : PluginConfig.InventoryHeight.Value
            );

            // _playerBody = LocalUserManager.GetFirstLocalUser().cachedBody; 

            //MAYBE: embed ui into the base game ui
            if(!_isInRun && InventoryArtifactProvider.IsEnabled())
            {
                _isInRun = true; 
                _inventoryUI = ComponentBuilder.BuildInventoryUI(null);
                _inventoryUI.Initialize(rect, _locks);
                _inventoryUI.SetUIVisibility(show: false);
                _inventoryUI.OnUIVisibilityChanged += HandleMouseVisibility;
                _inventoryUI.OnInventoryItemDropped += HandleInventoryItemDropped; 
            }

            Log.Info("InventoryUI Init Complete...");
        }

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
            if(!_inventoryUI) return; 
            Log.Info($"Removing: {pickup.pickupIndex}"); 

            // // do remove if not removed from dropping through inventory            
            bool inventoryDroppedCalledInStack = new System.Diagnostics.StackTrace(1, false)
                .GetFrames()
                .Select(f => f.GetMethod().Name)
                .Contains("HandleInventoryItemDropped"); 

            if(!inventoryDroppedCalledInStack)  
                _inventoryUI.RemoveFromInventory(pickup, count); 
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

        // private void GenericPickupController_OnInteractionBegin(On.RoR2.GenericPickupController.orig_OnInteractionBegin orig, GenericPickupController self, Interactor activator)
        // {
        //     if(!IsArtifactEnabled) orig(self, activator); 
        //     if(self.pickup.pickupIndex == PickupIndex.none) orig(self, activator); 

        //     PickupDef pickup = PickupCatalog.GetPickupDef(self.pickup.pickupIndex); 
        //     if(pickup.itemIndex != ItemIndex.None) _nextItemToPickUp = new(pickup.itemIndex);
        //     if(pickup.equipmentIndex != EquipmentIndex.None) _nextItemToPickUp = new(pickup.equipmentIndex);

        //     orig(self, activator); 
        // }

        // returns true if did add
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
                    _inventoryUI.RemoveFromInventory(new UniquePickup(PickupCatalog.FindPickupIndex(pair.itemDef1.itemIndex)), count);
                }
            }

            // check void items, TODO: see if there is a better way to do this
            var pairs = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem]; 
            foreach(var pair in pairs)
            {
                if(pair.itemDef1.itemIndex == pickupDef.itemIndex)
                {
                    targetPickup = new UniquePickup(PickupCatalog.FindPickupIndex(pair.itemDef2.itemIndex)); 
                    break; 
                }
            }
            
            InventoryResultCode resultCode = _inventoryUI.AddToInventory(targetPickup, addToNonEquip); 

            if(resultCode == InventoryResultCode.FAILED)
                return false; 
            else if (resultCode == InventoryResultCode.HOLD_INSERT || resultCode == InventoryResultCode.HOLD_UPDATE)
                _inventoryUI.SetUIVisibility(true);  //Force UI open when items are added to hold

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


        private void HandleMouseVisibility(bool show)
        {
            var pes = MPEventSystemManager.primaryEventSystem;
            // Log.Info($"Selected Obj: {pes.currentSelectedGameObject}");
            pes.cursorOpenerCount = show ? 1 : 0;
            pes.SetSelectedGameObject(null); //make sure ui components are deselected
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

        private void LevelUpEffectManager_OnCharacterLevelUp(On.RoR2.LevelUpEffectManager.orig_OnCharacterLevelUp orig, CharacterBody characterBody)
        {
            if(_inventoryUI)
            {
                _inventoryUI.SetPlayerLevel((int)characterBody.level); 
            }
            orig(characterBody); 
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

        private void Start()
        {
            On.RoR2.UI.HUD.Awake    += HUD_OnAwake;
            On.RoR2.Run.BeginStage  += Run_BeginStage;
            On.RoR2.Run.EndStage    += Run_EndStage;

            On.RoR2.UI.PauseScreenController.OnPauseStart   += PauseScreenController_OnPauseStart; 
            On.RoR2.UI.PauseScreenController.OnPauseEnd     += PauseScreenController_OnPauseEnd; 

            On.RoR2.GameOverController.GenerateReportScreen     += GameOverController_GenerateReportScreen;

            On.RoR2.GenericPickupController.AttemptGrant        += GenericPickupController_AttemptGrant;
            On.RoR2.GenericPickupController.OnTriggerStay       += GenericPickupController_OnTriggerStay;
            // On.RoR2.GenericPickupController.OnInteractionBegin += GenericPickupController_OnInteractionBegin;

            On.RoR2.Inventory.RemoveItemChanneled               += Inventory_RemoveItemChanneled; 
            On.RoR2.Inventory.RemoveItemPermanent_ItemDef_int   += Inventory_RemoveItemPermanent_ItemDef_int;
            On.RoR2.Inventory.RemoveItemPermanent_ItemIndex_int += Inventory_RemoveItemPermanent_ItemIndex_int; 
            On.RoR2.Inventory.RemoveItem_ItemDef_int            += Inventory_RemoveItem_ItemDef_int;  
            On.RoR2.Inventory.RemoveItem_ItemIndex_int          += Inventory_RemoveItem_ItemIndex_int; 
            On.RoR2.Inventory.GiveItemPermanent_ItemDef_int     += Inventory_GiveItemPermanent_ItemDef_int; 
            On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int   += Inventory_GiveItemPermanent_ItemIndex_int; 

            On.RoR2.LevelUpEffectManager.OnCharacterLevelUp     += LevelUpEffectManager_OnCharacterLevelUp; 
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
            On.RoR2.GenericPickupController.OnTriggerStay -= GenericPickupController_OnTriggerStay;
            // On.RoR2.GenericPickupController.OnInteractionBegin -= GenericPickupController_OnInteractionBegin;

            On.RoR2.Inventory.RemoveItemChanneled -= Inventory_RemoveItemChanneled; 
            On.RoR2.Inventory.RemoveItemPermanent_ItemDef_int -= Inventory_RemoveItemPermanent_ItemDef_int;
            On.RoR2.Inventory.RemoveItemPermanent_ItemIndex_int -= Inventory_RemoveItemPermanent_ItemIndex_int;
            On.RoR2.Inventory.RemoveItem_ItemDef_int -= Inventory_RemoveItem_ItemDef_int; 
            On.RoR2.Inventory.RemoveItem_ItemIndex_int -= Inventory_RemoveItem_ItemIndex_int; 
            On.RoR2.Inventory.GiveItemPermanent_ItemDef_int -= Inventory_GiveItemPermanent_ItemDef_int; 
            On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int -= Inventory_GiveItemPermanent_ItemIndex_int; 

            On.RoR2.LevelUpEffectManager.OnCharacterLevelUp -= LevelUpEffectManager_OnCharacterLevelUp; 
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