




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

                Log.Info($"Player pressed F2. Spawning our custom item at coordinates {transform.position}");
                var dropList = Run.instance.availableTier1DropList;
                PickupDropletController.CreatePickupDroplet(dropList[(int)R2ItemCode.Bandolier], transform.position, transform.forward * 20f);
            }
        }
    }
}