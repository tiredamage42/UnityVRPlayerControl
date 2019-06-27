using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// using VRPlayer;

using GameBase;
using SimpleUI;
using InventorySystem;
using InteractionSystem;

namespace GameUI {


public class InventoryUI : MonoBehaviour
{
    public enum UIType {
        QuickInventory = 0,
        FullInventory = 1,
        QuickTrade = 2,
        FullTrade = 3,
    };

    const int uiTypesCount = 4;

    public Inventory inventory;
    public UIElementHolder quickInventory, fullInventory;
    
    public UIElementHolder quickTrade, fullTrade;

    public int maxFullInventoryButtons = 16;
    public int maxQuickTradeButtons = 8;

    public bool IsOpen (UIType type) {
        return Type2UI(type).gameObject.activeInHierarchy;
    }

    // public bool quickInventoryOpen { get { return quickInventory.gameObject.activeInHierarchy; } }
    // public bool fullInventoryOpen { get { return fullInventory.gameObject.activeInHierarchy; } }

    public event System.Action<UIType, UIElementHolder> onUIOpen, onUIClose;

    // Inventory primaryUIInventory, secondaryUIInventory;

    // System.Func<int> getWorkingInventorySlot;

    const int maxInventories = 2; // need 2 for trade, cant think of a situation for any more

    SelectableElement[][] inventoryButtonsPerInventory = new SelectableElement[maxInventories][];
    //  {
    //     null, null
    // };
    int[] currentPaginatedOffsets = new int[maxInventories];
    // int currentPaginatedOffset;
    // Inventory[] workingInventories = new Inventory[2];
    // public int maxPerPageInventory = 16;

    // int GetWorkingInventorySlot (UIType type) {
    //     if (callbackToGetWorkingInventorySlot != null) {
    //         return callbackToGetWorkingInventorySlot(type);
    //         // System.Func<int> getWorkingInventorySlot = callbackToGetWorkingInventorySlot(type);
    //         // if (getWorkingInventorySlot != null) {
    //         //     return getWorkingInventorySlot();
    //         // }
    //     }
    //     Debug.LogError(type.ToString() + " wasnt supplied callback to get working inventory slot, defaulting to -1");
    //     return -1;
    // }

    Inventory currentLinkedInventory;


    System.Func<UIType, System.Func<Vector2Int>> callbackToGetAlternativeSubmit;
    public void SetAlternateSubmitCallback (System.Func<UIType, System.Func<Vector2Int>> callback) {
        callbackToGetAlternativeSubmit = callback;
    }
    // System.Func<UIType, int> callbackToGetWorkingInventorySlot;
    // public void SetGetWorkingInventorySlotCallback (System.Func<UIType, int> callback) {
    //     callbackToGetWorkingInventorySlot = callback;
    // }

    bool CheckForAlternateCallback (UIType type) {
        if (callbackToGetAlternativeSubmit == null) {
            Debug.LogError("cant open " + type.ToString() + " UI, no callbackToGetAlternativeSubmit supplied");
            return false;
        }
        return true;
    }
    
    void InitializeCallbacksForUIs (bool isRadial, UIType type, System.Action<GameObject[], object[], Vector2Int> onSubmit) {
        for (int i = 0; i < currentPaginatedOffsets.Length; i++) currentPaginatedOffsets[i] = 0;
        
        UIElementHolder uiHolder = Type2UI(type);
        UIManager.ShowUI(uiHolder, true, !isRadial);

        uiHolder.onSubmitEvent += onSubmit;

        // // quick inventory doesnt need alternative inputs, just the one submit
        // if (type != UIType.QuickInventory) {
        // }
        uiHolder.alternativeSubmit += callbackToGetAlternativeSubmit(type);
        
        if (!isRadial) {
            uiHolder.onSelectEvent += OnPaginatedUISelect;
        }
    }
    void InitializeSingleInventoryUI (UIType type, bool isRadial, Inventory forInventory, Inventory linkedInventory, int maxButtons, System.Action<GameObject[], object[], Vector2Int> onSubmit) {
        // if (type != UIType.QuickInventory) {
        // }
        if (!CheckForAlternateCallback(type)) return;
        InitializeCallbacksForUIs (isRadial, type, onSubmit);        
        BeginShow(!isRadial, !isRadial, forInventory, linkedInventory, Type2UI(type), maxButtons, 0);
        BroadcastUIOpen(type);
    }

