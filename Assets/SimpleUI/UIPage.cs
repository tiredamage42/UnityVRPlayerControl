using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI {

    [ExecuteInEditMode]
    public class UIPage : UIElementHolder
    {

       

        protected override SelectableElement ElementPrefab () {
            return UIManager.instance.buttonPrefab;

        }
      
        UIText _pageTitle;
        UIText pageTitle {
            get {
                if (_pageTitle == null) {
                    _pageTitle = transform.GetChild(0).GetComponent<UIText>();
                }
                return _pageTitle;
            }
        }


        protected override Image Background () {
            return transform.GetChild(1).GetComponent<Image>();

        }
        protected override Image BackgroundOverlay () {
            return backGround.transform.GetChild(0).GetComponent<Image>();
        }
        


        public void SetTitle (string title) {
            pageTitleText = title;
            UpdateElementHolder();
        }

        public string pageTitleText = "Window Title";
        public int lineHeight = 10;
        public int width = 64;

        public TextAnchor textAlignment;


        public void SetSize (int width, int lineHeight) {
            this.width = width;
            this.lineHeight = lineHeight;
            UpdateElementHolder();
        }

        public override void UpdateElementHolder () {

            base.UpdateElementHolder();

            pageTitle.SetText(pageTitleText);

            pageTitle.rectTransform.sizeDelta = new Vector2(width, lineHeight);
            pageTitle.transform.localScale = Vector3.one * textScale;

            for (int i = 0; i < allElements.Count; i++) {
                allElements[i].rectTransform.sizeDelta = new Vector2(width, lineHeight);
                

                UIText textC = allElements[i].uiText;
                if (allElements[i].hasText) {

                    textC.SetAnchor( textAlignment );

                    RectTransform textRect = allElements[i].UITextRectTransform;
                    if (textAlignment == TextAnchor.MiddleCenter || textAlignment == TextAnchor.LowerCenter || textAlignment == TextAnchor.UpperCenter ) {
                        textRect.anchorMin = new Vector2(.5f, .5f);
                        textRect.anchorMax = new Vector2(.5f, .5f);
                        textRect.pivot = new Vector2(0.5f, 0.5f);
                    }
                    else if (textAlignment == TextAnchor.MiddleRight || textAlignment == TextAnchor.LowerRight || textAlignment == TextAnchor.UpperRight  ) {
                        textRect.anchorMin = new Vector2(1, 0.5f);
                        textRect.anchorMax = new Vector2(1, 0.5f);
                        textRect.pivot = new Vector2(1, 0.5f);
                    }
                    else if (textAlignment == TextAnchor.MiddleLeft || textAlignment == TextAnchor.LowerLeft || textAlignment == TextAnchor.UpperLeft  ) {
                        textRect.anchorMin = new Vector2(0, 0.5f);
                        textRect.anchorMax = new Vector2(0, 0.5f);
                        textRect.pivot = new Vector2(0, 0.5f);
                    }
                }
                
            }
        }
    }
}
