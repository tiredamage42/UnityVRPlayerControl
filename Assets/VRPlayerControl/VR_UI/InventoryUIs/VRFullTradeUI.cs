using UnityEngine;

using Valve.VR;
using GameUI;

using InventorySystem;

// namespace VRPlayer.UI 
// {
//     public class VRFullTradeUI : VRInventoryManagementUIInputHandler//<FullTradeUIHandler>
//     {
//         public SteamVR_Action_Boolean singleTradeAction, tradeAllAction, consumeAction;
        
//         protected override SteamVR_Action[] GetMainUIActions () { return new SteamVR_Action[] { singleTradeAction, tradeAllAction, consumeAction }; }
//         protected override string[] GetMainUIActionHints () { return new string[] { "Trade Item", "Trade All", "Use Item" }; }
//         protected override string HandlingContext () { return Inventory.fullTradeContext; }
        
//         protected override Vector2Int GetUIInputs (int equipID, SteamVR_Input_Sources hand) {
            
//             if (singleTradeAction.GetStateDown(hand)) return new Vector2Int(FullTradeUIHandler.singleTradeAction, equipID);
//             if (tradeAllAction.GetStateDown(hand)) return new Vector2Int(FullTradeUIHandler.tradeAllAction, equipID);

//             StandardizedVRInput.ButtonState[] buttonStates;
//             StandardizedVRInput.instance.GetInputActionInfo(consumeAction, out buttonStates);
//             for (int i =0 ; i < buttonStates.Length; i++) {
//                 if (buttonStates[i] == StandardizedVRInput.ButtonState.Down) {
//                     return new Vector2Int(FullTradeUIHandler.consumeAction, i);
//                 }
//             }
                    
//             return new Vector2Int(-1, equipID);        
//         }       
//     }
// }
