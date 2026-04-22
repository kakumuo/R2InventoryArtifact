
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using R2InventoryArtifact.UI.Components;
using R2InventoryArtifact.UI.Layouts;
using RoR2.UI;
using UnityEngine.AddressableAssets;

namespace R2InventoryArtifact.UI.Builders
{    
    public static partial class ComponentBuilder
    {
        public static InventorySlotComponent BuildGridSlot(Transform parent, string objName = "")
        {
            RectTransform slotRect = BuildPanel(parent, objName, SpritePanelType.TILE); 
            GameObject obj = slotRect.gameObject; 
            obj.AddComponent<HorizontalLayoutGroup>();
            obj.AddComponent<TooltipProvider>(); 
            
            //TODO: find way to import shader
            // var shader = Shader.Find("Hidden/Custom/R2InventoryArtifact/InventoryItemOutlineShader");
            // img.material = new Material(shader);

            return obj.AddComponent<InventorySlotComponent>();
        }

        public static InventoryItemElement BuildItemElement(string objName="")
        {
            // var obj = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/ItemIcon.prefab").WaitForCompletion();
            // ItemIcon icon = obj.GetComponent<ItemIcon>(); 
            // icon.SetItemIndex(item.Pickup.pickupIndex.pickupDef.itemIndex, 1, 1); 
            
            GameObject obj = new GameObject(objName); 
            obj.AddComponent<RectTransform>(); 
            obj.AddComponent<Image>(); 
            obj.AddComponent<CanvasGroup>(); 
            obj.AddComponent<HorizontalLayoutGroup>(); 
            obj.AddComponent<LayoutElement>(); 
            // AspectRatioFitter fitter = obj.AddComponent<AspectRatioFitter>(); 
            // fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth; 
            // fitter.aspectRatio = 1; 

                GameObject labelObj = new GameObject("label");
                labelObj.AddComponent<RectTransform>(); 
                TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();  
                label.color = Color.black; 
                label.horizontalAlignment = HorizontalAlignmentOptions.Right; 
                label.verticalAlignment = VerticalAlignmentOptions.Bottom; 
                labelObj.transform.SetParent(obj.transform); 
            
            var output = obj.AddComponent<InventoryItemElement>(); 

            return output; 
        }

        public static InventoryEffectElement BuildEffectElement(string objName="")
        {
            GameObject obj = new GameObject(objName); 
            RectTransform rect = obj.AddComponent<RectTransform>(); 
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 40); 
            obj.AddComponent<Image>(); 
            obj.AddComponent<CanvasGroup>(); 
            obj.AddComponent<VerticalLayoutGroup>(); 

                GameObject labelObj = new GameObject("label");
                labelObj.AddComponent<RectTransform>(); 
                TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();  
                label.fontSize = UIConstants.FONT_MD; 
                label.color = Color.black; 
                label.autoSizeTextContainer = true; 
                labelObj.transform.SetParent(obj.transform); 
            
            return obj.AddComponent<InventoryEffectElement>(); 
        }

