using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace SimpleUI {



[System.Serializable]
public class UIButtonClickWData : UnityEvent<GameObject[]>
{
}
[ExecuteInEditMode]
public class UIButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{

    public UIPage pageDestination;
    [HideInInspector] public Text text;
    [HideInInspector] public RectTransform rectTransform, textRectTransform;
    [HideInInspector] public UIPage parentPage;

    public GameObject[] data;
    
    Button button;
    Image buttonImage;


    public UIButtonClickWData onClick;


    public string buttonText = "newButton";
    
    public void SetButtonText(string text) {
        buttonText = text;
    }
    
    void OnButtonClick () {
        if (pageDestination != null) {
            pageDestination.gameObject.SetActive(true);
            pageDestination.parentPage = parentPage;
            parentPage.gameObject.SetActive(false);
        }
        else if (onClick != null) {
            onClick.Invoke(data);
        }
        
    }

    void OnEnable () {
        button = GetComponent<Button>();
        text = GetComponentInChildren<Text>();
        buttonImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        textRectTransform = text.GetComponent<RectTransform>();
        
        text.color = UIManager.instance.mainLightColor;
        buttonImage.color = Color.clear;// UIManager.instance.mainDarkColor;
        

        if (Application.isPlaying) {
            button.onClick.AddListener(OnButtonClick);
        }
    }
    void OnDisable () {
        if (Application.isPlaying) {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }


    // void Awake () {
    // }

    

     //Do this when the selectable UI object is selected.
    public void OnSelect(BaseEventData eventData)
    {
        buttonImage.color = UIManager.instance.mainLightColor;
        text.color = UIManager.instance.mainDarkColor;
        Debug.Log(this.gameObject.name + " was selected");
    }
    public void OnDeselect(BaseEventData data)
    {
        text.color = UIManager.instance.mainLightColor;
        buttonImage.color = Color.clear;// UIManager.instance.mainDarkColor;
        // Debug.Log("Deselected");
    }

}

}