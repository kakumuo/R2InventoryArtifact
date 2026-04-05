


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using R2InventoryArtifact.Model;
using R2InventoryArtifact.UI.Builders;
using TMPro;
using UnityEngine;

namespace R2InventoryArtifact.UI.Components
{
    public class InventoryEffectComponent : MonoBehaviour
    {
        private TextMeshProUGUI textLbl;
        private Dictionary<InventoryEffectCode, InventoryEffectElement> _effectDict;

        public void Initialize()
        {
            InventoryModel.OnEffectAdd += AddEffect;
            InventoryModel.OnEffectRemove += RemoveEffect;
        }

        void Awake()
        {
            textLbl = GetComponentInChildren<TextMeshProUGUI>();
            _effectDict = new();
        }

        void OnDestroy()
        {
            InventoryModel.OnEffectAdd -= AddEffect;
            InventoryModel.OnEffectRemove -= RemoveEffect;
        }

        private void AddEffect(InventoryEffectCode code, int stackCount)
        {
            if (_effectDict.ContainsKey(code))
                _effectDict[code].UpdateContent(code, stackCount);
            else
            {
                InventoryEffectElement element = ComponentBuilder.BuildEffectElement(code.ToString());
                _effectDict[code] = element;
                element.UpdateContent(code, stackCount);
                element.transform.SetParent(transform);
            }
        }

        private void RemoveEffect(InventoryEffectCode code, int stackCount)
        {
            if (stackCount > 0)
                _effectDict[code].UpdateContent(code, stackCount);
            else
            {
                Destroy(_effectDict[code].gameObject);
                _effectDict.Remove(code);
            }
        }
    }
}