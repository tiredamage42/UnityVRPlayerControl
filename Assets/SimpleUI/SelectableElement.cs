using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SimpleUI {

    [System.Serializable]
    public class UIButtonClickWData : UnityEvent<GameObject[]>
    {
    }
    
    public abstract class SelectableElement : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler
    {

        public bool selected;
        public string elementText;

        public UIElementHolder destination;
        [HideInInspector] public UIElementHolder parentHolder;


        UIText _text;
        public UIText text {
            get {
                if (_text == null) {
                    _text = GetComponentInChildren<UIText>();
                }
                return _text;
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
        RectTransform _textRect;
        public RectTransform textRectTransform {
            get {
                if (_textRect == null) {
                    _textRect = text.GetComponent<RectTransform>();
                }
                return _textRect;
            }
        }

        public GameObject[] data;
        public object[] customData;
        public UIButtonClickWData onClick;
        

        // public System.Action<object[]> onClickWCustomData;
        // public System.Action<GameObject[], object[]> onSubmit, onSelect;
    
    // public System.Action onSelect;
    // public System.Action<SelectableElement> onSubmit;
    


    protected virtual void OnEnable () {
        _UpdateElement();
    }


    

        
    //Do this when the selectable UI object is selected.
    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log("Selected " + name);
        selected = true;
        OnSelect();
        _UpdateElement();



        if (parentHolder.onSelectToUse != null) {
            parentHolder.onSelectToUse(data, customData);
        }


        // if (onSelect != null) {
        //     onSelect(data, customData);
        // }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        // Debug.Log("Submitted on " + name);
        
        if (onClick != null) {
            onClick.Invoke(data);
        }

        if (parentHolder.onSubmitToUse != null) {
            parentHolder.onSubmitToUse(data, customData);
        }

        // if (onSubmit != null) {
        //     onSubmit(data, customData);
        // }
        // if (onClickWCustomData != null) {
        //     onClickWCustomData(customData);
        // }

        if (destination != null) {
            destination.gameObject.SetActive(true);

            if (!destination.isBase) {
                destination.parentHolder = parentHolder;
            }
            parentHolder.gameObject.SetActive(false);
        }

        OnSubmit();

        // if (onSubmit != null) {
        //     onSubmit(this);
        // }
    }
    public void OnDeselect(BaseEventData data)
    {
        selected = false;
        OnDeselect();
        _UpdateElement();
    }


    public void _UpdateElement () {
        text.SetText(elementText);
        UpdateElement();
    }
    

    protected abstract void UpdateElement ();

        protected abstract void OnSelect ();

        protected abstract void OnDeselect ();
protected abstract void OnSubmit ();

        




    }
}
