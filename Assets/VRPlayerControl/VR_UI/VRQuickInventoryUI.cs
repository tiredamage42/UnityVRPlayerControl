using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;
using GameUI;
using SimpleUI;

namespace VRPlayer.UI 
{


    public abstract class InventoryManagementUIHandlerInputHandler<T> : MonoBehaviour where T : InventoryManagementUIHandler {
        protected T myUIHandler;
        protected abstract string ContextKey ();


        // bool subscribed;

        // protected virtual void Start () {
        //     // myUIHandler = (T)InventoryManagementUIHandler.GetHandler(ContextKey());
        //     myUIHandler.SetUIInputCallback(_GetUISubmits);

            
        //     // subscribed = true;

        // }
        System.Func<Vector2Int> _GetUISubmits () {
            return GetUISubmits;
        }

        protected abstract Vector2Int GetUISubmits();

        protected virtual void Start () {
            myUIHandler = (T)InventoryManagementUIHandler.GetHandler(ContextKey());
            myUIHandler.SetUIInputCallback(_GetUISubmits);
            
            myUIHandler.onUIClose += OnUIClose;
            myUIHandler.onUIOpen += OnUIOpen;
        }
        
        protected virtual void OnEnable () {
            // myUIHandler = (T)InventoryManagementUIHandler.GetHandler(ContextKey());
            
            // myUIHandler.onUIClose += OnUIClose;
            // myUIHandler.onUIOpen += OnUIOpen;
        }
        protected virtual void OnDisable () {
            
            myUIHandler.onUIClose -= OnUIClose;
            myUIHandler.onUIOpen -= OnUIOpen;
            
        }


        protected abstract void OnUIClose (UIElementHolder uiObject);
        protected abstract void OnUIOpen(UIElementHolder uiObject);

    }

            

    public class VRQuickInventoryUI : InventoryManagementUIHandlerInputHandler<QuickInventoryUIHandler>
    {
        protected override string ContextKey () 
        {
            return "QuickInventory";
        }
        protected override void OnUIClose (UIElementHolder uiObject) {
            StandardizedVRInput.MarkActionUnoccupied(uiConsumeAction);
            StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, uiConsumeAction);
            

        }
        protected override void OnUIOpen(UIElementHolder uiObject) {
            SteamVR_Input_Sources hand;
            hand = VRManager.Int2Hand( myUIHandler.usingEquipPoint );

            StandardizedVRInput.MarkActionOccupied(uiConsumeAction, VRUIInput.GetUIHand());
            StandardizedVRInput.instance.ShowHint(hand, uiConsumeAction, "Use");    

            TransformBehavior.AdjustTransform(uiObject.baseObject.transform, Player.instance.GetHand(hand).transform, quickInventoryEquip, 0);
            VRUIInput.SetUIHand(hand);
            

        }



        void Update()
        {
            
            UpdateQuickInventory();
        }




        bool CheckHandForWristRadialOpen (SteamVR_Input_Sources hand) {
			if (uiToggleAction.GetStateDown(hand)) {
                // Debug.LogError("YO");

                myUIHandler.linkedInventory.InitiateInventoryManagement (ContextKey(), VRManager.Hand2Int(hand), null);

				// inventoryUI.OpenQuickInventoryUI(VRManager.Hand2Int(hand));
                
                // VRUIInput.SetUIHand(hand);
                // TransformBehavior.AdjustTransform(inventoryUI.quickInventory.baseObject.transform, Player.instance.GetHand(hand).transform, quickInventoryEquip, 0);

				return true;
			}
			return false;
		}
        void UpdateQuickInventory () {

            if (UIManager.AnyUIOpen())
                return;
            
            // if (GameManager.isPaused) 
            //     return;
            
            // if (inventoryUI.IsOpen(InventoryUI.UIType.FullInventory)) 
            //     return;
            // if (inventoryUI.IsOpen(InventoryUI.UIType.FullTrade)) 
            //     return;
            // if (inventoryUI.IsOpen(InventoryUI.UIType.QuickInventory)) 
            //     return;
            
			if (Player.instance.handsTogether) {
                

                StandardizedVRInput.MarkActionOccupied(uiToggleAction, SteamVR_Input_Sources.Any);

                if (!CheckHandForWristRadialOpen(SteamVR_Input_Sources.LeftHand)) {
                    CheckHandForWristRadialOpen(SteamVR_Input_Sources.RightHand);
                }
            }
            else {
                StandardizedVRInput.MarkActionUnoccupied(uiToggleAction);
            }   
        } 
     

        public SteamVR_Action_Boolean uiToggleAction;
        public SteamVR_Action_Boolean uiConsumeAction;


        
        

        protected override Vector2Int GetUISubmits () {
                    
            if (uiConsumeAction.GetStateDown(VRManager.Int2Hand( myUIHandler.usingEquipPoint ))) {
                return new Vector2Int(QuickInventoryUIHandler.QUICK_INVENTORY_CONSUME_ACTION, myUIHandler.usingEquipPoint);//VRManager.Hand2Int(hand));
            }
            
            return new Vector2Int(-1, 1);
        }
        
        // System.Func<Vector2Int> GetAlternativeSubmits () {
        //     return GetAlternativeSubmitsQI;
        // }
            



        public TransformBehavior quickInventoryEquip;
     
    }
}

