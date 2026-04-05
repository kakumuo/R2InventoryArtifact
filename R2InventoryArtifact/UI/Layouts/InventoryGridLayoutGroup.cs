

using UnityEngine;
using UnityEngine.UI;


namespace R2InventoryArtifact.UI.Layouts
{       
    public class InventoryGridLayoutGroup : LayoutGroup
    {
        private int _cols = 5; 
        private float _cellSize = 100; 

        public void Initialize(int cols)
        {
            _cols = cols; 
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            float width = rectTransform.rect.width - padding.horizontal; 
            _cellSize = width / _cols; 
            SetLayoutInputForAxis(width, width, width, 0); 
        }

        public override void CalculateLayoutInputVertical()
        {
            CalculateLayoutInputHorizontal(); 
            float height = (_cellSize * (rectChildren.Count / _cols)) + padding.vertical; 
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height); 
            SetLayoutInputForAxis(height, height, height, 1); 
        }

        public override void SetLayoutHorizontal()
        {
            float start = padding.left; 
            int i = 0; 

            rectChildren.ForEach(r =>
            {
                SetChildAlongAxis(rectChildren[i], 0, start + (_cellSize * (i % _cols)), _cellSize); 
                i += 1; 
            });
        }

        public override void SetLayoutVertical()
        {
            float start = padding.top; 
            int i = 0; 

            rectChildren.ForEach( r =>
            {
                SetChildAlongAxis(rectChildren[i], 1, start + (_cellSize * (i / _cols)), _cellSize); 
                i += 1; 
            });
        }
    }
}