using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI {
    [ExecuteInEditMode] public class UIText : MonoBehaviour
    {
        Text _text;
        Text text {
            get {
                if (_text == null) _text = GetComponent<Text>();
                return _text;
            }
        }

        Outline _outline;
        Outline outline {
            get {
                if (_outline == null) _outline = GetComponent<Outline>();
                return _outline;
            }
        }
        public RectTransform rectTransform { get { return text.rectTransform; } }
        
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

        public void AdjustAnchorSet (Vector2 offsets){//float marginOffset, float yOffset) {
            
            bool isMiddle = text.alignment == TextAnchor.MiddleCenter || text.alignment == TextAnchor.MiddleRight || text.alignment == TextAnchor.MiddleLeft;
            bool isLower = text.alignment == TextAnchor.LowerCenter || text.alignment == TextAnchor.LowerRight || text.alignment == TextAnchor.LowerLeft;
            bool isUpper = text.alignment == TextAnchor.UpperCenter || text.alignment == TextAnchor.UpperRight || text.alignment == TextAnchor.UpperLeft;
            
            bool isCenter = text.alignment == TextAnchor.MiddleCenter || text.alignment == TextAnchor.LowerCenter || text.alignment == TextAnchor.UpperCenter;
            bool isRight = text.alignment == TextAnchor.MiddleRight || text.alignment == TextAnchor.LowerRight || text.alignment == TextAnchor.UpperRight;
            bool isLeft = text.alignment == TextAnchor.MiddleLeft || text.alignment == TextAnchor.LowerLeft || text.alignment == TextAnchor.UpperLeft;

            Vector2 anchorPivot = Vector2.zero;

            if (isMiddle) anchorPivot.y = .5f;
            else if (isLower) anchorPivot.y = offsets.y;
            else if (isUpper) anchorPivot.y = 1 - offsets.y;

            if (isCenter) anchorPivot.x = .5f;
            else if (isRight) anchorPivot.x = 1-offsets.x;
            else if (isLeft) anchorPivot.x = offsets.x;
            
            rectTransform.anchorMin = anchorPivot;
            rectTransform.anchorMax = anchorPivot;
            rectTransform.pivot = anchorPivot;
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

