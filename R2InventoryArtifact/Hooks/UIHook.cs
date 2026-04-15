



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

        private void UI_HUD_OnAwake(On.RoR2.UI.HUD.orig_Awake orig, HUD self)
        {
            orig(self);

            if (!InventoryArtifactProvider.IsEnabled())
            {
                Log.Info("Inventory Artifact not enabled...");
                return;
            }

            IntRect rect = new IntRect(
                PluginConfig.InventoryWidth.IsNullOrDestroyed() ? 8 : PluginConfig.InventoryWidth.Value,
                PluginConfig.InventoryHeight.IsNullOrDestroyed() ? 10 : PluginConfig.InventoryHeight.Value
            );

            if(!_isInRun && InventoryArtifactProvider.IsEnabled())
            {
                _isInRun = true; 
                InventoryUI = ComponentBuilder.BuildInventoryUI(null); //MAYBE: use null or embed into base game ui
                InventoryUI.Initialize(rect, _locks);
                InventoryUI.ResetInventory(); 
                InventoryUI.SetUIVisibility(show: false);
                InventoryUI.OnUIVisibilityChanged += HandleCursorVisibility;
                InventoryUI.OnInventoryItemDropped += OnInventoryItemDropped; 

                PlayerBody = PlayerCharacterMasterController.instances[0].body; 
                OnInitializeUI.Invoke(); 
            }
        }

        private void HandleCursorVisibility(bool show)
        {
            var pes = MPEventSystemManager.primaryEventSystem;
            // Log.Info($"Selected Obj: {pes.currentSelectedGameObject}");
            pes.cursorOpenerCount = show ? 1 : 0;
            pes.SetSelectedGameObject(null); //make sure ui components are deselected
        }

        private void Run_BeginStage(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            // if(_inventoryUI) _inventoryUI.SetUIVisibility(true);
            orig(self);
        }

        private void Run_EndStage(On.RoR2.Run.orig_EndStage orig, Run self)
        {
            Log.Info(""); 
            if(InventoryUI) InventoryUI.SetUIVisibility(false);
            orig(self);
        }

        private void UI_PauseScreenController_OnPauseEnd(On.RoR2.UI.PauseScreenController.orig_OnPauseEnd orig, PauseScreenController self)
        {
            Log.Info(""); 
            orig(self);
        }

        private void UI_PauseScreenController_OnPauseStart(On.RoR2.UI.PauseScreenController.orig_OnPauseStart orig, PauseScreenController self)
        {
            Log.Info(""); 
            orig(self);
        }

        private void GameOverController_RpcClientGameOver(On.RoR2.GameOverController.orig_RpcClientGameOver orig, GameOverController self)
        {
            Log.Info(""); 
            _isInRun = false; 
            Destroy(InventoryUI.gameObject); 
            orig(self); 
        }
       
        private void Start()
        {
            On.RoR2.Run.BeginStage  += Run_BeginStage;
            On.RoR2.Run.EndStage    += Run_EndStage;

            On.RoR2.UI.HUD.Awake                            += UI_HUD_OnAwake;
            On.RoR2.UI.PauseScreenController.OnPauseStart   += UI_PauseScreenController_OnPauseStart; 
            On.RoR2.UI.PauseScreenController.OnPauseEnd     += UI_PauseScreenController_OnPauseEnd; 

            On.RoR2.GameOverController.RpcClientGameOver    += GameOverController_RpcClientGameOver;
        }

        private void OnDestroy()
        {
            On.RoR2.Run.BeginStage  -= Run_BeginStage;
            On.RoR2.Run.EndStage    -= Run_EndStage;

            On.RoR2.UI.HUD.Awake                            -= UI_HUD_OnAwake;
            On.RoR2.UI.PauseScreenController.OnPauseStart   -= UI_PauseScreenController_OnPauseStart; 
            On.RoR2.UI.PauseScreenController.OnPauseEnd     -= UI_PauseScreenController_OnPauseEnd; 

            On.RoR2.GameOverController.RpcClientGameOver  -= GameOverController_RpcClientGameOver;
        }

        void Update()
        {
            if(IsArtifactEnabled)
            {
                switch(PluginConfig.InventoryShowType.Value)
                {
                    case InventoryShowType.ToggleShow: 
                        if(PluginConfig.ShowInventoryKey.Value.IsUp() /* && !_preventInventoryMenuShow */) InventoryUI.SetUIVisibility(!InventoryUI.IsVisible); 
                    break;
                    case InventoryShowType.HoldToShow: 
                        if(PluginConfig.ShowInventoryKey.Value.IsDown() /* && !_preventInventoryMenuShow */) InventoryUI.SetUIVisibility(true); 
                        else if (PluginConfig.ShowInventoryKey.Value.IsUp()) InventoryUI.SetUIVisibility(false); 
                    break; 
                }

                if(PluginConfig.RotateInventoryItemKey.Value.IsUp()) InventoryUI.RotateCursorItem(); 
            }
        }
    }
}