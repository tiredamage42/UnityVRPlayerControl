using UnityEngine;

namespace SimpleUI {

    [System.Serializable] public class TextPanelParameters {
        public Vector2 rectSize = new Vector2 (4, 4);
        [Header("Range(0,1)")] public Vector2 textOffsets = new Vector2(.05f,.05f);
        public float textScale = 0.005f;
        public int maxCharacters = 64;
        public TextAnchor textAlignment = TextAnchor.UpperLeft;

        public TextPanelParameters(Vector2 rectSize, Vector2 textOffsets, float textScale, TextAnchor textAlignment, int maxCharacters) {
            this.rectSize = rectSize;
            this.textOffsets = textOffsets;
            this.textScale = textScale;
            this.textAlignment = textAlignment;
            this.maxCharacters = maxCharacters;
        }
        public TextPanelParameters () {
            this.rectSize = new Vector2 (4, 4);
            this.textOffsets = new Vector2(.05f,.05f);
            this.textScale = 0.005f;
            this.textAlignment = TextAnchor.UpperLeft;
            this.maxCharacters = 64;
        }
    }


    [ExecuteInEditMode] public class UITextPanel : BaseUIElement
    {
        public TextPanelParameters parameters = new TextPanelParameters();
        [TextArea] public string panelText;        
        UIText text;
        RectTransform[] panelRects;

        void OnEnable () {
            text = GetComponentInChildren<UIText>();

            panelRects = new RectTransform[] {
                GetComponent<RectTransform>(),
                transform.GetChild(0).GetComponent<RectTransform>(),
            };
            SetRectSizes();
        }

        void SetRectSizes () {
            for (int i = 0; i < panelRects.Length; i++) {
                panelRects[i].sizeDelta = parameters.rectSize;
            }

            text.transform.localScale = Vector3.one * parameters.textScale;
            text.SetAnchor( parameters.textAlignment );
            text.AdjustAnchorSet(parameters.textOffsets);
        }

        public void SetText(string newText) {
            panelText = newText;
            text.SetText(panelText, parameters.maxCharacters);
        }

    #if UNITY_EDITOR
        void Update () {
            if (!Application.isPlaying) {
                SetRectSizes();
                SetText(panelText);
            }
        }
    #endif

    }
}
