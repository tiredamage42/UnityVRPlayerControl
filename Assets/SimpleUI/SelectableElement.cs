using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SimpleUI {

    [System.Serializable] public class UIButtonClickWData : UnityEvent<GameObject[]> {}
    [ExecuteInEditMode] public class SelectableElement : MonoBehaviour, ISelectHandler, IDeselectHandler//, ISubmitHandler
    {
        public bool selectInvertsTextColor;
        public UIElementHolder destination;
        public bool selected;
        [HideInInspector] public UIElementHolder parentHolder;
        
        UIText _text;
        public UIText uiText {
            get {
                if (_text == null) _text = GetComponentInChildren<UIText>();
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
        Image _mainImage;
        public Image mainImage {
            get {
                if (_mainImage == null) _mainImage = GetComponentsInChildren<Image>()[0];
                return _mainImage;
            }
        }   
        
        public GameObject[] data;
        public object[] customData;
        public UIButtonClickWData onClick;
        
        void OnEnable () {
            UpdateElement();
        }

        public void UpdateElement () {
            mainImage.color = selected ? UIManager.instance.mainLightColor : (Color32)Color.clear;
            
            // prefab editing scene was messing up because of this... 
            // so check if we're in a real scene context
            // if (parentHolder != null) gameObject.name = elementText + "_Button";
            
            if (selectInvertsTextColor) uiText.SetColorScheme(uiText.colorScheme, selected);
        }
        
        //Do this when the selectable UI object is selected.
        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
            UpdateElement();
            parentHolder.BroadcastSelectEvent(gameObject, data, customData);
        }
        public void OnDeselect(BaseEventData data)
        {
            selected = false;
            UpdateElement();
        }
       
        public void DoSubmit (Vector2Int submitAction) {
            // Debug.Log("Submitted on " + name);
            
            if (onClick != null) {
                onClick.Invoke(data);
            }

            // trigger other ui holder active (page switching...)
            if (destination != null) {
                destination.gameObject.SetActive(true);

                if (!destination.isBase) {
                    destination.parentElement = parentHolder;
                }
                parentHolder.gameObject.SetActive(false);
            }
        }

    }
}