    public int quickInventoryInteractorID;

    public void OpenQuickInventoryUI (int throughID){
        if (quickInventoryInteractorID < 0 && quickTradeInteractorID != throughID) {        
            quickInventoryInteractorID = throughID;
            InitializeSingleInventoryUI(UIType.QuickInventory, true, inventory, null, inventory.favoritesCount, OnQuickInventorySubmit);
        }
    }
    public void OpenFullInventoryUI (){
        CloseAllUIs();
        
        InitializeSingleInventoryUI(UIType.FullInventory, false, inventory, null, maxFullInventoryButtons, OnFullInventorySubmit);   
    }
    public void OpenQuickTradeUI (Inventory showInventory, int throughID) {
        if (quickTradeInteractorID < 0 && quickInventoryInteractorID != throughID) {
            quickTradeInteractorID = throughID;
            InitializeSingleInventoryUI(UIType.QuickTrade, false, showInventory, inventory, maxQuickTradeButtons, OnQuickTradeSubmit);
        }
    }
    public void OpenFullTradeUI (Inventory showInventory){//System.Func<int> getWorkingInventorySlot) {
        CloseAllUIs();

        UIType type = UIType.FullTrade;
        if (!CheckForAlternateCallback(type)) return;

        InitializeCallbacksForUIs(false, type, OnFullTradeSubmit);
        
        int uiIndex = 0;
        BeginShow(true, true, inventory, showInventory, fullTrade.subHolders[uiIndex], maxFullInventoryButtons, uiIndex);
        
        uiIndex = 1;
        BeginShow(true, false, showInventory, inventory, fullTrade.subHolders[uiIndex], maxFullInventoryButtons, uiIndex);

        BroadcastUIOpen(UIType.FullTrade);
    }
    

        void BeginShow (bool paginate, bool setSelection, Inventory forInventory, Inventory linkedInventory, UIElementHolder uiHolder, int maxButtons, int uiIndex) {
            if (forInventory != this.inventory) {
                currentLinkedInventory = forInventory;
            }
            
            if (inventoryButtonsPerInventory[uiIndex] == null) inventoryButtonsPerInventory[uiIndex] = uiHolder.GetAllElements(maxButtons);
            UpdateUIButtons(paginate, forInventory, linkedInventory, maxButtons, uiIndex);
            if (setSelection) {
                UIManager.SetSelection(inventoryButtonsPerInventory[uiIndex][0].gameObject);
            }
        }

        void BeginClose (UIType type, UIElementHolder uiHolder) {
            UIManager.HideUI(uiHolder);
            
            currentLinkedInventory = null;

            for (int i = 0; i < inventoryButtonsPerInventory.Length; i++)
                inventoryButtonsPerInventory[i] = null;
            for (int i = 0; i < currentPaginatedOffsets.Length; i++)
                currentPaginatedOffsets[i] = 0;

            BroadcastUIClose(type);
        }

        UIElementHolder Type2UI (UIType type) {
            switch(type) {
                case UIType.QuickInventory:
                    return quickInventory;
                case UIType.FullInventory:
                    return fullInventory;
                case UIType.QuickTrade:
                    return quickTrade;
                case UIType.FullTrade:
                    return fullTrade;
            }
            return null;
        }

    void OnGamePaused(bool isPaused, float routineTime) {
        if (isPaused) {
            CloseAllUIs();
        }
    }

