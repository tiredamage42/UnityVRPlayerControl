using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI {
    /*
        rectangular panel of selectable element buttons....
    */
    [System.Serializable] public class UIPageParameters {

        public Vector2 elementsSize = new Vector2(4, .5f);
        public float titleScale = .01f;
        public float titleHeight = .5f;
        public TextAnchor textAlignment = TextAnchor.MiddleLeft;
        [Range(0,1)] public float textOffset = 0.05f;
        public float textScale = .005f;

        public float flairIndent = .125f;
        public float flairSize = .2f;

        public UIPageParameters (Vector2 elementsSize, float titleHeight, float titleScale, TextAnchor textAlignment, float textOffset, float textScale ) {
            this.elementsSize = elementsSize;
            this.titleHeight = titleHeight;
            this.titleScale = titleScale;
            this.textAlignment = textAlignment;
            this.textOffset = textOffset;
            this.textScale = textScale;
        }

        public UIPageParameters () {
            elementsSize = new Vector2(4, .5f);
            titleHeight = .5f;
            textAlignment = TextAnchor.MiddleLeft;
            textOffset = 0.05f;
            textScale = 0.005f;
            titleScale = .01f;
        }
    }

    [ExecuteInEditMode] public class UIPage : UIElementHolder
    {

        protected override bool ShouldWiggleLayoutChanges() { return true; }
        protected override float TextScale() { return parameters.textScale; }
        
        public string pageTitleText = "Window Title";
        public UIPageParameters parameters = new UIPageParameters();

        protected override void OnEnable() {
            if (parameters == null) parameters = new UIPageParameters();
            base.OnEnable();   
        }
        
        protected override SelectableElement ElementPrefab () {
            return UIManager.instance.buttonPrefab ;
        }
      
        UIText _pageTitle;
        UIText pageTitle {
            get {
                if (_pageTitle == null) _pageTitle = transform.GetChild(0).GetComponent<UIText>();
                return _pageTitle;
            }
        }

        protected override Transform ElementsParent() {
            return transform.GetChild(1).GetChild(0);
        }
        
        public void SetTitle (string title) {
            pageTitleText = title;
            pageTitle.SetText(pageTitleText, -1);
            // gameObject.name = parameters.pageTitleText + "_Page";
        }

        public override void UpdateSelectableElementHolder () {

            base.UpdateSelectableElementHolder();

            pageTitle.rectTransform.sizeDelta = new Vector2(parameters.elementsSize.x, parameters.titleHeight);
            pageTitle.transform.localScale = Vector3.one * parameters.titleScale;

            Vector2 flairSize = Vector2.one * parameters.flairSize;
            for (int i = 0; i < allElements.Count; i++) {
                allElements[i].rectTransform.sizeDelta = parameters.elementsSize;
                allElements[i].uiText.SetAnchor( parameters.textAlignment );
                allElements[i].uiText.AdjustAnchorSet(new Vector2(parameters.textOffset, 0));

                for (int x = 0; x < allElements[i].flairs.Length; x++) {
                    if (allElements[i].flairs[x].gameObject.activeInHierarchy) {
                        allElements[i].flairs[x].sizeDelta = flairSize;
                        if (x == 0) {
                            allElements[i].flairs[x].localPosition = new Vector3(parameters.flairIndent, 0, 0);

                        }
                        else {
                            allElements[i].flairs[x].localPosition = new Vector3(parameters.elementsSize.x - (parameters.flairIndent), 0, 0);

                        }
                    }
                }
            }
        }
    }
}