        public static InventoryHoldElement BuildHoldElement(Transform parent, string objName="")
        {
            // GameObject obj = new GameObject(objName); 
            // obj.AddComponent<RectTransform>(); 
            // obj.AddComponent<Image>(); 
            RectTransform objRect = BuildPanel(parent, objName, SpritePanelType.TILE); 
            SetRectTransformAnchor(objRect, AnchorPreset.STRETCH, AnchorPreset.STRETCH, new(10, 40));

            GameObject obj = objRect.gameObject; 
            obj.AddComponent<CanvasGroup>(); 
            LayoutElement layoutElement = obj.AddComponent<LayoutElement>(); 
            layoutElement.flexibleHeight = 1; 
            layoutElement.minHeight = 75f; 
            layoutElement.preferredHeight = 75f; 
            BentoLayoutGroup layout = obj.AddComponent<BentoLayoutGroup>(); 
            layout.UnitWidth = 8; 
            layout.UnitHeight = 4; 
            layout.Spans = new List<BentoSpan>()
            {
                new(){Col=0, Row=0, ColSpan=3, RowSpan=4}, // icon
                new(){Col=3, Row=0, ColSpan=5, RowSpan=2}, // title label
                new(){Col=3, Row=2, ColSpan=5, RowSpan=2}, // stack label
            }; 

        // icon
            GameObject iconObj = new GameObject("icon"); 
            iconObj.AddComponent<RectTransform>(); 
            iconObj.AddComponent<Image>(); 
            AspectRatioFitter fitter = iconObj.AddComponent<AspectRatioFitter>(); 
            fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight; 
            fitter.aspectRatio = 1; 
            iconObj.transform.SetParent(obj.transform);

        // title
            GameObject titleObj = new GameObject("title"); 
            titleObj.AddComponent<RectTransform>(); 
            TextMeshProUGUI titleLbl = titleObj.AddComponent<TextMeshProUGUI>(); 
            titleLbl.color = Color.black; 
            titleLbl.fontSize = UIConstants.FONT_LG; 
            titleObj.transform.SetParent(obj.transform); 

        // stack label
            GameObject stackObj = new GameObject("stack"); 
            stackObj.AddComponent<RectTransform>(); 
            TextMeshProUGUI stackLbl = stackObj.AddComponent<TextMeshProUGUI>(); 
            stackLbl.color = Color.black; 
            stackLbl.fontSize = UIConstants.FONT_LG;  
            stackObj.transform.SetParent(obj.transform); 
        
            return obj.AddComponent<InventoryHoldElement>(); 
        }

        public static R2Tooltip BuildR2TooltipComponent(string objName="")
        {
            GameObject obj = new GameObject(objName); 
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 100); 
            rect.pivot = Vector2.up; 
            rect.position = Vector2.zero;  
            obj.AddComponent<Image>(); 
            CanvasGroup group = obj.AddComponent<CanvasGroup>(); 
            VerticalLayoutGroup layout = obj.AddComponent<VerticalLayoutGroup>(); 
            layout.childControlWidth = true; 
            layout.childForceExpandWidth = true; 
            
                GameObject headerObj = new GameObject("header"); 
                headerObj.AddComponent<RectTransform>().sizeDelta = new Vector2(200, 50); 
                Image headerImg = headerObj.AddComponent<Image>();
                headerObj.AddComponent<HorizontalLayoutGroup>(); 
                headerObj.transform.SetParent(obj.transform); 

                    GameObject headerLblObj = new GameObject("headerLbl"); 
                    headerLblObj.AddComponent<RectTransform>(); 
                    TextMeshProUGUI headerLbl = headerLblObj.AddComponent<TextMeshProUGUI>(); 
                    headerLbl.color = Color.black; 
                    headerLbl.fontSize = UIConstants.FONT_MD; 
                    headerLblObj.transform.SetParent(headerObj.transform); 

                GameObject bodyObj = new GameObject("body"); 
                bodyObj.AddComponent<RectTransform>(); 
                bodyObj.AddComponent<Image>();
                bodyObj.AddComponent<HorizontalLayoutGroup>(); 
                bodyObj.transform.SetParent(obj.transform); 

                    GameObject bodyLblObj = new GameObject("bodyLbl"); 
                    bodyLblObj.AddComponent<RectTransform>(); 
                    TextMeshProUGUI bodyLbl = bodyLblObj.AddComponent<TextMeshProUGUI>(); 
                    bodyLbl.color = Color.black; 
                    bodyLbl.fontSize = UIConstants.FONT_MD; 
                    bodyLblObj.transform.SetParent(bodyObj.transform); 
            R2Tooltip tootlip =  obj.AddComponent<R2Tooltip>(); 
            tootlip.Initialize(headerLbl, bodyLbl, group); 
            tootlip.Hide(); 
        
            return tootlip; 
        }
    }
}