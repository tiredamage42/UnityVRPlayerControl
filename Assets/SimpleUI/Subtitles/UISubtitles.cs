using UnityEngine;
namespace SimpleUI {

    [System.Serializable] public class UISubtitlesParameters {
        public float textScale = .005f;
        public Vector2 singleLineSize = new Vector2(40, .5f);
        public int maxCharacters = 64;
        public float textDuration = 3;


        public UISubtitlesParameters() {
            textScale = .005f;
            textDuration = 3;
            maxCharacters = 64;
            singleLineSize = new Vector2(40, .5f);
        }
        public UISubtitlesParameters(float textScale, Vector2 singleLineSize, float textDuration, int maxCharacters) {
            this.textScale = textScale;
            this.textDuration = textDuration;
            this.maxCharacters = maxCharacters;
            this.singleLineSize = singleLineSize;
        }
    }

    [ExecuteInEditMode] public class UISubtitles : BaseUIElement
    {
        public UISubtitlesParameters parameters = new UISubtitlesParameters();
        public string speaker;
        [TextArea] public string subtitles;
        
        UIText[] texts;
        RectTransform[] rectTransforms;
        float timer;
        bool showing { get { return gameObject.activeSelf; } }

        void OnEnable () {
            texts = GetComponentsInChildren<UIText>();
            rectTransforms = GetComponentsInChildren<RectTransform>();

            if (texts.Length == 2) {
                for (int i = 0; i < 2; i++) {
                    texts[i].SetAnchor(i == 0 ? TextAnchor.UpperCenter : TextAnchor.MiddleCenter);
                    texts[i].AdjustAnchorSet(Vector2.zero);
                }
            }
        }
        void Start () {
            if (Application.isPlaying) {
                StopShowing();
            }
        } 

        public void ShowSubtitles (string speaker, string subtitles) {
            this.speaker = speaker;
            this.subtitles = subtitles;

            if (texts.Length == 2) {
                
                texts[0].SetText(speaker + ":", -1);
                texts[1].SetText(subtitles, parameters.maxCharacters);

                // adjust the size of the background rects to match the line heights (approximation)
                
                Vector2 sizeDelta = new Vector2(parameters.singleLineSize.x, parameters.singleLineSize.y * texts[1].currentLines);

                for (int i = 0; i < rectTransforms.Length; i++) 
                    rectTransforms[i].sizeDelta = sizeDelta;
            }

            if (Application.isPlaying) {
                if (!showing) baseObject.SetActive(true);
                timer = 0;
            }
        }

        void StopShowing () {
            baseObject.SetActive(false);
        }
        
        void Update () {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                ShowSubtitles(speaker, subtitles);
            }
#endif
            if (Application.isPlaying) {
                if (showing) {
                    timer += Time.deltaTime;
                    if (timer >= parameters.textDuration) {
                        StopShowing();
                        timer = 0;
                    }
                }
            }
        }
    }
}
