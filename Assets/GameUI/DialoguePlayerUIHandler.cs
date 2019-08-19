using UnityEngine;
using System.Collections.Generic;
using System;

using Game.DialogueSystem;
namespace Game.UI {


    public class DialoguePlayerUIHandler : UISelectableElementHandler{
    
        static DialoguePlayerUIHandler _instance;
        public static DialoguePlayerUIHandler instance {
            get {
                if (_instance == null) _instance = GameObject.FindObjectOfType<DialoguePlayerUIHandler>();
                return _instance;
            }
        }
        public void OpenDialogueResponseUI (List<DialogueResponse> responses, Action<DialogueResponse> onRespond) {
            this.onRespond = onRespond;
            OpenUI (  0, new object[] { responses } );
        }


        Action<DialogueResponse> onRespond;
        protected override void OnOpenUI() { 
            maxButtons = (openedWithParams[0] as List<DialogueResponse>).Count;
            BuildButtons("", true, 0);
        }

        protected override void OnUISelect (GameObject selectedObject, GameObject[] data, object[] customData) { }
        
        // dont allow cold open
        protected override object[] GetDefaultColdOpenParams() { return null; }
        
        protected override List<object> BuildButtonObjectsListForDisplay (int panelIndex){
            return ToObjectList(openedWithParams[0] as List<DialogueResponse>);
        }

        protected override string GetDisplayForButtonObject(object obj) { 
            return (obj as DialogueResponse).bark; 
        }

        protected override void OnUIInput (GameObject selectedObject, GameObject[] data, object[] customData, Vector2Int input, int actionOffset) {
        	if (input.x == actionOffset) {
                onRespond(customData[0] as DialogueResponse);
                CloseUI();
            }
		}
    }
}
