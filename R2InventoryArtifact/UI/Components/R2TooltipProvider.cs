using UnityEngine;
using UnityEngine.EventSystems;
using R2InventoryArtifact.UI.Builders;

namespace R2InventoryArtifact.UI.Components
{
    public class R2TooltipProvider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        private static R2Tooltip _tooltipInstance;
        private R2TooltipContext _context;

        void Awake()
        {
            if (_tooltipInstance == null)
            {
                _tooltipInstance = ComponentBuilder.BuildR2TooltipComponent("Tooltip");
                _tooltipInstance.transform.SetParent(InventoryUI.Instance.transform);
            }
        }

        public void Initialize(R2TooltipContext context)
        {
            _context = context;
        }

        public void SetTooltipVisiblity(bool show)
        {
            if (show)
            {
                _tooltipInstance.UpdateContext(_context);
                _tooltipInstance.Show();
            }
            else _tooltipInstance.Hide();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _tooltipInstance.gameObject.transform.position = eventData.position;
            SetTooltipVisiblity(true);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            _tooltipInstance.gameObject.transform.position = eventData.position;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _tooltipInstance.gameObject.transform.position = eventData.position;
            SetTooltipVisiblity(false);
        }

        public void ForceShow()
        {
            _tooltipInstance.Show();
        }

        public void ForceHide()
        {
            _tooltipInstance.Hide();
        }
    }

    public struct R2TooltipContext
    {
        public string Title;
        public string Body;
        public Color HeaderBkg;
    }
}