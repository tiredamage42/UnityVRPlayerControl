using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using SimpleUI;

namespace Game.GameUI {
    public abstract class UIHandler : MonoBehaviour
    {
        protected 
        // override 
        GameObject GetUIBaseObject() {
            return uiObject.baseObject;
        }
        public BaseUIElement uiObject;

        [HideInInspector] public Actor myActor;
        public string context;

        protected abstract bool CheckParameters (object[] parameters);
        
        public static UIHandler GetUIHandlerByContext (GameObject checkObject, string context) {
            UIHandler[] allHandlers = checkObject.GetComponents<UIHandler>();// GameObject.FindObjectsOfType<UIHandler>();
            for (int i = 0; i < allHandlers.Length; i++) {
                if (allHandlers[i].context == context) {
                    return allHandlers[i];
                }
            }
            return null;
        }
        
        #if UNITY_EDITOR
            public static string[] GetHandlerInputNames (GameObject checkObject, string context) {//where X : UIHandler {
                UIHandler handler = GetUIHandlerByContext(checkObject, context);
                return handler != null ? handler.GetInputNames() : null;
            }
        #endif
        public bool requiresCustomInputMethod;
        protected Func<Vector2Int> customGetInputMethod;
        public void SetUIInputCallback (Func<Vector2Int> callback) {
            customGetInputMethod = callback;
        }
        protected bool CheckForGetInputCallback () {
            if (customGetInputMethod == null) {
                Debug.LogError("cant open " + context + " UI, no getUIInputs callback supplied");
                return false;
            }
            return true;
        }
        

        public Func<object[], bool> shouldOpenCheck, shouldCloseCheck;

        protected bool OpenUIDenied (object[] parameters) {
            if (UIObjectActive()) {
                Debug.LogError("object active");
                
                return true;
            }
            
            // if (UIManager.AnyUIOpen()) 
                {
// Debug.LogError("ui active");
                
                // return true;
                }
            
            if (shouldOpenCheck != null && !shouldOpenCheck(parameters)) 
                return true;

            if (requiresCustomInputMethod) {
                if (!CheckForGetInputCallback()) 
                    return true;
            }
            return false;
        }

        protected bool UICloseDenied (object[] parameters) {
            if (!UIObjectActive()) {
                Debug.LogError("object not active");
                return true;
            } 
            if (shouldCloseCheck != null && !shouldCloseCheck(parameters)) return true;
            return false;
        }

        protected int GetInputActionOffset () {
            int offset = 0;
            if (nextUIHandler != null) offset++;
            if (previousUIHandler != null) offset++;
            return offset;
        }

        


        public UIHandler previousUIHandler, nextUIHandler;

        protected bool HandleNextAndPreviousUIHandlerOpen (Vector2Int input) {
            if (nextUIHandler == null && previousUIHandler == null)
                return false;

            if (previousUIHandler != null) {
                if (input.x == 0) {
                    CloseUI();
                    previousUIHandler.OpenUI();
                    return true;
                }
            }
            if (nextUIHandler != null) {
                if (input.x == 1) {
                    CloseUI();
                    nextUIHandler.OpenUI();
                    return true;
                }
            }
    		return false;
        }

        protected void _OnUIInput(GameObject[] data, object[] customData, Vector2Int input) {
            if (HandleNextAndPreviousUIHandlerOpen ( input ))
                return;

            OnUIInput (data, customData, input, GetInputActionOffset());
        }

        protected abstract void OnUIInput (GameObject[] data, object[] customData, Vector2Int input, int actionOffset);
        

        // public abstract bool UIObjectActive ();
        public bool UIObjectActive() {
            return uiObject.gameObject.activeInHierarchy;

        }

        // protected abstract void StartShow();

        void StartShow() {
            UIManager.ShowUI(uiObject, 
            // true, 
            usesPage);
            
            uiObject.onBaseCancel = CloseUI;

            if (requiresCustomInputMethod) {
                uiObject.runtimeSubmitHandler = customGetInputMethod;
            }
            
            uiObject.SubscribeToSubmitEvent(_OnUIInput);
            uiObject.SubscribeToSelectEvent(OnUISelect);

            SubscribeToUIObjectEvents();

            // if (Paginated()) {
            // // if (isPaginated) {
            //     uiObject.SubscribeToSelectEvent(OnPaginatedUISelect);
            //     for(int i = 0; i < currentPaginatedOffsets.Length; i++) currentPaginatedOffsets[i] = 0;
            // }
        }

        protected abstract void SubscribeToUIObjectEvents ();

        // int[] currentPaginatedOffsets;


        protected abstract void OnUISelect (GameObject[] data, object[] customData);
        bool usesPage { get { return (uiObject as UIPage) != null || (uiObject as ElementHolderCollection) != null; } }
        
        // protected abstract bool Paginated();

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
        


        
        [NeatArray] public NeatStringArray inputNames;

        public string[] GetInputNames () {

            int c = inputNames.list.Length;
            c += GetInputActionOffset();
            
            string[] r = new string[c];

            if (previousUIHandler != null) {
                r[0] = previousUIHandler.context;
            }
            if (nextUIHandler != null) {
                r[1] = nextUIHandler.context;
            }   

            int x = 0;
            for (int i = GetInputActionOffset(); i < c; i++) {
                r[i] = inputNames.list[x];
                x++; 
            }
            return r;
        }


        protected abstract object[] GetDefaultColdOpenParams ();

        public void OpenUI () {
            OpenUI(GetDefaultColdOpenParams());
        }
        public void CloseUI () {
            CloseUI(GetDefaultColdOpenParams());
        }

        protected abstract void OnSetUIObject();

        public void SetUIObject (UIElementHolder uiObject) {
            this.uiObject = uiObject;

            OnSetUIObject();
            // int maxUIPages = isCollection ? 2 : 1;

            // buttonReferences = new SelectableElement[maxUIPages][];

            // if (Paginated()) {
            //     currentPaginatedOffsets = new int[maxUIPages];
            // }

            // lastElementsShownCount = new int[maxUIPages];
        }
        

        protected abstract int ParamsLength();
        public void OpenUI(object[] parameters) {

            if (parameters == null || parameters.Length != ParamsLength()) {
                Debug.LogError("Cant open " + GetType().ToString() + " ui handler, params null or not the right length");
                return;
            }



        
            if (!CheckParameters(parameters)) 
                {
                    Debug.LogError("parameter check denied");
                    return;
                }
            if (OpenUIDenied(parameters)) 
                {
                    Debug.LogError("ui open denied");
                return;

                }

            StartShow();
            
            OnOpenUI(parameters);
            BroadcastUIOpen(parameters);
        }

        public void CloseUI (object[] parameters) {

            if (UICloseDenied (parameters)) {
                Debug.LogError("close denied");
                return;
            } 

            UIManager.HideUI(uiObject);
            
            
            OnCloseUI(parameters);
            BroadcastUIClose();
        }

        protected abstract void OnOpenUI (object[] parameters);
        protected abstract void OnCloseUI(object[] parameters);

        // protected abstract GameObject GetUIBaseObject ();
        
        protected void BroadcastUIOpen(object[] parameters) {
            if (onUIOpen != null) {
                onUIOpen (GetUIBaseObject(), parameters);
            }
        }
        protected void BroadcastUIClose() {
            if (onUIClose != null) {
                onUIClose (GetUIBaseObject());
            }
        }
        public event System.Action<GameObject, object[]> onUIOpen;
        public event System.Action<GameObject> onUIClose;



    }
}