        void CloseAllUIs () {
            for (int i = 0; i < uiTypesCount; i++) {
                UIType t = (UIType)i;
                if (IsOpen(t)) {
                    Type2Close(t) ();
                }
            }
                
                 
            
            // for (int i = 0; i < 4; i++) {
            //     UIType t = (UIType)i;
            //     if (IsOpen(t)) {
            //         BeginClose(t, Type2UI(t));
            //     }
            // }
        }

        
        public void CloseQuickInventoryUI () {
            BeginClose(UIType.QuickInventory, quickInventory);
            quickInventoryInteractorID = - 1;
        }
        public void CloseFullInventoryUI () {
            BeginClose(UIType.FullInventory, fullInventory);
        }
        public void CloseQuickTradeUI () {
            // quickInventoryInteractorID = - 1;
            BeginClose(UIType.QuickTrade, quickTrade);
            quickTradeInteractorID = -1;
        }
        public void CloseFullTradeUI () {
            BeginClose(UIType.FullTrade, quickTrade);
        }
        System.Action Type2Close (UIType type) {
            switch(type) {
                case UIType.QuickInventory:
                    return CloseQuickInventoryUI;
                case UIType.FullInventory:
                    return CloseFullInventoryUI;
                case UIType.QuickTrade:
                    return CloseQuickTradeUI;
                case UIType.FullTrade:
                    return CloseFullTradeUI;
            }
            return null;
        }
            

    void BroadcastUIOpen(UIType type) {
        if (onUIOpen != null) {
            onUIOpen (type, Type2UI(type));
        }
    }
    void BroadcastUIClose(UIType type) {
        if (onUIClose != null) {
            onUIClose (type, Type2UI(type));
        }
    }

    void Awake () {
        if (inventory == null) {
            inventory = GetComponent<Inventory>();
        }
    }

    public int quickTradeInteractorID;

    void OnQuickTradeStart (Inventory mine, Inventory trader, int interactorID) {
        OpenQuickTradeUI(trader, interactorID);
    }

    void OnQuickTradeEnd (Inventory mine, Inventory trader) {
        if (currentLinkedInventory == trader) {
            CloseQuickTradeUI();
        }
        else {
            Debug.LogError("trying to end quick trade with " + trader.name + ", but we're already quick trading with " + currentLinkedInventory.name);
        }
    }
    void OnTradeStart (Inventory mine, Inventory trader) {
        OpenFullTradeUI (trader);
        
    }


    // void OnInspectStart (Interactor interactor, Interactable interactable) {

    // }
    // void OnInspectEnd (Interactor interactor, Interactable interactable) {

    // }

        void OnEnable () {
        
            // VRManager.onUISelection += OnUISelection;
            // UIManager.onUISelect += OnUISelection;
            
            GameManager.onPauseRoutineStart += OnGamePaused;    

            for (int i = 0; i < uiTypesCount; i++) {
                Type2UI((UIType)i).onBaseCancel += Type2Close((UIType)i);
            }        
            // quickInventory.onBaseCancel += CloseQuickInventory;
            // normalInventory.onBaseCancel += CloseNormalInventory;

            inventory.onStash += OnStash;
            inventory.onDrop += OnDrop;
            inventory.onEquip += OnEquip;
            inventory.onUnequip += OnUnequip;

            inventory.onTradeStart += OnTradeStart;
            inventory.onQuickTradeStart += OnQuickTradeStart;
            inventory.onQuickTradeEnd += OnQuickTradeEnd;





            // GetComponent<Interactor>().onInspectStart += OnInspectStart;
            // GetComponent<Interactor>().onInspectEnd += OnInspectEnd;

            
            for (int i = 0; i < uiTypesCount; i++) {
               UIManager.HideUI( Type2UI((UIType)i) );
            }
            // UIManager.HideUI(quickInventory);
            // UIManager.HideUI(normalInventory);
            // UIManager.HideUI(quickTrade);
            // UIManager.HideUI(normalTrade);
        }
        void OnDisable () {
            GameManager.onPauseRoutineStart -= OnGamePaused;

            for (int i = 0; i < uiTypesCount; i++) {
                Type2UI((UIType)i).onBaseCancel -= Type2Close((UIType)i);
            }        
            
            // quickInventory.onBaseCancel -= CloseQuickInventory;
            // normalInventory.onBaseCancel -= CloseNormalInventory;
            
            inventory.onStash -= OnStash;
            inventory.onDrop -= OnDrop;
			inventory.onEquip -= OnEquip;
            inventory.onUnequip -= OnUnequip;


            inventory.onTradeStart -= OnTradeStart;
            inventory.onQuickTradeStart -= OnQuickTradeStart;
            inventory.onQuickTradeEnd -= OnQuickTradeEnd;


            // GetComponent<Interacto>().onInspectStart -= OnInspectStart;
            // GetComponent<Interacto>().onInspectEnd -= OnInspectEnd;
        }
        void OnStash (Inventory inventory, ItemBehavior item, int count) {
            UIManager.ShowGameMessage("Stashed " + item.itemName + " ( x" + count+" )", 0);
        }
        void OnDrop (Inventory inventory, ItemBehavior item, int count) {
            UIManager.ShowGameMessage("Dropped " + item.itemName + " ( x" + count+" )", 0);
        }
        void OnEquip (Inventory inventory, Item item, int slot, bool quickEquip) {
            // UIManager.ShowGameMessage("Equipped " + item.itemBehavior.itemName + " to slot " + slot + (quickEquip ? "*quick*" : ""), 0);
        }
        void OnUnequip (Inventory inventory, Item item, int slot, bool quickEquip) {
            // UIManager.ShowGameMessage("Unequipped " + item.itemBehavior.itemName + " from slot " + slot + (quickEquip ? "*quick*" : ""), 0);
        }

        
        
