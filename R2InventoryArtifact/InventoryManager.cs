
using R2InventoryArtifact.Model;
using R2InventoryArtifact.Util.Math;
using UnityEngine;
using System.Collections.Generic;
using R2InventoryArtifact.Util.R2API;
using R2InventoryArtifact.UI.Builders;
using R2InventoryArtifact.UI;

namespace R2InventoryArtifact
{
    public class InventoryManager : MonoBehaviour
    {
        public IntRect GridRect = new(5, 5);
        public List<InventoryLock> InventoryLocks = new List<InventoryLock>();
        private InventoryUI _inventoryUI;

        private void Awake()
        {
            InventoryModel.Initialize(GridRect, InventoryLocks);
            ComponentBuilder.Initialize();

            _inventoryUI = ComponentBuilder.BuildInventoryUI(null);
            _inventoryUI.Initialize(GridRect);
        }

        // DEBUG: test item setting
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                _inventoryUI.AddToInventory(R2ItemCode.BisonSteak);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                _inventoryUI.AddToInventory(R2ItemCode.BundleOfFireworks);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                _inventoryUI.AddToInventory(R2ItemCode.IgnitionTank);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                _inventoryUI.AddToInventory(R2ItemCode.RunaldsBand);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha5))
            {
                _inventoryUI.AddToInventory(R2ItemCode.SingularityBand);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha6))
            {
                _inventoryUI.AddToInventory(R2ItemCode.EmptyBottle);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha7))
            {
                _inventoryUI.IncreasePlayerLevel();
            }
        }
    }
}
