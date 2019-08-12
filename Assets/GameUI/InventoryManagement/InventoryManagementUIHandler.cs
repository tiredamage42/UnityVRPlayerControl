using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleUI;
using InventorySystem;

namespace GameUI {

    /*
        TODO: Add close ui on game pause
    */

    public abstract class InventoryManagementUIHandler : MonoBehaviour
    {
    
        public static InventoryManagementUIHandler GetUIHandlerByContext (string context) {
            InventoryManagementUIHandler[] allHandlers = GameObject.FindObjectsOfType<InventoryManagementUIHandler>();
            for (int i = 0; i < allHandlers.Length; i++) {
                if (allHandlers[i].ContextKey() == context) {
                    return allHandlers[i];
                }
            }
            return null;
        }

    // #if UNITY_EDITOR
        public static string[] GetHandlerInputNames (string context) {
            InventoryManagementUIHandler handler = GetUIHandlerByContext(context);
            if (handler != null) 
                return handler.GetInputNames();
            return null;
        }
        public abstract string[] GetInputNames ();
    // #endif


        [HideInInspector] public Inventory inventory;
        public UIElementHolder uiObject;
        public int maxButtons = 8;

        public abstract string ContextKey();

        public bool UIObjectActive () {
            return uiObject.gameObject.activeInHierarchy;
        }
        
        public abstract bool EquipIDSpecific ();
        [HideInInspector] public int workingWithEquipID = -1;

        bool UIActiveForEquipID(int equipID) {
            return !EquipIDSpecific() || equipID == workingWithEquipID || equipID == -1;
        }

        void OnUIOpenCheck (int data) {
            if (UIObjectActive() && UIActiveForEquipID(data)) {
                UIManager.DeclareUIOpen();
            }
        }
        
        protected System.Func<Vector2Int> getUIInputs;
        public void SetUIInputCallback (System.Func<Vector2Int> callback) {
            getUIInputs = callback;
        }

        protected bool CheckForGetInputCallback () {
            if (getUIInputs == null) {
                Debug.LogError("cant open " + ContextKey() + " UI, no getUIInputs callback supplied");
                return false;
            }
            return true;
        }

        protected abstract bool UsesRadial () ;
        protected abstract void OnUIInput (GameObject[] data, object[] customData, Vector2Int input);

        const int maxInventories = 2; // need 2 for trade, cant think of a situation for any more
        protected SelectableElement[][] inventoryButtonsPerInventory = new SelectableElement[maxInventories][];
        protected int[] currentPaginatedOffsets = new int[maxInventories];

        public void SetUIObject (UIElementHolder uiObject) {
            if (uiObject != this.uiObject) {

                // if (this.uiObject != null) {
                //     this.uiObject.onBaseCancel -= CloseUI;
                // }
                this.uiObject = uiObject;
                // if (this.uiObject != null) {
                //     this.uiObject.onBaseCancel += CloseUI;
                // }
            }
        }
    


        protected virtual void OnEnable () {
            inventory = GetComponent<Inventory>();
            inventory.onInventoryManagementInitiate += _OnInventoryManagementInitiate;
            inventory.onEndInventoryManagement += _OnEndInventoryManagement;
            UIManager.onUIOpenCheck += OnUIOpenCheck;
            
            // uiObject.onBaseCancel += CloseUI;
            // if (this.uiObject != null) {
            //     this.uiObject.onBaseCancel += CloseUI;
            // }

            //subscripbe to close when game paused
            
            // hide on start


            //  for (int i = 1; i < uiTypesCount; i++) {
            //    UIManager.HideUI( Type2UI((UIType)i) );
            // }

        }

        public void CloseUI () {
            _OnEndInventoryManagement(null, -1, null);
        }
    
        protected virtual void OnDisable () {
            inventory.onInventoryManagementInitiate -= _OnInventoryManagementInitiate;
            inventory.onEndInventoryManagement -= _OnEndInventoryManagement;
            
            // uiObject.onBaseCancel -= CloseUI;
            // if (this.uiObject != null) {
            //     this.uiObject.onBaseCancel -= CloseUI;
            // }


            UIManager.onUIOpenCheck -= OnUIOpenCheck;
        }