        public const int QUICK_INVENTORY_CONSUME_ACTION = 0;
        public const int FULL_INVENTORY_CONSUME_ACTION = 0;
        public const int FULL_INVENTORY_DROP_ACTION = 1;
        public const int FULL_INVENTORY_FAVORITE_ACTION = 2;
        
        public const int QUICK_TRADE_SINGLE_TRADE_ACTION = 0;
        public const int QUICK_TRADE_TRADE_ALL_ACTION = 1;
        public const int QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION = 2;
        
        public const int FULL_TRADE_SINGLE_TRADE_ACTION = 0;
        public const int FULL_TRADE_TRADE_ALL_ACTION = 1;
        public const int FULL_TRADE_CONSUME_ACTION = 2;
        

        void OnQuickInventorySubmit (GameObject[] data, object[] customData, Vector2Int submitType) {
			if (customData != null) {
                ItemBehavior item = customData[0] as ItemBehavior;
                if (item) {
                    int count = 1;
                    // item.OnConsume((Inventory)customData[1], GetWorkingInventorySlot(UIType.QuickInventory));
                    item.OnConsume((Inventory)customData[1], count, submitType.y);
                }
            }
            CloseQuickInventoryUI();
		}

        
        

        void OnFullInventorySubmit (GameObject[] data, object[] customData, Vector2Int submitType) {
			if (customData != null) {
                ItemBehavior item = (ItemBehavior)customData[0];
                if (item != null) {
                    Inventory forInventory = (Inventory)customData[1];
                    
                    bool updateButtons = false;
                    if (submitType.x == FULL_INVENTORY_CONSUME_ACTION) {
                    
                        int count = 1;
                        // if (item.OnConsume(forInventory, GetWorkingInventorySlot(UIType.FullInventory))) {
                        if (item.OnConsume(forInventory, count, submitType.y)){
                        
                            updateButtons = true;
                        }
                    }
                    // drop
                    else if (submitType.x == FULL_INVENTORY_DROP_ACTION) {
                        int itemIndex;
                        if (forInventory.CanDropItem(item, out itemIndex, out _, false)) {
                            forInventory.DropItem(item, 1, true, itemIndex);
                            updateButtons = true;
                        }
                    }
                    // favorite
                    // else if (submitType == FULL_INVENTORY_FAVORITE_ACTION) {
                    //     if (forInventory.FavoriteItem(item)) {
                    //         updateButtons = true;
                    //     }
                    // }
                    
                    if (updateButtons){
                        UpdateUIButtons(true, (Inventory)customData[1], (Inventory)customData[2], (int)customData[3], (int)customData[4]);
                    }
                }
            }
		}

