using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleUI {

    [System.Serializable] public class UISubtitlesParameters {
        public float textScale = .005f;
        public float textDuration = 3;

        public int maxCharacters = 64;
        public float width = 40;

        public UISubtitlesParameters() {
            textScale = .005f;
            textDuration = 3;
            maxCharacters = 64;
            width = 40;
        }
        public UISubtitlesParameters(float textScale, float textDuration, int maxCharacters, float width) {
            this.textScale = textScale;
            this.textDuration = textDuration;
            this.maxCharacters = maxCharacters;
            this.width = width;
        }
    }

    [ExecuteInEditMode] public class UISubtitles : MonoBehaviour
    {

        public static UISubtitles instance;
        public UISubtitlesParameters parameters = new UISubtitlesParameters();

        public string speaker;
        [TextArea] public string subtitles;
        public GameObject baseObject;
        UIText[] texts;
        RectTransform[] rectTransforms;

        void Awake () {
            instance = this;   
        }
        void Start () {

            StopShowing();
        } 
        void OnEnable () {
            texts = GetComponentsInChildren<UIText>();
            rectTransforms = GetComponentsInChildren<RectTransform>();

            if (texts.Length == 2) {
                for (int i = 0; i < 2; i++) {
                    texts[i].SetAnchor(i == 0 ? TextAnchor.UpperCenter : TextAnchor.MiddleCenter);
                    texts[i].invert = false;// i == 0;
                    texts[i].UpdateColors();

                    texts[i].AdjustAnchorSet(Vector2.zero);
                }
            }
        }
        
        


        public void ShowSubtitles (string speaker, string subtitles) {
            this.speaker = speaker;
            this.subtitles = subtitles;

            if (texts.Length == 2) {
                texts[0].SetText(speaker + ":");
                int lines;
                texts[1].SetText(UITextPanel.AdjustTextToMaxLength (subtitles, parameters.maxCharacters, out lines));

                for (int i = 0; i < rectTransforms.Length; i++) {
                    rectTransforms[i].sizeDelta = new Vector2(parameters.width, (.5f * lines));
                }
            }

            
            if (Application.isPlaying) {
                if (!showing) {
                    baseObject.SetActive(true);
                    Debug.Log("show");;;
                    showing = true;
                }
                timer = 0;
            }
        }

        void StopShowing () {
            baseObject.SetActive(false);
            showing = false;
        
        }









        void Update () {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                ShowSubtitles(speaker, subtitles);
            }
#endif

            if (showing) {
                if (Application.isPlaying) {
                    timer += Time.deltaTime;
                    if (timer >= parameters.textDuration) {
                        StopShowing();
                        timer = 0;
                    }
                }
            }
        }

        float timer;
        bool showing;

        
    }
}
