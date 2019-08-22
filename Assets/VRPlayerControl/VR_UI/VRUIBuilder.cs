using UnityEngine;

// using Game.UI;
using SimpleUI;

namespace VRPlayer.UI {

    public static class VRUIBuilder  {
        
        // build a seperate worldspace canvas for each ui element
        static RectTransform BuildCanvasObject (string name, VRMenuFollowerParameters menuBehaviorParams, TransformBehavior followBehavior) {
            GameObject g = new GameObject(name);
            g.layer = LayerMask.NameToLayer("UI");

            RectTransform t = g.AddComponent<RectTransform>();
            t.sizeDelta = Vector2.one;
        
            Canvas c = g.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;

            UnityEngine.UI.CanvasScaler scaler = g.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 1;
            scaler.referencePixelsPerUnit = 1;

            // UnityEngine.UI.GraphicRaycaster gr = g.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            // gr.ignoreReversedGraphics = true;
            // gr.blockingObjects = UnityEngine.UI.GraphicRaycaster.BlockingObjects.None;

            if (menuBehaviorParams != null) {
                VRMenu vrm = g.AddComponent<VRMenu>();
                vrm.parameters = menuBehaviorParams;
                vrm.followBehavior = followBehavior;
            }

            return t;
        }

        static void SetAnchor(RectTransform t, Vector2 anchor) {
            t.anchorMin = anchor;
            t.anchorMax = anchor;
            t.pivot = anchor;
        }
        static void SetParent(RectTransform t, RectTransform parent, Vector2 anchor, Vector3 lPos, Vector3 lRot) {
            SetParent(t, parent, anchor);
            t.localRotation = Quaternion.Euler(lRot);
            t.localPosition = lPos;
        }
        static void SetParent(RectTransform t, RectTransform parent, Vector2 anchor) {
            SetAnchor(t, anchor);
            t.SetParent(parent);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
        static void SetParent(RectTransform t, RectTransform parent, Vector2 anchor, Vector3 lPos) {
            SetParent(t, parent, anchor);
            t.localPosition = lPos;
        }

        public static ControllerHintsPanel InstantiateControllerPanelFull (string goName, ControllerHintsPanelParameters parameters) {
            ControllerHintsPanel newPanel = GameObject.Instantiate (UIManager.instance.controllerHintsPanelPrefab);
            newPanel.parameters = parameters;
            newPanel = InitializeBaseUIElement<ControllerHintsPanel> (newPanel, BuildCanvasObject(goName, null, null), new Vector2(.5f, .5f));
            UIManager.HideUI(newPanel);
            return newPanel;
        } 


        public static ControllerHintsPanel InstantiateControllerPanel (RectTransform parent, Vector3 controllerHintsLocalPos, ControllerHintsPanelParameters parameters, UIElementHolder[] associatedHolders) {
            ControllerHintsPanel newPanel = GameObject.Instantiate (UIManager.instance.controllerHintsPanelPrefab);
            newPanel.parameters = parameters;

            SetParent(newPanel.rectTransform, parent, new Vector2(.5f, .5f), controllerHintsLocalPos);
            
            for (int i = 0; i < associatedHolders.Length; i++) associatedHolders[i].controlHintsPanel = newPanel;

            return newPanel;
        } 

        public static UISliderPopup MakeSliderPopup (string goName, UISliderPopupParameters parameters, TransformBehavior equipBehavior, VRMenuFollowerParameters menuBehaviorParams) {
            UISliderPopup msgCenter = GameObject.Instantiate( UIManager.instance.sliderPopupPrefab );
            msgCenter.parameters = parameters;
            msgCenter = InitializeBaseUIElement<UISliderPopup> (msgCenter, BuildCanvasObject(goName, menuBehaviorParams, equipBehavior), new Vector2(.5f, 1f));
            UIManager.HideUI(msgCenter);
            return msgCenter;
        }
        public static UISelectionPopup MakeSelectionPopup (string goName, UISelectionPopupParameters parameters, TransformBehavior equipBehavior, VRMenuFollowerParameters menuBehaviorParams) {
            UISelectionPopup msgCenter = GameObject.Instantiate( UIManager.instance.selectionPopupPrefab );
            msgCenter.parameters = parameters;
            msgCenter = InitializeBaseUIElement<UISelectionPopup> (msgCenter, BuildCanvasObject(goName, menuBehaviorParams, equipBehavior), new Vector2(.5f, 1f));
            UIManager.HideUI(msgCenter);
            return msgCenter;
        }


        public static UIMessageCenter MakeMessageCenter (string goName, UIMessageCenterParameters parameters, TransformBehavior equipBehavior, VRMenuFollowerParameters menuBehaviorParams) {
            UIMessageCenter msgCenter = GameObject.Instantiate( UIManager.instance.messageCenterPrefab );
            msgCenter.parameters = parameters;
            return InitializeBaseUIElement<UIMessageCenter> (msgCenter, BuildCanvasObject(goName, menuBehaviorParams, equipBehavior), new Vector2(.5f, .5f));
        }
        public static UISubtitles MakeSubtitles ( string goName, UISubtitlesParameters parameters, TransformBehavior equipBehavior, VRMenuFollowerParameters menuBehaviorParams) {
            UISubtitles returnElement = GameObject.Instantiate( UIManager.instance.subtitlesPrefab );
            returnElement.parameters = parameters;
            return InitializeBaseUIElement<UISubtitles> (returnElement, BuildCanvasObject(goName, menuBehaviorParams, equipBehavior), new Vector2(.5f, .5f));
        }

        public static UIValueTracker MakeValueTrackerUI (string goName, UIValueTrackerParameters parameters, bool useVertical) {
            UIValueTracker newValueTracker = GameObject.Instantiate( useVertical ? UIManager.instance.valueTrackerVerticalPrefab : UIManager.instance.valueTrackerHorizontalPrefab );
            newValueTracker.parameters = parameters;
            newValueTracker.UpdateLayout();
            return InitializeBaseUIElement<UIValueTracker> (newValueTracker, BuildCanvasObject(goName, null, null), new Vector2(.5f, .5f));
        }

        public static UIElementHolder MakeRadial (string goName, UIRadialParameters radialParameters, Vector3 controllerHintsLocalPos, ControllerHintsPanelParameters controllerHintsParameters){
            UIRadial newRadial = InitializeBaseUIElement<UIRadial> (GameObject.Instantiate( UIManager.instance.radialPrefab ), BuildCanvasObject(goName, null, null), new Vector2(.5f, .5f));
            newRadial.parameters = radialParameters;
            newRadial.isBase = true;

            InstantiateControllerPanel (newRadial.rectTransform, controllerHintsLocalPos, controllerHintsParameters, new UIElementHolder[] { newRadial });

            UIManager.HideUI(newRadial);
            return newRadial;
        }
        public static UIElementHolder MakeButtonsPage (string goName, UIPageParameters pageParameters, TransformBehavior equipBehavior, VRMenuFollowerParameters menuBehaviorParams, Vector3 controllerHintsLocalPos, ControllerHintsPanelParameters controllerHintsParameters){
            UIPage newPage = InitializeBaseUIElement<UIPage> (InstantiatePage (pageParameters), BuildCanvasObject(goName, menuBehaviorParams, equipBehavior), new Vector2(.5f, 1f));
            
            InstantiateControllerPanel (newPage.rectTransform, controllerHintsLocalPos, controllerHintsParameters, new UIElementHolder[] { newPage });

            UIManager.HideUI(newPage);
            return newPage;
        }

        static UITextPanel InstantiateTextPanel (TextPanelParameters parameters, UIElementHolder[] associatedHolders) {
            UITextPanel textPanel = GameObject.Instantiate( UIManager.instance.textPanelPrefab );
            textPanel.parameters = parameters;
            for (int i = 0; i < associatedHolders.Length; i++) associatedHolders[i].textPanel = textPanel;
            return textPanel;
        }

        static UIPage InstantiatePage (UIPageParameters parameters) {
            UIPage newPage = GameObject.Instantiate( UIManager.instance.pagePrefab );
            newPage.isBase = true;
            newPage.parameters = parameters;
            return newPage;
        }
        
        public static UIElementHolder MakeButtonsPage (string goName, UIPageParameters pageParameters, TextPanelParameters textPanelParams, TransformBehavior equipBehavior, VRMenuFollowerParameters menuBehaviorParams, float buildRotationOffset, Vector3 controllerHintsLocalPos, ControllerHintsPanelParameters controllerHintsParameters){
            RectTransform canvasObject = BuildCanvasObject(goName, menuBehaviorParams, equipBehavior);
            
            UIPage newPage = InstantiatePage (pageParameters);
            
            float width = pageParameters.elementsSize.x;
            float titleHeight = pageParameters.titleHeight;

            SetParent(newPage.rectTransform, canvasObject, new Vector2(.5f, 1f), new Vector3(-(width*.5f), 0, 0), new Vector3(0,buildRotationOffset,0));
            SetParent(InstantiateTextPanel(textPanelParams, new UIElementHolder[] { newPage }).rectTransform, canvasObject, new Vector2(.5f, 1f), new Vector3(width*.5f, -pageParameters.titleHeight, 0), new Vector3(0,-buildRotationOffset,0));
            
            InstantiateControllerPanel (canvasObject, controllerHintsLocalPos, controllerHintsParameters, new UIElementHolder[] { newPage });

            newPage.baseObject = canvasObject.gameObject;
            UIManager.HideUI(newPage);
            return newPage;
        }

        
        static T InitializeBaseUIElement<T> (BaseUIElement element, RectTransform baseObject, Vector2 anchor) where T : BaseUIElement {
            element.baseObject = baseObject.gameObject;
            SetParent(element.rectTransform, baseObject, anchor);
            return element as T;
        }


        public static UIElementHolder MakeFullTradeUI (string goName, UIPageParameters pageParameters, TextPanelParameters textPanelParams, TransformBehavior equipBehavior, VRMenuFollowerParameters menuBehaviorParams, float buildRotationOffset, float textPanelRotationZOffset, Vector3 controllerHintsLocalPos, ControllerHintsPanelParameters controllerHintsParameters){//, float textScale) {
            RectTransform canvasObject = BuildCanvasObject(goName, menuBehaviorParams, equipBehavior);

            ElementHolderCollection collection = canvasObject.gameObject.AddComponent<ElementHolderCollection>();
            collection.isBase = true;
            collection.baseObject = canvasObject.gameObject;

            float width = pageParameters.elementsSize.x;
                            
            Vector2 anchor = new Vector2(.5f, 1f);

            UIPage newPage0 = InstantiatePage(pageParameters);
            SetParent(newPage0.rectTransform, collection.rectTransform, anchor, new Vector3(-(width*1f), 0, 0), new Vector3(0,buildRotationOffset,0));
    
            UIPage newPage1 = InstantiatePage(pageParameters);
            SetParent(newPage1.rectTransform, collection.rectTransform, anchor, new Vector3((width*1f), 0, 0), new Vector3(0,-buildRotationOffset,0));

            collection.subHolders = new UIElementHolder[] { newPage0 , newPage1 };

            SetParent(InstantiateTextPanel(textPanelParams, new UIElementHolder[] {collection, newPage0 , newPage1 }).rectTransform, collection.rectTransform, anchor, new Vector3(0, -pageParameters.titleHeight, textPanelRotationZOffset));
            

            InstantiateControllerPanel (collection.rectTransform, controllerHintsLocalPos, controllerHintsParameters, new UIElementHolder[] { collection, newPage0 , newPage1 });

            UIManager.HideUI(collection);
            return collection;
        }  
    }
}