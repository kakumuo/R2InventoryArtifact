
using R2InventoryArtifact.Model;
using TMPro;
using UnityEngine;

namespace R2InventoryArtifact.UI.Components
{
    public class InventoryEffectElement : MonoBehaviour
    {
        TextMeshProUGUI text;

        void Awake()
        {
            text = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void UpdateContent(InventoryEffectCode code, int stackCount)
        {
            text.text = $"{code} ({stackCount})";
        }

    }
}