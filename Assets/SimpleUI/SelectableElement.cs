using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SimpleUI {

    [System.Serializable] public class UIButtonClickWData : UnityEvent<GameObject[]> {}
    [ExecuteInEditMode] public class SelectableElement : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        public bool selected;
        public string elementText;
        public bool selectInvertsTextColor;

        public UIElementHolder destination;
        [HideInInspector] public UIElementHolder parentHolder;
        [HideInInspector] public bool hasText = true;

        UIText _text;
        public UIText uiText {
            get {
                if (hasText && _text == null) {
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
                if (_rect == null) _rect = GetComponent<RectTransform>();
                return _rect;
            }
        }

        bool hasRuntimeSubmitCallback {
            get {
                return parentHolder.runtimeSubmitHandler != null;
            }
        }
        
        public GameObject[] data;
        public object[] customData;
        public UIButtonClickWData onClick;
        
        void OnEnable () {
            UpdateElement();
        }
        void OnDisable() {
            // selected = false;
        }
        
        //Do this when the selectable UI object is selected.
        public void OnSelect(BaseEventData eventData)
        {
            // Debug.Log("Selected " + name);
            selected = true;
            UpdateElement();
            parentHolder.BroadcastSelectEvent(data, customData);
        }

        void Update () {
            if (!Application.isPlaying) return;
            if (!selected) return;
            if (!hasRuntimeSubmitCallback) return;
            
            Vector2Int alternativeSubmit = parentHolder.runtimeSubmitHandler();
            if (alternativeSubmit.x >= 0) {
                DoSubmit(alternativeSubmit);
            }
        }


        void DoSubmit (Vector2Int submitAction) {
            // Debug.Log("Submitted on " + name);
            
            if (onClick != null) {
                onClick.Invoke(data);
            }

            parentHolder.BroadcastSubmitEvent(data, customData, submitAction);

            // trigger other ui holder active (page switching...)
            if (destination != null) {
                destination.gameObject.SetActive(true);

                if (!destination.isBase) {
                    destination.parentHolder = parentHolder;
                }
                parentHolder.gameObject.SetActive(false);
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (hasRuntimeSubmitCallback) return;
         
            DoSubmit(Vector2Int.zero);
        }

        public void OnDeselect(BaseEventData data)
        {
            selected = false;
            UpdateElement();
        }

        Image _mainImage;
        public Image mainImage {
            get {
                if (_mainImage == null) _mainImage = GetComponent<Image>();
                return _mainImage;
            }
        }   
        
        public void UpdateElement () {
            mainImage.color = selected ? UIManager.instance.mainLightColor : (Color32)Color.clear;

            gameObject.name = elementText + "_Button";
            if (hasText) {
                uiText.SetText(elementText);
                if (selectInvertsTextColor) {
                    uiText.invert = selected;
                    uiText.UpdateColors();
                }
            }
        }
    }
}
