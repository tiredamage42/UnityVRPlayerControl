using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


using StandaloneInputModule = SimpleUI.StandaloneInputModule;

namespace SimpleUI {
    // [RequireComponent(typeof(StandaloneInputModule))]
    public class UIManager : MonoBehaviour
    {
        public static void SetSelection(GameObject selection) {
            inputModule.GetComponent<EventSystem>().SetSelectedGameObject(selection);
        }
        static StandaloneInputModule _inputModule;
        public static StandaloneInputModule inputModule {
            get {
                if (_inputModule == null) {
                    _inputModule = GameObject.FindObjectOfType<StandaloneInputModule>();
                }
                return _inputModule;
            }
        }

        void Awake () {
            _i = this;
        }

        void Start() {
            inputModule.gameObject.SetActive(false);
        }
        


        public static BaseInput input {
            get {

                if (inputModule == null) {
                    Debug.LogError("No standaloneInputModule in scene or UIInputManager");
                    return null;
                }
                return inputModule.GetInput();
            }
        }
        public static string verticalAxis { get { return inputModule.verticalAxis; } }
        public static string horizontalAxis { get { return inputModule.horizontalAxis; } }
        public static string cancelButton { get { return inputModule.cancelButton; } }
        public static string submitButton { get { return inputModule.submitButton; } }

        
        static UIManager _i;
        public static UIManager instance {
            get {
                if (_i == null) {
                    _i = GameObject.FindObjectOfType<UIManager>();
                }
                return _i;
            }
        }
        


        public static bool uiInputActive {
            get {
                return inputModule.gameObject.activeSelf;
            }
        }

        public Color32 mainLightColor = Color.red, mainDarkColor = Color.blue;

        public SelectableElement buttonPrefab, radialElementPrefab;
        public UIPage pagePrefab;
        public UIRadial radialPrefab;
        public UITextPanel textPanelPrefab;
        public UIMessageElement messagePrefab;
        public UIValueTracker valueTrackerHorizontalPrefab, valueTrackerVerticalPrefab;


        static System.Func<Vector2> getSelectionAxis;
        public static void OverrideSelectionAxis (System.Func<Vector2> getAxis) {
            getSelectionAxis = getAxis;
        }

        public static Vector2 selectionAxis {
            get {
                // new Vector2 (input.GetAxisRaw(horizontalAxis), input.GetAxisRaw(verticalAxis) );
                Vector2 axis = input.mousePosition;
                if (getSelectionAxis != null) {
                    axis = getSelectionAxis();
                }
                return axis;
            }
        }


        HashSet<GameObject> shownUIsWithInput = new HashSet<GameObject>();

        static IEnumerator _ShowUI (GameObject uiObject, UIElementHolder uiObjectC, bool needsInput, bool tryRepeat) {
            if (needsInput) {
                inputModule.gameObject.SetActive(true);
                instance.shownUIsWithInput.Add(uiObject);
            }
            
            uiObject.SetActive(true);
            
            // for some reason, layout groups need to be enabled and disabled a few times
            // to show correctly
            if (tryRepeat) {
                yield return null; uiObject.SetActive(false);
                yield return null; uiObject.SetActive(true);
                yield return null; uiObject.SetActive(false);
                yield return null; uiObject.SetActive(true);
            }
                            
            //  Debug.LogError("adding callbacks");
            if (onUISelect != null) {
                foreach (var d in onUISelect.GetInvocationList()) {
                    uiObjectC.SubscribeToSelectEvent((System.Action<GameObject[], object[]>)d);
                }
            }

            if (onUISubmit != null) {
                foreach (var d in onUISubmit.GetInvocationList()) {
                    uiObjectC.SubscribeToSubmitEvent((System.Action<GameObject[], object[], Vector2Int>)d);
                }
            }
        }

        public static event System.Action<GameObject[], object[]> onUISelect;
        public static event System.Action<GameObject[], object[], Vector2Int> onUISubmit;
        
        
        static void ShowUI (GameObject uiObject, UIElementHolder uiObjectC, bool needsInput, bool tryRepeat) {
            //set active so we can call coroutines on it    
            instance.StartCoroutine(_ShowUI(uiObject, uiObjectC, needsInput, tryRepeat));
        }
            
        static GameObject GetUIObj (UIElementHolder uiObject) {
            return uiObject.baseObject != null ? uiObject.baseObject : uiObject.gameObject;
        }



        static bool uiOpenCheck;
        public static event System.Action<int> onUIOpenCheck;
        public static void DeclareUIOpen () {
            uiOpenCheck = true;
        }
        public static bool AnyUIOpen(int data) {
            uiOpenCheck = false;
            if (onUIOpenCheck!=null) {
                onUIOpenCheck(data);
            }
            return uiOpenCheck;
        }
            
        // public static bool AnyUIOpen() {
        //     return instance.shownUIsWithInput.Count != 0;
        // }
        
        public static void ShowUI(UIElementHolder uiObject, bool needsInput, bool tryRepeat) {
            ShowUI(GetUIObj(uiObject), uiObject, needsInput, tryRepeat);
        }

        public static void HideUI (UIElementHolder uiObject) {
            HideUI(GetUIObj(uiObject));
            uiObject.RemoveAllEvents();
        }
        static void HideUI (GameObject baseUIObject) {
            if (instance.shownUIsWithInput.Contains(baseUIObject)) {
                instance.shownUIsWithInput.Remove(baseUIObject);
            }
            baseUIObject.SetActive(false);
        }


        // remove input module control in late update, so we dont use inputs
        // the same frame they're closing
        void LateUpdate () {
            if (inputModule.gameObject.activeSelf) {

                if (instance.shownUIsWithInput.Count == 0) {

                    inputModule.gameObject.SetActive(false);
                }
            }
        }   
    }
}