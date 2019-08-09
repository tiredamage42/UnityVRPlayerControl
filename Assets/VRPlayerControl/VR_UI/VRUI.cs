using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using InventorySystem;
using SimpleUI;
using Valve.VR;
using GameUI;
using GameBase;


/*

    interactable:
        lootable (for ammo chests, tradeable inventories, etc...)
            linked_inventory
        

        inventoryManager: (eg: crafting table)
            used category = -1 (negative values in item categories are hidden)
            on use: displays inventory ui


*/


namespace VRPlayer {

    public class VRUI : MonoBehaviour
    {
        // void Start()
        // {
            // SetUpMessageCenter();
        // }

        // void Update()
        // {
            
            // SetUpMessageCenter();

            // UpdateQuickInventory();
            // UpdateNormalInventory();
        // }

        InventoryUI inventoryUI;
        Inventory inventory;



        void OnEnable () {
        
            GameManager.onPauseRoutineStart += OnGamePaused;
            
            UIManager.onUISelect += OnUISelection;
            // UIManager.onShowGameMessage += OnShowGameMessage;
            inventory = Player.instance.GetComponent<Inventory>();

            inventoryUI = Player.instance.GetComponent<InventoryUI>();
            // inventoryUI.onUIClose += OnInventoryUIClose;
            // inventoryUI.onUIOpen += OnInventoryUIOpen;

            // inventoryUI.SetAlternateSubmitCallback(GetAlternativeSubmits);
            
        }
            
            
        void OnDisable () {
            GameManager.onPauseRoutineStart -= OnGamePaused;

            UIManager.onUISelect -= OnUISelection;
            // UIManager.onShowGameMessage -= OnShowGameMessage;

        }



        // bool CheckHandForWristRadialOpen (SteamVR_Input_Sources hand) {
		// 	if (quickInventoryToggle.GetStateDown(hand)) {
        //         // Debug.LogError("YO");
		// 		inventoryUI.OpenQuickInventoryUI(VRManager.Hand2Int(hand));
                
        //         // VRUIInput.SetUIHand(hand);
        //         // TransformBehavior.AdjustTransform(inventoryUI.quickInventory.baseObject.transform, Player.instance.GetHand(hand).transform, quickInventoryEquip, 0);

		// 		return true;
		// 	}
		// 	return false;
		// }
        // void UpdateQuickInventory () {
        //     if (GameManager.isPaused) 
        //         return;
            
        //     if (inventoryUI.IsOpen(InventoryUI.UIType.FullInventory)) 
        //         return;
        //     if (inventoryUI.IsOpen(InventoryUI.UIType.FullTrade)) 
        //         return;
        //     if (inventoryUI.IsOpen(InventoryUI.UIType.QuickInventory)) 
        //         return;
            
		// 	if (Player.instance.handsTogether) {

        //         StandardizedVRInput.MarkActionOccupied(quickInventoryToggle, SteamVR_Input_Sources.Any);

        //         if (!CheckHandForWristRadialOpen(SteamVR_Input_Sources.LeftHand)) {
        //             CheckHandForWristRadialOpen(SteamVR_Input_Sources.RightHand);
        //         }
        //     }
        //     else {
        //         StandardizedVRInput.MarkActionUnoccupied(quickInventoryToggle);
        //     }   
        // } 
     

        // [Header("Main Menu")]
        // public SteamVR_Action_Boolean mainMenuToggle;
        
        // [Space]

        // [Header("Inventory")]
        // public SteamVR_Action_Boolean quickInventoryToggle, fullInventoryToggle;
        // public SteamVR_Action_Boolean fullInventoryToggle;
        
        // public SteamVR_Input_Sources inventoryToggleHand = SteamVR_Input_Sources.RightHand;
        
        // [Space]
        // [Header("UI ACTIONS")]
        // [Space]
        // [Header("Quick Inventory")]
        // public SteamVR_Action_Boolean QUICK_INVENTORY_CONSUME_ACTION;
        
        // [Header("Full Inventory")]
        // public SteamVR_Action_Boolean FULL_INVENTORY_CONSUME_ACTION;
        // public SteamVR_Action_Boolean FULL_INVENTORY_DROP_ACTION;
        // public SteamVR_Action_Boolean FULL_INVENTORY_FAVORITE_ACTION;
        
        // [Header("Quick Trade")]
        // public SteamVR_Action_Boolean QUICK_TRADE_SINGLE_TRADE_ACTION;
        // public SteamVR_Action_Boolean QUICK_TRADE_TRADE_ALL_ACTION;
        // public SteamVR_Action_Boolean QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION;
        
