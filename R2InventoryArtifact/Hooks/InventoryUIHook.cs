



using System;
using R2InventoryArtifact.Model;
using R2InventoryArtifact.UI;
using R2InventoryArtifact.UI.Builders;
using R2InventoryArtifact.Util;
using Rewired.Utils;
using RoR2;
using UnityEngine;

namespace R2InventoryArtifact.Hooks
{
    public class InventoryUIHook : MonoBehaviour
    {
        private InventoryUI _inventoryUI; 

        private void HUD_OnAwake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self); 

            if(!InventoryArtifactProvider.IsEnabled())
            {
                Log.Debug("Inventory Artifact not enabled..."); 
                return; 
            }

            Log.Debug("Init InventoryUI..."); 

            IntRect rect = new IntRect(
                PluginConfig.InventoryWidth.IsNullOrDestroyed() ? 7 : PluginConfig.InventoryWidth.Value,
                PluginConfig.InventoryHeight.IsNullOrDestroyed() ? 10 : PluginConfig.InventoryHeight.Value
            ); 

            InventoryModel.Initialize(rect, new()); 
             //TODO: create pagination and embed ui into the base game ui
            _inventoryUI = ComponentBuilder.BuildInventoryUI(null);
            _inventoryUI.Initialize(rect); 
            _inventoryUI.HideUI(); 

            Log.Debug("InventoryUI Init Complete..."); 
        }

        private void GenericPickupController_AttemptGrant(On.RoR2.GenericPickupController.orig_AttemptGrant orig, RoR2.GenericPickupController self, RoR2.CharacterBody body)
        {
            if(!InventoryArtifactProvider.IsEnabled() || !body.isPlayerControlled)
            {
                orig(self, body); 
                return; 
            }

            PickupDef pickupDef = PickupCatalog.GetPickupDef(self.pickup.pickupIndex); 

            if(pickupDef.itemIndex == ItemIndex.None && pickupDef.equipmentIndex == EquipmentIndex.None) // only allow items and equiptment in inventory
                orig(self, body); 

            Debug.Log($"GenericPickupController_AttemptGrant: {body.name} {self.pickup.pickupIndex}");
            if(_inventoryUI.AddToInventory(pickupDef.itemIndex))
                orig(self, body); 
        }


        
        private void Start()
        {
            On.RoR2.UI.HUD.Awake += HUD_OnAwake; 
            On.RoR2.GenericPickupController.AttemptGrant += GenericPickupController_AttemptGrant; 
        }

        private void Oestroy()
        {
            On.RoR2.UI.HUD.Awake -= HUD_OnAwake; 
            On.RoR2.GenericPickupController.AttemptGrant -= GenericPickupController_AttemptGrant; 
        }
    }
}