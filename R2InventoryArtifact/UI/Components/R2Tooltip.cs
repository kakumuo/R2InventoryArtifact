



using TMPro;
using UnityEngine;

namespace R2InventoryArtifact.UI.Components
{
    public class R2Tooltip : MonoBehaviour
    {
        private TextMeshProUGUI _headerText; 
        private TextMeshProUGUI _bodyText;
        private CanvasGroup _group; 

        public void Initialize(TextMeshProUGUI headerText, TextMeshProUGUI bodyText, CanvasGroup group)
        {
            _headerText = headerText; 
            _bodyText = bodyText; 
            _group = group; 
            _group.blocksRaycasts = false; 
        } 

        public void UpdateContext(R2TooltipContext context)
        {
            _headerText.text = context.Title; 
            _bodyText.text = context.Body; 
        }

        public void Show()
        {
            _group.alpha = 1;
        }

        public void Hide()
        {
            _group.alpha = 0; 
        }
    }
}