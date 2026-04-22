
using R2API;
using RoR2;
using UnityEngine;

namespace R2InventoryArtifact
{ 
    public class DebugManager : MonoBehaviour
    {
        public static DebugManager Instance; 
        public static Run currentRun; 

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
        }

        private void SpawnItem(ItemIndex itemindex, bool isTemp=false)
        {
            var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
            var unique = new UniquePickup(PickupCatalog.FindPickupIndex(itemindex)); 
            // Log.Info($"Spawning {(isTemp ? "temp" : "permanent")} {unique.pickupIndex}"); 
            if(transform) PickupDropletController.CreatePickupDroplet(unique, transform.position, transform.forward * 20f, false);
        }

        
        private void SpawnItem(EquipmentIndex itemindex, bool isTemp=false)
        {
            var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
            var unique = new UniquePickup(PickupCatalog.FindPickupIndex(itemindex)); 
            // Log.Info($"Spawning {(isTemp ? "temp" : "permanent")} {unique.pickupIndex}"); 
            if(transform) PickupDropletController.CreatePickupDroplet(unique, transform.position, transform.forward * 20f, false);
        }

        private void PrintItems()
        {
            Log.Info("Printing items..."); 
            foreach(var item in ItemCatalog.allItemDefs)
            {
                Log.Info($"\"{item.nameToken}\":\t{{IconPath: \"{Language.GetString(item.nameToken).Replace(" ", "_") + "webp"}\", BaseLabel:\"{item.name}\", Label:\"{Language.GetString(item.nameToken)}\"}}"); 
            }

            Log.Info("Printing Equipment..."); 
            foreach(var item in EquipmentCatalog.equipmentDefs)
            {
                Log.Info($"\"{item.nameToken}\":\t{{IconPath: \"{Language.GetString(item.nameToken).Replace(" ", "_") + "webp"}\", BaseLabel:\"{item.name}\", Label:\"{Language.GetString(item.nameToken)}\"}}"); 
            }
        }

        private void AdvanceStage()
        {
            if(!currentRun) return; 
            currentRun.AdvanceStage(RoR2.SceneCatalog.FindSceneDef("golemplains")); 
        }

        // DEBUG: test item setting
        private void Update()
        {
            if(Input.GetKeyUp(KeyCode.Alpha1)) SpawnItem(DLC2Content.Items.IncreaseDamageOnMultiKill.itemIndex);
            if(Input.GetKeyUp(KeyCode.Alpha2)) SpawnItem(DLC1Content.Equipment.Molotov.equipmentIndex);
            if(Input.GetKeyUp(KeyCode.Alpha3)) SpawnItem(RoR2Content.Items.Mushroom.itemIndex);
            if(Input.GetKeyUp(KeyCode.Alpha4)) SpawnItem(DLC1Content.Items.MushroomVoid.itemIndex);
            if(Input.GetKeyUp(KeyCode.Alpha5)) SpawnItem(DLC2Content.Items.LowerPricedChests.itemIndex);
            if(Input.GetKeyUp(KeyCode.Alpha6)) SpawnItem(DLC1Content.Items.HealingPotion.itemIndex);
            if(Input.GetKeyUp(KeyCode.Alpha7))
            {
                var body = PlayerCharacterMasterController.instances[0].master;
                if(body) body.GetBody().InflictLavaDamage(); 
            }
            if(Input.GetKeyUp(KeyCode.Alpha8)) AdvanceStage();
            // if(Input.GetKeyUp(KeyCode.Alpha8)) SpawnItem(DLC1Content.Equipment.GummyClone.equipmentIndex);
            // if(Input.GetKeyUp(KeyCode.Alpha6)) SpawnItem(DLC1Content.Items.HealingPotion.itemIndex);
            // if(Input.GetKeyUp(KeyCode.Alpha6)) SpawnItem(DLC1Content.Items.HealingPotion.itemIndex);
        }
    }
}