using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


using StandaloneInputModule = SimpleUI.StandaloneInputModule;
using _GAME_MANAGER_TYPE_ = GameBase.GameManager;

namespace SimpleUI {
    // [RequireComponent(typeof(StandaloneInputModule))]
    public class UIManager : MonoBehaviour
    {

        public static void SetSelection(GameObject selection) {
            // // inputModule.GetComponent<EventSystem>().currentSelectedGameObject	=null;
            // inputModule.GetComponent<EventSystem>().SetSelectedGameObject(null);

            inputModule.GetComponent<EventSystem>().SetSelectedGameObject(selection);

        }

        static _GAME_MANAGER_TYPE_ _gameManager;
        static _GAME_MANAGER_TYPE_ gameManager {
            get {
                if (_gameManager == null) {
                    _gameManager = GameObject.FindObjectOfType<_GAME_MANAGER_TYPE_>();
                }
                return _gameManager;
            }
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


        // public UIPage mainMenuBasePage;

        void Awake () {
            // uiInputModule = GetComponent<StandaloneInputModule>();
            _i = this;
        }

        void Start() {
            inputModule.gameObject.SetActive(false);
        }



        void OnEnable ()  {
            // gameManager.onPauseRoutineEnd += OnPauseRoutineEnd;
            // gameManager.onShowGameMessage += OnShowGameMessage;
            // mainMenuBasePage.onBaseCancel += OnCancelMainMenuPage;
        }
        void OnDisable ()  {
            // gameManager.onPauseRoutineEnd -= OnPauseRoutineEnd;
            // gameManager.onShowGameMessage -= OnShowGameMessage;
            // mainMenuBasePage.onBaseCancel -= OnCancelMainMenuPage;
            
        }

        
        // public static void ToggleGamePause () {
        //     gameManager.TogglePause();
        // }

        public static event System.Action<string, int> onShowGameMessage;
        // void OnShowGameMessage (string message, int key) {
            
        //     if (onShowGameMessage != null) {
        //         onShowGameMessage (message, key);
        //     }
        // }
        // public event System.Action<string, int> onShowGameMessage;
        public static void ShowGameMessage (string message, int key) {

            if (onShowGameMessage != null) {
                onShowGameMessage(message, key);
            }
        }
        public static void ShowGameMessage (string message) {
            ShowGameMessage(message, 0);
            
        }


















        // void OnCancelMainMenuPage () {
        //     gameManager.TogglePause();
        // }

        // void OnPauseRoutineEnd(bool isPaused, float routineTime) {
        //     if (isPaused) {
        //         UIManager.ShowUI (mainMenuBasePage, true, true);
        //     }
        //     else
        //         UIManager.HideUI (mainMenuBasePage);

        // }

        public static BaseInput input {
            get {

                if (inputModule == null) {
                    Debug.LogError("No standaloneInputModule in scene or UIInputManager");
                    return null;
                }
                return inputModule.GetInput();
            }
        }
        // static StandaloneInputModule uiInputModule;
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

        public UIButton buttonPrefab;
        public UIPage pagePrefab;
        public UIRadialElement radialElementPrefab;
        public UIMessageElement messagePrefab;

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

        // public static event System.Action<UIElementHolder> onUIShow;


        static IEnumerator _ShowUI (GameObject uiObject, UIElementHolder uiObjectC, bool needsInput, bool tryRepeat) {
            if (needsInput) {
                
                inputModule.gameObject.SetActive(true);
            }
            
            uiObject.SetActive(true);

            // for some reason, layout groups need to be enabled and disabled a few times
            // to show correctly
            if (needsInput) {
                instance.shownUIsWithInput.Add(uiObject);
                
                // inputModule.gameObject.SetActive(true);
            }

            if (tryRepeat) {
                yield return null;
                uiObject.SetActive(false);
                yield return null;
                uiObject.SetActive(true);
                yield return null;
                uiObject.SetActive(false);
                yield return null;
                uiObject.SetActive(true);
            }
                            

            // else {
            //     instance.gameObject.SetActive(false);
            // }

            //  Debug.LogError("adding callbacks");
            // foreach (var d in gameManager.GetUISelectInvocations()) {
                if (onUISelect != null) {

                foreach (var d in GetUISelectInvocations()) {
                
                    uiObjectC.onSelectEvent += (System.Action<GameObject[], object[]>)d;
                }
                }
            // foreach (var d in gameManager.GetUISubmitInvocations()) {
            if (onUISubmit != null) {

                foreach (var d in GetUISubmitInvocations()) {
                
                    uiObjectC.onSubmitEvent += (System.Action<GameObject[], object[], Vector2Int>)d;
                }
            }
        }

        public static event System.Action<GameObject[], object[]> onUISelect;//, onUISubmit;
        public static event System.Action<GameObject[], object[], Vector2Int> onUISubmit;
        
        public static System.Delegate[] GetUISelectInvocations () {
            return onUISelect.GetInvocationList();
        }
        public static System.Delegate[] GetUISubmitInvocations () {
            return onUISubmit.GetInvocationList();
        }


           

        static void ShowUI (GameObject uiObject, UIElementHolder uiObjectC, bool needsInput, bool tryRepeat) {
            //set active so we can call coroutines on it
            // instance.gameObject.SetActive(true);
                
            instance.StartCoroutine(_ShowUI(uiObject, uiObjectC, needsInput, tryRepeat));
            
        }
        static GameObject GetUIObj (UIElementHolder uiObject) {
            return uiObject.baseObject != null ? uiObject.baseObject : uiObject.gameObject;
        }
        
        public static void ShowUI(UIElementHolder uiObject, bool needsInput, bool tryRepeat) {
            ShowUI(GetUIObj(uiObject), uiObject, needsInput, tryRepeat);
            
            // Debug.LogError("adding callbacks");
            // foreach (var d in gameManager.GetUISelectInvocations()) {
            //     uiObject.onSelectEvent += (System.Action<GameObject[], object[]>)d;
            // }
            // foreach (var d in gameManager.GetUISubmitInvocations()) {
            //     uiObject.onSubmitEvent += (System.Action<GameObject[], object[]>)d;
            // }

            // if (onUIShow != null) {
            //     onUIShow(uiObject);
            // }
        }

        public static void HideUI (UIElementHolder uiObject) {
            // GameObject objToUse = GetUIObj(uiObject);
            HideUI(GetUIObj(uiObject));
            uiObject.RemoveAllEvents();
        }

        void LateUpdate () {
            if (inputModule.gameObject.activeSelf) {

                if (instance.shownUIsWithInput.Count == 0) {


                    inputModule.gameObject.SetActive(false);
                    // instance.gameObject.SetActive(false);
                }
            }

        }

        static void HideUI (GameObject uiObject) {
            if (instance.shownUIsWithInput.Contains(uiObject)) {
                instance.shownUIsWithInput.Remove(uiObject);
                if (instance.shownUIsWithInput.Count == 0) {


                    // inputModule.gameObject.SetActive(false);
                    // instance.gameObject.SetActive(false);
                }
            }
            uiObject.SetActive(false);
        }
    }

}

