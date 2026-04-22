



using System;
using System.Collections.Generic;
using R2InventoryArtifact.Model;
using R2InventoryArtifact.UI;
using R2InventoryArtifact.UI.Builders;
using R2InventoryArtifact.Util;
using Rewired.Utils;
using RoR2;
using UnityEngine;
using R2InventoryArtifact.Artifact;
using System.Linq;
using RoR2BepInExPack.GameAssetPaths;

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

        private List<InventoryLock> _locks = new();  //testing only

        private bool _isInRun = false;
        private bool _isPaused = false;

        public static Action<UniquePickup, int> OnInventoryItemDropped;
        public static Action OnInitializeUI;

        public static InventoryUI InventoryUI;
        public static CharacterBody PlayerBody;



        private void HandleRunStart(Run run)
        {
            _isInRun = true;
            IntRect rect = new IntRect(PluginConfig.InventoryWidth.Value, PluginConfig.InventoryHeight.Value);

            _locks = GenerateGridLocks(rect, 3);

            InventoryUI = ComponentBuilder.BuildInventoryUI(null); //MAYBE: use null or embed into base game ui
            InventoryUI.Initialize(rect, _locks);
            InventoryUI.SetUIVisibility(show: false);
            InventoryUI.OnUIVisibilityChanged += HandleCursorVisibility;
            InventoryUI.OnInventoryItemDropped += OnInventoryItemDropped;
            OnInitializeUI.Invoke();
        }

        private List<InventoryLock> GenerateGridLocks(IntRect grid, int startRow, int minLevel=5, int maxLevel=30)
        {
            List<InventoryLock> locks = new List<InventoryLock>(); 
            int n = Math.Max(grid.Height - startRow, 0); 
            float levelDelta = (maxLevel - minLevel) / (float) Math.Max(n, 1); 
            for(int i = 0; i < n; i++)
            {
                locks.Add(new()
                {
                    UnlockLevel = (int) Math.Ceiling(minLevel + (i * levelDelta)), 
                    Nodes= Enumerable.Range(0, grid.Width).Select(col => new GridPosition(col, startRow + i)).ToList()
                }); 
            }

            return locks; 
        }

        private void HandleRunEnd(Run run)
        {
            InventoryUI.ResetInventory();
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

        private void PauseScreenController_OnEnable(On.RoR2.UI.PauseScreenController.orig_OnEnable orig, RoR2.UI.PauseScreenController self)
        {
            orig(self);
            _isPaused = true;
            if (InventoryUI && InventoryUI.IsVisible)
            {
                InventoryUI.SetUIVisibility(false);
            }
        }

        private void PauseScreenController_OnDisable(On.RoR2.UI.PauseScreenController.orig_OnDisable orig, RoR2.UI.PauseScreenController self)
        {
            orig(self);
            _isPaused = false;
        }

        private void HandleCursorVisibility(bool show)
        {
            var pes = MPEventSystemManager.primaryEventSystem;
            // Log.Info($"Selected Obj: {pes.currentSelectedGameObject}");
            pes.cursorOpenerCount = show || _isPaused ? 1 : 0;
            pes.SetSelectedGameObject(null); //make sure ui components are deselected   
        }

        private void Awake()
        {
            Run.onRunStartGlobal += (run) =>
            {
                if (!InventoryArtifactProvider.IsEnabled()) return;
                _isInRun = false;
                HandleRunStart(run);
                On.RoR2.PlayerCharacterMasterController.OnBodyStart += PlayerCharacterMasterController_OnBodyStart;
                On.RoR2.UI.PauseScreenController.OnDisable += PauseScreenController_OnDisable; 
                On.RoR2.UI.PauseScreenController.OnEnable += PauseScreenController_OnEnable; 
            };

            Run.onRunDestroyGlobal += (run) =>
            {
                if (!_isInRun) return;
                _isInRun = false;
                HandleRunEnd(run);
                On.RoR2.PlayerCharacterMasterController.OnBodyStart -= PlayerCharacterMasterController_OnBodyStart;
                On.RoR2.UI.PauseScreenController.OnDisable -= PauseScreenController_OnDisable; 
                On.RoR2.UI.PauseScreenController.OnEnable -= PauseScreenController_OnEnable; 
            };
        }

        // Handle Input
        void Update()
        {
            if (IsArtifactEnabled && !_isPaused)
            {
                switch (PluginConfig.InventoryShowType.Value)
                {
                    case InventoryShowType.ToggleShow:
                        if (PluginConfig.ShowInventoryKey.Value.IsUp()) InventoryUI.SetUIVisibility(!InventoryUI.IsVisible);
                        break;
                    case InventoryShowType.HoldToShow:
                        if (PluginConfig.ShowInventoryKey.Value.IsDown()) InventoryUI.SetUIVisibility(true);
                        else if (PluginConfig.ShowInventoryKey.Value.IsUp()) InventoryUI.SetUIVisibility(false);
                        break;
                }

                if (PluginConfig.RotateInventoryItemKey.Value.IsUp())
                {
                    InventoryUI.RotateCursorItem();
                }
            }
        }
    }
}