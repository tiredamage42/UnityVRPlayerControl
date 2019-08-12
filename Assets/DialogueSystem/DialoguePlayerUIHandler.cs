// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// using ActorSystem;
// using InventorySystem;
// using QuestSystem;


// #if UNITY_EDITOR 
// using UnityEditor;
// #endif

using SimpleUI;
// using GameUI;
using DialogueSystem;

namespace GameUI{

    public class DialoguePlayerUIHandler : MonoBehaviour {
        public UIElementHolder uiObject;
        
        public bool UIObjectActive () {
            return uiObject.gameObject.activeInHierarchy;
        }
      
        void OnUIOpenCheck (int data) {
            if (UIObjectActive()) {
                UIManager.DeclareUIOpen();
            }
        }
        
        protected void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
        	if (input.x == 0){
            // if (customData != null) {
                DialogueResponse responseChosen = customData[0] as DialogueResponse;
                if (responseChosen != null) {

                    Debug.LogWarning("chose response " + responseChosen.bark);
                    onRespond(responseChosen);
                }
            }
            CloseResponseUI();
		}

        protected SelectableElement[] inventoryButtonsPerInventory = new SelectableElement[0];
        
        public void SetUIObject (UIElementHolder uiObject) {
            this.uiObject = uiObject;
        }
    
        protected virtual void OnEnable () {
            UIManager.onUIOpenCheck += OnUIOpenCheck;
        }

        protected virtual void OnDisable () {
            UIManager.onUIOpenCheck -= OnUIOpenCheck;
        }


        System.Action<DialogueResponse> onRespond;
        
        public void ShowResponses (List<DialogueResponse> responses, System.Action<DialogueResponse> onRespond) {
            if (UIObjectActive()) return;
            
            if (UIManager.AnyUIOpen(-1)) return;

            this.onRespond = onRespond;
            
            InitializeCallbacksForUIs();

            inventoryButtonsPerInventory = uiObject.GetAllElements(responses.Count);
            UIManager.SetSelection(inventoryButtonsPerInventory[0].gameObject);

            for (int i = 0 ; i < responses.Count; i++) {
                MakeButton( inventoryButtonsPerInventory[i], responses[i].bark, new object[] { responses[i] } );
            }
            
            BroadcastUIOpen();
        }
        
        void InitializeCallbacksForUIs ( ) {
            UIManager.ShowUI(uiObject, true, true);
            uiObject.onBaseCancel = CloseResponseUI;
            uiObject.SubscribeToSubmitEvent(OnUIInput);
        }

        void BroadcastUIOpen() {
            if (onUIOpen != null) {
                onUIOpen (uiObject);
            }
        }
        void BroadcastUIClose() {
            if (onUIClose != null) {
                onUIClose (uiObject);
            }
        }
        public event System.Action<UIElementHolder> onUIOpen, onUIClose;

        public void CloseResponseUI () {
            if (!UIObjectActive()) return;
            UIManager.HideUI(uiObject);
            inventoryButtonsPerInventory = null;
            BroadcastUIClose();
        
        }

        void MakeButton (SelectableElement element, string text, object[] customData) {
            element.elementText = text;
            element.uiText.SetText(text);
            element.customData = customData;
        }
    }
}
