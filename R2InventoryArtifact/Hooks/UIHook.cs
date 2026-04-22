



using System;
using System.Collections.Generic;
using R2InventoryArtifact.Model;
using R2InventoryArtifact.UI;
using R2InventoryArtifact.UI.Builders;
using R2InventoryArtifact.Util;
using Rewired.Utils;
using RoR2;
using RoR2.UI;
using UnityEngine;
using R2InventoryArtifact.Artifact;
using UnityEngine.Networking;
using System.Linq;

namespace R2InventoryArtifact.Hooks
{
    /// <summary>
    /// Hooks for all lifecycle methods (awake, destroy, etc...)
    /// </summary>
    public class UIHook : MonoBehaviour
    {
        private bool IsArtifactEnabled
        {
            get => InventoryArtifactProvider.IsEnabled() && InventoryUI != null;
        }

        private List<InventoryLock> _locks = new() //testing only
        {
            new(){UnlockLevel=10, Root=new(0, 0), Nodes=new(){new(0, 0), new(0, 1), new(0, 2)}},
            new(){UnlockLevel=20, Root=new(1, 0), Nodes=new(){new(1, 0), new(1, 1), new(1, 2)}}
        };

        private bool _isInRun = false;

        public static Action<UniquePickup, int> OnInventoryItemDropped;
        public static Action OnInitializeUI;

        public static InventoryUI InventoryUI;
        public static CharacterBody PlayerBody;


        private void HandleRunStart(Run run)
        {
            _isInRun = true; 
            // if(NetworkServer.active)
            // {
            //     UIHook.PlayerBody = NetworkUser.localPlayers[0].GetCurrentBody();  
            // }
            // else
            // {    
            //     UIHook.PlayerBody = PlayerCharacterMasterController.instances[0].master.GetBody();
            // }

            IntRect rect = new IntRect(
                PluginConfig.InventoryWidth.IsNullOrDestroyed() ? 8 : PluginConfig.InventoryWidth.Value,
                PluginConfig.InventoryHeight.IsNullOrDestroyed() ? 10 : PluginConfig.InventoryHeight.Value
            );

            InventoryUI = ComponentBuilder.BuildInventoryUI(null); //MAYBE: use null or embed into base game ui
            InventoryUI.Initialize(rect, _locks);
            InventoryUI.SetUIVisibility(show: false);
            InventoryUI.OnUIVisibilityChanged += HandleCursorVisibility;
            InventoryUI.OnInventoryItemDropped += OnInventoryItemDropped;
            OnInitializeUI.Invoke();
        }

        private void HandleRunEnd(Run run)
        {
            // InventoryUI.ResetInventory(); 
            Destroy(InventoryUI.gameObject);
        }

        private void PlayerCharacterMasterController_OnBodyStart(On.RoR2.PlayerCharacterMasterController.orig_OnBodyStart orig, PlayerCharacterMasterController self)
        {
            orig(self); 
            // TODO: make sure to get main player during network game
            UIHook.PlayerBody = self.master.GetBody(); 
            // if(NetworkServer.active)
            // {
            //     NetworkUser.instancesList
            // }
        }

        private void HandleCursorVisibility(bool show)
        {
            var pes = MPEventSystemManager.primaryEventSystem;
            // Log.Info($"Selected Obj: {pes.currentSelectedGameObject}");
            pes.cursorOpenerCount = show ? 1 : 0;
            pes.SetSelectedGameObject(null); //make sure ui components are deselected
        }

        private void Awake()
        {
            Run.onRunStartGlobal += (run) =>
            {
                if(!InventoryArtifactProvider.IsEnabled()) return; 
                _isInRun = false; 
                HandleRunStart(run); 
                On.RoR2.PlayerCharacterMasterController.OnBodyStart += PlayerCharacterMasterController_OnBodyStart;  
            }; 

            Run.onRunDestroyGlobal += (run) =>
            {
                if(!InventoryArtifactProvider.IsEnabled()) return; 
                _isInRun = false; 
                On.RoR2.PlayerCharacterMasterController.OnBodyStart -= PlayerCharacterMasterController_OnBodyStart;  
                HandleRunEnd(run); 
            }; 
        }



        // Handle Input
        void Update()
        {
            if (IsArtifactEnabled)
            {
                switch (PluginConfig.InventoryShowType.Value)
                {
                    case InventoryShowType.ToggleShow:
                        if (PluginConfig.ShowInventoryKey.Value.IsUp() /* && !_preventInventoryMenuShow */) InventoryUI.SetUIVisibility(!InventoryUI.IsVisible);
                        break;
                    case InventoryShowType.HoldToShow:
                        if (PluginConfig.ShowInventoryKey.Value.IsDown() /* && !_preventInventoryMenuShow */) InventoryUI.SetUIVisibility(true);
                        else if (PluginConfig.ShowInventoryKey.Value.IsUp()) InventoryUI.SetUIVisibility(false);
                        break;
                }

                if (PluginConfig.RotateInventoryItemKey.Value.IsUp()) InventoryUI.RotateCursorItem();
            }
        }
    }
}