        // [Header("Full Trade")]
        // public SteamVR_Action_Boolean FULL_TRADE_SINGLE_TRADE_ACTION;
        // public SteamVR_Action_Boolean FULL_TRADE_TRADE_ALL_ACTION;
        // public SteamVR_Action_Boolean FULL_TRADE_CONSUME_ACTION;
        


        // Vector2Int GetAlternativeSubmitsQI () {
                    
        //     if (QUICK_INVENTORY_CONSUME_ACTION.GetStateDown(VRManager.Int2Hand( inventoryUI.QI_EquipPointID ))) {
        //         return new Vector2Int(InventoryUI.QUICK_INVENTORY_CONSUME_ACTION, inventoryUI.QI_EquipPointID);//VRManager.Hand2Int(hand));
        //     }
            
        //     return new Vector2Int(-1, 1);
            
        
        // }
        // Vector2Int GetAlternativeSubmitsFI () {
        //     if (FULL_INVENTORY_DROP_ACTION.GetStateDown(SteamVR_Input_Sources.Any)) {
        //                 // VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
        //                 return new Vector2Int(InventoryUI.FULL_INVENTORY_DROP_ACTION, 0);
        //             }
                    
                    
        //             StandardizedVRInput.ButtonState[] buttonStates;
        //             StandardizedVRInput.instance.GetInputActionInfo(FULL_INVENTORY_CONSUME_ACTION, out buttonStates);
        //             for (int i =0 ; i < buttonStates.Length; i++) {
        //                 if (buttonStates[i] == StandardizedVRInput.ButtonState.Down) {
        //                     return new Vector2Int(InventoryUI.FULL_INVENTORY_CONSUME_ACTION, i);
        //                 }
        //             }
                    
        //     return new Vector2Int(-1, 1);
        
        // }
        // Vector2Int GetAlternativeSubmitsQT () {
            
        //             SteamVR_Input_Sources hand = VRManager.Int2Hand( inventoryUI.quickTradeInteractorID );
        //             if (QUICK_TRADE_SINGLE_TRADE_ACTION.GetStateDown(hand)) {
        //                 return new Vector2Int(InventoryUI.QUICK_TRADE_SINGLE_TRADE_ACTION, inventoryUI.quickTradeInteractorID);
        //             }
        //             if (QUICK_TRADE_TRADE_ALL_ACTION.GetStateDown(hand)) {
        //                 return new Vector2Int(InventoryUI.QUICK_TRADE_TRADE_ALL_ACTION, inventoryUI.quickTradeInteractorID);
        //             }
        //             if (QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION.GetStateDown(hand)) {
        //                 return new Vector2Int(InventoryUI.QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION, inventoryUI.quickTradeInteractorID);
        //             }
                    
        //     return new Vector2Int(-1, 1);
        // }
                    
        
        // Vector2Int GetAlternativeSubmitsFT () {
            
        //             if (FULL_TRADE_SINGLE_TRADE_ACTION.GetStateDown(SteamVR_Input_Sources.Any)) {
        //                 return new Vector2Int(InventoryUI.FULL_TRADE_SINGLE_TRADE_ACTION, 0);
        //             }
        //             if (FULL_TRADE_TRADE_ALL_ACTION.GetStateDown(SteamVR_Input_Sources.Any)) {
        //                 return new Vector2Int(InventoryUI.FULL_TRADE_TRADE_ALL_ACTION, 0);
        //             }

        //             StandardizedVRInput.ButtonState[] buttonStates;
        //             StandardizedVRInput.instance.GetInputActionInfo(FULL_TRADE_CONSUME_ACTION, out buttonStates);
        //             for (int i =0 ; i < buttonStates.Length; i++) {
        //                 if (buttonStates[i] == StandardizedVRInput.ButtonState.Down) {
        //                     return new Vector2Int(InventoryUI.FULL_TRADE_CONSUME_ACTION, i);
        //                 }
        //             }
                    
        //     return new Vector2Int(-1, 1);
        
        // }





        // System.Func<Vector2Int> GetAlternativeSubmits (InventoryUI.UIType type) {
        //     switch (type) {
        //         // quick inventory
        //         // case InventoryUI.UIType.QuickInventory:
        //         //     return GetAlternativeSubmitsQI;
                    
        //         // full inventory
        //         // case InventoryUI.UIType.FullInventory:
        //         // return GetAlternativeSubmitsFI;
                
        //         // quick trade
        //         // case InventoryUI.UIType.QuickTrade:
        //         // return GetAlternativeSubmitsQT;
                    
                
        //         case InventoryUI.UIType.FullTrade: 
        //         return GetAlternativeSubmitsFT;
        //     }
        //     return null;

        // }
       
