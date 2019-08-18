using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace SimpleUI {

    [System.Serializable] public class UISelectionPopupParameters {
        public int maxCharacters = 30;
        public float lineSize = .16f;
        public float textSize = .002f;
        public float panelWidth = 3;
        public Vector3 buttonSize = new Vector3( 1.5f, .175f, .005f);
        public UISelectionPopupParameters () {
            maxCharacters = 30;
            lineSize = .16f;
            textSize = .002f;
            panelWidth = 3;
            buttonSize = new Vector3( 1.5f, .175f, .005f);    
        }
    }

    [ExecuteInEditMode] public class UISelectionPopup : UIElementHolder
    {
        protected override Transform ElementsParent () { return transform; }
        protected override float TextScale () { return parameters.textSize; }
        protected override SelectableElement ElementPrefab () { return UIManager.instance.buttonPrefab; }
        public override bool RequiresInput() { return true; }
        public UIText messageText;
        public RectTransform panelRect;
        public UISelectionPopupParameters parameters = new UISelectionPopupParameters();
        [TextArea] public string msgText;

        
        public void SetMessage (string txt) {
            if (messageText != null) 
                messageText.SetText(txt, parameters.maxCharacters);
        }

        void UpdateLayout () {
            float fullButtonSize = parameters.buttonSize.y + parameters.buttonSize.z;
            float textSizeAll = messageText.currentLines * parameters.lineSize;
            for (int i =0 ; i< allElements.Count; i++) {
                float yPos = -(textSizeAll + (fullButtonSize * i));
                allElements[i].transform.localPosition = new Vector3(0, yPos, 0);
                allElements[i].rectTransform.sizeDelta = new Vector2(parameters.buttonSize.x, parameters.buttonSize.y);
                allElements[i].UpdateElement();  
            }
            
            if (messageText != null) 
                messageText.rectTransform.localScale = Vector3.one * parameters.textSize;

        
            if (panelRect != null) {
                float buttonsAllSize = allElements.Count * fullButtonSize;
                panelRect.sizeDelta = new Vector2(parameters.panelWidth, parameters.buttonSize.z + textSizeAll + buttonsAllSize);
            }

            SetMessage(msgText);                
        }

        protected override void OnEnable() {
            if (parameters == null) parameters = new UISelectionPopupParameters();
            base.OnEnable();   
        }
        
        public override void UpdateSelectableElementHolder () {
            base.UpdateSelectableElementHolder();
            UpdateLayout();
        }
    }
}