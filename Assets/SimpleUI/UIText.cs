using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI {
    /*
        used to keep text in ui consistent
    */
    [RequireComponent(typeof(Text))] [RequireComponent(typeof(Outline))]
    [ExecuteInEditMode] public class UIText : UIGraphic<Text>
    {

        Text text { get { return mainGraphic; } }
        
        Outline _outline;
        Outline outline {
            get {
                if (_outline == null) _outline = GetComponent<Outline>();
                return _outline;
            }
        }
        public int currentLines = 1;
        const char lineBreak = '\n';
        const string lineBreakAdd = "-\n";

        public static string AdjustTextToMaxLength (string input, int maxCharacters, out int lines) {
            lines = 1;

            int length = input.Length;
            string adjusted = "";
            int l = 0;
            for (int i = 0; i < length; i++) {                
                adjusted += input[i];
                l++;
                if (input[i] == lineBreak) {
                    l = 0;
                    lines++;
                }
                else {
                    if ( l == maxCharacters ) {
                        adjusted += lineBreakAdd;
                        l = 0;
                        lines++;
                    }
                }
            }
            return adjusted;
        }
        static int LineCount (string input) {
            int lines = 1;
            int length = input.Length;
            for (int i = 0; i < length; i++) {                
                if (input[i] == lineBreak) lines++;
            }
            return lines;
        }

        
        public void SetText (string text, int maxLineChars) {
            if (maxLineChars > 0) {
                this.text.text = AdjustTextToMaxLength (text, maxLineChars, out currentLines);
            }
            else {
                this.text.text = text;
                currentLines = LineCount(text);
            }
        }
        public void SetAnchor(TextAnchor textAnchor) {
            text.alignment = textAnchor;
        }

        public void AdjustAnchorSet (Vector2 offsets){
            
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

        public override void UpdateGraphicColors() {
            base.UpdateGraphicColors();
            if (!overrideColors) outline.effectColor = UIManager.GetColor(colorScheme, !useDark );
        }
    }
}

