
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace R2InventoryArtifact.UI.Builders
{
    public static partial class ComponentBuilder
    {
        /// <summary>
        /// Builds a scroll view, returns the content's gameobject reference
        /// </summary>
        /// <param name="objName"></param>
        /// <param name="horizontal"></param>
        /// <param name="vertical"></param>
        /// <returns></returns>
        public static GameObject BuildScrollView(Transform parent, string objName = "", bool horizontal = false, bool vertical = false)
        {
            RectTransform scrollViewRect = BuildPanel(parent, objName);
            GameObject obj = scrollViewRect.gameObject;
            ScrollRect scrollRect = scrollViewRect.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = horizontal;
            scrollRect.vertical = vertical;
            scrollRect.inertia = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 5; //TODO: change sensitivity based off ui size 

            RectTransform viewportRect = BuildPanel(obj.transform, "Viewport");
            SetRectTransformAnchor(viewportRect, horizontal: AnchorPreset.STRETCH, vertical: AnchorPreset.STRETCH);
            viewportRect.pivot = Vector2.zero; 
            viewportRect.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            scrollRect.viewport = viewportRect.GetComponent<RectTransform>();

            RectTransform contentRect = BuildPanel(viewportRect.transform, "Content", .25f);
            SetRectTransformAnchor(
                contentRect, 
                horizontal: horizontal && !vertical ? AnchorPreset.START : AnchorPreset.STRETCH, 
                vertical: vertical && !horizontal ? AnchorPreset.START : AnchorPreset.STRETCH
            );
            ContentSizeFitter fitter = contentRect.gameObject.AddComponent<ContentSizeFitter>(); 
            if(horizontal)  fitter.horizontalFit = ContentSizeFitter.FitMode.MinSize; 
            if(vertical)    fitter.verticalFit = ContentSizeFitter.FitMode.MinSize; 
            scrollRect.content = contentRect;

            if (horizontal)
            {
                scrollRect.horizontalScrollbar = BuildScrollBar(obj.transform, Scrollbar.Direction.LeftToRight);
                scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                scrollRect.horizontalScrollbarSpacing = -3;
            }

            if (vertical)
            {
                scrollRect.verticalScrollbar = BuildScrollBar(obj.transform, Scrollbar.Direction.BottomToTop);
                scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                scrollRect.verticalScrollbarSpacing = -3;
            }

            obj.transform.SetParent(parent);
            return contentRect.gameObject;
        }

        private static Scrollbar BuildScrollBar(Transform parent, Scrollbar.Direction direction)
        {
            bool hori = direction == Scrollbar.Direction.LeftToRight || direction == Scrollbar.Direction.RightToLeft;
            string targetName = hori ? "HScrollBar" : "VScrollBar";

            RectTransform scrollRect = BuildPanel(parent, targetName);
            SetRectTransformAnchor(
                scrollRect,
                horizontal: hori ? AnchorPreset.STRETCH : AnchorPreset.END,
                vertical: hori ? AnchorPreset.START : AnchorPreset.STRETCH,
                sizeDelta: (hori ? Vector2.up : Vector2.right) * UIConstants.SPACING_MD
            );
            scrollRect.pivot = hori ? Vector2.zero : Vector2.one;
            Scrollbar scrollbar = scrollRect.gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = direction;

            RectTransform slidingRect = BuildRect(scrollRect.gameObject.transform, "SlidingArea");
            SetRectTransformAnchor(slidingRect, horizontal: AnchorPreset.STRETCH, vertical: AnchorPreset.STRETCH);
            slidingRect.sizeDelta = Vector2.zero;
            slidingRect.offsetMin = Vector2.zero;
            slidingRect.offsetMax = Vector2.zero;

            RectTransform handleRect = BuildPanel(slidingRect.transform, "Handle");
            scrollbar.handleRect = handleRect;
            SetRectTransformAnchor(handleRect, horizontal: AnchorPreset.STRETCH, vertical: AnchorPreset.STRETCH);

            scrollbar.targetGraphic = handleRect.GetComponent<Image>();
            return scrollbar;
        }

        public static RectTransform BuildRect(Transform parent, string objName = "")
        {
            GameObject obj = new GameObject(objName);
            RectTransform rect = obj.AddComponent<RectTransform>();

            obj.transform.SetParent(parent);
            return rect;
        }


        public static RectTransform BuildPanel(Transform parent, string objName = "", float alpha = .25f)
        {
            GameObject obj = new GameObject(objName);
            RectTransform rect = obj.AddComponent<RectTransform>();
            obj.AddComponent<CanvasRenderer>();
            Image bkg = obj.AddComponent<Image>();
            Color c  = Color.white; 
            c.a = alpha; 
            bkg.color = c; 

            obj.transform.SetParent(parent);

            return rect;
        }


        enum AnchorPreset
        {
            START, CENTER, END, STRETCH
        }

        private static void SetRectTransformAnchor(RectTransform transform, AnchorPreset horizontal, AnchorPreset vertical, Vector2 sizeDelta = new())
        {
            float xMin = 0, xMax = 0, yMin = 0, yMax = 0;

            switch (horizontal)
            {
                case AnchorPreset.START: xMin = xMax = 0f; break;
                case AnchorPreset.CENTER: xMin = xMax = .5f; break;
                case AnchorPreset.END: xMin = xMax = 1f; break;
                case AnchorPreset.STRETCH: xMin = 0f; xMax = 1f; break;
            }

            switch (vertical)
            {
                case AnchorPreset.START: yMin = yMax = 0f; break;
                case AnchorPreset.CENTER: yMin = yMax = .5f; break;
                case AnchorPreset.END: yMin = yMax = 1f; break;
                case AnchorPreset.STRETCH: yMin = 0f; yMax = 1f; break;
            }

            transform.anchorMin = new Vector2(xMin, yMin);
            transform.anchorMax = new Vector2(xMax, yMax);
            transform.sizeDelta = sizeDelta;
            transform.pivot = Vector2.one;
            if (horizontal == AnchorPreset.STRETCH && vertical == AnchorPreset.STRETCH)
            {
                transform.offsetMin = Vector2.zero;
                transform.offsetMax = Vector2.zero;
            }
        }
    }
}