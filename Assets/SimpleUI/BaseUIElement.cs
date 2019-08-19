using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace SimpleUI {

    public abstract class BaseUIElement : MonoBehaviour
    {
        public bool isBase;
        public GameObject baseObject;
        RectTransform _rectTransform;
        public RectTransform rectTransform {
            get {
                if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }
        [HideInInspector] public BaseUIElement parentElement;
        public abstract bool RequiresInput ();
        protected abstract bool CurrentSelectedIsOurs (GameObject currentSelected);

        public bool isPopup;

        void DoSubmit (Vector2Int submitAction) {
            // Debug.Log("Submitted on " + name);
        
            
            GameObject[] data;
            object[] customData;
            SelectableElement selected = GetCurrentSelectedData (out data, out customData);
            
            if (selected != null) {
                selected.DoSubmit(submitAction);
            }
        
            BroadcastSubmitEvent(null, data, customData, submitAction);
        }

        public abstract void SetSelectableActive(bool active);
        
        protected virtual void Update () {

            if (RequiresInput()) {

                if (Application.isPlaying) {
                    if (!UIManager.popupOpen || isPopup) {
                        if (runtimeSubmitHandler != null) {
                            Vector2Int alternativeSubmit = runtimeSubmitHandler();
                            if (alternativeSubmit.x >= 0) {
                                DoSubmit(alternativeSubmit);
                            }
                        }
                        else {
                            if (UIManager.currentInput.GetButtonDown(UIManager.standaloneInputModule.submitButton)) {
                                DoSubmit(new Vector2Int(0,0));
                            }
                        }

                        // check if we hit the cancel button specified in the project's
                        // standalone input module
                        if (UIManager.currentInput.GetButtonDown(UIManager.standaloneInputModule.cancelButton)) {
                            // if we're not the base element (page), then "switch pages" to our parent one
                            if (!isBase) {
                                gameObject.SetActive(false);
                                parentElement.gameObject.SetActive(true);
                            }
                            else {
                                OnBaseCancel();
                            }
                        }
                    }  
                }
            }
        }


        void OnBaseCancel () {
            if (RequiresInput()) {
                if (parentElement != null) {
                    parentElement.OnBaseCancel();
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

        Func<Vector2Int> _runtimeSubmitHandler;
        public Func<Vector2Int> runtimeSubmitHandler { 
            get { return parentElement != null ? parentElement.runtimeSubmitHandler : _runtimeSubmitHandler; } 
            set {
                if (parentElement != null) {
                    parentElement.runtimeSubmitHandler = value;
                }
                else {
                    _runtimeSubmitHandler = value;
                }
            }
        }

        List<Action<GameObject, GameObject[], object[]>> onSelectdelegates = new List<Action<GameObject, GameObject[], object[]>>();
        event Action<GameObject, GameObject[], object[]> _onSelect;
        
        SelectableElement GetCurrentSelectedData (out GameObject[] data, out object[] customData) {
            data = null;
            customData = null;

            GameObject currentSelected = UIManager.CurrentSelected();
            if (currentSelected == null) {
                return null;
            }
            if (!CurrentSelectedIsOurs(currentSelected)) {
                Debug.LogError("Getting Input for" + name + " but current selected isnt part of our set, "+currentSelected.name);
                return null;
            }
            SelectableElement currentSelectedCheck = currentSelected.GetComponent<SelectableElement>();
            if (currentSelectedCheck == null){
                return null;
            }

            data = currentSelectedCheck.data;
            customData = currentSelectedCheck.customData;

            return currentSelectedCheck;
        }


        public void BroadcastSelectEvent (GameObject buttonObject, GameObject[] data, object[] customData) {
            if (RequiresInput()) {
                if (parentElement != null) {
                    parentElement.BroadcastSelectEvent(buttonObject, data, customData);
                    return;
                }
                if (_onSelect != null) _onSelect(buttonObject, data, customData);
            }
        }
        public void SubscribeToSelectEvent (Action<GameObject, GameObject[], object[]> callback) {
            if (RequiresInput()) {
                if (parentElement != null) {
                    parentElement.SubscribeToSelectEvent(callback);
                    return;
                }
                _onSelect += callback;
                onSelectdelegates.Add(callback);
            }
        }

        List<Action<GameObject, GameObject[], object[], Vector2Int>> onSubmitdelegates = new List<Action<GameObject, GameObject[], object[], Vector2Int>>();
        event Action<GameObject, GameObject[], object[], Vector2Int> _onSubmit;
        public void BroadcastSubmitEvent (GameObject buttonObject, GameObject[] data, object[] customData, Vector2Int submit) {
            if (RequiresInput()) {
                if (parentElement != null) {
                    parentElement.BroadcastSubmitEvent(buttonObject, data, customData, submit);
                    return;
                }
                if (_onSubmit != null) _onSubmit(buttonObject, data, customData, submit);
            }
        }
        public void SubscribeToSubmitEvent (Action<GameObject, GameObject[], object[], Vector2Int> callback) {
            if (RequiresInput()) {
                if (parentElement != null) {
                    parentElement.SubscribeToSubmitEvent(callback);
                    return;
                }
                _onSubmit += callback;
                onSubmitdelegates.Add(callback);
            }
        }

        public Action onBaseCancel;
        
        public void RemoveAllEvents()
        {
            if (RequiresInput()) {

                foreach(var eh in onSelectdelegates) _onSelect -= eh;
                onSelectdelegates.Clear();
                
                foreach(var  eh in onSubmitdelegates) _onSubmit -= eh;
                onSubmitdelegates.Clear();

                runtimeSubmitHandler = null;
            }
        }        
    }
}
