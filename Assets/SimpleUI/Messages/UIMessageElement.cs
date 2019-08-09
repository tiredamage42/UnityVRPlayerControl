using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI
{
     public class UIMessageElement : MonoBehaviour {
        [HideInInspector] public UIText text;
        Image[] allImages;
        float[] originalAlphas;
        void Awake () {
            
            allImages = GetComponentsInChildren<Image>();
            originalAlphas = new float[allImages.Length];
            for (int i = 0; i < allImages.Length; i++) {
                originalAlphas[i] = allImages[i].color.a;
            }
            text = GetComponentInChildren<UIText>();
        }

        public void SetSizeDelta (Vector2 sizeDelta) {
            for (int i = 0; i < allImages.Length; i++) {
                allImages[i].rectTransform.sizeDelta = sizeDelta;
            }          
        }

         
        bool inExit;
        float alpha, timer, duration, fadeIn, fadeOut;
        public bool isAvailable { get { return !gameObject.activeSelf; } }

        public void DisableMessage () {
            gameObject.SetActive(false);
        }

        void SetAlpha () {
            for (int i = 0; i < allImages.Length; i++) {
                Color c = allImages[i].color;
                c.a = Mathf.Lerp(0, originalAlphas[i], alpha);
                allImages[i].color = c;
            }
            text.SetAlpha(alpha);
        }

        public void ShowMessage (float duration, float fadeIn, float fadeOut){
            
            gameObject.SetActive(true);

            alpha = 0;
            timer = 0;
            inExit = false;
            this.duration = duration;
            this.fadeIn = fadeIn;
            this.fadeOut = fadeOut;
            
            SetAlpha();
        }

        public void UpdateElement (float deltaTime) {
            if (!inExit) {
                if (alpha != 1) {
                    alpha += deltaTime / fadeIn;
                    if (alpha > 1) alpha = 1;
                }
            }
            else {
                if (alpha != 0) {
                    alpha -= deltaTime / fadeOut;
                    if (alpha < 0) alpha = 0;
                }
            }
            if (alpha == 1) {
                timer += deltaTime;
                if (timer >= duration) inExit = true;
            }
            SetAlpha();
            
            if (inExit && alpha == 0) {
                gameObject.SetActive(false);
            }
        }
    }
}