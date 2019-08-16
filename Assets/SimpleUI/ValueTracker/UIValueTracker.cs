using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI {

    /* 
        slider for hud visuals (eg health bar) 
    */
    
    [System.Serializable] public class UIValueTrackerParameters {
        public Vector2 trackerSize = new Vector2(2,1);
        public float textScale = .005f;
        [Range(0,1)] public float textOffset = .05f;
        public bool emptyBackground;
        
        public UIValueTrackerParameters () {
            trackerSize = new Vector2(2,1);
            textScale = .005f;
            textOffset = .05f;
        }
        public UIValueTrackerParameters(Vector2 trackerSize, float textScale, float textOffset, bool emptyBackground) {
            this.trackerSize = trackerSize;
            this.textScale = textScale;
            this.textOffset = textOffset;
            this.emptyBackground = emptyBackground;
        }
    }
    [ExecuteInEditMode] public class UIValueTracker : BaseUIElement
    {
        
        public UIValueTrackerParameters parameters = new UIValueTrackerParameters();
        [Range(0,1)] public float value = .5f;
        public string shownText = "Text";
        bool isHorizontal;
        UIText valueText;
        UIImage mainImage, fillImage;
        
        void OnEnable () {
            isHorizontal = GetComponent<HorizontalLayoutGroup>() != null;
            valueText = GetComponentInChildren<UIText>();
            mainImage = GetComponentInChildren<UIImage>();
            fillImage = mainImage.rectTransform.GetChild(0).GetChild(0).GetComponent<UIImage>();
            UpdateAll();
        }

        void UpdateAll () {
            UpdateLayout();
            SetText(shownText);
            SetValue(value, fillImage.colorScheme);
        }

        public void SetText(string txt) {
            shownText = txt;
            valueText.SetText(txt, -1);
        }

        public void SetValue (float val, UIColorScheme scheme) {
            value = val;
            fillImage.mainGraphic.fillAmount = value;
            fillImage.SetColorScheme(scheme, false);
        }

        public void UpdateLayout() {
            valueText.SetAnchor( isHorizontal ? TextAnchor.MiddleLeft : TextAnchor.UpperCenter );    
            
            valueText.rectTransform.sizeDelta = new Vector2(
                isHorizontal ? parameters.textOffset : 0, 
                isHorizontal ? 0 : parameters.textOffset
            );
            
            valueText.transform.localScale = Vector3.one * parameters.textScale;   
            
            mainImage.mainGraphic.enabled = !parameters.emptyBackground;
            mainImage.rectTransform.sizeDelta = parameters.trackerSize;    
        }
    }
}
