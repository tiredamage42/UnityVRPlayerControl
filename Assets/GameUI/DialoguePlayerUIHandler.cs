using UnityEngine;
using SimpleUI;
using DialogueSystem;
using System.Collections.Generic;
using System;

namespace GameUI{

    public class DialoguePlayerUIHandler : UISelectableElementHandler<DialogueResponse> {
        
        protected override int MaxUIPages() { return 1; }
        protected override int MaxButtons() { return currentMaxButtons; }
        protected override bool Paginated () { return false; }
        protected override bool UsesRadial() { return false; }
        public override void OpenUI() { }

        protected override List<DialogueResponse> BuildButtonObjectsListForDisplay (object[] updateButtonsParams) {
            return updateButtonsParams[1] as List<DialogueResponse>;
        }

        protected override string GetDisplayForButtonObject(DialogueResponse obj) {
            return obj.bark;
        }

        protected override void OnEnable () {
            base.OnEnable();
            GetComponent<DialoguePlayer>().onResponseRequested += OnResponseRequested;
        }
        protected override void OnDisable () {
            base.OnDisable();
            GetComponent<DialoguePlayer>().onResponseRequested -= OnResponseRequested;
        }

        protected override void OnUISelect (GameObject[] data, object[] customData) { }

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
        	if (input.x == 0){
                onRespond(customData[0] as DialogueResponse);
            }
            CloseUI();
		}

        Action<DialogueResponse> onRespond;
        Action onResponseCancelled;
      
        
        int currentMaxButtons;
        public void OnResponseRequested (List<DialogueResponse> responses, Action<DialogueResponse> onRespond, Action onResponseCancelled) {
            this.onResponseCancelled = onResponseCancelled;
            this.onRespond = onRespond;
            

            object[] parameters = new object[] { responses };
            // if (UIObjectActive()) return;
            // if (UIManager.AnyUIOpen()) return;



            if (OpenUIDenied (parameters)) return;

            StartShow();
            // UIManager.ShowUI(uiObject, true, true);
            // uiObject.onBaseCancel = CloseUI;
            // uiObject.SubscribeToSubmitEvent(OnUIInput);

            (uiObject as UIPage).SetTitle("");
            

            currentMaxButtons = responses.Count;


            object[] updateButtonsParams = new object[] { 0, responses };

            BuildButtons(null, true, updateButtonsParams);

            // buttonReferences[0] = uiObject.GetAllSelectableElements(responses.Count);

            // UpdateUIButtons (new object[] { 0, responses }) ;

            // for (int i = 0 ; i < responses.Count; i++) {
            //     MakeButton( buttonReferences[0][i], responses[i].bark, new object[] { responses[i] } );
            // }

            // UIManager.SetSelection(buttonReferences[0][0].gameObject);
            
            BroadcastUIOpen(parameters);
        }

        public override void CloseUI() {

            if (UICloseDenied (null)) return;
            // if (!UIObjectActive()) return;

            HideUIAndReset();
            
            if (onResponseCancelled != null) {
                onResponseCancelled();
                onResponseCancelled = null;
            }   
        }
    }
}
