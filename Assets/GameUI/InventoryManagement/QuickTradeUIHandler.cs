using System.Collections.Generic;
using UnityEngine;

using Game.InventorySystem;

namespace Game.GameUI {
    public class QuickTradeUIHandler : InventoryManagementUIHandler
    {
        public FullTradeUIHandler fullTradeUIHandler;
        protected override void OnUISelect (GameObject[] data, object[] customData) { }
        protected override List<InventorySlot> BuildInventorySlotsForDisplay (int uiIndex, Inventory shownInventory, List<int> categoryFilter) {
            return shownInventory.allInventory;
        }
        protected override void OnOpenInventoryUI(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            BuildButtons(otherInventory.GetDisplayName(), true, 0, new object[] { otherInventory, inventory, categoryFilter });
        }

        const int singleTradeAction = 0, tradeAllAction = 1, switchToFullTradeAction = 2;
        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input, int actionOffset) {
    		if (customData != null) {
                bool updateButtons = false;

                object[] updateButtonsParameters = customData[2] as object[];
                Inventory shownInventory = updateButtonsParameters[0] as Inventory;
                Inventory taker = updateButtonsParameters[1] as Inventory;
                
                // single trade
                if (input.x == singleTradeAction+actionOffset) {
                    InventorySlot item = customData[0] as InventorySlot;
                    if (item != null) {
                        shownInventory.TransferItemAlreadyInInventoryTo(item, 1, taker, sendMessage: false);
                        updateButtons = true;

                    }
                }
                // take all
                else if (input.x == tradeAllAction+actionOffset) {

                    // TODO: check if shown inventory has anything
                    shownInventory.TransferInventoryContentsTo(taker, sendMessage: false);
                    updateButtons = true;
                }
                // else if (input.x == switchToFullTradeAction+actionOffset) {
                //     CloseUI();
                //     fullTradeUIHandler.OpenUI(new object[] { taker, input.y, shownInventory, null });
                // }
                if (updateButtons){
                    UpdateUIButtons(0, updateButtonsParameters);
                }

                if (input.x == switchToFullTradeAction+actionOffset) {
                    Debug.LogError("closing");
                    CloseUI();

                    Debug.Log("opening ull trade");

                    // fullTradeUIHandler.OpenUI(new object[] { taker, input.y, shownInventory, null });
                    StartCoroutine(OpenFullTrade(new object[] { taker, input.y, shownInventory, null }));
                }
            }
		}

        System.Collections.IEnumerator OpenFullTrade (object[] parameters) {
            yield return null;
            yield return null;
            fullTradeUIHandler.OpenUI(parameters);

        }
    }
}