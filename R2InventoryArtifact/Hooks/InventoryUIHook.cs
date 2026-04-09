



using System;
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
    public class InventoryUIHook : MonoBehaviour
    {
        private InventoryUI _inventoryUI;
        private RoR2.CharacterBody _playerBody; 

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
                InventoryModel.Initialize(rect, new());
                _inventoryUI = ComponentBuilder.BuildInventoryUI(null);
                _inventoryUI.Initialize(rect);
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
            Vector3 launchDir = (playerBody.transform.forward + playerBody.transform.up) * 20f; //MAYBE: launch in direction of the camera
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
        }

        private void GenericPickupController_AttemptGrant(On.RoR2.GenericPickupController.orig_AttemptGrant orig, RoR2.GenericPickupController self, RoR2.CharacterBody body)
        {
            if (!InventoryArtifactProvider.IsEnabled() || !body.isPlayerControlled)
            {
                orig(self, body);
                return;
            }

            PickupDef pickupDef = PickupCatalog.GetPickupDef(self.pickup.pickupIndex);

            if (pickupDef.itemIndex == ItemIndex.None && pickupDef.equipmentIndex == EquipmentIndex.None) // only allow items and equiptment in inventory
                orig(self, body);

            Debug.Log($"GenericPickupController_AttemptGrant: {body.name} {self.pickup.pickupIndex}");
            if (_inventoryUI.AddToInventory(pickupDef.itemIndex))
                orig(self, body);
        }

        private void HandleMouseVisibility(bool show)
        {
            var pes = MPEventSystemManager.primaryEventSystem;
            pes.cursorOpenerCount = show ? 1 : 0;
            // pes.SetCursorIndicatorEnabled(show); 
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
            On.RoR2.GenericPickupController.AttemptGrant += GenericPickupController_AttemptGrant;
            On.RoR2.Run.BeginStage += Run_BeginStage;
            On.RoR2.Run.EndStage += Run_EndStage;
            On.RoR2.GameOverController.GenerateReportScreen += GameOverController_GenerateReportScreen;
        }

        private void Oestroy()
        {
            On.RoR2.UI.HUD.Awake -= HUD_OnAwake;
            On.RoR2.UI.PauseScreenController.OnPauseStart -= PauseScreenController_OnPauseStart; 
            On.RoR2.UI.PauseScreenController.OnPauseEnd -= PauseScreenController_OnPauseEnd; 
            On.RoR2.GenericPickupController.AttemptGrant -= GenericPickupController_AttemptGrant;
            On.RoR2.Run.BeginStage -= Run_BeginStage;
            On.RoR2.Run.EndStage -= Run_EndStage;
            On.RoR2.GameOverController.GenerateReportScreen -= GameOverController_GenerateReportScreen;
        }

    }
}