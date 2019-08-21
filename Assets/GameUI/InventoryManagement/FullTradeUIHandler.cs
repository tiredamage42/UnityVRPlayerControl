using System.Collections.Generic;
using UnityEngine;

using Game.InventorySystem;

using SimpleUI;

namespace Game.UI {


    public class FullTradeUIHandler : UISelectableElementHandler
    {
        Inventory[] invs;
        protected override string GetDisplayForButtonObject(object buttonObject) {
            InventorySlot slot = buttonObject as InventorySlot;
            return slot.item.itemName + " ( x"+slot.count+" )";
        }


        static FullTradeUIHandler _instance;
        public static FullTradeUIHandler instance {
            get {
                if (_instance == null) _instance = GameObject.FindObjectOfType<FullTradeUIHandler>();
                return _instance;
            }
        }
        public void OpenTradUI (Inventory inventory0, Inventory inventory1, List<int> categoryFilter) {
            OpenUI(  0, new object[] { inventory0, inventory1, categoryFilter } );
        }

        // no cold open
        protected override object[] GetDefaultColdOpenParams () { return null;}
        
        // TODO: limit equip ID for consume action to 0 when equipping on ai for instance
        protected override void OnUISelect (GameObject selectedObject, GameObject[] data, object[] customData) { }
        
        protected override List<object> BuildButtonObjectsListForDisplay(int panelIndex){
            return ToObjectList(invs[panelIndex].GetFilteredInventory(openedWithParams[2] as List<int>));
        }

        protected override void OnOpenUI() {

            invs = new Inventory[] { (openedWithParams[0] as Inventory), (openedWithParams[1] as Inventory) };
            for (int i = 0; i < 2; i++) {
                BuildButtons(invs[i].GetDisplayName(), i == 0, i);
            }
        }
        

        const int singleTradeAction = 0, tradeAllAction = 1, consumeAction = 2;

        Inventory giver, receiver;
        InventorySlot highlightedItem;
        void OnDropSliderReturnValue(bool used, int value) {
            if (used) {
                if (value > 0) {
                    giver.TransferItemAlreadyInInventoryTo(highlightedItem, value, receiver, sendMessage: false);
                    UpdateUIButtons( );
                }
            }       
        }

        protected override void OnUIInput (GameObject selectedObject, GameObject[] data, object[] customData, Vector2Int input, int actionOffset) {
    
        	if (customData != null) {
                highlightedItem = customData[0] as InventorySlot;
                    
                bool updateButtons = false;
                int panelIndex = (int)customData[1];

                giver = invs[panelIndex];
                receiver = invs[1-panelIndex];

                // single trade
                if (input.x == singleTradeAction+actionOffset) {
                    if (highlightedItem != null) {

                            Debug.Log("sinle Trade in full trade handler!!!!");
                        // get count from slider
                        if (highlightedItem.count > 5) {
                            UIManager.ShowIntSliderPopup(true, "Trade " + highlightedItem.item.itemName + ":", 0, highlightedItem.count, OnDropSliderReturnValue);
                        }
                        else {
                            giver.TransferItemAlreadyInInventoryTo(highlightedItem, 1, receiver, sendMessage: false);
                            updateButtons = true;
                        }
                    }
                }
                // take all
                else if (input.x == tradeAllAction+actionOffset) {   
                    //TODO: check if any actually transfeerrrd
                    giver.TransferInventoryContentsTo(receiver, sendMessage: false);
                    updateButtons = true;
                }

                //consume on selected inventory (limit to other inventory)
                else if (input.x == consumeAction+actionOffset) {
                    if (highlightedItem != null) {
                        if (highlightedItem.item.OnConsume(giver, count: 1, input.y)){
                            updateButtons = true;
                        }
                    }
                }
                
                if (updateButtons){
                    UpdateUIButtons( );
                }
            }
		}    
    }
}