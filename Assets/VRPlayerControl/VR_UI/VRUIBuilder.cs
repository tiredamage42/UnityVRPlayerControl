using UnityEngine;

using GameUI;
using SimpleUI;

namespace VRPlayer.UI {

    public static class VRUIBuilder  {
        
        // build a seperate worldspace canvas for each ui element
        static RectTransform BuildCanvasObject (string name, bool addVRMenuBehavior, TransformBehavior followBehavior) {
            GameObject g = new GameObject(name);

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

            if (addVRMenuBehavior) {

                VRMenu vrm = g.AddComponent<VRMenu>();
                vrm.matchXRotation = false;
                vrm.angleThreshold = 45;
                vrm.followSpeed = 5;
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

        public static UIMessageCenter MakeMessageCenter (string goName, UIMessageCenterParameters parameters, TransformBehavior equipBehavior) {
            UIMessageCenter msgCenter = GameObject.Instantiate( UIManager.instance.messageCenterPrefab );
            msgCenter.parameters = parameters;
            return InitializeBaseUIElement<UIMessageCenter> (msgCenter, BuildCanvasObject(goName, true, equipBehavior), new Vector2(.5f, .5f));
        }
        public static UISubtitles MakeSubtitles ( string goName, UISubtitlesParameters parameters, TransformBehavior equipBehavior ) {
            UISubtitles returnElement = GameObject.Instantiate( UIManager.instance.subtitlesPrefab );
            returnElement.parameters = parameters;
            return InitializeBaseUIElement<UISubtitles> (returnElement, BuildCanvasObject(goName, true, equipBehavior), new Vector2(.5f, .5f));
        }

        public static UIValueTracker MakeValueTrackerUI (string goName, UIValueTrackerParameters parameters, bool useVertical) {
            UIValueTracker newValueTracker = GameObject.Instantiate( useVertical ? UIManager.instance.valueTrackerVerticalPrefab : UIManager.instance.valueTrackerHorizontalPrefab );
            newValueTracker.parameters = parameters;
            return InitializeBaseUIElement<UIValueTracker> (newValueTracker, BuildCanvasObject(goName, false, null), new Vector2(.5f, .5f));
        }

        public static UIElementHolder MakeRadial (string goName, UIRadialParameters radialParameters){
            UIRadial newRadial = InitializeBaseUIElement<UIRadial> (GameObject.Instantiate( UIManager.instance.radialPrefab ), BuildCanvasObject(goName, false, null), new Vector2(.5f, .5f));
            newRadial.parameters = radialParameters;
            newRadial.isBase = true;
            UIManager.HideUI(newRadial);
            return newRadial;
        }
        public static UIElementHolder MakeButtonsPage (string goName, UIPageParameters pageParameters, bool addVRMenuBehavior, TransformBehavior equipBehavior){
            UIPage newPage = InitializeBaseUIElement<UIPage> (InstantiatePage (pageParameters), BuildCanvasObject(goName, addVRMenuBehavior, equipBehavior), new Vector2(.5f, 1f));
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

        
        public static UIElementHolder MakeButtonsPage (string goName, UIPageParameters pageParameters, TextPanelParameters textPanelParams, bool addVRMenuBehavior, TransformBehavior equipBehavior, float buildRotationOffset){
            RectTransform canvasObject = BuildCanvasObject(goName, addVRMenuBehavior, equipBehavior);
            
            UIPage newPage = InstantiatePage (pageParameters);
            
            // RectTransform childRect = new GameObject(goName+"pagePanel_holder").AddComponent<RectTransform>();
            // childRect.sizeDelta = Vector2.one;
            // SetParent(childRect, canvasObject, new Vector2(.5f, 1f));

            RectTransform childRect = canvasObject;

            float width = pageParameters.elementsSize.x;
            float titleHeight = pageParameters.titleHeight;

            SetParent(newPage.rectTransform, childRect, new Vector2(.5f, 1f), new Vector3(-(width*.5f), 0, 0), new Vector3(0,buildRotationOffset,0));
            SetParent(InstantiateTextPanel(textPanelParams, new UIElementHolder[] { newPage }).rectTransform, childRect, new Vector2(.5f, 1f), new Vector3(width*.5f, -pageParameters.titleHeight, 0), new Vector3(0,-buildRotationOffset,0));
            

            newPage.baseObject = canvasObject.gameObject;
            UIManager.HideUI(newPage);
            return newPage;
        }

        

        static T InitializeBaseUIElement<T> (BaseUIElement element, RectTransform baseObject, Vector2 anchor) where T : BaseUIElement {
            element.baseObject = baseObject.gameObject;
            SetParent(element.rectTransform, baseObject, anchor);
            return element as T;
        }


        public static UIElementHolder MakeFullTradeUI (string goName, UIPageParameters pageParameters, TextPanelParameters textPanelParams, TransformBehavior equipBehavior, float buildRotationOffset){//, float textScale) {
            RectTransform canvasObject = BuildCanvasObject(goName, true, equipBehavior);

            // RectTransform childRect = new GameObject(goName+"_holder").AddComponent<RectTransform>();
            // SetParent(childRect, canvasObject, new Vector2(.5f, .5f));
            RectTransform childRect = canvasObject;
                    
            ElementHolderCollection collection = childRect.gameObject.AddComponent<ElementHolderCollection>();
            collection.isBase = true;
            collection.baseObject = canvasObject.gameObject;

            // RectTransform collectionT = collection.GetComponent<RectTransform>();

            float width = pageParameters.elementsSize.x;
                            
            Vector2 anchor = new Vector2(.5f, 1f);

            UIPage newPage0 = InstantiatePage(pageParameters);
            SetParent(newPage0.rectTransform, collection.rectTransform, anchor, new Vector3(-(width*1f), 0, 0), new Vector3(0,buildRotationOffset,0));
    
            UIPage newPage1 = InstantiatePage(pageParameters);
            SetParent(newPage1.rectTransform, collection.rectTransform, anchor, new Vector3((width*1f), 0, 0), new Vector3(0,-buildRotationOffset,0));

            collection.subHolders = new UIElementHolder[] { newPage0 , newPage1 };

            SetParent(InstantiateTextPanel(textPanelParams, collection.subHolders).rectTransform, collection.rectTransform, anchor, new Vector3(0, -pageParameters.titleHeight, 0));

            UIManager.HideUI(collection);
            return collection;
        }  
    }
}