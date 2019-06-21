using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace SimpleUI {

[ExecuteInEditMode]
public class UIButton : SelectableElement
{

        


        Image _mainImage;
        Image buttonImage {
            get {
                if (_mainImage == null) {
                    _mainImage = GetComponent<Image>();
                }
                return _mainImage;
            }
        }
        


    // public UIPage pageDestination;
    // [HideInInspector] public UIPage parentPage;
    
    // Image buttonImage;

    // public string buttonText = "newButton";
    
    // public void SetButtonText(string text) {
    //     buttonText = text;
    // }
    
    // void OnButtonClick () {
        
        
    // }


    protected override void UpdateElement () {
        buttonImage.color = selected ? UIManager.instance.mainLightColor : UIManager.instance.mainDarkColor;
        text.invert = selected;
        text.UpdateColors();
    }

    // void OnEnable () {
        // buttonImage = GetComponent<Image>();
        

        // OnDeselect();
        

        // if (Application.isPlaying) {
        //     Button button = GetComponent<Button>();
        //     button.onClick.AddListener(OnButtonClick);
        // }
    // }
    // void OnDisable () {
        // if (Application.isPlaying) {
        //     Button button = GetComponent<Button>();
        //     button.onClick.RemoveListener(OnButtonClick);
        // }
    // }

    protected override void OnSubmit () {
        // if (pageDestination != null) {
        //     pageDestination.gameObject.SetActive(true);
        //     pageDestination.parentPage = parentPage;
        //     parentPage.gameObject.SetActive(false);
        // }
    }

    //Do this when the selectable UI object is selected.
    protected override void OnSelect()
    {
        // UpdateButton();

        // buttonImage.color = UIManager.instance.mainLightColor;
        // text.invert = true;
        // text.UpdateColors();
    }

    protected override void OnDeselect()
    {
        // UpdateButton();
        
        // buttonImage.color = Color.clear;
        // text.invert = false;
        // text.UpdateColors();
    }

}

}