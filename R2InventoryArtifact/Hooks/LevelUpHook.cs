

using RoR2;
using UnityEngine;
namespace R2InventoryArtifact.Hooks
{
    public class LevelUpHook : MonoBehaviour
    {

        private void LevelUpEffectManager_OnCharacterLevelUp(On.RoR2.LevelUpEffectManager.orig_OnCharacterLevelUp orig, CharacterBody characterBody)
        {
            if(UIHook.InventoryUI)
            {
                UIHook.InventoryUI.SetPlayerLevel((int)characterBody.level); 
            }
            orig(characterBody); 
        }

        private void Start()
        {
            On.RoR2.LevelUpEffectManager.OnCharacterLevelUp += LevelUpEffectManager_OnCharacterLevelUp; 
        }

        private void OnDestroy()
        {
            On.RoR2.LevelUpEffectManager.OnCharacterLevelUp -= LevelUpEffectManager_OnCharacterLevelUp; 
        }
    }
}