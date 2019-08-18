using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleUI;

namespace Game.GameUI {
    public abstract class UISelectableElementHandler : UIHandler//MonoBehaviour
    {
        public int maxButtons = 8;
        
        int[] lastElementsShownCount;
        protected SelectableElement[][] buttonReferences;
        int[] currentPaginatedOffsets;

        protected void HideUIAndReset (object[] parameters) {
            // UIManager.HideUI(uiObject);
            for(int i = 0; i < buttonReferences.Length; i++) buttonReferences[i] = null;
        }

        protected abstract List<object> BuildButtonObjectsListForDisplay(int panelIndex, object[] updateButtonsParams);
        protected abstract string GetDisplayForButtonObject(object obj);
        // protected abstract void OnUISelect (GameObject[] data, object[] customData);


        // protected override GameObject GetUIBaseObject() {
        //     return uiObject.baseObject;
        // }
        // public UIElementHolder uiObject;

        bool isCollection { get { return (uiObject as ElementHolderCollection) != null; } }
        bool usesRadial { get { return (uiObject as UIRadial) != null; } }
        bool isPaginated { get { return !usesRadial && maxButtons > 0; } }

        

        protected void SetTitle(string title, int panel = 0) {
            UIPage page = (isCollection ? (uiObject as ElementHolderCollection).subHolders[panel] : uiObject) as UIPage;
            if (page != null) page.SetTitle(title);
        }
        
        // public override bool UIObjectActive () {
        //     return uiObject.gameObject.activeInHierarchy;
        // }


        protected override void OnSetUIObject () {
        int maxUIPages = isCollection ? 2 : 1;
            buttonReferences = new SelectableElement[maxUIPages][];
            currentPaginatedOffsets = new int[maxUIPages];
            lastElementsShownCount = new int[maxUIPages];
        }
        // public void SetUIObject (UIElementHolder uiObject) {
        //     this.uiObject = uiObject;
        //     int maxUIPages = isCollection ? 2 : 1;
        //     buttonReferences = new SelectableElement[maxUIPages][];
        //     currentPaginatedOffsets = new int[maxUIPages];
        //     lastElementsShownCount = new int[maxUIPages];
        // }
        protected void MakeButton (SelectableElement element, string text, object[] customData) {
            element.elementText = text;
            element.uiText.SetText(text, -1);
            element.customData = customData;
        }

        // // handle paginated scrolling
        // protected void OnPaginatedUISelect (GameObject[] data, object[] customData) {
		// 	if (customData != null) {
        //         string buttonSelectText = customData[0] as string;
        //         if (buttonSelectText != null) {

        //             int panelIndex = (int)customData[1];
                    
        //             object[] updateButtonsParams = customData[2] as object[];

        //             bool updateButtons = false;
        //             SelectableElement newSelection = null;

        //             // hovered over the page up button
        //             if (buttonSelectText == "B") {
        //                 currentPaginatedOffsets[panelIndex]--;
        //                 if (currentPaginatedOffsets[panelIndex] != 0) {
        //                     newSelection = buttonReferences[panelIndex][1];
        //                 }
        //                 updateButtons = true;
        //             } 
                    
        //             // hovered over the page down button
        //             else if (buttonSelectText == "F") {
        //                 currentPaginatedOffsets[panelIndex]++;
        //                 bool isAtEnd = currentPaginatedOffsets[panelIndex] >= lastElementsShownCount[panelIndex] - maxButtons;

        //                 if (!isAtEnd) {
        //                     newSelection = buttonReferences[panelIndex][maxButtons - 2];
        //                 }
        
        //                 updateButtons = true;
        //             }

        //             if (updateButtons){
        //                 UpdateUIButtons( panelIndex, updateButtonsParams );
                        
        //                 if (newSelection != null) {
        //                     StartCoroutine(SetSelection(newSelection.gameObject));
        //                 }
        //             }
        //         }   
        //     }
		// }
        // IEnumerator SetSelection(GameObject selection) {
        //     yield return new WaitForEndOfFrame();
        //     UIManager.SetSelection(selection);
        // }




        
            





        // protected override void StartShow () {
        //     UIManager.ShowUI(uiObject, 
        //     // true, 
        //     !usesRadial);
        //     uiObject.onBaseCancel = CloseUI;
        //     uiObject.runtimeSubmitHandler = customGetInputMethod;
            
        //     uiObject.SubscribeToSubmitEvent(_OnUIInput);
        //     uiObject.SubscribeToSelectEvent(OnUISelect);

        //     if (isPaginated) {
        //         uiObject.SubscribeToSelectEvent(OnPaginatedUISelect);
        //         for(int i = 0; i < currentPaginatedOffsets.Length; i++) currentPaginatedOffsets[i] = 0;
        //     }
        // }

