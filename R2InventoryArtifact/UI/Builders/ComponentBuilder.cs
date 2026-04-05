
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using R2InventoryArtifact.UI.Components;
using R2InventoryArtifact.UI.Layouts;


namespace R2InventoryArtifact.UI.Builders
{    
    public static partial class ComponentBuilder
    {
        public static InventoryDropComponent BuildInventoryDropComponent(Transform parent)
        {
            RectTransform panel = BuildPanel(parent, "DropArea"); 
            return panel.gameObject.AddComponent<InventoryDropComponent>(); 
        }

        public static InventoryHoldComponent BuildInventoryHoldComponent(Transform parent)
        {
            GameObject contentObj = BuildScrollView(parent, "HoldList", vertical: true); 
            VerticalLayoutGroup verticalLayoutGroup = contentObj.AddComponent<VerticalLayoutGroup>(); 
            verticalLayoutGroup.childControlWidth = true; 
            verticalLayoutGroup.childControlHeight = false; 
            verticalLayoutGroup.childForceExpandWidth = true; 
            verticalLayoutGroup.spacing = UIConstants.SPACING_MD; 

            return contentObj.transform.parent.gameObject.AddComponent<InventoryHoldComponent>(); 
        }

        public static InventoryNonEquipComponent BuildInventoryNonEquipComponent(Transform parent)
        {
            GameObject contentObj = BuildScrollView(parent, "NonEqiup", horizontal: true);
            HorizontalLayoutGroup horizontalLayoutGroup = contentObj.AddComponent<HorizontalLayoutGroup>(); 
            horizontalLayoutGroup.childControlHeight = true; 
            horizontalLayoutGroup.childControlWidth = false; 
            horizontalLayoutGroup.childForceExpandHeight = true; 
            return contentObj.AddComponent<InventoryNonEquipComponent>();  
        } 

        public static InventoryEffectComponent BuildInventoryEffectComponent(Transform parent)
        {
            GameObject scrollView = BuildScrollView(parent, "EffectList", vertical: true);
            return scrollView.AddComponent<InventoryEffectComponent>();  
        }

        public static InventoryGridComponent BuildInventoryGridComponent(Transform parent)
        {            
            GameObject scrollView = BuildScrollView(parent, "Grid", vertical: true, horizontal: true);
            return scrollView.AddComponent<InventoryGridComponent>();  
        } 

        public static InventoryUI BuildInventoryUI(Transform parent)
        {
            RectTransform panelRect = BuildPanel(parent, "InventoryUI");
            GameObject panel = panelRect.gameObject; 
            Canvas canvas = panel.AddComponent<Canvas>(); 
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; 

            panel.AddComponent<CanvasGroup>(); 
            panel.AddComponent<CanvasScaler>(); 
            panel.AddComponent<GraphicRaycaster>(); 


            BentoLayoutGroup bentoGroup = panel.AddComponent<BentoLayoutGroup>(); 
            bentoGroup.Spans = new List<BentoSpan>()
            {
                new(){Col=0, Row=0, ColSpan=2, RowSpan=7},
                new(){Col=0, Row=7, ColSpan=2, RowSpan=3},
                new(){Col=2, Row=1, ColSpan=8, RowSpan=9},
                new(){Col=2, Row=0, ColSpan=8, RowSpan=1},
                // new(){Col=8, Row=0, ColSpan=2, RowSpan=10},
            }; 
            int spacing = 8; 
            bentoGroup.UnitWidth = 10; 
            bentoGroup.UnitHeight = 10; 
            bentoGroup.Spacing = spacing; 
            bentoGroup.padding = new(spacing, spacing, spacing, spacing); 

            return panel.AddComponent<InventoryUI>(); 
        }
    }
}