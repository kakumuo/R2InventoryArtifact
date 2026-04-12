
using RoR2;
using UnityEngine;

namespace R2InventoryArtifact
{ 
    public class DebugManager : MonoBehaviour
    {
        public static DebugManager Instance; 

        private void Awake()
        {
            if(Instance) Destroy(Instance);
            Instance = this;  
        }

        private void SpawnItem(ItemIndex itemindex, bool isTemp=false)
        {
            var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
            var unique = new UniquePickup(PickupCatalog.FindPickupIndex(itemindex)); 
            Log.Info($"Spawning {(isTemp ? "temp" : "permanent")} {unique.pickupIndex}"); 
            PickupDropletController.CreatePickupDroplet(unique, transform.position, transform.forward * 20f, false);
        }

        // DEBUG: test item setting
        private void Update()
        {
            if(Input.GetKeyUp(KeyCode.Alpha1)) SpawnItem(RoR2Content.Items.Crowbar.itemIndex);
            if(Input.GetKeyUp(KeyCode.Alpha2)) SpawnItem(RoR2Content.Items.Crowbar.itemIndex, true);
            if(Input.GetKeyUp(KeyCode.Alpha3)) SpawnItem(RoR2Content.Items.Mushroom.itemIndex);
            if(Input.GetKeyUp(KeyCode.Alpha4)) SpawnItem(DLC1Content.Items.MushroomVoid.itemIndex);
            if(Input.GetKeyUp(KeyCode.Alpha5)) SpawnItem(DLC2Content.Items.LowerPricedChests.itemIndex);
            if(Input.GetKeyUp(KeyCode.Alpha6)) SpawnItem(DLC1Content.Items.HealingPotion.itemIndex);
            if(Input.GetKeyUp(KeyCode.Alpha7))
            {
                var body = PlayerCharacterMasterController.instances[0].master;
                body.GetBody().InflictLavaDamage(); 
            }
            // if(Input.GetKeyUp(KeyCode.Alpha6)) SpawnItem(DLC1Content.Items.HealingPotion.itemIndex);
            // if(Input.GetKeyUp(KeyCode.Alpha6)) SpawnItem(DLC1Content.Items.HealingPotion.itemIndex);
            // if(Input.GetKeyUp(KeyCode.Alpha6)) SpawnItem(DLC1Content.Items.HealingPotion.itemIndex);
        }
    }
}