

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