        // void OnInventoryUIOpen(InventoryUI.UIType type, UIElementHolder uiHolder) {
        //     SteamVR_Input_Sources hand;
        //     switch (type) {
        //         // quick inventory
        //         // case InventoryUI.UIType.QuickInventory:
        //         //     hand = VRManager.Int2Hand( inventoryUI.QI_EquipPointID );

        //         //     StandardizedVRInput.MarkActionOccupied(QUICK_INVENTORY_CONSUME_ACTION, VRUIInput.GetUIHand());
        //         //     StandardizedVRInput.instance.ShowHint(hand, QUICK_INVENTORY_CONSUME_ACTION, "Use");    

        //         //     TransformBehavior.AdjustTransform(uiHolder.baseObject.transform, Player.instance.GetHand(hand).transform, quickInventoryEquip, 0);
        //         //     VRUIInput.SetUIHand(hand);
        //         //     break;
        //         // full inventory
        //         // case InventoryUI.UIType.FullInventory:
        //         //     StandardizedVRInput.MarkActionOccupied(FULL_INVENTORY_CONSUME_ACTION, SteamVR_Input_Sources.Any);
        //         //     StandardizedVRInput.MarkActionOccupied(FULL_INVENTORY_DROP_ACTION, SteamVR_Input_Sources.Any);
        //         //     // StandardizedVRInput.MarkActionOccupied(FULL_INVENTORY_FAVORITE_ACTION, SteamVR_Input_Sources.Any);
                    
        //         //     StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.RightHand, FULL_INVENTORY_CONSUME_ACTION, "Use Right Hand");
        //         //     StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.LeftHand, FULL_INVENTORY_CONSUME_ACTION, "Use Left Hand");
        //         //     StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_DROP_ACTION, "Drop");
        //         //     // StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_FAVORITE_ACTION, "Favorite");
                    
        //         //     VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
        //         //     break;
        //         // quick trade
        //         // case InventoryUI.UIType.QuickTrade:
        //         //     hand = VRManager.Int2Hand( inventoryUI.quickTradeInteractorID );

        //         //     StandardizedVRInput.MarkActionOccupied(QUICK_TRADE_SINGLE_TRADE_ACTION, hand);
        //         //     StandardizedVRInput.MarkActionOccupied(QUICK_TRADE_TRADE_ALL_ACTION, hand);
        //         //     StandardizedVRInput.MarkActionOccupied(QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION, hand);
                    
        //         //     StandardizedVRInput.instance.ShowHint(hand, QUICK_TRADE_SINGLE_TRADE_ACTION, "Take Item");
        //         //     StandardizedVRInput.instance.ShowHint(hand, QUICK_TRADE_TRADE_ALL_ACTION, "Take All");
        //         //     StandardizedVRInput.instance.ShowHint(hand, QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION, "Open Trade");
                    
        //         //     TransformBehavior.AdjustTransform(uiHolder.baseObject.transform, Player.instance.GetHand(hand).transform, quickTradeEquip, 0);
                    
        //         //     VRUIInput.SetUIHand(hand);
                
        //         //     break;
        //         // full trade
        //         // case InventoryUI.UIType.FullTrade: 

        //         //     StandardizedVRInput.MarkActionOccupied(FULL_TRADE_CONSUME_ACTION, SteamVR_Input_Sources.Any);
        //         //     StandardizedVRInput.MarkActionOccupied(FULL_TRADE_TRADE_ALL_ACTION, SteamVR_Input_Sources.Any);
        //         //     StandardizedVRInput.MarkActionOccupied(FULL_TRADE_SINGLE_TRADE_ACTION, SteamVR_Input_Sources.Any);
                    
        //         //     StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.RightHand, FULL_TRADE_CONSUME_ACTION, "Use Right Hand");
        //         //     StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.LeftHand, FULL_TRADE_CONSUME_ACTION, "Use Left Hand");
        //         //     StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_TRADE_SINGLE_TRADE_ACTION, "Trade Item");
        //         //     StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_TRADE_TRADE_ALL_ACTION, "Trade All");
                    
        //         //     VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
        //         //     break;
        //     }
        // }








        // void OnInventoryUIClose(InventoryUI.UIType type, UIElementHolder uiHolder) {
        //     switch (type) {
        //         // quick inventory
        //         // case InventoryUI.UIType.QuickInventory:
        //         //     StandardizedVRInput.MarkActionUnoccupied(QUICK_INVENTORY_CONSUME_ACTION);
        //         //     StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, QUICK_INVENTORY_CONSUME_ACTION);
        //         //     break;
        //         // full inventory
        //         // case InventoryUI.UIType.FullInventory:
        //         //     StandardizedVRInput.MarkActionUnoccupied(FULL_INVENTORY_CONSUME_ACTION);
        //         //     StandardizedVRInput.MarkActionUnoccupied(FULL_INVENTORY_DROP_ACTION);
        //         //     // StandardizedVRInput.MarkActionUnoccupied(FULL_INVENTORY_FAVORITE_ACTION);
                    
