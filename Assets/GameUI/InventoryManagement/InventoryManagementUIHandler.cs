using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleUI;
using InventorySystem;

namespace GameUI {

    public abstract class InventoryManagementUIHandler : MonoBehaviour
    {
        public Inventory linkedInventory;
        public UIElementHolder mainUIElement;
        public int maxButtons = 8;




        protected abstract string GetInventoryManagementContext();
        bool IsActive () {
            return mainUIElement.gameObject.activeInHierarchy;
        }

        protected bool isDuplicate;

        public static bool ContextOpen (string context) {
            if (context2UIHandler.ContainsKey(context)) {
                return context2UIHandler[context].IsActive();
            }
            Debug.LogWarning("No inventory ui handlers using context: " + context);
            return false;
        }


        static Dictionary<string, InventoryManagementUIHandler> context2UIHandler = new Dictionary<string, InventoryManagementUIHandler>();
        public static InventoryManagementUIHandler GetHandler(string byContext) {
            if (context2UIHandler.ContainsKey(byContext)) {
                return context2UIHandler[byContext];
            }
            Debug.LogWarning("No inventory ui handlers using context: " + byContext);
            return null;
        }
        
        protected System.Func<System.Func<Vector2Int>> getUIInputs;
        public void SetUIInputCallback (System.Func<System.Func<Vector2Int>> callback) {
            getUIInputs = callback;
        }

        protected bool CheckForGetInputCallback () {
            if (getUIInputs == null) {
                Debug.LogError("cant open " + GetInventoryManagementContext() + " UI, no getUIInputs callback supplied");
                return false;
            }
            return true;
        }

        protected abstract bool UsesRadial () ;
        protected void InitializeCallbacksForUIs (System.Action<GameObject[], object[], Vector2Int> onSubmit) {
            for (int i = 0; i < currentPaginatedOffsets.Length; i++) currentPaginatedOffsets[i] = 0;
            
            UIManager.ShowUI(mainUIElement, true, !UsesRadial());

            mainUIElement.onSubmitEvent += onSubmit;

            mainUIElement.alternativeSubmit += getUIInputs();
            
            if (!UsesRadial()) {
                mainUIElement.onSelectEvent += OnPaginatedUISelect;
            }
        }


        const int maxInventories = 2; // need 2 for trade, cant think of a situation for any more

        protected SelectableElement[][] inventoryButtonsPerInventory = new SelectableElement[maxInventories][];
        protected int[] currentPaginatedOffsets = new int[maxInventories];
    
        
        void AddToDictionary () {
            string myContext = GetInventoryManagementContext();
            if (context2UIHandler.ContainsKey(myContext)) {
                isDuplicate = true;
                return;
            }
            context2UIHandler.Add(myContext, this);
        }

        void GetLinkedInventory () {
            if (linkedInventory == null)
                linkedInventory = GetComponent<Inventory>();
        }

        void Awake () {
            GetLinkedInventory();
            AddToDictionary();
        }

        protected virtual void OnEnable () {
            if (!isDuplicate) {
                linkedInventory.onInventoryManagementInitiate += _OnInventoryManagementInitiate;
                mainUIElement.onBaseCancel += OnEndInventoryManagementInternal;
            }
        }

        protected void OnEndInventoryManagementInternal () {
            _OnEndInventoryManagement(null, null);
        }
            

        protected virtual void OnDisable () {
            if (!isDuplicate) {
                linkedInventory.onInventoryManagementInitiate -= _OnInventoryManagementInitiate;
                mainUIElement.onBaseCancel -= OnEndInventoryManagementInternal;
            }
        }


        protected abstract bool ShouldOpenUI(Inventory inventory, int usingEquipPoint, Inventory otherInventory);

        void _OnInventoryManagementInitiate (Inventory inventory, int usingEquipPoint, Inventory otherInventory, string context) {
            if (!isDuplicate) {
                string myContext = GetInventoryManagementContext();
                if (myContext == context) {
                    if (ShouldOpenUI( inventory, usingEquipPoint, otherInventory)) {
                        OnInventoryManagementInitiate(inventory, usingEquipPoint, otherInventory);

                        BroadcastUIOpen();
                    }
                }
            }

        }
        protected abstract void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory);


        void BeginClose () {
            UIManager.HideUI(mainUIElement);
            
            for (int i = 0; i < inventoryButtonsPerInventory.Length; i++)
                inventoryButtonsPerInventory[i] = null;
            for (int i = 0; i < currentPaginatedOffsets.Length; i++)
                currentPaginatedOffsets[i] = 0;

            BroadcastUIClose();
        }

        protected void BroadcastUIOpen() {
            if (onUIOpen != null) {
                onUIOpen (mainUIElement);
            }
        }
        void BroadcastUIClose() {
            if (onUIClose != null) {
                onUIClose (mainUIElement);
            }
        }
        public event System.Action<UIElementHolder> onUIOpen, onUIClose;

        

        void _OnEndInventoryManagement(Inventory inventory, string context) {
            if (!isDuplicate) {
        
                if (context == null || GetInventoryManagementContext() == context) {
                    BeginClose();

                    OnEndInventoryManagement(inventory);
                }
            }
        }


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

                        UpdateUIButtons(true, forInventory, linkedInventory, uiIndex, (int)customData[4]);
                        
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

        protected abstract void OnEndInventoryManagement(Inventory inventory);


        void MakeItemButton (SelectableElement element, Inventory.InventorySlot slot, Inventory forInventory, Inventory linkedInventory, int uiIndex, int otherIndex) {
            string display = slot.item.itemName + " ( x"+slot.count+" )";
            MakeButton( element, display, new object[] { slot.item, forInventory, linkedInventory, uiIndex, otherIndex } );
        }

        void MakeButton (SelectableElement element, string text, object[] customData) {
            element.elementText = text;
            element.uiText.SetText(text);
            element.customData = customData;
        }

        protected void UpdateUIButtons (bool paginate, Inventory forInventory, Inventory linkedInventory, int uiIndex, int otherIndex) 
        {
            
            int allInventoryCount = forInventory.allInventory.Count;
            SelectableElement[] elements = inventoryButtonsPerInventory[uiIndex];
            
            int start = 0;
            int end = maxButtons;
            if (paginate) {

                bool isAtEnd = currentPaginatedOffsets[uiIndex] >= allInventoryCount - maxButtons;
                bool isAtBeginning = currentPaginatedOffsets[uiIndex] == 0;

                if (!isAtBeginning){
                    MakeButton(elements[0], " [Page Up ] ", new object[]{"BACK", forInventory, linkedInventory, uiIndex, otherIndex });
                    start = 1;
                }
                if (!isAtEnd) {
                    MakeButton(elements[maxButtons-1], "[ Page Down ] ", new object[]{"FWD", forInventory, linkedInventory, uiIndex, otherIndex });
                    end = maxButtons - 1;
                }
            }
            
            for (int i = start ; i < end; i++) {
                int inventoryIndex = paginate ? (i-start) + currentPaginatedOffsets[uiIndex] : i;
                if (inventoryIndex < allInventoryCount) {
                    MakeItemButton ( elements[i], forInventory.allInventory[inventoryIndex], forInventory, linkedInventory, uiIndex, otherIndex );
                }
                else {
                    MakeButton( elements[i], "Empty ", new object[] { null, forInventory, linkedInventory, uiIndex, otherIndex } );
                }
            }
        }

         






        
    }
}
