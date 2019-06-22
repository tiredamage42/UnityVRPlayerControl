using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI {

    [ExecuteInEditMode]
    public class UIText : MonoBehaviour
    {
        Text _text;
        Text text {
            get {
                if (_text == null) {
                    _text = GetComponent<Text>();
                }
                return _text;
            }
        }

        Outline _outline;
        Outline outline {
            get {
                if (_outline == null) {
                    _outline = GetComponent<Outline>();
                }
                return _outline;
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
        
        public bool invert;
        void OnEnable () {
            UpdateColors();
        }

        public void SetText (string text) {
            this.text.text = text;
        }
        public void SetAnchor(TextAnchor textAnchor) {
            text.alignment = textAnchor;
        }
        public void SetAlpha (float alpha) {
            Color c = text.color;
            c.a = alpha;
            text.color = c;

            c = outline.effectColor;
            c.a = alpha;
            outline.effectColor = c;
        }

        public void UpdateColors () {
            text.color = invert ? UIManager.instance.mainDarkColor : UIManager.instance.mainLightColor;
            outline.effectColor = invert ? UIManager.instance.mainLightColor : UIManager.instance.mainDarkColor;
        }
    }
}

