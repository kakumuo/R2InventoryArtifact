



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

        private void HUD_OnAwake(On.RoR2.UI.HUD.orig_Awake orig, HUD self)
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

            // _playerBody = LocalUserManager.GetFirstLocalUser().cachedBody; 

            //MAYBE: embed ui into the base game ui
            if(!_isInRun && InventoryArtifactProvider.IsEnabled())
            {
                _isInRun = true; 
                InventoryUI = ComponentBuilder.BuildInventoryUI(null);
                InventoryUI.Initialize(rect, _locks);
                InventoryUI.SetUIVisibility(show: false);
                InventoryUI.OnUIVisibilityChanged += HandleCursorVisibility;
                InventoryUI.OnInventoryItemDropped += OnInventoryItemDropped; 

                PlayerBody = PlayerCharacterMasterController.instances[0].body; 
                OnInitializeUI.Invoke(); 
            }
        }

        // private void GenericPickupController_OnInteractionBegin(On.RoR2.GenericPickupController.orig_OnInteractionBegin orig, GenericPickupController self, Interactor activator)
        // {
        //     if(!IsArtifactEnabled) orig(self, activator); 
        //     if(self.pickup.pickupIndex == PickupIndex.none) orig(self, activator); 

        //     PickupDef pickup = PickupCatalog.GetPickupDef(self.pickup.pickupIndex); 
        //     if(pickup.itemIndex != ItemIndex.None) _nextItemToPickUp = new(pickup.itemIndex);
        //     if(pickup.equipmentIndex != EquipmentIndex.None) _nextItemToPickUp = new(pickup.equipmentIndex);

        //     orig(self, activator); 
        // }


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
           if(InventoryUI) InventoryUI.SetUIVisibility(false);
            orig(self);
        }

        // private void PauseScreenController_OnPauseEnd(On.RoR2.UI.PauseScreenController.orig_OnPauseEnd orig, PauseScreenController self)
        // {
        //     _preventInventoryMenuShow = false; 
        //     // if(_inventoryUI) _inventoryUI.SetUIVisibility(false); 
        //     orig(self);
        // }

        // private void PauseScreenController_OnPauseStart(On.RoR2.UI.PauseScreenController.orig_OnPauseStart orig, PauseScreenController self)
        // {
        //     _preventInventoryMenuShow = true; 
        //     if(_inventoryUI) _inventoryUI.SetUIVisibility(false); 
        //     HandleMouseVisibility(true); 
        //     orig(self);
        // }

        private GameEndReportPanelController GameOverController_GenerateReportScreen(On.RoR2.GameOverController.orig_GenerateReportScreen orig, GameOverController self, HUD hud)
        {
            if(InventoryUI) InventoryUI.SetUIVisibility(false);
            return orig(self, hud);
        }
       
        private void Start()
        {
            // On.RoR2.UI.PauseScreenController.OnPauseStart   += PauseScreenController_OnPauseStart; 
            // On.RoR2.UI.PauseScreenController.OnPauseEnd     += PauseScreenController_OnPauseEnd; 
            On.RoR2.UI.HUD.Awake    += HUD_OnAwake;
            On.RoR2.Run.BeginStage  += Run_BeginStage;
            On.RoR2.Run.EndStage    += Run_EndStage;

            On.RoR2.GameOverController.GenerateReportScreen     += GameOverController_GenerateReportScreen;
        }

        private void OnDestroy()
        {

            // On.RoR2.UI.PauseScreenController.OnPauseStart   -= PauseScreenController_OnPauseStart; 
            // On.RoR2.UI.PauseScreenController.OnPauseEnd     -= PauseScreenController_OnPauseEnd; 
            On.RoR2.UI.HUD.Awake    -= HUD_OnAwake;
            On.RoR2.Run.BeginStage  -= Run_BeginStage;
            On.RoR2.Run.EndStage    -= Run_EndStage;

            On.RoR2.GameOverController.GenerateReportScreen -= GameOverController_GenerateReportScreen;
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