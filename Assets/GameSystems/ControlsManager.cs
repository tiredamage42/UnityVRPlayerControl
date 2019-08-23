using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.UI;
using SimpleUI;
namespace Game {

    
    public class ControlsManager : MonoBehaviour
    {

        #region UI
        public static int currentUIController = -1;
        public static bool ControllerOccuped_UI (int controllerIndex) {
            return UIManager.uiInputActive && (controllerIndex == currentUIController || currentUIController < 0);
        }
        public static bool ActionOccupied_UI (int action, int controllerIndex) {
            return ControllerOccuped_UI(controllerIndex) && (
                action == UIManager.instance.submitAction || action == UIManager.instance.cancelAction || (action == instance.selectionAxisActionIndex) 
            );
        }
        public static int GetUIController () {
            return currentUIController != -1 ? currentUIController : lastUsedUIController;
        }

        public static void SetUIController (int hand) {
            currentUIController = hand;
        }
        public static int lastUsedUIController = -1;
        #endregion
        


        // action, controller
        static Dictionary<int, int> occupiedActions = new Dictionary<int, int>();
        const int unoccupiedController = int.MinValue;

        public static void MarkActionOccupied(int action, int controllerIndex) {
            occupiedActions[action] = controllerIndex;
        }
        public static void MarkActionUnoccupied(int action) {
            occupiedActions[action] = unoccupiedController;
        }
        public static bool ActionOccupied (int action, int controllerIndex) {
            if (ActionOccupied_UI(action, controllerIndex)) return true;
            
            int controllerValue;
            if (occupiedActions.TryGetValue(action, out controllerValue)) {
                return controllerValue != unoccupiedController && (controllerIndex == controllerValue || controllerIndex < 0);
            }
            else {
                return false;
            }
        }














        static ControlsManager _i;
        public static ControlsManager instance {
            get {
                if (_i == null) {
                    _i = GameObject.FindObjectOfType<ControlsManager>();
                    if (_i != null) {
                        SimpleUI.UIManager.InitializeGetActionCallback(GetActionStart, _i.maxControllersCount);
                    }    
                }
                if (_i == null) Debug.LogError("Contolrs Manager no instance in scene...");
                
                return _i;
            }
        }

        public int selectionAxisActionIndex;
        public int maxControllersCount = 2; //2 for vr, 1 for fps...



        public static int maxControllers {
            get {
                return instance.maxControllersCount;
            }
        }
        System.Func<int, int, bool> getActionStarts, getActionUpdates, getActionEnds;
        bool UninitializedCheck () {
            if (getActionStarts == null || getActionUpdates == null || getActionEnds == null) {
                Debug.LogError("Contolrs Manager not initialized with action getters");
                return true;
            }
            return false;
        }
        public void InitializeActionGetters (System.Func<int, int, bool> getActionStarts, System.Func<int, int, bool> getActionUpdates, System.Func<int, int, bool> getActionEnds) {
            this.getActionStarts = getActionStarts;
            this.getActionUpdates = getActionUpdates;
            this.getActionEnds = getActionEnds;
        }

        public bool _GetActionStart (int action, int controller) {
            if (UninitializedCheck()) return false;
            return getActionStarts(action, controller);
        }
        public bool _GetActionUpdate (int action, int controller) {
            if (UninitializedCheck()) return false;
            return getActionUpdates(action, controller);
        }
        public bool _GetActionEnd (int action, int controller) {
            if (UninitializedCheck()) return false;
            return getActionEnds(action, controller);
        }

        public static bool GetActionStart (int action, int controller) {
            if (instance == null) return false;
            
            return instance._GetActionStart(action, controller);
        }
        public static bool GetActionUpdate (int action, int controller) {
            if (instance == null) return false;
            
            return instance._GetActionUpdate(action, controller);
        }
        public static bool GetActionEnd (int action, int controller) {
            if (instance == null) return false;
            
            return instance._GetActionEnd(action, controller);
        }


        Actor actor;

        void Awake () {
            actor = GetComponent<Actor>();
        }

        void OnEnable () {
            actor.onActionStart += OnActionStart;
            actor.onActionUpdate += OnActionUpdate;
            actor.onActionEnd += OnActionEnd;
        }
        void OnDisable () {
            actor.onActionStart -= OnActionStart;
            actor.onActionUpdate -= OnActionUpdate;
            actor.onActionEnd -= OnActionEnd;
        }


        public float holdTime = 1;
        public Vector2Int inGameMenuAction = new Vector2Int(4, 0);
        bool isInWorkshopMode { get { return GameUI.workshopUI.UIObjectActive(false); } }
        bool inGameMenuOpen { get { return GameUI.inventoryManagementUI.UIObjectActive(true); } }
        float startedInGameMenuPress;
        // bool attemptOpenInGameMenu;
        void OnActionStart (int controllerIndex, int action) {
            if (controllerIndex == inGameMenuAction.y && action == inGameMenuAction.x) {
                startedInGameMenuPress = 0;
                if (isInWorkshopMode) {
                    GameUI.workshopUI.CloseUI();
                }
                // else {
                //     attemptOpenInGameMenu = true;
                // }
            }
        }
        void OnActionUpdate (int controllerIndex, int action) {
            if (controllerIndex == inGameMenuAction.y && action == inGameMenuAction.x) {
                if (!isInWorkshopMode) {
                    if (!inGameMenuOpen) {
                        startedInGameMenuPress += Time.deltaTime;
                        if (startedInGameMenuPress >= holdTime) {
                            upDoesntCancelWorkshop = true;
                            GameUI.workshopUI.OpenWorkshopUI(actor.inventory, actor.inventory.workshopItemsFilter); // maybe get current settlement's inventory as well
                        }
                    }
                }
            }
        }

        // GetActionStart(action, controller)
        bool upDoesntCancelWorkshop;
        void OnActionEnd (int controllerIndex, int action) {
            if (controllerIndex == inGameMenuAction.y && action == inGameMenuAction.x) {
                if (!isInWorkshopMode) {

                    Debug.LogError("showing or closing inventory menu");
                    if (inGameMenuOpen)
                        GameUI.inventoryManagementUI.CloseUI();
                    else
                        GameUI.inventoryManagementUI.OpenInventoryManagementUI(actor.inventory, null);
                }
                else {
                    if (upDoesntCancelWorkshop) {
                        upDoesntCancelWorkshop = false;
                    }
                    else {
                        GameUI.workshopUI.CloseUI();
                    }

                }
            }
        }
    }
}