        bool ShouldOpenUI (int equipID) {
            return !EquipIDSpecific() || (workingWithEquipID < 0 && workingWithEquipID != equipID);
        }

        protected List<int> usingCategoryFilter;
        
        void _OnInventoryManagementInitiate (Inventory inventory, int usingEquipPoint, Inventory otherInventory, string context, List<int> categoryFilter) {
            if (ContextKey() != context) return;
            if (!CheckForGetInputCallback()) return;
            if (UIObjectActive()) return;
            if (UIManager.AnyUIOpen(usingEquipPoint)) return;
            if (!ShouldOpenUI( usingEquipPoint )) return;

            usingCategoryFilter = categoryFilter;
            workingWithEquipID = usingEquipPoint;
            InitializeCallbacksForUIs();
            OnInventoryManagementInitiate(inventory, usingEquipPoint, otherInventory);
            BroadcastUIOpen();
        }
        void InitializeCallbacksForUIs ( ) {
            //reset pagination offsets
            for (int i = 0; i < currentPaginatedOffsets.Length; i++) currentPaginatedOffsets[i] = 0;
            
            UIManager.ShowUI(uiObject, true, !UsesRadial());

            uiObject.onBaseCancel = CloseUI;
            uiObject.runtimeSubmitHandler = getUIInputs;
            
            uiObject.SubscribeToSubmitEvent(OnUIInput);
            
            uiObject.SubscribeToSelectEvent(OnUISelect);
            
            if (!UsesRadial()) {
                uiObject.SubscribeToSelectEvent(OnPaginatedUISelect);
            }
        }

        protected void SetUpButtons (Inventory forInventory, Inventory linkedInventory, int uiIndex, int otherIndex, bool setSelection, UIElementHolder uiObject){
            if (uiObject == null)
                uiObject = this.uiObject;

            if (inventoryButtonsPerInventory[uiIndex] == null) 
                inventoryButtonsPerInventory[uiIndex] = uiObject.GetAllElements(maxButtons);
            
            if (setSelection)
                UIManager.SetSelection(inventoryButtonsPerInventory[uiIndex][0].gameObject);


            UpdateUIButtons(forInventory, linkedInventory, uiIndex, otherIndex, usingCategoryFilter);
        }

        protected abstract void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory);

        void BroadcastUIOpen() {
            if (onUIOpen != null) {
                onUIOpen (uiObject);
            }
        }
        void BroadcastUIClose() {
            if (onUIClose != null) {
                onUIClose (uiObject);
            }
        }
        public event System.Action<UIElementHolder> onUIOpen, onUIClose;

        bool ShouldCloseUI (int equipID) {
            return (!EquipIDSpecific() || (workingWithEquipID == equipID));
        }

        
        void _OnEndInventoryManagement(Inventory inventory, int usingEquipPoint, string context) {
            if (context != null && ContextKey() != context) return;
            if (!UIObjectActive()) return;
            if (!ShouldCloseUI(usingEquipPoint)) return;
        
            UIManager.HideUI(uiObject);
            for (int i = 0; i < inventoryButtonsPerInventory.Length; i++) inventoryButtonsPerInventory[i] = null;
            for (int i = 0; i < currentPaginatedOffsets.Length; i++) currentPaginatedOffsets[i] = 0;
            BroadcastUIClose();
        
            workingWithEquipID = -1;
            usingCategoryFilter = null;
        }

        protected abstract void OnUISelect (GameObject[] data, object[] customData);
        

