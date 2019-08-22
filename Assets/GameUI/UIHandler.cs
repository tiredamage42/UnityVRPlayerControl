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

        // [Header("If false, ")]

        // protected abstract bool RequiresCustomInputMethod () ;
        // public bool requiresCustomInputMethod;
        Func<Vector2Int> customGetInputMethod;
        public void SetUIInputCallback (Func<Vector2Int> callback) {
            customGetInputMethod = callback;
        }

        [HideInInspector] public List<int> allActions = new List<int>();
        List<string> allActionNames = new List<string>();

        protected abstract List<int> InitializeInputsAndNames (out List<string> names);

        public bool cancelCloses = true;
        protected virtual void OnEnable () {
            // if (RequiresCustomInputMethod()) {
                SetUIInputCallback(GetUIInputs);
            // }

            List<string> myNames;
            List<int> myActions = InitializeInputsAndNames(out myNames);

            allActions.AddRange(myActions);
            allActionNames.AddRange(myNames);

            if (previousUIHandler != null) allActions.Add(prevNextOpenAction.x);
            if (nextUIHandler != null) allActions.Add(prevNextOpenAction.y);

            if (previousUIHandler != null) allActionNames.Add(previousUIHandler.context);
            if (nextUIHandler != null) allActionNames.Add(nextUIHandler.context);

            allActions.Add(cancelAction);
            allActionNames.Add("Cancel");

            
            
            
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
            // if (requiresCustomInputMethod) {
            //     if (customGetInputMethod == null) {
            //         Debug.LogError("cant open " + context + " UI, no getUIInputs callback supplied");
            //         return true;
            //     }
            // }
            return false;
        }

        
        // protected int GetInputActionOffset () {
        //     int offset = 0;
        //     if (nextUIHandler != null) offset++;
        //     if (previousUIHandler != null) offset++;
        //     return offset;
        // }

        public UIHandler previousUIHandler, nextUIHandler;
        public Vector2Int prevNextOpenAction;
        public int cancelAction = 1;

        protected bool HandleNextAndPreviousUIHandlerOpen (Vector2Int input) {
            if (input.x == cancelAction) {
                if (cancelCloses) {
                    CloseUI();
                    return true;
                }
            }
            
            if (nextUIHandler == null && previousUIHandler == null) {
                return false;
            }
            // if (previousUIHandler != null && input.x == 0) {
            if (previousUIHandler != null && input.x == prevNextOpenAction.x) {
                OpenOtherUI(previousUIHandler);
                return true;
            }
            // if (nextUIHandler != null && input.x == 1) {
            if (nextUIHandler != null && input.x == prevNextOpenAction.y) {
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
                
            OnUIInput ( selectedObject, data, customData, input);//, GetInputActionOffset());
        }

        protected abstract void OnUIInput (GameObject selectedObject, GameObject[] data, object[] customData, Vector2Int input);//, int actionOffset);
        

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


        // [Header("-1 for any")]
        // public int inputFromControllerIndex = -1;
        // ControlsManager currentControlsManager;

        // bool needsSingleControllerInput { get { return inputFromControllerIndex != -1; } }
        Vector2Int GetUIInputs (){
            List<int> actions = allActions;// GetInputActions();
            bool usesAnyController = controllerIndex < 0;
            for (int i = 0; i < actions.Count; i++) {
                int action = actions[i];
                if (usesAnyController) {
                    for (int controller = 0; controller < ControlsManager.maxControllers; controller++) {
                        if (ControlsManager.GetActionStart(action, controller)) {
                            return new Vector2Int(action, controller);
                        }
                    }
                }
                else {
                    if (ControlsManager.GetActionStart(action, controllerIndex)) {
                        return new Vector2Int(action, controllerIndex);
                    }
                }
            }
            return new Vector2Int(-1, controllerIndex);        
        }











        protected abstract void OnUISelect (GameObject selectedObject, GameObject[] data, object[] customData);
        
        // [NeatArray] public NeatStringList inputNames;
        // [NeatArray] public NeatIntList inputActions;
        

        // public List<int> GetInputActions () {
        //     return allInputActions;
        //     int c = inputActions.list.Count;
        //     // c += GetInputActionOffset();
            
        //     List<int> r = new List<int>();

        //     if (previousUIHandler != null) r.Add(prevNextOpenAction.x);
        //     if (nextUIHandler != null) r.Add(prevNextOpenAction.y);
        //     for (int i = 0; i < c; i++) r.Add(inputActions.list[i]);
        //     return r;

        // }
        // public List<string> GetInputNames () {

        //     int c = inputNames.list.Length;
        //     // c += GetInputActionOffset();
            
        //     // int offset = GetInputActionOffset();

        //     // string[] r = new string[c + offset];
        //     List<string> r = new List<string>();

        //     if (previousUIHandler != null) r.Add(previousUIHandler.context);
        //     if (nextUIHandler != null) r.Add(nextUIHandler.context);
            
        //     // if (previousUIHandler != null) {
        //     //     r[0] = previousUIHandler.context;
        //     // }
        //     // if (nextUIHandler != null) {
        //     //     r[1] = nextUIHandler.context;
        //     // }   

        //     // int x = 0;
        //     for (int i = 0; i < c; i++) {
        //         r.Add(inputNames.list[i]);
        //         // r[i+offset] = inputNames.list[i];
        //         // x++; 
        //     }
        //     return r;
        // }

        protected abstract object[] GetDefaultColdOpenParams ();
        public void OpenUI () {
            OpenUI(-1, GetDefaultColdOpenParams());
        }
        
        protected abstract void OnSetUIObject();
        public void SetUIObject (UIElementHolder uiObject) {
            this.uiObject = uiObject;
            OnSetUIObject();
        }

        protected object[] openedWithParams;


        // [Header("-1 for any")]
        [HideInInspector] public int controllerIndex = -1;
        // ControlsManager currentControlsManager;

        
        public void OpenUI(
            // ControlsManager currentControlsManager, 
            int controllerIndex, object[] parameters) {

            if (parameters == null){
                Debug.LogError("Cant open " + GetType().ToString() + " ui handler, params null");
                return;
            }
            if (OpenUIDenied(parameters)) {
                Debug.LogError("ui open denied");
                return;
            }
            // this.currentControlsManager = currentControlsManager;
            this.controllerIndex = controllerIndex;

            this.openedWithParams = parameters;

            UIManager.ShowUI(uiObject);
            
            // uiObject.onBaseCancel = CloseUI;
            
            // if (requiresCustomInputMethod) {
            // }
                uiObject.runtimeSubmitHandler = customGetInputMethod;
            
            uiObject.SubscribeToSubmitEvent(_OnUIInput);
            uiObject.SubscribeToSelectEvent(OnUISelect);

            SubscribeToUIObjectEvents();
            OnOpenUI();


            for (int i =0 ; i < allActionNames.Count; i++) {
                Debug.LogError(allActionNames[i]);
            }
            // uiObject.AddHintElements (inputActions, inputNames);
            uiObject.AddControllerHints (allActions, allActionNames);


            if (onUIOpen != null) onUIOpen (uiObject.baseObject, controllerIndex);
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
