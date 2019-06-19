using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleUI {

public class UIManager : MonoBehaviour
{

    public Color32 mainLightColor = Color.red, mainDarkColor = Color.blue;

    public UIButton buttonPrefab;
    public UIPage pagePrefab;


    public static UIPage ShowPage (Canvas parentCanvas, string title, int width, int lineHeight, float scale = 1f) {
        UIPage page = Instantiate(instance.pagePrefab);
        page.SetTitle(title);
        page.SetSize(width, lineHeight);
        page.transform.parent = parentCanvas.transform;
        page.transform.localPosition = Vector3.zero;
        page.transform.localScale = Vector3.one * scale;
        return page;
    }


    static StandaloneInputModule standaloneInputModule;
    
    static UIManager _i;
    public static UIManager instance {
        get {
            if (_i == null) {
                _i = GameObject.FindObjectOfType<UIManager>();
            }
            return _i;
        }

    }
    
    void Awake () {
        standaloneInputModule = GetComponent<StandaloneInputModule>();
    }


    public static string verticalAxis {
        get {
            return standaloneInputModule.verticalAxis;
        }
    }
public static string horizontalAxis {
        get {
            return standaloneInputModule.horizontalAxis;
        }
    }
public static string cancelButton {
        get {
            return standaloneInputModule.cancelButton;
        }
    }
public static string submitButton {
        get {
            return standaloneInputModule.submitButton;
        }
    }




    public static BaseInput input {
        get {

        if (standaloneInputModule == null) {
            Debug.LogError("No standaloneInputModule in scene or UIInputManager");
            return null;
        }
        if (standaloneInputModule.inputOverride != null) {
            return standaloneInputModule.inputOverride;
        }
        return standaloneInputModule.input;
        }
    }
}

}

