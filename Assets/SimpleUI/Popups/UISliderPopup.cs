using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;


namespace SimpleUI {


    [System.Serializable] public class UISliderPopupParameters {
        public Vector2 panelSize = new Vector2(2, 1);
        public float titleTextSize = .0025f;
        public float amountTextSize = .0025f;
        public float sliderScale = .001f;
        public Vector2 sliderHeadSize = new Vector2( .35f, .45f);
        public Vector2 sliderBGSize = new Vector2( 175, 15 );
        public UISliderPopupParameters () {
            panelSize = new Vector2(2, 1);
            titleTextSize = .0025f;
            amountTextSize = .0025f;
            sliderScale = .001f;
            sliderHeadSize = new Vector2( .35f, .45f);
            sliderBGSize = new Vector2( 175, 15 );
        }
    }

    [ExecuteInEditMode] public class UISliderPopup : BaseUIElement
    {
        public override void SetSelectableActive(bool active) { }
        public override bool RequiresInput() { return true; }

        protected override bool CurrentSelectedIsOurs (GameObject currentSelected) { 
            return slider.gameObject == currentSelected;
        }
        
        public UIText titleText, amountText;
        public RectTransform sliderRect, handleHeadRect, panelRect;
        public Slider slider;
        public UISliderPopupParameters parameters = new UISliderPopupParameters();
    
        public float sliderValue {
            get {
                if (slider == null) 
                    return 0;
                return slider.value;
            }
        }
        public void SetTitle (string txt) {
            if (titleText != null) 
                titleText.SetText(txt, -1);
        }

        void UpdateLayout () {
            if (titleText != null) 
                titleText.rectTransform.localScale = Vector3.one * parameters.titleTextSize;

            if (amountText != null) 
                amountText.rectTransform.localScale = Vector3.one * parameters.amountTextSize;
            
            
            if (panelRect != null)
                panelRect.sizeDelta = parameters.panelSize;


            if (handleHeadRect != null)
                handleHeadRect.localScale = new Vector3(parameters.sliderHeadSize.x, parameters.sliderHeadSize.y, 1);

            if (sliderRect != null)
                sliderRect.sizeDelta = parameters.sliderBGSize;
                
        }

        //TODO: broadcast selection on slider change value

        void UpdateText () {
            if (amountText != null) {
                amountText.SetText( sliderValue.ToString(), -1 );
            }
        }
            

        protected override void Update()
        {
            base.Update();
            #if UNITY_EDITOR
            UpdateLayout();
            #endif
            UpdateText();

            if (UIManager.CurrentSelected() != slider.gameObject) {
                UIManager.SetSelection(slider.gameObject);
            }
        }

        public void OnValueChanged (float value) {
            BroadcastSelectEvent(null, null, null);
        }

        void OnEnable () {
            slider.onValueChanged.AddListener(OnValueChanged);
            UpdateLayout();
        }

        void OnDisable () {
            slider.onValueChanged.RemoveListener(OnValueChanged);
        }
        
    }

}