        // handle paginated scrolling
        void OnPaginatedUISelect (GameObject[] data, object[] customData) {
			if (customData != null) {
                string buttonSelectText = customData[0] as string;
                if (buttonSelectText != null) {

                    Inventory forInventory = (Inventory)customData[1];
                    
                    int uiIndex = (int)customData[3];

                    bool updateButtons = false;
                    SelectableElement newSelection = null;

                    // hovered over the page up button
                    if (buttonSelectText == "BACK") {
                        currentPaginatedOffsets[uiIndex]--;

                        if (currentPaginatedOffsets[uiIndex] != 0) {
                            newSelection = inventoryButtonsPerInventory[uiIndex][1];
                        }

                        updateButtons = true;
                    } 
                    
                    // hovered over the page down button
                    else if (buttonSelectText == "FWD") {
                        currentPaginatedOffsets[uiIndex]++;
                        bool isAtEnd = currentPaginatedOffsets[uiIndex] >= forInventory.allInventory.Count - maxButtons;
                        
                        if (!isAtEnd) {
                            newSelection = inventoryButtonsPerInventory[uiIndex][maxButtons - 2];
                        }
        
                        updateButtons = true;
                    }
                    if (updateButtons){
                        Inventory linkedInventory = (Inventory)customData[2];

                        UpdateUIButtons(forInventory, linkedInventory, uiIndex, (int)customData[4], usingCategoryFilter);
                        
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

        protected void UnpackButtonData (object[] customData, out Inventory.InventorySlot slot, out Inventory forInventory, out Inventory linkedInventory, out int uiIndex, out int otherUIIndex)
        {
            slot = (Inventory.InventorySlot)customData[0];
            forInventory = (Inventory)customData[1];
            linkedInventory = (Inventory)customData[2];
            uiIndex = (int)customData[3];
            otherUIIndex = (int)customData[4];
        }
        
        void MakeItemButton (SelectableElement element, Inventory.InventorySlot slot, Inventory forInventory, Inventory linkedInventory, int uiIndex, int otherIndex) {
            string display = slot.item.itemName + " ( x"+slot.count+" )";
            MakeButton( element, display, new object[] { slot, forInventory, linkedInventory, uiIndex, otherIndex } );
        }

        void MakeButton (SelectableElement element, string text, object[] customData) {
            element.elementText = text;
            element.uiText.SetText(text);
            element.customData = customData;
        }

        protected abstract List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (Inventory forInventory, List<int> categoryFilter) ;

        protected void UpdateUIButtons (Inventory forInventory, Inventory linkedInventory, int uiIndex, int otherIndex, List<int> categoryFilter) 
        // protected void UpdateUIButtons (Inventory forInventory, int uiIndex, object[] customData)
        
        {
            bool paginate = !UsesRadial();
            List<Inventory.InventorySlot> invSlots = BuildInventorySlotsForDisplay ( forInventory, categoryFilter);
            int invCount = invSlots.Count;
            

            SelectableElement[] elements = inventoryButtonsPerInventory[uiIndex];
            
            int start = 0;
            int end = maxButtons;
            if (paginate) {

                bool isAtEnd = currentPaginatedOffsets[uiIndex] >= invCount - maxButtons;
                bool isAtBeginning = currentPaginatedOffsets[uiIndex] == 0;

                if (!isAtBeginning){
                    MakeButton(elements[0], " [ Page Up ] ", new object[]{ "BACK", forInventory, linkedInventory, uiIndex, otherIndex });
                    start = 1;
                }
                if (!isAtEnd) {
                    MakeButton(elements[maxButtons-1], "[ Page Down ] ", new object[]{ "FWD", forInventory, linkedInventory, uiIndex, otherIndex });
                    end = maxButtons - 1;
                }
            }
            
            for (int i = start ; i < end; i++) {
                int inventoryIndex = paginate ? (i-start) + currentPaginatedOffsets[uiIndex] : i;

                if (inventoryIndex < invCount) {
                    MakeItemButton ( elements[i], invSlots[inventoryIndex], forInventory, linkedInventory, uiIndex, otherIndex );
                }
                else {
                    MakeButton( elements[i], "Empty ", new object[] { null, forInventory, linkedInventory, uiIndex, otherIndex } );
                }
            }
        }  
    }
}
