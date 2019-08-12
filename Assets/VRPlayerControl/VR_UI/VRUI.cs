using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using InventorySystem;
using SimpleUI;
using Valve.VR;
using GameUI;
using GameBase;


namespace VRPlayer {

    public class VRUI : MonoBehaviour
    {
        

        InventoryUI inventoryUI;
        Inventory inventory;



        void OnEnable () {
        
            GameManager.onPauseRoutineStart += OnGamePaused;
            
            UIManager.onUISelect += OnUISelection;
            inventory = Player.instance.GetComponent<Inventory>();
            inventoryUI = Player.instance.GetComponent<InventoryUI>();
            
        }
            
            
        void OnDisable () {
            GameManager.onPauseRoutineStart -= OnGamePaused;
            UIManager.onUISelect -= OnUISelection;
        }

        
        void OnUISelection (GameObject[] data, object[] customData) {
            // float duration,  float frequency, float amplitude
            StandardizedVRInput.instance.TriggerHapticPulse( VRUIInput.GetUIHand (), .1f, 1.0f, 1.0f );   
        }
        
        void OnGamePaused(bool isPaused, float routineTime) {
            if (isPaused) {
                VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
            }
        }        





















         
        [Space] public SteamVR_Action_Boolean uiInvToggleAction;
        public string invContext = "FullInventory";


        protected InventoryManagementUIHandler invUIHandler;
        
       
        // open full inventory logic
        void Update()
        {            
            UpdateNormalInventory();
                 UpdateQuickInventory();
       
        }
        void UpdateNormalInventory () {
            // if (GameManager.isPaused)
            //     return;
            // if (inventoryUI.IsOpen(InventoryUI.UIType.FullTrade))
            //     return;
            
            if (uiInvToggleAction.GetStateDown(VRManager.instance.mainHand)) {
                if (invUIHandler == null) {
                    invUIHandler = InventoryManagementUIHandler.GetUIHandlerByContext(invContext);
                }
                            

                if (invUIHandler.UIObjectActive())
                    inventory.EndInventoryManagement (invContext, 0);
                else
                    inventory.InitiateInventoryManagement (invContext, 0, null, null);
                    
            }
        } 




        [Space] public SteamVR_Action_Boolean uiQuickInvToggleAction;
        public string quickInvContext = "QuickInventory";
       

        // Opening quick invnetory logic
        

