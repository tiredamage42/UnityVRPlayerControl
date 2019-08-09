using UnityEngine;

using InventorySystem;
using Valve.VR;
using GameUI;

// namespace VRPlayer.UI 
// {
//     public class VRQuickTradeUI : VRInventoryManagementUIInputHandler//<QuickTradeUIHandler>
//     {
//         public SteamVR_Action_Boolean singleTradeAction, tradeAllAction, switchToFullAction;
        
//         protected override Vector2Int GetUIInputs (int equipID, SteamVR_Input_Sources hand) {
            
//             if (singleTradeAction.GetStateDown(hand)) return new Vector2Int(QuickTradeUIHandler.singleTradeAction, equipID);
//             if (tradeAllAction.GetStateDown(hand)) return new Vector2Int(QuickTradeUIHandler.tradeAllAction, equipID);
//             if (switchToFullAction.GetStateDown(hand)) return new Vector2Int(QuickTradeUIHandler.switchToFullTradeAction, equipID);
//             return new Vector2Int(-1, equipID);
//         }
//         protected override string HandlingContext () { return "QuickTrade"; }
        

//         protected override SteamVR_Action[] GetMainUIActions () { return new SteamVR_Action[] { singleTradeAction, tradeAllAction, switchToFullAction }; }
//         protected override string[] GetMainUIActionHints () { return new string[] { "Take Item", "Take All", "Open Trade" }; }
//     }
// }