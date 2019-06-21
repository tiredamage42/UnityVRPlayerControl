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
    public UIRadialElement radialElementPrefab;


    HashSet<GameObject> shownUIsWithInput = new HashSet<GameObject>();

    public static event System.Action<UIElementHolder> onUIShow;
    static IEnumerator _ShowUI (GameObject uiObject, bool needsInput, bool tryRepeat) {
        uiObject.SetActive(true);

        if (tryRepeat) {
            yield return null;
            uiObject.SetActive(false);
            yield return null;
            uiObject.SetActive(true);
            yield return null;
            uiObject.SetActive(false);
            yield return null;
            uiObject.SetActive(true);
        }
						
        if (needsInput) {
            instance.gameObject.SetActive(true);
            instance.shownUIsWithInput.Add(uiObject);
        }

    }
    static void ShowUI (GameObject uiObject, bool needsInput, bool tryRepeat) {
        instance.StartCoroutine(_ShowUI(uiObject, needsInput, tryRepeat));
    }
    static GameObject GetUIObj (UIElementHolder uiObject) {
        return uiObject.baseObject != null ? uiObject.baseObject : uiObject.gameObject;
    }
    public static void ShowUI(UIElementHolder uiObject, bool needsInput, bool tryRepeat) {
        
        GameObject objToUse = GetUIObj(uiObject);
        // uiObject.baseObject;
        // if (objToUse == null) {
        //     objToUse = uiObject.gameObject;
        // }
        ShowUI(objToUse, needsInput, tryRepeat);

        if (onUIShow != null) {
            onUIShow(uiObject);
        }
    }

    public static void HideUI (UIElementHolder uiObject) {
        GameObject objToUse = GetUIObj(uiObject);
        

        // GameObject objToUse = uiObject.baseObject;
        // if (objToUse == null) {
        //     objToUse = uiObject.gameObject;
        // }
        HideUI(objToUse);
    }


        

    static void HideUI (GameObject uiObject) {
        if (instance.shownUIsWithInput.Contains(uiObject)) {
            instance.shownUIsWithInput.Remove(uiObject);
            if (instance.shownUIsWithInput.Count == 0) {
                instance.gameObject.SetActive(false);
            }
        }
        uiObject.SetActive(false);
    }

    // public static UIPage ShowPage (Transform parent, string title, int width, int lineHeight, float scale = 1f) {
    //     UIPage page = Instantiate(instance.pagePrefab);
    //     page.SetTitle(title);
    //     page.SetSize(width, lineHeight);
    //     page.transform.parent = parent;
    //     page.transform.localPosition = Vector3.zero;
    //     page.transform.localScale = Vector3.one * scale;
    //     return page;
    // }


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


//     public static string verticalAxis {
//         get {
//             return standaloneInputModule.verticalAxis;
//         }
//     }
// public static string horizontalAxis {
//         get {
//             return standaloneInputModule.horizontalAxis;
//         }
//     }
public static string cancelButton {
        get {
            return standaloneInputModule.cancelButton;
        }
    }
// public static string submitButton {
//         get {
//             return standaloneInputModule.submitButton;
//         }
//     }




    public static BaseInput input {
        get {

        if (standaloneInputModule == null) {
            Debug.LogError("No standaloneInputModule in scene or UIInputManager");
            return null;
        }
        return standaloneInputModule.GetInput();
        // if (standaloneInputModule.inputOverride != null) {
        //     return standaloneInputModule.inputOverride;
        // }
        // return standaloneInputModule.input;
        }
    }
}

}

