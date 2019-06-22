using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace SimpleUI {

    // public delegate void ElementCallback (GameObject[] data, object[] customData);

    [ExecuteInEditMode]
    public abstract class UIElementHolder : MonoBehaviour
    {

        public GameObject baseObject;
        EventSystem _evsys;
        public EventSystem eventSystem {
            get {
                if (_evsys == null) {
                    _evsys = GameObject.FindObjectOfType<EventSystem>();
                }
                return _evsys;
            }
        }

        
        public bool needsDefaultSelection = true;
        public float textScale = 0.125f;
        public float scale = 0.01f;

        [HideInInspector] public UIElementHolder parentHolder;
        public bool isBase;




        // public ElementCallback onSelect;
        // ElementCallback _onSelect {
        //     get {
        //         if (parentHolder != null) {
        //             return parentHolder._onSelect;
        //         }
        //         return onSelect;
        //     }
        // }
        // public System.Action<GameObject[], object[]> onSubmit;
        // System.Action<GameObject[], object[]> _onSubmit {
        //     get {
        //         if (parentHolder != null) {
        //             return parentHolder._onSubmit;
        //         }
        //         return onSubmit;
        //     }
        // }


        

        List<System.Action<GameObject[], object[]>> onSelectdelegates = new List<System.Action<GameObject[], object[]>>();
        event System.Action<GameObject[], object[]> onSelect;
        public System.Action<GameObject[], object[]> onSelectToUse {
            get {
                if (parentHolder != null) {
                    return parentHolder.onSelectToUse;
                }
                return onSelect;
            }
        }
        
        public event System.Action<GameObject[], object[]> onSelectEvent {
            add {
                onSelect += value;
                onSelectdelegates.Add(value);
            }
            remove{
                onSelect -= value;
                onSelectdelegates.Remove(value);
            }
        }


        List<System.Action<GameObject[], object[]>> onSubmitdelegates = new List<System.Action<GameObject[], object[]>>();
        event System.Action<GameObject[], object[]> onSubmit;
        public System.Action<GameObject[], object[]> onSubmitToUse {
            get {
                if (parentHolder != null) {
                    return parentHolder.onSubmitToUse;
                }
                return onSubmit;
            }
        }
        
        public event System.Action<GameObject[], object[]> onSubmitEvent {
            add {
                onSubmit += value;
                onSubmitdelegates.Add(value);
            }
            remove{
                onSubmit -= value;
                onSubmitdelegates.Remove(value);
            }
        }

        public void RemoveAllEvents()
        {
            foreach(System.Action<GameObject[], object[]> eh in onSelectdelegates)
            {
                onSelect -= eh;
            }
            onSelectdelegates.Clear();
            
            foreach(System.Action<GameObject[], object[]> eh in onSubmitdelegates)
            {
                onSubmit -= eh;
            }
            onSubmitdelegates.Clear();
        }



        protected abstract SelectableElement ElementPrefab () ;
        protected abstract Image Background ();
        protected abstract Image BackgroundOverlay ();


        Image _backgroundPanel;
        protected Image backGround {
            get {
                if (_backgroundPanel == null) {
                    _backgroundPanel = Background();
                }
                return _backgroundPanel;
            }
        }
        Image _overlayPanel;
        protected Image backGroundOverlay {
            get {
                if (_overlayPanel == null) {
                    _overlayPanel = BackgroundOverlay();
                }
                return _overlayPanel;
            }
        }

        
        RectTransform _backgroundPanelRect;
        protected RectTransform backGroundRect {
            get {
                if (_backgroundPanelRect == null) {
                    _backgroundPanelRect = backGround.GetComponent<RectTransform>();
                }
                return _backgroundPanelRect;
            }
        }
        RectTransform _overlayPanelRect;
        protected RectTransform backGroundOverlayRect {
            get {
                if (_overlayPanelRect == null) {
                    _overlayPanelRect = backGroundOverlay.GetComponent<RectTransform>();
                }
                return _overlayPanelRect;
            }
        }

        


        public event System.Action onBaseCancel;
        

        protected List<SelectableElement> allElements = new List<SelectableElement>();

        void GetElementReferences () {
            allElements.Clear();
            SelectableElement[] _allElements = GetComponentsInChildren<SelectableElement>();
            for (int i = 0; i < _allElements.Length; i++) {
                _allElements[i].parentHolder = this;


                // _allElements[i].onSelect = _onSelect;
                // _allElements[i].onSubmit = _onSubmit;
                
                allElements.Add(_allElements[i]);                
            }
        }

        protected virtual void UpdateElementHolder () {

            backGround.color = UIManager.instance.mainDarkColor;
            backGroundOverlay.color = UIManager.instance.mainLightColor;
            transform.localScale = Vector3.one * scale;

            for (int i = 0; i < allElements.Count; i++) {
                UIText t = allElements[i].uiText;
                if (allElements[i].hasText) {

                    t.transform.localScale = Vector3.one * textScale;  
                }
                allElements[i]._UpdateElement();  
            }

            if (Application.isPlaying) {
                if (needsDefaultSelection && allElements.Count > 0) {
                    SelectOnEnable selectOnEnable = GetComponent<SelectOnEnable>();
                    if (selectOnEnable != null) {
                        selectOnEnable.toSelect = allElements[0].gameObject;
                    }
                }
            }
        }

        protected virtual void OnEnable () {
            GetElementReferences();
            UpdateElementHolder();    
        }
        protected virtual void OnDisable () {
            allElements.Clear();

            RemoveAllEvents();
        }
        protected virtual void Update () {
            if (!Application.isPlaying) {
                GetElementReferences();
                UpdateElementHolder();
            }

            if (Application.isPlaying) {
                if (UIManager.input.GetButtonDown(UIManager.cancelButton)) {
                    if (parentHolder != null) {
                        gameObject.SetActive(false);
                        parentHolder.gameObject.SetActive(true);
                    }
                    else {
                        if (onBaseCancel != null) {
                            onBaseCancel ();
                        }
                        else {
                            Debug.LogError(name + " has no onBaseCancel");
                        }
                    }
                }
            }
        }

        public SelectableElement AddNewElement (string elementText) {
            SelectableElement newElement = Instantiate(ElementPrefab());
            newElement.parentHolder = this;
            newElement.transform.SetParent( backGroundOverlay.transform );
            newElement.transform.localScale = Vector3.one;
            newElement.transform.localPosition = Vector3.zero;
            newElement.transform.localRotation = Quaternion.identity;
            
            allElements.Add(newElement);
            newElement.uiText.SetText( elementText );
            UpdateElementHolder();
            return newElement;
        }



        



    }

}