
using System;
using IL.RoR2.UI;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace R2InventoryArtifact
{ 
    public class DebugManager : MonoBehaviour
    {
        public static DebugManager Instance; 
        public static Run currentRun;
        public static CharacterMaster PlayerMaster; 
        public bool SetDecayValue = false; 
        public static CharacterBody PlayerBody => PlayerMaster.GetBody();

        private void Awake()
        {
            if(Instance) Destroy(Instance);
            Instance = this;  

            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => {};
            Run.onRunStartGlobal += HandleRunStart;  
        }

        private void HandleRunStart(Run run)
        {
            currentRun = run; 
            PlayerMaster = PlayerCharacterMasterController.instances[0].master; 
            On.RoR2.Inventory.SetItemDecayDurationServer += Inventory_SetItemDecayDurationServer;
        }

        private void Inventory_SetItemDecayDurationServer(On.RoR2.Inventory.orig_SetItemDecayDurationServer orig, Inventory self, float duration)
        {
            if(!SetDecayValue)
            {
                self.SetItemDecayDurationServer(2);
                SetDecayValue = true; 
            } 
            // orig(self, duration); 
        }



        // DEBUG: test item setting
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1)) PlayerBody?.inventory.GiveItemPermanent(RoR2Content.Items.BleedOnHit); 
            if(Input.GetKeyDown(KeyCode.Alpha2)) PlayerBody?.inventory.GiveItemPermanent(DLC1Content.Items.BleedOnHitVoid); 
            if(Input.GetKeyDown(KeyCode.Alpha3)) PlayerBody?.inventory.GiveItemTemp(DLC1Content.Items.HealingPotion.itemIndex); 
            if(Input.GetKeyDown(KeyCode.Alpha4)) PlayerBody?.inventory.GiveItemPermanent(DLC1Content.Items.RegeneratingScrap); 
            if(Input.GetKeyDown(KeyCode.Alpha5)) PlayerBody?.inventory.GiveRandomItems(1, ItemTier.Tier1, ItemTier.Tier2); 
            if(Input.GetKeyDown(KeyCode.Alpha6)) PlayerBody?.inventory.GiveRandomEquipment();  
            if(Input.GetKeyDown(KeyCode.Alpha7)) PlayerMaster?.GiveExperience(40);  
            
            if(Input.GetKeyDown(KeyCode.Keypad1)) PlayerBody?.inventory.RemoveItemPermanent(RoR2Content.Items.AlienHead.itemIndex); 
            if(Input.GetKeyDown(KeyCode.Keypad2)) PlayerBody?.inventory.RemoveItemTemp(RoR2Content.Items.AlienHead.itemIndex); 
            if(Input.GetKeyDown(KeyCode.Keypad2)) PlayerBody?.inventory.RemoveEquipment(RoR2Content.Equipment.BFG.equipmentIndex); 
        }
    }
}