        //         //     StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_CONSUME_ACTION);
        //         //     StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_DROP_ACTION);
        //         //     // StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_FAVORITE_ACTION,);
        //         //     break;
        //         // quick trade
        //         // case InventoryUI.UIType.QuickTrade:
        //         //     StandardizedVRInput.MarkActionUnoccupied(QUICK_TRADE_SINGLE_TRADE_ACTION);
        //         //     StandardizedVRInput.MarkActionUnoccupied(QUICK_TRADE_TRADE_ALL_ACTION);
        //         //     StandardizedVRInput.MarkActionUnoccupied(QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION);
                    
        //         //     StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, QUICK_TRADE_SINGLE_TRADE_ACTION);
        //         //     StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, QUICK_TRADE_TRADE_ALL_ACTION);
        //         //     StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION);
        //         //     break;
        //         // full trade
        //         case InventoryUI.UIType.FullTrade: 
        //             StandardizedVRInput.MarkActionUnoccupied(FULL_TRADE_CONSUME_ACTION);
        //             StandardizedVRInput.MarkActionUnoccupied(FULL_TRADE_TRADE_ALL_ACTION);
        //             StandardizedVRInput.MarkActionUnoccupied(FULL_TRADE_SINGLE_TRADE_ACTION);
                    
        //             StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, FULL_TRADE_CONSUME_ACTION);
        //             StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, FULL_TRADE_SINGLE_TRADE_ACTION);
        //             StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, FULL_TRADE_TRADE_ALL_ACTION);
        //             break;
        //     }

        // }

        // void UpdateNormalInventory () {
        //     if (GameManager.isPaused)
        //         return;
        //     if (inventoryUI.IsOpen(InventoryUI.UIType.FullTrade))
        //         return;
            
        //     if (fullInventoryToggle.GetStateDown(inventoryToggleHand)) {
        //         if (inventoryUI.IsOpen(InventoryUI.UIType.FullInventory))
        //             inventoryUI.CloseFullInventoryUI();
        //         else
        //             inventoryUI.OpenFullInventoryUI();
        //     }
        // } 

        // public TransformBehavior quickInventoryEquip, quickTradeEquip;
        // public TransformBehavior quickTradeEquip;



        // public SteamVR_Input_Sources messagesHand = SteamVR_Input_Sources.LeftHand;
        // public TransformBehavior messagesEquip;
        // public UIMessageCenter messageCenter;

        // void SetUpMessageCenter () {
        //     Transform handTransform = Player.instance.GetHand(messagesHand).transform;
        //     TransformBehavior.AdjustTransform(messageCenter.transform, handTransform, messagesEquip, 0);
        // }
        
        void OnUISelection (GameObject[] data, object[] customData) {
            // float duration,  float frequency, float amplitude
            StandardizedVRInput.instance.TriggerHapticPulse( VRUIInput.GetUIHand (), .1f, 1.0f, 1.0f );   
        }
        // void OnShowGameMessage (string message, int key) {            
        //     StandardizedVRInput.instance.TriggerHapticPulse( messagesHand, .1f, 1.0f, 1.0f );   
        // }

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
        // void Update()
        // {            
        //     UpdateQuickInventory();
        // }

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
            SetParent(childRect, canvasObject);
            SetAnchor(childRect, new Vector2(.5f, .5f));
                    
            
            ElementHolderCollection collection = childRect.gameObject.AddComponent<ElementHolderCollection>();
            collection.textScale = textScale;
            collection.isBase = true;

            collection.baseObject = canvasObject.gameObject;
            RectTransform collectionT = collection.GetComponent<RectTransform>();

            UIPage newPage0 = GameObject.Instantiate( UIManager.instance.pagePrefab );
            newPage0.textScale = textScale;
            newPage0.isBase = true;
            newPage0.parameters = pageParameters;

            SetParent(newPage0.GetComponent<RectTransform>(), collectionT, new Vector3(-(pageParameters.width*.5f), 0, 0));

            UIPage newPage1 = GameObject.Instantiate( UIManager.instance.pagePrefab );
            newPage1.textScale = textScale;
            newPage1.isBase = true;
            newPage1.parameters = pageParameters;

            SetParent(newPage1.GetComponent<RectTransform>(), collectionT, new Vector3((pageParameters.width*.5f), 0, 0));

            collection.subHolders = new UIElementHolder[] { newPage0 , newPage1 };

            UIManager.HideUI(collection);
            return collection;
        }













            
    }
}
