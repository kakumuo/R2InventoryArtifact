


using UnityEngine;
using RoR2;
using System;
using System.Collections.Generic;
using R2InventoryArtifact.Artifact;
using System.Linq;

namespace R2InventoryArtifact.Hooks
{
    struct ItemAddResult
    {
        public ItemIndex ItemIndex;
        public int Delta;
    }

    public class InventoryDeltaHook : MonoBehaviour
    {

        // TODO: should try to use ItemColleciton, but when item stack decays, ItemCollection.GetNonZeroIndicesSpan() returns an empty list
        Dictionary<ItemIndex, int> permaItemSet;
        Dictionary<ItemIndex, int> tempItemSet;

        // int itemAcqIndex;
        // private int _equipCount;
        // private ItemIndex extraEquipItemIndex;
        // byte[] equipSet;

        private CharacterMaster CharacterMaster;
        private CharacterBody PlayerBody => CharacterMaster?.GetBody();
        bool _isInRun = false;
        private List<ItemIndex> _FORCE_ITEMS_TO_NONEQUIP = new();

        private void Initialize()
        {
            permaItemSet = new Dictionary<ItemIndex, int>();
            tempItemSet = new Dictionary<ItemIndex, int>();
            // itemAcqIndex = 0;

            // _equipCount = 1;
            // equipSet = new byte[1];  //TODO: get actual size of byteset
            // extraEquipItemIndex = DLC3Content.Items.ExtraEquipment.itemIndex;


            _FORCE_ITEMS_TO_NONEQUIP = new()
            {
                DLC2Content.Items.LowerPricedChests.itemIndex,
                DLC1Content.Items.RegeneratingScrap.itemIndex,

                // DLC2Content.Items.LowerPricedChestsConsumed.itemIndex,
                // DLC1Content.Items.RegeneratingScrapConsumed.itemIndex,
                // RoR2Content.Items.ExtraLifeConsumed.itemIndex,
                // DLC1Content.Items.ExtraLifeVoidConsumed.itemIndex,
                // DLC1Content.Items.HealingPotionConsumed.itemIndex,
                // DLC1Content.Items.FragileDamageBonusConsumed.itemIndex,
                // DLC2Content.Items.TeleportOnLowHealthConsumed.itemIndex,
            };
        }

        private void HandleItemAdd(UniquePickup pickup, bool toNonEquip, int count)
        {
            // TODO: edit to add multiple within InventoryModel
            for (int i = 0; i < count; i++)
            {
                Model.InventoryResultCode resultCode = UIHook.InventoryUI.AddToInventory(pickup, toNonEquip);
                if (UIHook.InventoryUI && (resultCode == Model.InventoryResultCode.HOLD_INSERT || resultCode == Model.InventoryResultCode.HOLD_UPDATE))
                {
                    UIHook.InventoryUI.SetUIVisibility(true);
                }
            }
        }

        private void HandleItemRemove(UniquePickup pickup, bool fromNonEquip, int count)
        {
            UIHook.InventoryUI.RemoveFromInventory(pickup, fromNonEquip, count);
        }

        private void HandleItemDrop(UniquePickup pickup, int count)
        {
            // Log.Debug($"Dropping item...{pickup.pickupIndex}, {count}");
            if (PlayerBody == null) return;

            if (pickup.pickupIndex.pickupDef.itemIndex != ItemIndex.None)
            {
                if (pickup.isTempItem) PlayerBody.inventory.RemoveItemTemp(pickup.pickupIndex.pickupDef.itemIndex, count);
                else PlayerBody.inventory.RemoveItemPermanent(pickup.pickupIndex.pickupDef.itemIndex, count);
            }
            else if (pickup.pickupIndex.pickupDef.equipmentIndex != EquipmentIndex.None)
            {
                for (int i = 0; i < count; i++)
                {
                    PlayerBody.inventory.RemoveEquipment(pickup.pickupIndex.pickupDef.equipmentIndex);
                }
            }

            for (int i = 0; i < count; i++)
            {
                Vector3 launchDir = (PlayerBody.transform.forward + PlayerBody.transform.up) * 10f; //MAYBE: launch in direction of the camera
                PickupDropletController.CreatePickupDroplet(pickup, PlayerBody.transform.position, launchDir, false);
            }
        }

        /*
            MAYBE:FIXME: prev.GetNonZeroIndicesSpan() returns nothing when a temp item decays, if able to fix, then use ItemCollection instead of Dictionary
        */
        // private List<ItemAddResult> FindInventoryDelta(ItemCollection prev, ItemCollection cur)
        // {
        //     List<ItemAddResult> res = new List<ItemAddResult>(); 
        //     foreach (ItemCollection col in new List<ItemCollection>() { prev, cur }) // check "prev in cur" and "cur in prev"
        //     {
        //         foreach (ItemIndex item in col.GetNonZeroIndicesSpan())
        //         {
        //             int prevStacks = prev.GetStackValue(item);
        //             int curStacks = cur.GetStackValue(item);
        //             int diff = curStacks - prevStacks;

        //             if (diff == 0) continue;
        //             prev.SetStackValue(item, curStacks);
        //             res.Add(new(){ItemIndex=item, Delta=diff}); 
        //         }
        //     }

