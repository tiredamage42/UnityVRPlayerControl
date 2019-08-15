using UnityEngine;
using SimpleUI;
using DialogueSystem;
using System.Collections.Generic;
namespace GameUI{

    public class DialoguePlayerUIHandler : UISelectableElementHandler {
        

        void OnEnable () {
            GetComponent<DialoguePlayer>().onResponseRequested += OnResponseRequested;
        }
        void OnDisable () {
            GetComponent<DialoguePlayer>().onResponseRequested -= OnResponseRequested;
        }
        
        protected void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
        	if (input.x == 0){
                onRespond(customData[0] as DialogueResponse);
            }
            CloseResponseUI();
		}

        SelectableElement[] buttonsReferences = new SelectableElement[0];
        System.Action<DialogueResponse> onRespond;
        System.Action onResponseCancelled;
      
        public void OnResponseRequested (List<DialogueResponse> responses, System.Action<DialogueResponse> onRespond, System.Action onResponseCancelled) {
            this.onResponseCancelled = onResponseCancelled;
            if (UIObjectActive()) return;
            if (UIManager.AnyUIOpen()) return;

            this.onRespond = onRespond;
            
            UIManager.ShowUI(uiObject, true, true);
            
            uiObject.SubscribeToSubmitEvent(OnUIInput);
            uiObject.onBaseCancel = CloseResponseUI;

            buttonsReferences = uiObject.GetAllSelectableElements(responses.Count);
            for (int i = 0 ; i < responses.Count; i++) {
                MakeButton( buttonsReferences[i], responses[i].bark, new object[] { responses[i] } );
            }

            UIManager.SetSelection(buttonsReferences[0].gameObject);
            
            BroadcastUIOpen();
        }
        
        public void CloseResponseUI () {
            if (!UIObjectActive()) return;
            UIManager.HideUI(uiObject);
            buttonsReferences = null;
            BroadcastUIClose();

            if (onResponseCancelled != null) {
                onResponseCancelled();
                onResponseCancelled = null;
            }
        }
    }
}
