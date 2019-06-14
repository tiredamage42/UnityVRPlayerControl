using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TextMeshMessageElement : MessageElement
{

    TextMesh messageText;
    public int fontSize = 120;
    public float characterSize = .001f;
    
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