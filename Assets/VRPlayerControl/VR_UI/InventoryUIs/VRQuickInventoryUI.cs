using UnityEngine;

using Valve.VR;
using GameUI;
using SimpleUI;

namespace VRPlayer.UI 
{


    /*

    

    */
    
    // public class VRQuickInventoryUI : VRInventoryManagementUIInputHandler//<QuickInventoryUIHandler>
    // {
        // public SteamVR_Action_Boolean consumeAction;
        // [Space] public SteamVR_Action_Boolean uiToggleAction;

        // // protected override SteamVR_Action[] GetMainUIActions () { return new SteamVR_Action[] { consumeAction }; }
        // // protected override string[] GetMainUIActionHints () { return new string[] { "Use Item" }; }
        // // protected override string HandlingContext () { return "QuickInventory"; }
        
        // // protected override Vector2Int GetUIInputs (int equipID, SteamVR_Input_Sources hand) {       
        // //     if (consumeAction.GetStateDown(hand)) return new Vector2Int(QuickInventoryUIHandler.consumeAction, equipID);
        // //     return new Vector2Int(-1, equipID);
        // // }

        // // Opening quick invnetory logic
        // void Update()
        // {            
        //     UpdateQuickInventory();
        // }

        // bool CheckHandForWristRadialOpen (SteamVR_Input_Sources hand) {
		// 	if (uiToggleAction.GetStateDown(hand)) {
        //         myUIHandler.inventory.InitiateInventoryManagement (myUIHandler.ContextKey(), VRManager.Hand2Int(hand), null);
		// 		return true;
		// 	}
		// 	return false;
		// }
        // void UpdateQuickInventory () {

        //     if (UIManager.AnyUIOpen(-1))
        //         return;
            
		// 	if (Player.instance.handsTogether) {
                
        //         StandardizedVRInput.MarkActionOccupied(uiToggleAction, SteamVR_Input_Sources.Any);

        //         if (!CheckHandForWristRadialOpen(SteamVR_Input_Sources.LeftHand)) {
        //             CheckHandForWristRadialOpen(SteamVR_Input_Sources.RightHand);
        //         }
        //     }
        //     else {
        //         StandardizedVRInput.MarkActionUnoccupied(uiToggleAction);
        //     }   
        // }      
    // }
}

