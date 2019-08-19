using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using SimpleUI;

namespace Game.UI {
    public abstract class UIHandler : MonoBehaviour
    {
        // static List<UIHandler> allUIHandlers = new List<UIHandler>();
        public BaseUIElement uiObject;
        public string context;
        public static UIHandler GetUIHandlerByContext (string context) {
            UIHandler[] allUIHandlers = GameObject.FindObjectsOfType<UIHandler>();
            for (int i = 0; i < allUIHandlers.Length; i++) {
                if (allUIHandlers[i].context == context) {
                    return allUIHandlers[i];
                }
            }
            return null;
        }
        
        // #if UNITY_EDITOR
        //     public static string[] GetHandlerInputNames (GameObject checkObject, string context) {//where X : UIHandler {
        //         UIHandler handler = GetUIHandlerByContext(checkObject, context);
        //         return handler != null ? handler.GetInputNames() : null;
        //     }
        // #endif
        public bool requiresCustomInputMethod;
        Func<Vector2Int> customGetInputMethod;
        public void SetUIInputCallback (Func<Vector2Int> callback) {
            customGetInputMethod = callback;
        }
        
        // public Func<object[], bool> shouldOpenCheck, shouldCloseCheck;

        bool OpenUIDenied (object[] parameters) {
            if (UIObjectActive(false)) {
                Debug.LogError("object active");
                return true;
            }
            string nm;
            if (UIManager.AnyUIOpen(out nm)) {
                Debug.LogError(nm + " any ui active");    
                return true;
            }
            if (requiresCustomInputMethod) {
                if (customGetInputMethod == null) {
                    Debug.LogError("cant open " + context + " UI, no getUIInputs callback supplied");
                    return true;
                }
            }
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
            if (nextUIHandler == null && previousUIHandler == null) {
                return false;
            }
            if (previousUIHandler != null && input.x == 0) {
                OpenOtherUI(previousUIHandler);
                return true;
            }
            if (nextUIHandler != null && input.x == 1) {
                OpenOtherUI(nextUIHandler);
                return true;
            }
    		return false;
        }

        protected void OpenOtherUI (UIHandler handler) {
            CloseUI();
            StartCoroutine(_OpenOtherUI(nextUIHandler));
        }

        IEnumerator _OpenOtherUI (UIHandler handler) {
            yield return null;
            handler.OpenUI();
        }
        
        protected void _OnUIInput(GameObject selectedObject, GameObject[] data, object[] customData, Vector2Int input) {
            if (HandleNextAndPreviousUIHandlerOpen ( input ))
                return;
                
            OnUIInput ( selectedObject, data, customData, input, GetInputActionOffset());
        }

        protected abstract void OnUIInput (GameObject selectedObject, GameObject[] data, object[] customData, Vector2Int input, int actionOffset);
        

        protected bool CheckActiveRecursive(UIHandler baseHandler) {
            if (baseHandler == this) {
                return uiObject.gameObject.activeInHierarchy;
            }
            if (previousUIHandler != null && previousUIHandler.UIObjectActive(true)) {
                return true;
            }
            if (nextUIHandler != null && nextUIHandler.UIObjectActive(true)) {
                return true;
            }
            return uiObject.gameObject.activeInHierarchy;

        }
        public bool UIObjectActive(bool checkLinked) {
            if (uiObject.gameObject.activeInHierarchy) {
                return true;
            }
            if (checkLinked) {
                if (CheckActiveRecursive(this))
                    return true;
                // if (previousUIHandler != null && previousUIHandler.UIObjectActive(true)) {
                //     return true;
                // }
                // if (nextUIHandler != null && nextUIHandler.UIObjectActive(true)) {
                //     return true;
                // }
            }
            return false;
        }

        protected abstract void OnUISelect (GameObject selectedObject, GameObject[] data, object[] customData);
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
            OpenUI(0, GetDefaultColdOpenParams());
        }
        
        protected abstract void OnSetUIObject();
        public void SetUIObject (UIElementHolder uiObject) {
            this.uiObject = uiObject;
            OnSetUIObject();
        }

        protected object[] openedWithParams;
        
        public void OpenUI(int interactorID, object[] parameters) {

            if (parameters == null){
                Debug.LogError("Cant open " + GetType().ToString() + " ui handler, params null");
                return;
            }
            if (OpenUIDenied(parameters)) {
                Debug.LogError("ui open denied");
                return;
            }
            this.openedWithParams = parameters;

            UIManager.ShowUI(uiObject);
            
            uiObject.onBaseCancel = CloseUI;
            
            if (requiresCustomInputMethod) {
                uiObject.runtimeSubmitHandler = customGetInputMethod;
            }
            
            uiObject.SubscribeToSubmitEvent(_OnUIInput);
            uiObject.SubscribeToSelectEvent(OnUISelect);

            SubscribeToUIObjectEvents();
            OnOpenUI();

            if (onUIOpen != null) onUIOpen (uiObject.baseObject, interactorID);
        }

        protected void SetSelection(GameObject selection) {
            StartCoroutine(_SetSelection(selection));
        }
        IEnumerator _SetSelection(GameObject selection) {
            yield return new WaitForEndOfFrame();
            UIManager.SetSelection(selection);
        }

        protected abstract void SubscribeToUIObjectEvents ();

        protected abstract void OnOpenUI ();
        
        protected void CloseUIRecursive (UIHandler startHandler) {
            if (startHandler == this) 
                return;

            if (previousUIHandler != null) previousUIHandler.CloseUIRecursive(startHandler);
            if (nextUIHandler != null) nextUIHandler.CloseUIRecursive(startHandler);
            
        }
        public void CloseUI () {

            CloseUIRecursive(this);
            // if (previousUIHandler != null) previousUIHandler.CloseUI();
            // if (nextUIHandler != null) nextUIHandler.CloseUI();
            
            if (!UIObjectActive(false)) {
                Debug.LogError("object not active");
                return;
            } 
            
            UIManager.HideUI(uiObject);
            OnCloseUI();
            if (onUIClose != null) onUIClose (uiObject.baseObject);
        }

        protected abstract void OnCloseUI();

        public event System.Action<GameObject, int> onUIOpen;
        public event System.Action<GameObject> onUIClose;
    }
}
