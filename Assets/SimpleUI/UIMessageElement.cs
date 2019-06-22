using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI
{
     public class UIMessageElement : MonoBehaviour {


        UIText _text;
        public UIText text {
            get {
                if (_text == null) {
                    _text = GetComponentInChildren<UIText>();
                }
                return _text;
            }
        }
        RectTransform _rect;
        public RectTransform rectTransform {
            get {
                if (_rect == null) {
                    _rect = GetComponent<RectTransform>();
                }
                return _rect;
            }
        }

        Image _backgroundPanel;
        protected Image backGround {
            get {
                if (_backgroundPanel == null) {
                    _backgroundPanel = transform.GetChild(0).GetComponent<Image>();
                }
                return _backgroundPanel;
            }
        }
        Image _overlayPanel;
        protected Image backGroundOverlay {
            get {
                if (_overlayPanel == null) {
                    _overlayPanel = backGround.transform.GetChild(0).GetComponent<Image>();
                }
                return _overlayPanel;
            }
        }





        RectTransform _backgroundPanelRectTransform;
        protected RectTransform backGroundRectTransform {
            get {
                if (_backgroundPanelRectTransform == null) {
                    _backgroundPanelRectTransform = backGround.GetComponent<RectTransform>();
                }
                return _backgroundPanelRectTransform;
            }
        }
        RectTransform _overlayPanelRectTransform;
        protected RectTransform backGroundOverlayRectTransform {
            get {
                if (_overlayPanelRectTransform == null) {
                    _overlayPanelRectTransform = backGroundOverlay.GetComponent<RectTransform>();
                }
                return _overlayPanelRectTransform;
            }
        }

        Image[] allImages;

        void Awake () {
            allImages = GetComponentsInChildren<Image>();
        }

        public void SetSizeDelta (Vector2 sizeDelta) {

            rectTransform.sizeDelta = sizeDelta;
            backGroundRectTransform.sizeDelta = sizeDelta;
            backGroundOverlayRectTransform.sizeDelta = sizeDelta;
            

            

        }

         
        bool inExit;
        protected float alpha;
        // protected TextAlignment textAlignment;
        float timer, duration, fadeIn, fadeOut;
        public bool isAvailable { get { return !gameObject.activeSelf; } }

        public void DisableMessage () {
            gameObject.SetActive(false);
        }

        void SetAlpha () {
            for (int i = 0; i < allImages.Length; i++) {
                Color c = allImages[i].color;
                c.a = alpha;
                allImages[i].color = c;
            }
            text.SetAlpha(alpha);
        }



        public void ShowMessage (float duration, float fadeIn, float fadeOut){//, TextAlignment textAlignment) {
            
            gameObject.SetActive(true);

            backGround.color = UIManager.instance.mainDarkColor;
            backGroundOverlay.color = UIManager.instance.mainLightColor;
            
            
            
            alpha = 0;
            timer = 0;
            inExit = false;
            this.duration = duration;
            this.fadeIn = fadeIn;
            this.fadeOut = fadeOut;
            // this.textAlignment = textAlignment;
            
            SetAlpha();
        }

        public void UpdateElement (float deltaTime) {
            if (!inExit) {
                if (alpha != 1) {
                    alpha += deltaTime / fadeIn;
                    if (alpha > 1) {
                        alpha = 1;
                    }
                }
            }
            else {
                if (alpha != 0) {
                    alpha -= deltaTime / fadeOut;
                    if (alpha < 0) {
                        alpha = 0;
                    }
                }
            }
            if (alpha == 1) {
                timer += deltaTime;
                if (timer >= duration) {
                    inExit = true;
                }
            }
            SetAlpha();
            

            if (inExit && alpha == 0) {
                gameObject.SetActive(false);
            }
        }


        
    }
}