        bool CheckHandForWristRadialOpen (SteamVR_Input_Sources hand) {
			if (uiQuickInvToggleAction.GetStateDown(hand)) {
                inventory.InitiateInventoryManagement (quickInvContext, VRManager.Hand2Int(hand), null, null);
				return true;
			}
			return false;
		}
        void UpdateQuickInventory () {

            if (UIManager.AnyUIOpen(-1))
                return;
            
			if (Player.instance.handsTogether) {
                
                StandardizedVRInput.MarkActionOccupied(uiQuickInvToggleAction, SteamVR_Input_Sources.Any);

                if (!CheckHandForWristRadialOpen(SteamVR_Input_Sources.LeftHand)) {
                    CheckHandForWristRadialOpen(SteamVR_Input_Sources.RightHand);
                }
            }
            else {
                StandardizedVRInput.MarkActionUnoccupied(uiQuickInvToggleAction);
            }   
        }   



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
        static void SetParent(RectTransform t, RectTransform parent) {
            t.SetParent(parent);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
        static void SetParent(RectTransform t, RectTransform parent, Vector3 lPos) {
            SetParent(t, parent);
            t.localPosition = lPos;
        }

        public static UIValueTracker MakeValueTrackerUI (string goName, UIValueTrackerParameters parameters, bool useVertical) {
            
            // build a seperate worldspace canvas for each ui element
            RectTransform canvasObject = BuildCanvasObject(goName, false, null);
            UIValueTracker newValueTracker = GameObject.Instantiate( useVertical ? UIManager.instance.valueTrackerVerticalPrefab : UIManager.instance.valueTrackerHorizontalPrefab );
            newValueTracker.parameters = parameters;
            newValueTracker.baseObject = canvasObject.gameObject;
            
            RectTransform newTrackerRect = newValueTracker.GetComponent<RectTransform>();
            SetAnchor(newTrackerRect, new Vector2(.5f, .5f));
            SetParent(newTrackerRect, canvasObject);
            
            return newValueTracker;
        }



        public static UISubtitles MakeSubtitles ( string goName, UISubtitlesParameters parameters, TransformBehavior equipBehavior ) {
            // build a seperate worldspace canvas for each ui element
            RectTransform canvasObject = BuildCanvasObject(goName, true, equipBehavior);
            UISubtitles returnElement = GameObject.Instantiate( UIManager.instance.subtitlesPrefab );
            returnElement.parameters = parameters;
            returnElement.baseObject = canvasObject.gameObject;
            
            RectTransform newPageRect = returnElement.GetComponent<RectTransform>();
            SetAnchor(newPageRect, new Vector2(.5f, .5f));
            SetParent(newPageRect, canvasObject);
            return returnElement;
        }



        public static UIElementHolder MakeButtonsPage (
            string goName, 
            UIRadialParameters radialParameters, UIPageParameters pageParameters, TextPanelParameters textPanelParams, 
            bool addVRMenuBehavior, TransformBehavior equipBehavior, float textScale
        ) {
            // build a seperate worldspace canvas for each ui element
            RectTransform canvasObject = BuildCanvasObject(goName, addVRMenuBehavior, equipBehavior);
            UIElementHolder returnElement = null;
            if (radialParameters != null) {
                
                UIRadial newRadial = GameObject.Instantiate( UIManager.instance.radialPrefab );
                newRadial.parameters = radialParameters;
                
                RectTransform newPageRect = newRadial.GetComponent<RectTransform>();
                SetAnchor(newPageRect, new Vector2(.5f, .5f));
                SetParent(newPageRect, canvasObject);

                returnElement = newRadial;

            }
            else {

                UIPage newPage = GameObject.Instantiate( UIManager.instance.pagePrefab );
                newPage.parameters = pageParameters;
                
                RectTransform newPageRect = newPage.GetComponent<RectTransform>();

                if (textPanelParams != null) {

                    RectTransform childRect = new GameObject(pageParameters.pageTitleText+"_holder").AddComponent<RectTransform>();
                    childRect.sizeDelta = Vector2.one;
                    SetAnchor(childRect, new Vector2(.5f, 1f));
                    SetParent(childRect, canvasObject);
                
    
                    SetAnchor(newPageRect, new Vector2(.5f, 1f));
                    SetParent(newPageRect, childRect, new Vector3(-(pageParameters.width*.5f), 0, 0));
        
                    UITextPanel textPanel = GameObject.Instantiate( UIManager.instance.textPanelPrefab );
                    RectTransform newPanelRect = textPanel.GetComponent<RectTransform>();
                    SetAnchor(newPanelRect, new Vector2(.5f, 1f));
                    SetParent(newPanelRect, childRect, new Vector3((pageParameters.width*.5f), -pageParameters.lineHeight, 0));
    
                    textPanel.parameters = textPanelParams;
                    newPage.textPanel = textPanel;
                }
                else {

                    SetAnchor(newPageRect, new Vector2(.5f, 1f));
                    SetParent(newPageRect, canvasObject);
                    
                }
                returnElement = newPage;
            }

            returnElement.textScale = textScale;
            returnElement.isBase = true;
            returnElement.baseObject = canvasObject.gameObject;
            UIManager.HideUI(returnElement);
            return returnElement;

        }


        public static UIElementHolder MakeFullTradeUI (string goName, UIPageParameters pageParameters, TransformBehavior equipBehavior, float textScale) {
            // build a seperate worldspace canvas for each ui element
            RectTransform canvasObject = BuildCanvasObject(goName, true, equipBehavior);

            RectTransform childRect = new GameObject(goName+"_holder").AddComponent<RectTransform>();
            SetAnchor(childRect, new Vector2(.5f, .5f));
            SetParent(childRect, canvasObject);
                    
            
            ElementHolderCollection collection = childRect.gameObject.AddComponent<ElementHolderCollection>();
            collection.textScale = textScale;
            collection.isBase = true;

            collection.baseObject = canvasObject.gameObject;
            RectTransform collectionT = collection.GetComponent<RectTransform>();

            UIPage newPage0 = GameObject.Instantiate( UIManager.instance.pagePrefab );
            newPage0.textScale = textScale;
            newPage0.isBase = true;
            newPage0.parameters = pageParameters;

                    
            SetAnchor(newPage0.GetComponent<RectTransform>(), new Vector2(.5f, 1f));
            SetParent(newPage0.GetComponent<RectTransform>(), collectionT, new Vector3(-(pageParameters.width*.5f), 0, 0));

            UIPage newPage1 = GameObject.Instantiate( UIManager.instance.pagePrefab );
            newPage1.textScale = textScale;
            newPage1.isBase = true;
            newPage1.parameters = pageParameters;

                    
            SetAnchor(newPage1.GetComponent<RectTransform>(), new Vector2(.5f, 1f));
            SetParent(newPage1.GetComponent<RectTransform>(), collectionT, new Vector3((pageParameters.width*.5f), 0, 0));

            collection.subHolders = new UIElementHolder[] { newPage0 , newPage1 };

            UIManager.HideUI(collection);
            return collection;
        }  
    }
}
