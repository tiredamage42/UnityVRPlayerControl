using System.Collections.Generic;
using UnityEngine;
using System;

namespace SimpleUI {

    [ExecuteInEditMode] public abstract class UIElementHolder : BaseUIElement
    {
        protected abstract float TextScale();
     
        public UITextPanel textPanel;

        public bool isBase;
        public UIElementHolder[] subHolders;
        [HideInInspector] public UIElementHolder parentHolder;


        
        Func<Vector2Int> _runtimeSubmitHandler;
        public Func<Vector2Int> runtimeSubmitHandler { 
            get { return parentHolder != null ? parentHolder.runtimeSubmitHandler : _runtimeSubmitHandler; } 
            set {
                if (parentHolder != null) {
                    parentHolder.runtimeSubmitHandler = value;
                }
                else {
                    _runtimeSubmitHandler = value;
                }
            }
        }

        List<Action<GameObject[], object[]>> onSelectdelegates = new List<Action<GameObject[], object[]>>();
        event Action<GameObject[], object[]> _onSelect;
        public void BroadcastSelectEvent (GameObject[] data, object[] customData) {
            if (parentHolder != null) {
                parentHolder.BroadcastSelectEvent(data, customData);
                return;
            }
            if (_onSelect != null) _onSelect(data, customData);
        }
        public void SubscribeToSelectEvent (Action<GameObject[], object[]> callback) {
            if (parentHolder != null) {
                parentHolder.SubscribeToSelectEvent(callback);
                return;
            }
            _onSelect += callback;
            onSelectdelegates.Add(callback);
        }

        List<Action<GameObject[], object[], Vector2Int>> onSubmitdelegates = new List<Action<GameObject[], object[], Vector2Int>>();
        event Action<GameObject[], object[], Vector2Int> _onSubmit;
        public void BroadcastSubmitEvent (GameObject[] data, object[] customData, Vector2Int submit) {
            if (parentHolder != null) {
                parentHolder.BroadcastSubmitEvent(data, customData, submit);
                return;
            }
            if (_onSubmit != null) _onSubmit(data, customData, submit);
        }
        public void SubscribeToSubmitEvent (Action<GameObject[], object[], Vector2Int> callback) {
            if (parentHolder != null) {
                parentHolder.SubscribeToSubmitEvent(callback);
                return;
            }
            _onSubmit += callback;
            onSubmitdelegates.Add(callback);
        }

        public Action onBaseCancel;
        
        
        public void RemoveAllEvents()
        {
            foreach(var eh in onSelectdelegates) _onSelect -= eh;
            onSelectdelegates.Clear();
            
            foreach(var  eh in onSubmitdelegates) _onSubmit -= eh;
            onSubmitdelegates.Clear();

            runtimeSubmitHandler = null;
        }

        protected abstract SelectableElement ElementPrefab () ;

        bool isHoldersCollection { get { return subHolders != null && subHolders.Length > 0; } }

        Transform _elementsParent;
        Transform elementsParent {
            get {
                if (isHoldersCollection) return null;
                if (_elementsParent == null) _elementsParent = ElementsParent();
                return _elementsParent;
            }
        }


        protected abstract Transform ElementsParent ();
        protected List<SelectableElement> allElements = new List<SelectableElement>();

        void GetSelectableElementReferences () {
            if (isHoldersCollection) return;
                
            allElements.Clear();
                
            SelectableElement[] _allElements = GetComponentsInChildren<SelectableElement>();
            for (int i = 0; i < _allElements.Length; i++) {
                _allElements[i].parentHolder = this;
                allElements.Add(_allElements[i]);                
            }
        }

        public virtual void UpdateSelectableElementHolder () {
            if (isHoldersCollection) return;

            Vector3 textScale = Vector3.one * TextScale();
            
            for (int i = 0; i < allElements.Count; i++) {
                allElements[i].uiText.transform.localScale = textScale;  
                allElements[i].UpdateElement();  
            }
        }

        void InitializeSubSelectableElementHolders () {
            if (!isHoldersCollection) return;
            for (int i = 0 ; i < subHolders.Length; i++) {
                subHolders[i].parentHolder = this;
            }
        }

        void InitializeSelectableElements () {
            GetSelectableElementReferences();
            UpdateSelectableElementHolder();    
        }
        
        protected virtual void OnEnable () {
            InitializeSubSelectableElementHolders();
            InitializeSelectableElements();
        }
            
        protected virtual void OnDisable () {
            if (isHoldersCollection) return;
            for (int i = 0; i < allElements.Count; i++) {
                allElements[i].gameObject.SetActive(true);
                allElements[i].selected = false;
            }
            allElements.Clear();
        }
           
        protected virtual void Update () {

#if UNITY_EDITOR 
            if (!Application.isPlaying) {
                InitializeSelectableElements();
            }
#endif

            if (Application.isPlaying) {

                // check if we hit the cancel button specified in the project's
                // standalone input module
                if (UIManager.currentInput.GetButtonDown(UIManager.standaloneInputModule.cancelButton)) {
                    if (!isBase) {
                        gameObject.SetActive(false);
                        parentHolder.gameObject.SetActive(true);
                    }
                    else {
                        OnBaseCancel();
                    }
                }
            }
        }

        void OnBaseCancel () {
            if (parentHolder != null) {
                parentHolder.OnBaseCancel();
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

        public SelectableElement[] GetAllSelectableElements (int targetCount) {
            if (isHoldersCollection) return null;
            
            int c = allElements.Count;
            if (c < targetCount) {
                int cnt = targetCount - c;
                for (int i = 0 ; i < targetCount - c; i++) {
                    AddNewSelectableElement("Adding new", i == (cnt - 1));
                }
            }
            else if (c > targetCount) {
                for (int i = targetCount ; i < c; i++) {
                    allElements[i].gameObject.SetActive(false);
                }
            }

            return allElements.ToArray();
        }

        public SelectableElement AddNewSelectableElement (string elementText, bool updateHolder) {
            if (isHoldersCollection) return null;
            
            SelectableElement newElement = Instantiate(ElementPrefab());
            newElement.parentHolder = this;

            newElement.transform.SetParent( elementsParent.transform );
            newElement.transform.localScale = Vector3.one;
            newElement.transform.localPosition = Vector3.zero;
            newElement.transform.localRotation = Quaternion.identity;
            
            allElements.Add(newElement);
            newElement.uiText.SetText( elementText, -1);

            if (updateHolder) UpdateSelectableElementHolder();
            return newElement;
        }
    }
}