        protected override void SubscribeToUIObjectEvents() {
            if (isPaginated) {
                uiObject.SubscribeToSelectEvent(OnPaginatedUISelect);
                for(int i = 0; i < currentPaginatedOffsets.Length; i++) currentPaginatedOffsets[i] = 0;
            }
        }

        
        protected override void OnCloseUI(object[] parameters) {
            HideUIAndReset(parameters);
        }
            

        
        protected void BuildButtons (string title, bool setSelection, int panelIndex, object[] updateButtonsParams) {

            SetTitle(title, panelIndex);
            
            UIElementHolder uiObject = this.uiObject as UIElementHolder;
            if (isCollection) {
                uiObject = uiObject.subHolders[panelIndex];
            }

        
            // int uiIndex = (int)updateButtonsParams[0];
            // if (uiObject == null) uiObject = this.uiObject;

            // if (buttonReferences[uiIndex] == null) 
            buttonReferences[panelIndex] = uiObject.GetAllSelectableElements(maxButtons);

            Debug.LogError(buttonReferences[panelIndex].Length);
            Debug.LogError("length");
            if (setSelection) {
                // SetSelection(buttonReferences[panelIndex][0].gameObject);
                StartCoroutine(SetSelection(buttonReferences[panelIndex][0].gameObject));
            }


            UpdateUIButtons(panelIndex, updateButtonsParams);
        }

        // handle paginated scrolling
        protected void OnPaginatedUISelect (GameObject[] data, object[] customData) {
			if (customData != null) {
                string buttonSelectText = customData[0] as string;
                if (buttonSelectText != null) {

                    int panelIndex = (int)customData[1];
                    
                    object[] updateButtonsParams = customData[2] as object[];

                    bool updateButtons = false;
                    SelectableElement newSelection = null;

                    // hovered over the page up button
                    if (buttonSelectText == "B") {
                        currentPaginatedOffsets[panelIndex]--;
                        if (currentPaginatedOffsets[panelIndex] != 0) {
                            newSelection = buttonReferences[panelIndex][1];
                        }
                        updateButtons = true;
                    } 
                    
                    // hovered over the page down button
                    else if (buttonSelectText == "F") {
                        currentPaginatedOffsets[panelIndex]++;
                        bool isAtEnd = currentPaginatedOffsets[panelIndex] >= lastElementsShownCount[panelIndex] - maxButtons;

                        if (!isAtEnd) {
                            newSelection = buttonReferences[panelIndex][maxButtons - 2];
                        }
        
                        updateButtons = true;
                    }

                    if (updateButtons){
                        UpdateUIButtons( panelIndex, updateButtonsParams );
                        
                        if (newSelection != null) {
                            StartCoroutine(SetSelection(newSelection.gameObject));
                        }
                    }
                }   
            }
		}
        protected IEnumerator SetSelection(GameObject selection) {
            yield return new WaitForEndOfFrame();
            // yield return new WaitForEndOfFrame();
            Debug.Log("setting selection " + selection.name);
            UIManager.SetSelection(selection);
        }


        

        protected void UpdateUIButtons (int panelIndex, object[] updateButtonsParams) 
        {
            // int uiIndex = (int)updateButtonsParams[0];
        
            List<object> buttonObjects = BuildButtonObjectsListForDisplay( panelIndex, updateButtonsParams );
            int buttonObjectsCount = buttonObjects.Count;

            lastElementsShownCount[panelIndex] = buttonObjectsCount;
            

            SelectableElement[] elements = buttonReferences[panelIndex];
            
            int start = 0;
            int end = maxButtons;
            if (isPaginated) {

                bool isAtEnd = currentPaginatedOffsets[panelIndex] >= buttonObjectsCount - maxButtons;
                bool isAtBeginning = currentPaginatedOffsets[panelIndex] == 0;

                if (!isAtBeginning){
                    MakeButton(elements[0], " [ Page Up ] ", new object[]{ "B", panelIndex, updateButtonsParams });
                    start = 1;
                }
                if (!isAtEnd) {
                    MakeButton(elements[maxButtons-1], "[ Page Down ] ", new object[]{ "F", panelIndex, updateButtonsParams });
                    end = maxButtons - 1;
                }
            }
            
            for (int i = start ; i < end; i++) {
                int index = isPaginated ? (i-start) + currentPaginatedOffsets[panelIndex] : i;

                if (index < buttonObjectsCount) {
                    MakeButton( elements[i], GetDisplayForButtonObject(buttonObjects[index]), new object[] { buttonObjects[index], panelIndex, updateButtonsParams } );
                }
                else {
                    MakeButton( elements[i], "Empty", new object[] { null, panelIndex, updateButtonsParams } );
                }
            }
        }  
    }


}
