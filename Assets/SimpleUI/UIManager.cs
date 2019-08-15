using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


using StandaloneInputModule = SimpleUI.StandaloneInputModule;


/*
    outside namepsace so we dont have to include it just for the enum...
*/
public enum UIColorScheme { Normal = 0, Warning = 1, Invalid = 2 };
[System.Serializable] public class UIColorSchemeArray : NeatArrayWrapper<UIColorScheme> {}

namespace SimpleUI {
    
    // TODO: add working / error color system...
    // TODO: abstract ui element class
    // TODO: button flair (e.g. square next to equipped item, heart next to favorited)


    // [RequireComponent(typeof(StandaloneInputModule))]

    

    public class UIManager : MonoBehaviour
    {
        public static void SetSelection(GameObject selection) {
            eventSystem.SetSelectedGameObject(selection);
        }
        static StandaloneInputModule _inputModule;
        public static StandaloneInputModule standaloneInputModule {
            get {
                if (_inputModule == null) _inputModule = GameObject.FindObjectOfType<StandaloneInputModule>();
                return _inputModule;
            }
        }
        static EventSystem _eventSystem;
        public static EventSystem eventSystem {
            get {
                if (_eventSystem == null) _eventSystem = standaloneInputModule.GetComponent<EventSystem>();
                return _eventSystem;
            }
        }

        void Awake () {
            _i = this;
        }

        void Start() {
            EventSystem es = eventSystem; // just to get the reference
            standaloneInputModule.gameObject.SetActive(false);
        }
        


        public static BaseInput currentInput {
            get {
                if (standaloneInputModule == null) {
                    Debug.LogError("No standaloneInputModule in scene or UIInputManager");
                    return null;
                }
                return standaloneInputModule.GetInput();
            }
        }
        // public static string verticalAxis { get { return standaloneInputModule.verticalAxis; } }
        // public static string horizontalAxis { get { return standaloneInputModule.horizontalAxis; } }
        // public static string cancelButton { get { return standaloneInputModule.cancelButton; } }
        // public static string submitButton { get { return standaloneInputModule.submitButton; } }

        
        static UIManager _i;
        public static UIManager instance {
            get {
                if (_i == null) _i = GameObject.FindObjectOfType<UIManager>();
                return _i;
            }
        }
        
        public static bool uiInputActive { get { return standaloneInputModule.gameObject.activeSelf; } }

        
        public Color32 mainLightColor = Color.red, mainDarkColor = Color.blue;
        public Color32 warningLightColor = Color.yellow, warningDarkColor = Color.yellow;
        public Color32 invalidLightColor = Color.red, invalidDarkColor = Color.red;

        public static Color32 GetColor (UIColorScheme schemeType, bool useDarker) {
            if (instance == null) return Color.white;

            switch(schemeType) {
                case UIColorScheme.Normal: return useDarker ? instance.mainDarkColor : instance.mainLightColor;
                case UIColorScheme.Warning: return useDarker ? instance.warningDarkColor : instance.warningLightColor;
                case UIColorScheme.Invalid: return useDarker ? instance.invalidDarkColor : instance.invalidLightColor;
            }
            return Color.white;
        }
        

        [Header("Rectangular Buttons Page")]
        public UIPage pagePrefab;
        public SelectableElement buttonPrefab;
        
        [Header("Radial Selection")]
        public UIRadial radialPrefab;
        public SelectableElement radialElementPrefab;
        
        [Header("Text Panel")]
        public UITextPanel textPanelPrefab;
        
        [Header("Subtitles")]
        public UIValueTracker valueTrackerVerticalPrefab;
        public UIValueTracker valueTrackerHorizontalPrefab;
        
        [Header("Subtitles")]
        public UISubtitles subtitlesPrefab;
        
        [Header("Message Center")]
        public UIMessageCenter messageCenterPrefab;
        public UIMessageElement messagePrefab;


        static System.Func<Vector2> getSelectionAxis;
        public static void OverrideSelectionAxis (System.Func<Vector2> getAxis) {
            getSelectionAxis = getAxis;
        }

        public static Vector2 selectionAxis {
            get {
                // new Vector2 (input.GetAxisRaw(horizontalAxis), input.GetAxisRaw(verticalAxis) );
                Vector2 axis = currentInput.mousePosition;
                if (getSelectionAxis != null) {
                    axis = getSelectionAxis();
                }
                return axis;
            }
        }


        HashSet<GameObject> shownUIsWithInput = new HashSet<GameObject>();

        static IEnumerator _ShowUI (GameObject uiObject, UIElementHolder uiObjectC, bool needsInput, bool tryRepeat) {
            if (needsInput) {
                standaloneInputModule.gameObject.SetActive(true);
                instance.shownUIsWithInput.Add(uiObject);
            }
            
            uiObject.SetActive(true);
            
            // because im using a mix of layout groups and content size fitters, 
            // layout groups need to be enabled and disabled a few times to show correctly
            // (unity editor warnings be damned, it's the only way to get the expexted layout behavior unfortunately)
            if (tryRepeat) {
                yield return null; uiObject.SetActive(false);
                yield return null; uiObject.SetActive(true);
                yield return null; uiObject.SetActive(false);
                yield return null; uiObject.SetActive(true);
            }
                            
            if (onAnyUISelect != null) {
                foreach (var d in onAnyUISelect.GetInvocationList()) {
                    uiObjectC.SubscribeToSelectEvent((System.Action<GameObject[], object[]>)d);
                }
            }

            if (onAnyUISubmit != null) {
                foreach (var d in onAnyUISubmit.GetInvocationList()) {
                    uiObjectC.SubscribeToSubmitEvent((System.Action<GameObject[], object[], Vector2Int>)d);
                }
            }
        }

        public static event System.Action<GameObject[], object[]> onAnyUISelect;
        public static event System.Action<GameObject[], object[], Vector2Int> onAnyUISubmit;
        
        static void ShowUI (GameObject uiObject, UIElementHolder uiObjectC, bool needsInput, bool tryRepeat) {
            //set active so we can call coroutines on it    
            instance.StartCoroutine(_ShowUI(uiObject, uiObjectC, needsInput, tryRepeat));
        }
            
        static GameObject GetUIObj (UIElementHolder uiObject) {
            return uiObject.baseObject != null ? uiObject.baseObject : uiObject.gameObject;
        }



        // static bool uiOpenCheck;
        // public static event System.Action<int> onUIOpenCheck;
        // public static void DeclareUIOpen () {
        //     uiOpenCheck = true;
        // }
        // public static bool AnyUIOpen(int data) {
        //     uiOpenCheck = false;
        //     if (onUIOpenCheck!=null) {
        //         onUIOpenCheck(data);
        //     }
        //     return uiOpenCheck;
        // }
            
        public static bool AnyUIOpen() {
            return uiInputActive;//instance.shownUIsWithInput.Count != 0;
        }
        
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
            if (uiInputActive && instance.shownUIsWithInput.Count == 0) {
                standaloneInputModule.gameObject.SetActive(false);
            }
        }   
    }
}