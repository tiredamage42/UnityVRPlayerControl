using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI {

    [System.Serializable] public class UIPageParameters {
        public string pageTitleText = "Window Title";
        public float lineHeight = .5f;
        public float width = 4;
        public TextAnchor textAlignment = TextAnchor.MiddleLeft;
        [Range(0,1)] public float textOffset = 0.05f;

        public UIPageParameters (string pageTitleText, float lineHeight, float width, TextAnchor textAlignment, float textOffset ) {

            this.pageTitleText = pageTitleText;
            this.lineHeight = lineHeight;
            this.width = width;
            this.textAlignment = textAlignment;
            this.textOffset = textOffset;

        }

        public UIPageParameters () {
            pageTitleText = "Window Title";
            lineHeight = .5f;
            width = 4;
            textAlignment = TextAnchor.MiddleLeft;
            textOffset = 0.05f;
        }

    }



    [ExecuteInEditMode] public class UIPage : UIElementHolder
    {
        // public string pageTitleText = "Window Title";
        // public float lineHeight = .5f;
        // public float width = 4;
        // public TextAnchor textAlignment = TextAnchor.MiddleLeft;
        // [Range(0,1)] public float textOffset = 0.05f;


        public UIPageParameters parameters = new UIPageParameters();

        protected override void OnEnable() {
            if (parameters == null) {
                parameters = new UIPageParameters();
            }
            base.OnEnable();

                
            
        }



        
        protected override SelectableElement ElementPrefab () {
            return UIManager.instance.buttonPrefab;
        }
      
        UIText _pageTitle;
        UIText pageTitle {
            get {
                if (_pageTitle == null) _pageTitle = transform.GetChild(0).GetComponent<UIText>();
                if (_pageTitle == null) Debug.LogError("no page title on " + name);
                return _pageTitle;
            }
        }

        protected override Transform ElementsParent() {
            return transform.GetChild(1).GetChild(0);
        }
        

        

        public void SetSize (int width, int lineHeight) {
            this.parameters.width = width;
            this.parameters.lineHeight = lineHeight;
            UpdateElementHolder();
        }
        public void SetTitle (string title) {
            parameters.pageTitleText = title;
            UpdateElementHolder();
        }


        public override void UpdateElementHolder () {

            base.UpdateElementHolder();

            pageTitle.SetText(parameters.pageTitleText);
            gameObject.name = parameters.pageTitleText + "_Page";




            Vector2 sizeDelta = new Vector2(parameters.width, parameters.lineHeight);
            pageTitle.rectTransform.sizeDelta = sizeDelta;
            pageTitle.transform.localScale = Vector3.one * textScale;

            for (int i = 0; i < allElements.Count; i++) {
                allElements[i].rectTransform.sizeDelta = sizeDelta;
                
                if (allElements[i].hasText) {
                    allElements[i].uiText.SetAnchor( parameters.textAlignment );
                    allElements[i].uiText.AdjustAnchorSet(new Vector2(parameters.textOffset, 0));
                }
            }
        }
    }
}
