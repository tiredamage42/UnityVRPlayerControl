using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using SimpleUI;

namespace GameUI {
    public abstract class UISelectableElementHandler<T> : UIHandler//MonoBehaviour
    {

        
        protected int[] lastElementsShownCount;
        protected SelectableElement[][] buttonReferences;
        protected int[] currentPaginatedOffsets;

        protected void ResetPagination () {
            for(int i = 0; i < currentPaginatedOffsets.Length; i++) currentPaginatedOffsets[i] = 0;
        }
        protected void ResetButtonReferences () {
            for(int i = 0; i < buttonReferences.Length; i++) buttonReferences[i] = null;
        }


        protected void HideUIAndReset () {
            UIManager.HideUI(uiObject);
            ResetButtonReferences();
            ResetPagination();
            BroadcastUIClose();
        
        }

        protected abstract int MaxUIPages ();

        protected virtual void OnEnable () {
            buttonReferences = new SelectableElement[MaxUIPages()][];
            currentPaginatedOffsets = new int[MaxUIPages()];
            lastElementsShownCount = new int[MaxUIPages()];
        }
        protected virtual void OnDisable () {

        }

        protected override GameObject GetUIBaseObject() {
            return uiObject.baseObject;
        }
        public UIElementHolder uiObject;
        
        public override bool UIObjectActive () {
            return uiObject.gameObject.activeInHierarchy;
        }
        public void SetUIObject (UIElementHolder uiObject) {
            this.uiObject = uiObject;
        }
        protected void MakeButton (SelectableElement element, string text, object[] customData) {
            element.elementText = text;
            element.uiText.SetText(text, -1);
            element.customData = customData;
        }


        protected abstract int MaxButtons ();



        // handle paginated scrolling
        protected void OnPaginatedUISelect (GameObject[] data, object[] customData) {
			if (customData != null) {
                string buttonSelectText = customData[0] as string;
                if (buttonSelectText != null) {

                    object[] updateButtonsParams = customData[1] as object[];
                    
                    int uiIndex = (int)updateButtonsParams[0];
                    // int uiIndex = (int)customData[3];

                    bool updateButtons = false;
                    SelectableElement newSelection = null;

                    // hovered over the page up button
                    if (buttonSelectText == "B") {
                        currentPaginatedOffsets[uiIndex]--;
                        if (currentPaginatedOffsets[uiIndex] != 0) {
                            newSelection = buttonReferences[uiIndex][1];
                        }
                        updateButtons = true;
                    } 
                    
                    // hovered over the page down button
                    else if (buttonSelectText == "F") {
                        currentPaginatedOffsets[uiIndex]++;
                        // bool isAtEnd = currentPaginatedOffsets[uiIndex] >= GetUnpaginatedShowCount(updateButtonsParams) - maxButtons;
                        bool isAtEnd = currentPaginatedOffsets[uiIndex] >= lastElementsShownCount[uiIndex] - MaxButtons();

                        
                        

                        if (!isAtEnd) {
                            newSelection = buttonReferences[uiIndex][MaxButtons() - 2];
                        }
        
                        updateButtons = true;
                    }

                    if (updateButtons){
                        UpdateUIButtons(
                            updateButtonsParams
                            
                            // (Inventory)customData[1], (Inventory)customData[2], uiIndex, (int)customData[4], usingCategoryFilter
                        );
                        
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

        protected abstract bool Paginated ();
        protected abstract bool UsesRadial ();

        protected abstract void OnUIInput (GameObject[] data, object[] customData, Vector2Int input);


        // protected System.Func<Vector2Int> customGetInputMethod;
        // public void SetUIInputCallback (System.Func<Vector2Int> callback) {
        //     customGetInputMethod = callback;
        // }



        protected void StartShow () {
            UIManager.ShowUI(uiObject, true, !UsesRadial());
            uiObject.onBaseCancel = CloseUI;
            uiObject.runtimeSubmitHandler = customGetInputMethod;
            
            uiObject.SubscribeToSubmitEvent(OnUIInput);
            uiObject.SubscribeToSelectEvent(OnUISelect);

            if (Paginated()) uiObject.SubscribeToSelectEvent(OnPaginatedUISelect);
            
        }

        protected abstract void OnUISelect (GameObject[] data, object[] customData);
        


        protected abstract List<T> BuildButtonObjectsListForDisplay (object[] updateButtonsParams);

        protected abstract string GetDisplayForButtonObject(T obj);


        protected void BuildButtons (UIElementHolder uiObject, bool setSelection, object[] updateButtonsParams) {

            int uiIndex = (int)updateButtonsParams[0];
        
            if (uiObject == null) uiObject = this.uiObject;

            if (buttonReferences[uiIndex] == null) buttonReferences[uiIndex] = uiObject.GetAllSelectableElements(MaxButtons());
            
            if (setSelection) UIManager.SetSelection(buttonReferences[uiIndex][0].gameObject);


            UpdateUIButtons(updateButtonsParams);//  forInventory, linkedInventory, uiIndex, otherIndex, categoryFilter);
        }

        protected void UpdateUIButtons (object[] updateButtonsParams) 
        {
            int uiIndex = (int)updateButtonsParams[0];
        
            bool paginate = Paginated();
            List<T> buttonObjects = BuildButtonObjectsListForDisplay ( updateButtonsParams );
            int buttonObjectsCount = buttonObjects.Count;

            lastElementsShownCount[uiIndex] = buttonObjectsCount;
            

            SelectableElement[] elements = buttonReferences[uiIndex];
            
            int start = 0;
            int end = MaxButtons();
            if (paginate) {

                bool isAtEnd = currentPaginatedOffsets[uiIndex] >= buttonObjectsCount - MaxButtons();
                bool isAtBeginning = currentPaginatedOffsets[uiIndex] == 0;

                if (!isAtBeginning){
                    // MakeButton(elements[0], " [ Page Up ] ", new object[]{ "BACK", shownInventory, linkedInventory, uiIndex, otherIndex });
                    MakeButton(elements[0], " [ Page Up ] ", new object[]{ "B", updateButtonsParams });
                    start = 1;
                }
                if (!isAtEnd) {
                    // MakeButton(elements[maxButtons-1], "[ Page Down ] ", new object[]{ "FWD", shownInventory, linkedInventory, uiIndex, otherIndex });
                    MakeButton(elements[MaxButtons()-1], "[ Page Down ] ", new object[]{ "F", updateButtonsParams });
                    
                    end = MaxButtons() - 1;
                }
            }
            
            for (int i = start ; i < end; i++) {
                int index = paginate ? (i-start) + currentPaginatedOffsets[uiIndex] : i;

                if (index < buttonObjectsCount) {
                    // MakeButton( elements[i], invSlots[index].item.itemName + " ( x"+invSlots[index].count+" )", new object[] { invSlots[index], shownInventory, linkedInventory, uiIndex, otherIndex } );
                    MakeButton( elements[i], GetDisplayForButtonObject(buttonObjects[index]), new object[] { buttonObjects[index], updateButtonsParams } );
                }
                else {
                    // MakeButton( elements[i], "Empty", new object[] { null, shownInventory, linkedInventory, uiIndex, otherIndex } );
                    MakeButton( elements[i], "Empty", new object[] { null, updateButtonsParams } );
                }
            }
        }  

    }
}