        //     return res;
        // }

        private List<ItemAddResult> FindInventoryDelta(Dictionary<ItemIndex, int> prev, ItemCollection cur)
        {
            List<ItemAddResult> res = new List<ItemAddResult>();

            foreach (ItemIndex item in cur.GetNonZeroIndicesSpan())
            {
                int prevStacks = prev.GetValueOrDefault(item, 0);
                int curStacks = cur.GetStackValue(item);
                int diff = curStacks - prevStacks;

                if (diff == 0) continue;

                if (curStacks == 0 && prev.ContainsKey(item)) prev.Remove(item);
                else prev[item] = curStacks;
                res.Add(new() { ItemIndex = item, Delta = diff });
            }

            List<ItemIndex> keyList = prev.Keys.ToList();
            foreach (ItemIndex item in keyList)
            {
                int prevStacks = prev.GetValueOrDefault(item, 0);
                int curStacks = cur.GetStackValue(item);
                int diff = curStacks - prevStacks;

                if (diff == 0) continue;

                if (curStacks == 0 && prev.ContainsKey(item)) prev.Remove(item);
                else prev[item] = curStacks;
                res.Add(new() { ItemIndex = item, Delta = diff });
            }

            // sort, keep void items last
            List<ItemTier> voidItemTiers = [ItemTier.VoidTier1, ItemTier.VoidBoss, ItemTier.VoidTier2, ItemTier.VoidTier3]; 
            res.Sort((a, b) =>
            {
                int aIsVoid = voidItemTiers.Contains(ItemCatalog.GetItemDef(a.ItemIndex).tier) ? 1 : 0; 
                int bIsVoid = voidItemTiers.Contains(ItemCatalog.GetItemDef(a.ItemIndex).tier) ? 1 : 0; 
                return aIsVoid - bIsVoid; 
            }); 

            return res;
        }

        // TODO: find way to attach to player inventory hook
        private void Inventory_onInventoryChangedGlobal(Inventory inventory)
        {
            if (!(PlayerBody && PlayerBody.inventory == inventory)) return;

            List<ItemAddResult> resultPerma = FindInventoryDelta(permaItemSet, inventory.permanentItemStacks);
            List<ItemAddResult> resultTemp = FindInventoryDelta(tempItemSet, inventory.tempItemsStorage.tempItemStacks);

            foreach (ItemAddResult res in resultPerma)
            {
                ItemDef def = ItemCatalog.GetItemDef(res.ItemIndex); 
                bool toNonEquip = _FORCE_ITEMS_TO_NONEQUIP.Contains(res.ItemIndex) || (def != null && def.isConsumed);
                UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(res.ItemIndex));
                if (res.Delta > 0) HandleItemAdd(pickup, toNonEquip, res.Delta);
                else HandleItemRemove(pickup, toNonEquip, Math.Abs(res.Delta));
            }

            foreach (ItemAddResult res in resultTemp)
            {
                UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(res.ItemIndex));
                pickup.decayValue = Inventory.baseItemDecayDuration;
                if (res.Delta > 0) HandleItemAdd(pickup, true, res.Delta);
                else HandleItemRemove(pickup, true, Math.Abs(res.Delta));
            }
        }

        private void CharacterBody_OnEquipmentLost(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);

            if (PlayerBody != self) return;

            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(equipmentDef.equipmentIndex));
            HandleItemRemove(pickup, false, 1);
        }

        private void CharacterBody_OnEquipmentGained(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);
            if (PlayerBody != self) return;

            UniquePickup pickup = new UniquePickup(PickupCatalog.FindPickupIndex(equipmentDef.equipmentIndex));
            HandleItemAdd(pickup, false, 1);
        }

        private void Awake()
        {
            _isInRun = false;
            Run.onRunStartGlobal += (Run run) =>
            {
                if (!InventoryArtifactProvider.IsEnabled()) return;
                _isInRun = true;
                UIHook.OnInventoryItemDropped   += HandleItemDrop;
                UIHook.OnInitializeUI           += Initialize;

                CharacterMaster = PlayerCharacterMasterController.instances[0].master;

                On.RoR2.CharacterBody.OnEquipmentGained         += CharacterBody_OnEquipmentGained;
                On.RoR2.CharacterBody.OnEquipmentLost           += CharacterBody_OnEquipmentLost;
                RoR2.Inventory.onInventoryChangedGlobal         += Inventory_onInventoryChangedGlobal;  //TODO: try to use character body's inventory changed, instead of global
                
            };

            Run.onRunDestroyGlobal += (Run run) =>
            {
                if (!InventoryArtifactProvider.IsEnabled() || !_isInRun) return;

                _isInRun = false;
                UIHook.OnInventoryItemDropped   -= HandleItemDrop;
                UIHook.OnInitializeUI           -= Initialize;

                On.RoR2.CharacterBody.OnEquipmentGained         -= CharacterBody_OnEquipmentGained;
                On.RoR2.CharacterBody.OnEquipmentLost           -= CharacterBody_OnEquipmentLost;
                RoR2.Inventory.onInventoryChangedGlobal         -= Inventory_onInventoryChangedGlobal;
            };
        }
    }
}