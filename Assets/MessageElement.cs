using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MessageElement : MonoBehaviour
{
    protected abstract void SetMessage();

    public void DisableMessage () {
        gameObject.SetActive(false);
    }
    public bool isAvailable {
        get {
            return !gameObject.activeSelf;
        }
    }

    public void ShowMessage (
        string message, Color color, float duration, float fadeIn, float fadeOut, TextAlignment textAlignment, TextAnchor textAnchor) {
        gameObject.SetActive(true);
        alpha = 0;
        timer = 0;
        inExit = false;
        this.duration = duration;
        this.fadeIn = fadeIn;
        this.fadeOut = fadeOut;
        this.color = color;
        this.message = message;
        this.textAlignment = textAlignment;
        this.textAnchor = textAnchor;

        SetMessage();
        SetAlpha();
    }
    public void UpdateElement (float deltaTime) {
        if (!inExit) {
            if (alpha != 1) {
                alpha += deltaTime / fadeIn;
                if (alpha > 1) {
                    alpha = 1;
                }
            }
        }
        else {
            if (alpha != 0) {
                alpha -= deltaTime / fadeOut;
                if (alpha < 0) {
                    alpha = 0;
                }
            }
        }
        if (alpha == 1) {
            timer += deltaTime;
            if (timer >= duration) {
                inExit = true;
            }
        }

        SetAlpha ();

        if (inExit && alpha == 0) {
            gameObject.SetActive(false);
        }
    }
    bool inExit;
    protected float alpha;
    protected Color color;
    protected string message;
    protected TextAlignment textAlignment;
    protected TextAnchor textAnchor;
    float timer, duration, fadeIn, fadeOut;
    protected abstract void SetAlpha();
    
}
