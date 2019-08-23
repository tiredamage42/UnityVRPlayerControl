using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleUI;

namespace Game.UI {
    public abstract class UISelectableElementHandler : UIHandler
    {
        public int maxButtons = 8;
        int[] lastElementsShownCount;
        protected SelectableElement[][] buttonReferences;
        int[] currentPaginatedOffsets;
        

        protected List<object> ToObjectList<T> (List<T> l) {
            List<object> r = new List<object>();
            for (int i = 0; i < l.Count; i++) {
                r.Add(l[i]);
            }
            return r;
        }
        protected abstract List<object> BuildButtonObjectsListForDisplay(int panelIndex);
        protected abstract string GetDisplayForButtonObject(object obj);
        bool isCollection { get { return (uiObject as ElementHolderCollection) != null; } }
        bool usesRadial { get { return (uiObject as UIRadial) != null; } }
        bool isPaginated { get { return !usesRadial && maxButtons > 0; } }

        void SetTitle(string title, int panel) {
            UIPage page = (isCollection ? (uiObject as ElementHolderCollection).subHolders[panel] : uiObject) as UIPage;
            if (page != null) page.SetTitle(title);
        }
        
        protected override void OnSetUIObject () {
            int maxUIPages = isCollection ? 2 : 1;
            buttonReferences = new SelectableElement[maxUIPages][];
            if (isPaginated) {
                currentPaginatedOffsets = new int[maxUIPages];
            }
            lastElementsShownCount = new int[maxUIPages];
        }

        protected abstract void SetFlairs(SelectableElement element, object mainObject, int panelIndex);
                
        void MakeButton (SelectableElement element, string text, object[] customData, int panelIndex) {
            element.uiText.SetText(text, -1);
            element.customData = customData;
            SetFlairs(element, customData[0], panelIndex);
        }


        protected override void SubscribeToUIObjectEvents() {
            if (isPaginated) {
                uiObject.SubscribeToSelectEvent(OnPaginatedUISelect);
                for(int i = 0; i < currentPaginatedOffsets.Length; i++) currentPaginatedOffsets[i] = 0;
            }
        }

        
        protected override void OnCloseUI() {
            for(int i = 0; i < buttonReferences.Length; i++) buttonReferences[i] = null;
        }

            
        protected void BuildButtons (string title, bool setSelection, int panelIndex) {

            SetTitle(title, panelIndex);
            
            UIElementHolder uiObject = this.uiObject as UIElementHolder;
            if (isCollection) {
                uiObject = uiObject.subHolders[panelIndex];
            }

            buttonReferences[panelIndex] = uiObject.GetAllSelectableElements(maxButtons);

            if (setSelection) {
                SetSelection(buttonReferences[panelIndex][0].gameObject);
            }

            UpdateUIButtons(panelIndex);
        }

        // handle paginated scrolling
        void OnPaginatedUISelect (GameObject selectedObject, GameObject[] data, object[] customData) {
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
                        UpdateUIButtons( panelIndex );
                        
                        if (newSelection != null) {
                            SetSelection(newSelection.gameObject);
                        }
                    }
                }   
            }
		}
        
        protected void UpdateUIButtons () {
            for (int i = 0; i < lastElementsShownCount.Length; i++) UpdateUIButtons(i);
        }
        void UpdateUIButtons (int panelIndex)
        {
            List<object> buttonObjects = BuildButtonObjectsListForDisplay( panelIndex );
            int buttonObjectsCount = buttonObjects.Count;

            lastElementsShownCount[panelIndex] = buttonObjectsCount;
            

            SelectableElement[] elements = buttonReferences[panelIndex];
            
            int start = 0;
            int end = maxButtons;
            if (isPaginated) {

                bool isAtEnd = currentPaginatedOffsets[panelIndex] >= buttonObjectsCount - maxButtons;
                bool isAtBeginning = currentPaginatedOffsets[panelIndex] == 0;

                if (!isAtBeginning){
                    MakeButton(elements[0], " [ Page Up ] ", new object[]{ "B", panelIndex}, panelIndex );
                    start = 1;
                }
                if (!isAtEnd) {
                    MakeButton(elements[maxButtons-1], "[ Page Down ] ", new object[]{ "F", panelIndex}, panelIndex );
                    end = maxButtons - 1;
                }
            }
            
            for (int i = start ; i < end; i++) {
                int index = isPaginated ? (i-start) + currentPaginatedOffsets[panelIndex] : i;

                if (index < buttonObjectsCount) {
                    MakeButton( elements[i], GetDisplayForButtonObject(buttonObjects[index]), new object[] { buttonObjects[index], panelIndex }, panelIndex );
                }
                else {
                    MakeButton( elements[i], "Empty", new object[] { null, panelIndex}, panelIndex );
                }
            }
        }  
    }


}
