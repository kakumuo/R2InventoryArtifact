


using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace R2InventoryArtifact.UI.Layouts
{
    [Serializable] public struct BentoSpan
    {
        public int Col; 
        public int Row; 
        [Min(1)] public int ColSpan; 
        [Min(1)] public int RowSpan; 
    }

    public class BentoLayoutGroup : LayoutGroup
    {
        [SerializeField] private float m_Spacing;
        [SerializeField] private int m_UnitWidth = 1; 
        [SerializeField] private int m_UnitHeight = 1; 
        [SerializeField] private List<BentoSpan> m_Spans; 

        [HideInInspector] public float Spacing          { get => m_Spacing;     set => m_Spacing = value; }
        [HideInInspector] public int UnitWidth          { get => m_UnitWidth;   set => m_UnitWidth = value; }
        [HideInInspector] public int UnitHeight         { get => m_UnitHeight;  set => m_UnitHeight = value; }
        [HideInInspector] public List<BentoSpan> Spans  { get => m_Spans;       set => m_Spans = value ?? new(); }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            SetLayoutInputForAxis(1, 1, 1, 0); 
        }

        public override void CalculateLayoutInputVertical()
        {
            CalculateLayoutInputHorizontal(); 
            SetLayoutInputForAxis(1, 1, 1, 1); 
        }

        private (float, float) CalcChildAxis(int axis, BentoSpan span) 
        {
            int cellSpan, cellStart;
            float cellSize;
            float start;

            if (axis == 0)
            {
                start = padding.left;
                cellSize = (rectTransform.rect.width - padding.horizontal - (Spacing * (UnitWidth - 1))) / UnitWidth;
                cellStart = span.Col;
                cellSpan = span.ColSpan;
            }
            else
            {
                start = padding.top;
                cellSize = (rectTransform.rect.height - padding.vertical - (Spacing * (UnitHeight - 1))) / UnitHeight;
                cellStart = span.Row;
                cellSpan = span.RowSpan;
            }

            float targetPos = start + cellSize * cellStart + Spacing * cellStart;
            float targetSize = cellSize * cellSpan + Spacing * (cellSpan - 1);

            return (targetPos, targetSize); 
        }

        public override void SetLayoutHorizontal()
        {
            int targetCount = Mathf.Min(rectChildren.Count, m_Spans.Count); 
            for(int i = 0; i < targetCount; i++)
            {
                var (targetPos, targetSize) = CalcChildAxis(0, Spans[i]); 

                SetChildAlongAxis(rectChildren[i], 0, targetPos, targetSize); 
            }
        }

        public override void SetLayoutVertical()
        {
            int targetCount = Mathf.Min(rectChildren.Count, m_Spans.Count); 
            for(int i = 0; i < targetCount; i++)
            {
                var (targetPos, targetSize) = CalcChildAxis(1, Spans[i]); 

                SetChildAlongAxis(rectChildren[i], 1, targetPos, targetSize); 
            }
        }
    }
}