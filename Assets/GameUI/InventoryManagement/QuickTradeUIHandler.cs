using System.Collections.Generic;
using UnityEngine;

using Game.InventorySystem;

namespace Game.UI {

    public class QuickTradeUIHandler : UISelectableElementHandler
    {
        Inventory showingInventory, takingInventory;

        static QuickTradeUIHandler _instance;
        public static QuickTradeUIHandler instance {
            get {
                if (_instance == null) _instance = GameObject.FindObjectOfType<QuickTradeUIHandler>();
                return _instance;
            }
        }
        public void OpenQuickTradeUI (int interactorID, Inventory shownInventory, Inventory taker) {
            OpenUI( interactorID, new object[] { shownInventory, taker } );
        }

        // dont allow cold open
        protected override object[] GetDefaultColdOpenParams () { return null; }
            
        protected override string GetDisplayForButtonObject(object buttonObject) {
            InventorySlot slot = buttonObject as InventorySlot;
            return slot.item.itemName + " ( x"+slot.count+" )";
        }

        protected override void OnUISelect (GameObject selectedObject, GameObject[] data, object[] customData) { 

        }
        
        protected override List<object> BuildButtonObjectsListForDisplay(int panelIndex){
            return ToObjectList(showingInventory.allInventory);
        }

        protected override void OnOpenUI() {
            showingInventory = openedWithParams[0] as Inventory;
            takingInventory = openedWithParams[1] as Inventory;

            BuildButtons(showingInventory.GetDisplayName(), true, 0);
        }


        // protected override bool RequiresCustomInputMethod () { return true; }

        protected override List<int> InitializeInputsAndNames (out List<string> names) {
            names = new List<string>() { "Take", "Take All", "Trade" };
            return new List<int>() { singleTradeAction, tradeAllAction, switchToFullTradeAction };
        }

        public int singleTradeAction = 0, tradeAllAction = 1, switchToFullTradeAction = 2;
        protected override void OnUIInput (GameObject selectedObject, GameObject[] data, object[] customData, Vector2Int input){//, int actionOffset) {
            bool updateButtons = false;

            
            // single trade
            if (input.x == singleTradeAction){//+actionOffset) {
                if (customData != null) {
                    InventorySlot item = customData[0] as InventorySlot;
                    if (item != null) {
                        showingInventory.TransferItemAlreadyInInventoryTo(item, item.count, takingInventory, sendMessage: false);
                        updateButtons = true;
                    }
                }
            }
            // take all
            else if (input.x == tradeAllAction){//+actionOffset) {
                // TODO: check if shown inventory has anything
                showingInventory.TransferInventoryContentsTo(takingInventory, sendMessage: false);
                updateButtons = true;
            }
            else if (input.x == switchToFullTradeAction){//+actionOffset) {
                CloseUI();
                StartCoroutine(OpenFullTrade());
            }
            
            if (updateButtons){
                UpdateUIButtons();
            }
		}

        System.Collections.IEnumerator OpenFullTrade () {
            yield return null;
            GameUI.tradeUI.OpenTradUI(takingInventory, showingInventory, null);
        }
    }
}