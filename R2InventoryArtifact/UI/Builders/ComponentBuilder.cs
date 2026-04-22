using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using R2InventoryArtifact.UI.Components;
using R2InventoryArtifact.UI.Layouts;
using RoR2;
using UnityEngine.AddressableAssets;
using RoR2.UI;


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
            // GameObject contentObj = BuildR2Panel(parent); 
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
            // GameObject panel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/DefaultPanel.prefab").WaitForCompletion(); 
            // panel.transform.SetParent(parent); 
            // UnityEngine.Component.Destroy(panel.GetComponent<RectTransform>()); 
            // panel.AddComponent<RectTransform>(); 
        
            RectTransform panelRect = BuildPanel(parent, "InventoryUI", SpritePanelType.BACKDROP);
            GameObject panelObj = panelRect.gameObject; 
            panelObj.layer = LayerIndex.ui.intVal; 
            // panelObj.AddComponent<LayoutElement>(); 
            Canvas canvas = panelObj.AddComponent<Canvas>(); 
            canvas.sortingOrder = 10; 
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; 

            panelObj.AddComponent<CanvasGroup>(); 
            CanvasScaler canvasScaler = panelObj.AddComponent<CanvasScaler>(); 
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            
            panelObj.AddComponent<GraphicRaycaster>(); 

            BentoLayoutGroup bentoGroup = panelObj.AddComponent<BentoLayoutGroup>(); 
            bentoGroup.Spans = new List<BentoSpan>()
            {
                new(){Col=0, Row=0, ColSpan=2, RowSpan=7},
                new(){Col=0, Row=7, ColSpan=2, RowSpan=3},
                new(){Col=2, Row=1, ColSpan=8, RowSpan=9},
                new(){Col=2, Row=0, ColSpan=8, RowSpan=1},
                // new(){Col=8, Row=0, ColSpan=2, RowSpan=10},
            }; 
            int spacing = 8; 
            int horipad = (int)(canvasScaler.referenceResolution.x * .20f); 
            int vertpad = (int)(canvasScaler.referenceResolution.y * .20f); 
            bentoGroup.UnitWidth = 10; 
            bentoGroup.UnitHeight = 10; 
            bentoGroup.Spacing = spacing; 
            bentoGroup.padding = new(horipad, horipad, vertpad, vertpad); 

            InventoryUI ui = panelObj.AddComponent<InventoryUI>(); 
            //MAYBE:
            // void Close() => ui.SetUIVisibility(show: false); 
            // panelObj.AddComponent<RiskOfOptions.Components.Options.RooEscapeRouter>().escapePressed.AddListener(Close); 

            return ui; 
        }
    }
}