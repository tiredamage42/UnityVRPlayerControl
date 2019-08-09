using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace SimpleUI {


    
    [ExecuteInEditMode] public abstract class UIElementHolder : MonoBehaviour
    {
        public float textScale = 0.005f;


     
        public GameObject baseObject;
        public UITextPanel textPanel;

        
        // EventSystem _evsys;
        // public EventSystem eventSystem {
        //     get {
        //         if (_evsys == null) {
        //             _evsys = GameObject.FindObjectOfType<EventSystem>();
        //         }
        //         return _evsys;
        //     }
        // }

        
        // public bool needsDefaultSelection = true;


        
        [HideInInspector] public UIElementHolder parentHolder;
        public bool isBase;
        public UIElementHolder[] subHolders;

        void InitializeSubHolders () {
            if (subHolders != null) {

            for (int i = 0 ; i < subHolders.Length; i++) {
                subHolders[i].parentHolder = this;
            }
            }
        }


        
        System.Func<Vector2Int> _runtimeSubmitHandler;
        public System.Func<Vector2Int> runtimeSubmitHandler { 
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


        List<System.Action<GameObject[], object[]>> onSelectdelegates = new List<System.Action<GameObject[], object[]>>();
        event System.Action<GameObject[], object[]> _onSelect;
        public void BroadcastSelectEvent (GameObject[] data, object[] customData) {
            if (parentHolder != null) {
                parentHolder.BroadcastSelectEvent(data, customData);
                return;
            }
            if (_onSelect != null) _onSelect(data, customData);
        }
        public void SubscribeToSelectEvent (System.Action<GameObject[], object[]> callback) {
            if (parentHolder != null) {
                parentHolder.SubscribeToSelectEvent(callback);
                return;
            }
            _onSelect += callback;
            onSelectdelegates.Add(callback);
        }

        List<System.Action<GameObject[], object[], Vector2Int>> onSubmitdelegates = new List<System.Action<GameObject[], object[], Vector2Int>>();
        event System.Action<GameObject[], object[], Vector2Int> _onSubmit;
        public void BroadcastSubmitEvent (GameObject[] data, object[] customData, Vector2Int submit) {
            if (parentHolder != null) {
                parentHolder.BroadcastSubmitEvent(data, customData, submit);
                return;
            }
            if (_onSubmit != null) _onSubmit(data, customData, submit);
        }
        public void SubscribeToSubmitEvent (System.Action<GameObject[], object[], Vector2Int> callback) {
            if (parentHolder != null) {
                parentHolder.SubscribeToSubmitEvent(callback);
                return;
            }
            _onSubmit += callback;
            onSubmitdelegates.Add(callback);
        }
        

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
        protected Transform elementsParent {
            get {
                if (isHoldersCollection) return null;
                if (_elementsParent == null) _elementsParent = ElementsParent();
                return _elementsParent;
            }
        }
        protected abstract Transform ElementsParent ();
        

        // public event System.Action onBaseCancel;
        public System.Action onBaseCancel;
        
        protected List<SelectableElement> allElements = new List<SelectableElement>();

        void GetElementReferences () {

            if (isHoldersCollection) return;
                
            allElements.Clear();
                
            SelectableElement[] _allElements = GetComponentsInChildren<SelectableElement>();
            for (int i = 0; i < _allElements.Length; i++) {
                _allElements[i].parentHolder = this;
                allElements.Add(_allElements[i]);                
            }
        }

        public virtual void UpdateElementHolder () {
            
            if (isHoldersCollection)  {
                for (int i = 0; i < subHolders.Length; i++) subHolders[i].textScale = textScale;
                return;
            }
                
            
            for (int i = 0; i < allElements.Count; i++) {
                UIText t = allElements[i].uiText;
                if (allElements[i].hasText) {

                    t.transform.localScale = Vector3.one * textScale;  
                }
                allElements[i].UpdateElement();  
            }

            // if (Application.isPlaying) {
            //     if (needsDefaultSelection && allElements.Count > 0) {
            //         SelectOnEnable selectOnEnable = GetComponent<SelectOnEnable>();
            //         if (selectOnEnable != null) {
            //             selectOnEnable.toSelect = allElements[0].gameObject;
            //         }
            //     }
            // }
        }
        
        protected virtual void OnEnable () {
            InitializeSubHolders();
            GetElementReferences();
            UpdateElementHolder();    
        }
            
        protected virtual void OnDisable () {

            if (isHoldersCollection) return;
            
            foreach(var e in allElements) e.selected = false;

            allElements.Clear();
        }
           
        protected virtual void Update () {

            if (!Application.isPlaying) {
                GetElementReferences();
                UpdateElementHolder();
            }

            if (Application.isPlaying) {
                if (UIManager.input.GetButtonDown(UIManager.cancelButton)) {
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

        public SelectableElement[] GetAllElements (int targetCount) {
            if (isHoldersCollection) return null;
            
            if (allElements.Count < targetCount) {
                int c = allElements.Count;
                for (int i = 0 ; i < targetCount - c; i++) {
                    AddNewElement("Adding new");
                }
            }

            return allElements.ToArray();
        }

        public SelectableElement AddNewElement (string elementText) {
            if (isHoldersCollection) return null;
            
            SelectableElement newElement = Instantiate(ElementPrefab());
            newElement.parentHolder = this;
            newElement.transform.SetParent( elementsParent.transform );
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