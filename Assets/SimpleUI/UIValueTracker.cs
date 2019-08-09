using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI {


    [System.Serializable] public class UIValueTrackerParameters {
        [Range(0,1)] public float textOffset = .05f;
        public Vector2 trackerSize = new Vector2(2,1);
        public bool emptyBackground;
        public float textScale = .005f;
        
        

        public UIValueTrackerParameters () {
            textOffset = .05f;
            trackerSize = new Vector2(2,1);
            emptyBackground = false;
            textScale = .005f;
        }

        public UIValueTrackerParameters(float textOffset, Vector2 trackerSize, bool emptyBackground, float textScale)
        {

            this.textOffset = textOffset;
            this.trackerSize = trackerSize;
            this.emptyBackground = emptyBackground;
            this.textScale = textScale;
            
        }
    }
    [ExecuteInEditMode] public class UIValueTracker : MonoBehaviour
    {
        public GameObject baseObject;

        public UIValueTrackerParameters parameters = new UIValueTrackerParameters();
        // [Range(0,1)] public float textOffset = .05f;
        // public Vector2 trackerSize = new Vector2(2,1);
        // public bool emptyBackground;
        // public float textScale = .005f;

        [Range(0,1)] public float value = .5f;
        public string valueText = "Text";
        bool isHorizontal;
        UIText texts;
        RectTransform mainImageRT;
        Image mainImage, fillImage;
        
        void OnEnable () {
            isHorizontal = GetComponent<HorizontalLayoutGroup>() != null;
            texts = GetComponentInChildren<UIText>();
            mainImage = GetComponentInChildren<Image>();
            mainImageRT = mainImage.rectTransform;
            fillImage = mainImageRT.GetChild(0).GetChild(0).GetComponent<Image>();
            UpdateAll();
        }

        void UpdateAll () {
            UpdateTexts();
            UpdateImages();
            SetText(valueText);
            SetValue(value);
        }

        public void SetText(string txt) {
            valueText = txt;
            texts.SetText(valueText);
        }
        public void SetValue (float val) {
            value = val;
            fillImage.fillAmount = value;
        }

        void UpdateTexts() {
            texts.SetAnchor( isHorizontal ? TextAnchor.MiddleLeft : TextAnchor.UpperCenter );    
            texts.rectTransform.sizeDelta = new Vector2(isHorizontal ? parameters.textOffset : 0, isHorizontal ? 0 : parameters.textOffset);
            texts.transform.localScale = Vector3.one * parameters.textScale;   
        }
        void UpdateImages () {
            mainImage.enabled = !parameters.emptyBackground;
            mainImageRT.sizeDelta = parameters.trackerSize;    
        }
#if UNITY_EDITOR
        void Update () {
            UpdateAll();
        }
#endif
    }
}
