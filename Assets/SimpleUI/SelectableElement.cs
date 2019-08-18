using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SimpleUI {

    [System.Serializable] public class UIButtonClickWData : UnityEvent<GameObject[]> {}
    [ExecuteInEditMode] public class SelectableElement : MonoBehaviour, ISelectHandler, IDeselectHandler//, ISubmitHandler
    {
        public string elementText;
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
        
        // bool hasRuntimeSubmitCallback { get { return parentHolder.runtimeSubmitHandler != null; } }
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
            if (parentHolder != null) gameObject.name = elementText + "_Button";
            
            uiText.SetText(elementText, -1);
            if (selectInvertsTextColor) uiText.SetColorScheme(uiText.colorScheme, selected);
        }
        
        //Do this when the selectable UI object is selected.
        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
            UpdateElement();
            parentHolder.BroadcastSelectEvent(data, customData);
        }
        public void OnDeselect(BaseEventData data)
        {
            selected = false;
            UpdateElement();
        }
        // public void OnSubmit(BaseEventData eventData)
        // {
        //     if (hasRuntimeSubmitCallback) return;
        //     DoSubmit(Vector2Int.zero);
        // }

        // void Update () {
        //     if (!Application.isPlaying || !selected || !hasRuntimeSubmitCallback) return;
            
        //     Vector2Int alternativeSubmit = parentHolder.runtimeSubmitHandler();
        //     if (alternativeSubmit.x >= 0) {
        //         DoSubmit(alternativeSubmit);
        //     }
        // }


        public void DoSubmit (Vector2Int submitAction) {
            // Debug.Log("Submitted on " + name);
            
            if (onClick != null) {
                onClick.Invoke(data);
            }

            // parentHolder.BroadcastSubmitEvent(data, customData, submitAction);

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
