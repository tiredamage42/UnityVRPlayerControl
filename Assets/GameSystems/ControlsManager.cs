using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.UI;
namespace Game {

    
    public class ControlsManager : MonoBehaviour
    {
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
                            GameUI.workshopUI.OpenWorkshopUI(actor.inventory, actor.inventory.workshopItemsFilter); // maybe get current settlement's inventory as well
                        }
                    }
                }
            }
        }

        void OnActionEnd (int controllerIndex, int action) {
            if (controllerIndex == inGameMenuAction.y && action == inGameMenuAction.x) {
                if (!isInWorkshopMode) {

                    Debug.LogError("showing or closing inventory menu");
                    if (inGameMenuOpen)
                        GameUI.inventoryManagementUI.CloseUI();
                    else
                        GameUI.inventoryManagementUI.OpenInventoryManagementUI(actor.inventory, null);
                }
            }
        }
    }
}

