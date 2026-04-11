




using R2InventoryArtifact.Util.R2API;
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

        // DEBUG: test item setting
        private void Update()
        {
            if(Input.GetKeyUp(KeyCode.Alpha1))
            { 
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                // And then drop our defined item in front of the player.

                Log.Info($"Player pressed 1. Spawning our custom item at coordinates {transform.position}");
                var dropList = Run.instance.availableTier1DropList;
                var unique = new UniquePickup(dropList[(int)R2ItemCode.Bandolier]); 
                PickupDropletController.CreatePickupDroplet(unique, transform.position, transform.forward * 20f, false, false);
            }

            if(Input.GetKeyUp(KeyCode.Alpha2))
            {
                Log.Info($"Player pressed 2. Adding temp item");
                    
                var dropList = Run.instance.availableTier1DropList;
                var unique = new UniquePickup(dropList[(int)R2ItemCode.Bandolier]); 
                PickupDropletController.CreatePickupDroplet(unique, transform.position, transform.forward * 20f, true, false);
            }

            // if(Input.GetKeyUp(KeyCode.Alpha2))
            // {
            //     Log.Info($"Player pressed 2. Removing item from player");
            //     CharacterBody body;

            //     if(body = LocalUserManager.GetFirstLocalUser().cachedBody)
            //     {
            //         var dropList = Run.instance.availableTier1DropList;
            //         var unique = new UniquePickup(dropList[(int)R2ItemCode.Bandolier]); 
            //         body.inventory.RemoveItemPermanent(dropList[(int)R2ItemCode.Bandolier].pickupDef.itemIndex, 1); 
            //     }
            // }


        }
    }
}