        void OnQuickTradeSubmit (GameObject[] data, object[] customData, Vector2Int submitType) {
			if (customData != null) {
                ItemBehavior item = (ItemBehavior)customData[0];
                if (item != null) {
                    
                    bool updateButtons = false;
                    Inventory trader = (Inventory)customData[1];
                    Inventory tradee = (Inventory)customData[2];
                    
                    // single trade
                    if (submitType.x == QUICK_TRADE_SINGLE_TRADE_ACTION) {
                        if (trader.TransferItemTo(item, 1, tradee)) {
                            updateButtons = true;
                        }
                    }
                    // take all
                    else if (submitType.x == QUICK_TRADE_TRADE_ALL_ACTION) {
                        if (trader.TransferInventoryContentsTo(tradee)) {
                            updateButtons = true;
                        }
                    }
                    else if (submitType.x == QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION) {
                        CloseQuickTradeUI();
                        OpenFullTradeUI(trader);
                    }
                    if (updateButtons){
                        UpdateUIButtons(true, trader, tradee, (int)customData[3], (int)customData[4]);
                    }
                }
            }
		}


        
        void OnFullTradeSubmit (GameObject[] data, object[] customData, Vector2Int submitType) {
			if (customData != null) {
                ItemBehavior item = (ItemBehavior)customData[0];
                if (item != null) {
                    
                    bool updateButtons = false;
                    Inventory trader = (Inventory)customData[1];
                    Inventory tradee = (Inventory)customData[2];
                    
                    // single trade
                    if (submitType.x == FULL_TRADE_SINGLE_TRADE_ACTION) {
                        if (trader.TransferItemTo(item, 1, tradee)) {
                            updateButtons = true;
                        }
                    }
                    // take all
                    else if (submitType.x == FULL_TRADE_TRADE_ALL_ACTION) {   
                        if (trader.TransferInventoryContentsTo(tradee)) {
                            updateButtons = true;
                        }
                    }
                    //consume on selected inventory
                    else if (submitType.x == FULL_TRADE_CONSUME_ACTION) {
                        // if (item.OnConsume(trader, GetWorkingInventorySlot(UIType.FullTrade))) {
                        
                        int count = 1;
                    
                        if (item.OnConsume(trader, count, submitType.y)){
                        
                            updateButtons = true;
                        }
                    }
                    //change working inventory
                    // else if (submitType == 3) {

                    // }
                    
                    if (updateButtons){
                        UpdateUIButtons(true, trader, tradee, (int)customData[3], (int)customData[4]);
                    }
                }
            }
		}

       


    




        
        
        void MakeItemButton (SelectableElement element, Inventory.InventorySlot slot, Inventory inventory, Inventory linkedInventory, int maxButtons, int uiIndex) {
            string display = slot.item.itemName + " ( x"+slot.count+" )";
            MakeButton( element, display, new object[] { slot.item, inventory, linkedInventory, maxButtons, uiIndex } );
        }

        void MakeButton (SelectableElement element, string text, object[] customData) {
            element.elementText = text;
            element.uiText.SetText(text);
            element.customData = customData;
        }



        // void UpdateNormalInventoryUIButtons () 
        // {

        //     // while (currentPageOffset > inventory.allInventory.Count - maxPerPageInventory) {
        //     //     currentPageOffset--;
        //     // }

        //     normalInventoryUIButtons = normalInventory.GetAllElements(maxPerPageInventory);
            
        //     bool isAtEnd = currentPageOffset >= inventory.allInventory.Count - maxPerPageInventory;
        //     bool isAtBeginning = currentPageOffset == 0;
            
        //     int start = isAtBeginning ? 0 : 1;
        //     int end = isAtEnd ? maxPerPageInventory : maxPerPageInventory - 1;

        //     for (int i = start ; i < end; i++) {
        //         SelectableElement element = normalInventoryUIButtons[i];

        //         int inventoryIndex = (i-start) + currentPageOffset;
                
        //         if (inventoryIndex < inventory.allInventory.Count) {
        //             ItemBehavior inventoryItem = inventory.allInventory[inventoryIndex].item;
        //             int count = inventory.allInventory[inventoryIndex].count;
                    
        //             string display = inventoryItem.itemName + " ( x"+count+" )";

        //             MakeButton( element, display, new object[] { inventoryItem } );
                    
        //         }
        //         else {
        //             MakeButton( element, " Empty ", null );
        //         }
        //     }
        //     if (!isAtBeginning) {
        //         MakeButton(elementsList[0], " [ Page Up ] ", new object[]{"BACK"});
        //     }
        //     if (!isAtEnd) {
        //         MakeButton(elementsList[maxPerPageInventory-1], " [ Page Down ] ", new object[]{"FWD"});
        //     }
        // }

        
        // void UpdateUnpaginatedUIButtons (UIElementHolder uiHolder, Inventory forInventory, Inventory linkedInventory, int maxButtons, int uiIndex) {
        //     int allInventoryCount = forInventory.allInventory.Count;

