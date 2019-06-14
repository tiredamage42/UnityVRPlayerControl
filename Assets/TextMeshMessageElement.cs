using UnityEngine;
namespace UIMessaging {
    public class TextMeshMessageElement : MessageElement {
        
        public int fontSize = 120;
        public float characterSize = .001f;
        TextMesh messageText;
        
        protected override void SetMessage() {
            
            if (messageText == null)
                messageText = gameObject.AddComponent<TextMesh>();
            
            messageText.fontSize = fontSize;
            messageText.characterSize = characterSize;
            messageText.alignment = textAlignment;
            messageText.anchor = textAnchor;
            messageText.color = color;
            messageText.text = message;
        }

        protected override void SetAlpha() {
            Color c = messageText.color;
            c.a = alpha;
            messageText.color = c;
        }
    }
}