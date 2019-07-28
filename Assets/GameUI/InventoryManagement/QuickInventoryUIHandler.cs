using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using SimpleUI;
using InventorySystem;

namespace GameUI {
    public class QuickInventoryUIHandler : InventoryManagementUIHandler
    {


        protected override string GetInventoryManagementContext() {
            return "QuickInventory";
        }



        [HideInInspector] public int usingEquipPoint=-1;


        protected override bool ShouldOpenUI (Inventory inventory, int usingEquipPoint, Inventory otherInventory) {
            return this.usingEquipPoint < 0 && this.usingEquipPoint != usingEquipPoint;
        }

        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory) {
            this.usingEquipPoint = usingEquipPoint;
            if (!CheckForGetInputCallback()) return;
            InitializeCallbacksForUIs (OnQuickInventorySubmit);        
            if (inventoryButtonsPerInventory[0] == null) inventoryButtonsPerInventory[0] = mainUIElement.GetAllElements(maxButtons);
            UpdateUIButtons(false, inventory, null, 0, 0);   
            // BroadcastUIOpen();
        }
            
        
        protected override void OnEndInventoryManagement(Inventory inventory) {
            // on close
            usingEquipPoint = - 1;
        }

        
        protected override bool UsesRadial() {
            return true;
        }



        


    // public event System.Action<UIType, UIElementHolder> onUIOpen, onUIClose;

    // Inventory currentLinkedInventory;


    
    

        
        // void BeginClose (UIType type, UIElementHolder uiHolder) {
        //     UIManager.HideUI(uiHolder);
            
        //     for (int i = 0; i < inventoryButtonsPerInventory.Length; i++)
        //         inventoryButtonsPerInventory[i] = null;
        //     for (int i = 0; i < currentPaginatedOffsets.Length; i++)
        //         currentPaginatedOffsets[i] = 0;

        //     BroadcastUIClose(type);
        //     currentLinkedInventory = null;
        // }

       

    // void OnGamePaused(bool isPaused, float routineTime) {
    //     if (isPaused) {
    //         CloseAllUIs();
    //     }
    // }

        // void CloseAllUIs () {
        //     for (int i = 0; i < uiTypesCount; i++) {
        //         UIType t = (UIType)i;
        //         if (IsOpen(t)) {
        //             Type2Close(t) ();
        //         }
        //     }
        // }

        
        // public void CloseQuickInventoryUI () {
        //     BeginClose(UIType.QuickInventory, quickInventory);
        //     QI_EquipPointID = - 1;
        // }


    // void BroadcastUIOpen(UIType type) {
    //     if (onUIOpen != null) {
    //         onUIOpen (type, Type2UI(type));
    //     }
    // }
    // void BroadcastUIClose(UIType type) {
    //     if (onUIClose != null) {
    //         onUIClose (type, Type2UI(type));
    //     }
    // }

    // void Awake () {
    //     if (inventory == null) {
    //         inventory = GetComponent<Inventory>();
    //     }
    // }

    // public int quickTradeInteractorID=-1;

    

        // void OnEnable () {
        
        //     GameManager.onPauseRoutineStart += OnGamePaused;    

        //     for (int i = 0; i < uiTypesCount; i++) {
        //         Type2UI((UIType)i).onBaseCancel += Type2Close((UIType)i);
        //     }        
            
            
            
            
        //     for (int i = 0; i < uiTypesCount; i++) {
        //        UIManager.HideUI( Type2UI((UIType)i) );
        //     }
        // }


        // void OnDisable () {
        //     GameManager.onPauseRoutineStart -= OnGamePaused;

        //     for (int i = 0; i < uiTypesCount; i++) {
        //         Type2UI((UIType)i).onBaseCancel -= Type2Close((UIType)i);
        //     }        
            
        //     inventory.onStash -= OnStash;
        //     inventory.onDrop -= OnDrop;
		// 	inventory.onEquip -= OnEquip;
        //     inventory.onUnequip -= OnUnequip;

        //     inventory.onTradeStart -= OnTradeStart;
        //     inventory.onQuickTradeStart -= OnQuickTradeStart;
        //     inventory.onQuickTradeEnd -= OnQuickTradeEnd;
        // }

        
        // void OnStash (Inventory inventory, ItemBehavior item, int count) {
        //     // UIManager.ShowGameMessage("Stashed " + item.itemName + " ( x" + count+" )", 0);
        //     GetComponent<GameMessageInbox>().ShowMessage("Stashed " + item.itemName + " ( x" + count+" )");//, 0);
        // }
        // void OnDrop (Inventory inventory, ItemBehavior item, int count) {
        //     // UIManager.ShowGameMessage("Dropped " + item.itemName + " ( x" + count+" )", 0);
        //     GetComponent<GameMessageInbox>().ShowMessage("Dropped " + item.itemName + " ( x" + count+" )");//, 0);
        // }
        // void OnEquip (Inventory inventory, Item item, int slot, bool quickEquip) {
        //     // UIManager.ShowGameMessage("Equipped " + item.itemBehavior.itemName + " to slot " + slot + (quickEquip ? "*quick*" : ""), 0);
        // }
        // void OnUnequip (Inventory inventory, Item item, int slot, bool quickEquip) {
        //     // UIManager.ShowGameMessage("Unequipped " + item.itemBehavior.itemName + " from slot " + slot + (quickEquip ? "*quick*" : ""), 0);
        // }

        
        
        public const int QUICK_INVENTORY_CONSUME_ACTION = 0;
        // public const int FULL_INVENTORY_CONSUME_ACTION = 0;
        // public const int FULL_INVENTORY_DROP_ACTION = 1;
        // public const int FULL_INVENTORY_FAVORITE_ACTION = 2;
        
        // public const int QUICK_TRADE_SINGLE_TRADE_ACTION = 0;
        // public const int QUICK_TRADE_TRADE_ALL_ACTION = 1;
        // public const int QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION = 2;
        
        // public const int FULL_TRADE_SINGLE_TRADE_ACTION = 0;
        // public const int FULL_TRADE_TRADE_ALL_ACTION = 1;
        // public const int FULL_TRADE_CONSUME_ACTION = 2;
        

        void OnQuickInventorySubmit (GameObject[] data, object[] customData, Vector2Int submitType) {
			if (customData != null) {
                ItemBehavior item = customData[0] as ItemBehavior;
                if (item) {
                    int count = 1;
                    // item.OnConsume((Inventory)customData[1], GetWorkingInventorySlot(UIType.QuickInventory));
                    item.OnConsume((Inventory)customData[1], count, submitType.y);
                }
            }

            OnEndInventoryManagementInternal();
		}

        
       
    




        
        
        
        
   

    }
}
