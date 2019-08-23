using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using SimpleUI;

namespace Game.UI {
    public abstract class UIHandler : MonoBehaviour
    {

        protected static T GetInstance<T> (ref T reference) where T : MonoBehaviour {
            if (reference == null) reference = GameObject.FindObjectOfType<T>();
            if (reference == null) Debug.LogError("Couldnt Find " + typeof(T) + " instance");
            return reference;
        }
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

        Func<Vector2Int> customGetInputMethod;
        public void SetUIInputCallback (Func<Vector2Int> callback) {
            customGetInputMethod = callback;
        }

        [HideInInspector] public List<int> allActions = new List<int>();
        List<string> allActionNames = new List<string>();

        protected abstract List<int> InitializeInputsAndNames (out List<string> names);

        public bool cancelCloses = true;
        protected virtual void OnEnable () {
            SetUIInputCallback(GetUIInputs);
            
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
            return false;
        }


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
            if (previousUIHandler != null && input.x == prevNextOpenAction.x) {
                OpenOtherUI(previousUIHandler);
                return true;
            }
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
            bool topLevel = baseHandler == null;
            if (topLevel) baseHandler = this;
            
            if (baseHandler == this && !topLevel) {
                return uiObject.gameObject.activeInHierarchy;
            }

            if (previousUIHandler != null && previousUIHandler.UIObjectActive(true, baseHandler)) return true;
            if (nextUIHandler != null && nextUIHandler.UIObjectActive(true, baseHandler)) return true;
            
            return uiObject.gameObject.activeInHierarchy;

        }
        public bool UIObjectActive(bool checkLinked, UIHandler baseHandler=null) {
            if (uiObject.gameObject.activeInHierarchy) {
                return true;
            }
            if (checkLinked) {
                if (CheckActiveRecursive( baseHandler))
                    return true;
                
            }
            return false;
        }

        Vector2Int GetUIInputs (){
            List<int> actions = allActions;
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


        [HideInInspector] public int controllerIndex = -1;
        
        
        public void OpenUI(int controllerIndex, object[] parameters) {

            if (parameters == null){
                Debug.LogError("Cant open " + GetType().ToString() + " ui handler, params null");
                return;
            }
            if (OpenUIDenied(parameters)) {
                Debug.LogError("ui open denied");
                return;
            }
            this.controllerIndex = controllerIndex;

            this.openedWithParams = parameters;

            UIManager.ShowUI(uiObject);
            
            uiObject.runtimeSubmitHandler = customGetInputMethod;
            
            uiObject.SubscribeToSubmitEvent(_OnUIInput);
            uiObject.SubscribeToSelectEvent(OnUISelect);

            SubscribeToUIObjectEvents();
            OnOpenUI();


            for (int i =0 ; i < allActionNames.Count; i++) {
                Debug.LogError(allActionNames[i]);
            }
            uiObject.AddControllerHints (allActions, allActionNames);


              List<int> inputActions = allActions;
            for (int i = 0; i < inputActions.Count; i++) {

                ControlsManager.MarkActionOccupied(inputActions[i], controllerIndex);
                // StandardizedVRInput.MarkActionOccupied(Player.instance.actions[inputActions[i]], hand);
            }

            ControlsManager.SetUIController(controllerIndex);



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
            if (startHandler == this) return;
            if (previousUIHandler != null) previousUIHandler.CloseUIRecursive(startHandler);
            if (nextUIHandler != null) nextUIHandler.CloseUIRecursive(startHandler);
        }
            
        public void CloseUI () {
            CloseUIRecursive(this);
            
            if (!UIObjectActive(false)) {
                Debug.LogError("object not active");
                return;
            } 
            
            UIManager.HideUI(uiObject);
            OnCloseUI();

            List<int> inputActions = allActions;
            for (int i = 0; i < inputActions.Count; i++) {
                ControlsManager.MarkActionUnoccupied(inputActions[i]);
                // StandardizedVRInput.MarkActionUnoccupied(Player.instance.actions[inputActions[i]]);
                
            }

            if (onUIClose != null) onUIClose (uiObject.baseObject);
        }

        protected abstract void OnCloseUI();

        public event System.Action<GameObject, int> onUIOpen;
        public event System.Action<GameObject> onUIClose;
    }
}