        //     if (inventoryButtonsPerInventory[uiIndex] == null)
        //         inventoryButtonsPerInventory[uiIndex] = uiHolder.GetAllElements(maxButtons);

        //     SelectableElement[] elements = inventoryButtonsPerInventory[uiIndex];
        //     for (int i =0 ; i < maxButtons; i++) {
        //         if (i < allInventoryCount)
        //             MakeItemButton ( elements[i], forInventory.allInventory[i], forInventory, linkedInventory, uiIndex );
        //         else 
        //             MakeButton( elements[i], " Empty ", null );
        //     }
        // }


        void UpdateUIButtons (bool paginate, Inventory forInventory, Inventory linkedInventory, int maxButtons, int uiIndex) 
        {
            
            int allInventoryCount = forInventory.allInventory.Count;
            SelectableElement[] elements = inventoryButtonsPerInventory[uiIndex];
            
            int start = 0;
            int end = maxButtons;
            if (paginate) {

                // while (currentPaginatedOffsets[uiIndex] > allInventoryCount - maxButtons) {
                //     currentPaginatedOffsets[uiIndex]--;
                // }
                bool isAtEnd = currentPaginatedOffsets[uiIndex] >= allInventoryCount - maxButtons;
                bool isAtBeginning = currentPaginatedOffsets[uiIndex] == 0;

                if (!isAtBeginning){
                    MakeButton(elements[0], " [ Page Up ] ", new object[]{"BACK", forInventory, linkedInventory, maxButtons, uiIndex });
                    start = 1;
                }
                if (!isAtEnd) {
                    MakeButton(elements[maxButtons-1], " [ Page Down ] ", new object[]{"FWD", forInventory, linkedInventory, maxButtons, uiIndex });
                    end = maxButtons - 1;
                }
            }
            
            for (int i = start ; i < end; i++) {
                int inventoryIndex = paginate ? (i-start) + currentPaginatedOffsets[uiIndex] : i;
                if (inventoryIndex < allInventoryCount) {
                    MakeItemButton ( elements[i], forInventory.allInventory[inventoryIndex], forInventory, linkedInventory, maxButtons, uiIndex );
                }
                else {
                    MakeButton( elements[i], " Empty ", null );
                }
            }
        }

         
        void OnPaginatedUISelect (GameObject[] data, object[] customData) {
			if (customData != null) {
                string asString = customData[0] as string;
                if (asString != null) {
                    Inventory forInventory = (Inventory)customData[1];
                    
                    int maxButtons = (int)customData[3];
                    int uiIndex = (int)customData[4];



                    bool updateButtons = false;
                    SelectableElement newSelection = null;

                    // hovered over the page up button
                    if (asString == "BACK") {
                        currentPaginatedOffsets[uiIndex]--;

                        if (currentPaginatedOffsets[uiIndex] != 0) {
                            newSelection = inventoryButtonsPerInventory[uiIndex][1];
                        }

                        updateButtons = true;
                    } 
                    
                    // hovered over the page down button
                    else if (asString == "FWD") {
                        currentPaginatedOffsets[uiIndex]++;
                        bool isAtEnd = currentPaginatedOffsets[uiIndex] >= forInventory.allInventory.Count - maxButtons;
                        
                        if (!isAtEnd) {
                            newSelection = inventoryButtonsPerInventory[uiIndex][maxButtons - 2];
                        }
        
                        updateButtons = true;
                    }
                    if (updateButtons){
                        Inventory linkedInventory = (Inventory)customData[2];

                        UpdateUIButtons(true, forInventory, linkedInventory, maxButtons, uiIndex);
                        
                        if (newSelection != null) {
                            StartCoroutine(SetSelection(newSelection.gameObject));
                        }
                    }
                }   
            }
		}
        IEnumerator SetSelection(GameObject selection) {
            yield return new WaitForEndOfFrame();
            UIManager.SetSelection(selection);
        }

        
       
    
}
}
