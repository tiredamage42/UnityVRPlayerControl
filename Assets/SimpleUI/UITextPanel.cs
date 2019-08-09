using UnityEngine;

namespace SimpleUI {

    [System.Serializable] public class TextPanelParameters {
        public Vector2 rectSize = new Vector2 (4, 4);
        [Header("Range(0,1)")] public Vector2 textOffsets = new Vector2(.05f,.05f);
        public float textScale = 0.005f;
        public TextAnchor textAlignment = TextAnchor.UpperLeft;
        public int maxCharacters = 64;
        [TextArea] public string panelText;

        public TextPanelParameters(
            Vector2 rectSize, Vector2 textOffsets, float textScale = 0.005f, TextAnchor textAlignment = TextAnchor.UpperLeft, int maxCharacters = 64
        ) {

            this.rectSize = rectSize;
            this.textOffsets = textOffsets;
            this.textScale = textScale;
            this.textAlignment = textAlignment;
            this.maxCharacters = maxCharacters;
            this.panelText = "";
        }
        public TextPanelParameters () {
            this.rectSize = new Vector2 (4, 4);
            this.textOffsets = new Vector2(.05f,.05f);
            this.textScale = 0.005f;
            this.textAlignment = TextAnchor.UpperLeft;
            this.maxCharacters = 64;
            this.panelText = "";
        }
    }


    [ExecuteInEditMode] public class UITextPanel : MonoBehaviour
    {
        public TextPanelParameters parameters = new TextPanelParameters();
        
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
            text.AdjustAnchorSet(parameters.textOffsets);// textOffset, textYOffset);
        }


        const char lineBreak = '\n';
        const string lineBreakAdd = "-\n";

        public void SetText(string newText) {
            parameters.panelText = newText;
            int length = parameters.panelText.Length;
            string adjusted = "";
            int l = 0;
            for (int i = 0; i < length; i++) {                
                adjusted += parameters.panelText[i];
                l++;
                if (parameters.panelText[i] == lineBreak) {
                    l = 0;
                }
                else {
                    if ( l == parameters.maxCharacters ) {
                        adjusted += lineBreakAdd;
                        l = 0;
                    }
                }
            }
            text.SetText(adjusted);
        }

    #if UNITY_EDITOR
        void Update () {
            SetRectSizes();
            SetText(parameters.panelText);
        }
    #endif

    }
}
