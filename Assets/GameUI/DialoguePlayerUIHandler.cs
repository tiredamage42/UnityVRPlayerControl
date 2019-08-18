using UnityEngine;
using System.Collections.Generic;
using System;

using Game.DialogueSystem;
namespace Game.GameUI {

    public class DialoguePlayerUIHandler : UISelectableElementHandler{
    
        public Action<DialogueResponse> onRespond;
        protected override int ParamsLength () { return 1; }
        protected override bool CheckParameters (object[] parameters) {
            if ((parameters[0] as List<DialogueResponse>) == null) {
                Debug.LogError("Cant open " + GetType().ToString() + " ui handler, responses null");
                return false;
            }
            return true;
        }
        
        protected override void OnOpenUI(object[] parameters) { 
            maxButtons = (parameters[0] as List<DialogueResponse>).Count;
            BuildButtons("", true, 0, parameters);
        }

        protected override void OnUISelect (GameObject[] data, object[] customData) { }
        
        // dont allow cold open
        protected override object[] GetDefaultColdOpenParams() { 
            return null; 
        }
        protected override List<object> BuildButtonObjectsListForDisplay (int panelIndex, object[] updateButtonsParams) { 

            List<object> r = new List<object>();
            List<DialogueResponse> s = updateButtonsParams[0] as List<DialogueResponse>; 
            for (int i = 0; i < s.Count; i++) {
                r.Add(s[i]);
            }
            return r;
        }

        protected override string GetDisplayForButtonObject(object obj) { 
            return (obj as DialogueResponse).bark; 
        }

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input, int actionOffset) {
        	if (input.x == actionOffset) {
                onRespond(customData[0] as DialogueResponse);
                CloseUI();
            }
		}
    }
}
