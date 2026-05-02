using System;
using System.Collections.Generic;
using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace R2InventoryArtifact.UI.Services
{
    public static class UIAssetService
    {
        public enum SpritePanelType
        {
            NONE,
            HEADER,
            PANEL,
            HANDLE,
            BACKDROP,
            DROP_ZONE,
            ELEMENT,
        }

        public enum SpriteTileType
        {
            TILEx____ = 0,
            TILEx___R = 1,
            TILEx__L_ = 2,
            TILEx__LR = 3,
            TILEx_B__ = 4,
            TILEx_B_R = 5,
            TILEx_BL_ = 6,
            TILEx_BLR = 7,
            TILExT___ = 8,
            TILExT__R = 9,
            TILExT_L_ = 10,
            TILExT_LR = 11,
            TILExTB__ = 12,
            TILExTB_R = 13,
            TILExTBL_ = 14,
            TILExTBLR = 15,
            TILE = 15,
            DISABLED_TILE
        }

        private static Dictionary<SpritePanelType, Sprite> _spritePanelDict;
        private static Dictionary<SpriteTileType, Sprite> _spriteBaseTileDict;
        private static Dictionary<SpriteTileType, Sprite> _spriteLockedTileDict;

        private static AssetBundle _bundle;
        private const string _bundleName = "assetbundle";

        public static void Initialize(PluginInfo pluginInfo)
        {
            _spritePanelDict = new Dictionary<SpritePanelType, Sprite>
            {
                { SpritePanelType.HEADER, Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHighlightHeader.png").WaitForCompletion() },
                { SpritePanelType.PANEL, Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUICleanPanel.png").WaitForCompletion() },
                { SpritePanelType.HANDLE, Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHandle.png").WaitForCompletion() },
                { SpritePanelType.BACKDROP, Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIBackdrop.png").WaitForCompletion() },
                { SpritePanelType.ELEMENT, Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texDetailPanel.png").WaitForCompletion() },
            };

            string basePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pluginInfo.Location), "Assets", _bundleName);
            try
            {
                _bundle = AssetBundle.LoadFromFile(basePath);
            }
            catch (Exception e)
            {
                Log.Debug($"Failed to load asset from file: {e}");
            }

            _spriteBaseTileDict = new Dictionary<SpriteTileType, Sprite>();
            _spriteLockedTileDict = new Dictionary<SpriteTileType, Sprite>(); 
            if (_bundle != null)
            {
                Sprite[] sprites = _bundle.LoadAllAssets<Sprite>();
                foreach (Sprite s in sprites)
                {
                    if (s.name.Contains("grid_tiles-"))
                    {
                        string tileName = s.name.Replace("grid_tiles-", "");
                        int targetType = 0;
                        if (tileName.Contains('T')) targetType |= 1 << 3;
                        if (tileName.Contains('B')) targetType |= 1 << 2;
                        if (tileName.Contains('L')) targetType |= 1 << 1;
                        if (tileName.Contains('R')) targetType |= 1;

                        _spriteBaseTileDict.Add((SpriteTileType)targetType, s);
                    }
                    else if (s.name.Contains("lock_tiles-"))
                    {
                        string tileName = s.name.Replace("lock_tiles-", "");
                        int targetType = 0;
                        if (tileName.Contains('T')) targetType |= 1 << 3;
                        if (tileName.Contains('B')) targetType |= 1 << 2;
                        if (tileName.Contains('L')) targetType |= 1 << 1;
                        if (tileName.Contains('R')) targetType |= 1;

                        _spriteLockedTileDict.Add((SpriteTileType)targetType, s);
                    }
                    else if (s.name.Contains("drop_zone"))
                    {
                        _spritePanelDict.Add(SpritePanelType.DROP_ZONE, s);
                    }
                }
            }
        }

        public static Sprite GetPickupSprite(UniquePickup pickup)
        {
            return pickup.pickupIndex.pickupDef.iconSprite;
        }

        public static Sprite GetUISprite(SpritePanelType type)
        {
            return _spritePanelDict.GetValueOrDefault(type, null);
        }

        public static Sprite GetTileSprite(bool isLocked, bool t, bool b, bool l, bool r)
        {
            int
                T = t ? 1 : 0,
                B = b ? 1 : 0,
                L = l ? 1 : 0,
                R = r ? 1 : 0
            ;
            SpriteTileType target = (SpriteTileType)((T << 3) | (B << 2) | (L << 1) | R);

            if(isLocked) return _spriteLockedTileDict[target]; 
            return _spriteBaseTileDict[target];
        }

        public static Sprite GetTileSprite(SpriteTileType tileType)
        {
            return _spriteBaseTileDict[tileType];
        }
    }
}