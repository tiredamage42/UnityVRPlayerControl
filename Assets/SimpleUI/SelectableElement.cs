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


        [HideInInspector] public bool hasText = true;

        UIText _text;
        public UIText uiText {
            get {
                if (_text == null && hasText) {
                    _text = GetComponentInChildren<UIText>();
                    if (_text == null) {
                        hasText = false;
                    }
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
        public RectTransform UITextRectTransform {
            get {
                if (_textRect == null && hasText) {
                    UIText t = uiText;

                    if (hasText) {

                        _textRect = uiText.GetComponent<RectTransform>();
                    }
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
    protected virtual void OnDisable() {
        // selected = false;
    }


    

        
    //Do this when the selectable UI object is selected.
    public void OnSelect(BaseEventData eventData)
    {
        // Debug.Log("Selected " + name);
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

    void Update () {
        if (Application.isPlaying) {
            if (parentHolder.needsInput) {
                if (selected) {
                    if (parentHolder.getAlternativeSubmitToUse != null) {
                        int alternativeSubmit = parentHolder.getAlternativeSubmitToUse();
                        if (alternativeSubmit >= 0) {
                            if (alternativeSubmit == 0) {
                                Debug.LogError("dont use action 0 for alternative ui submit, it's already used internally");
                            }
                            else {
                            
                                DoSubmit(alternativeSubmit);
                            }
                        }
                    }
                }
            }
        }
    }


    void DoSubmit (int submitAction) {
        // Debug.Log("Submitted on " + name);
        
        if (onClick != null) {
            onClick.Invoke(data);
        }

        if (parentHolder.onSubmitToUse != null) {
            parentHolder.onSubmitToUse(data, customData, submitAction);
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

    public void OnSubmit(BaseEventData eventData)
    {
        DoSubmit(0);
        // // Debug.Log("Submitted on " + name);
        
        // if (onClick != null) {
        //     onClick.Invoke(data);
        // }

        // if (parentHolder.onSubmitToUse != null) {
        //     parentHolder.onSubmitToUse(data, customData);
        // }

        // // if (onSubmit != null) {
        // //     onSubmit(data, customData);
        // // }
        // // if (onClickWCustomData != null) {
        // //     onClickWCustomData(customData);
        // // }

        // if (destination != null) {
        //     destination.gameObject.SetActive(true);

        //     if (!destination.isBase) {
        //         destination.parentHolder = parentHolder;
        //     }
        //     parentHolder.gameObject.SetActive(false);
        // }

        // OnSubmit();

        // // if (onSubmit != null) {
        // //     onSubmit(this);
        // // }
    }
    public void OnDeselect(BaseEventData data)
    {
        selected = false;
        OnDeselect();
        _UpdateElement();
    }


    public void _UpdateElement () {
        if (hasText) {
            uiText.SetText(elementText);
        }
        UpdateElement();
    }
    

    protected abstract void UpdateElement ();

        protected abstract void OnSelect ();

        protected abstract void OnDeselect ();
protected abstract void OnSubmit ();

        




    }
}
