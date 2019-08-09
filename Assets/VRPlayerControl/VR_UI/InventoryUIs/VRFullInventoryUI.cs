using UnityEngine;

using Valve.VR;
using GameUI;

namespace VRPlayer.UI 
{

    // public class VRFullInventoryUI : VRInventoryManagementUIInputHandler//<FullInventoryUIHandler>
    // {

    //     // public SteamVR_Action_Boolean consumeAction, dropAction, favoriteAction;
        
    //     [Space] public SteamVR_Action_Boolean uiInvToggleAction;
        
    //     // protected override SteamVR_Action[] GetMainUIActions () { return new SteamVR_Action[] { consumeAction, dropAction, favoriteAction }; }
    //     // protected override string[] GetMainUIActionHints () { return new string[] { "Use Item", "Drop", "Favorite" }; }
    //     // protected override string HandlingContext () { return "FullInventory"; }
                

    //     // protected override Vector2Int GetUIInputs (int equipID, SteamVR_Input_Sources hand) {
                
    //     //     if (dropAction.GetStateDown(hand)) return new Vector2Int(FullInventoryUIHandler.dropAction, equipID);
    //     //     if (favoriteAction.GetStateDown(hand)) return new Vector2Int(FullInventoryUIHandler.favoriteAction, equipID);

    //     //     StandardizedVRInput.ButtonState[] buttonStates;
    //     //     StandardizedVRInput.instance.GetInputActionInfo(consumeAction, out buttonStates);
    //     //     for (int i =0 ; i < buttonStates.Length; i++) {
    //     //         if (buttonStates[i] == StandardizedVRInput.ButtonState.Down) {
    //     //             return new Vector2Int(FullInventoryUIHandler.consumeAction, i);
    //     //         }
    //     //     }
                    
    //     //     return new Vector2Int(-1, 1);
    //     // }   

    //     // open full inventory logic
    //     void Update()
    //     {            
    //         UpdateNormalInventory();
    //     }
    //     void UpdateNormalInventory () {
    //         // if (GameManager.isPaused)
    //         //     return;
    //         // if (inventoryUI.IsOpen(InventoryUI.UIType.FullTrade))
    //         //     return;
            
    //         if (uiInvToggleAction.GetStateDown(VRManager.instance.mainHand)) {
    //             if (myUIHandler.UIObjectActive())
    //                 myUIHandler.CloseUI();
    //             else
    //                 myUIHandler.inventory.InitiateInventoryManagement (myUIHandler.ContextKey(), 0, null);
                    
    //         }
    //     } 
    // }
}