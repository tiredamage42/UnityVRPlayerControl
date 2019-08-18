// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
namespace SimpleUI {

    /*
    
    using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
 
public class SkipNonInteractable : MonoBehaviour, ISelectHandler
{
   private Selectable m_Selectable;
 
   // Use this for initialization
   void Awake()
   {
     m_Selectable = GetComponent<Selectable>();
   }
   
   public void OnSelect(BaseEventData evData)
   {
     // Don't apply skipping unless we are not interactable.
     if (m_Selectable.interactable) return;
 
     // Check if the user navigated to this selectable.
     if (Input.GetAxis("Horizontal") < 0)
     {
       Selectable select = m_Selectable.FindSelectableOnLeft();
       if (select == null || !select.gameObject.activeInHierarchy)
         select = m_Selectable.FindSelectableOnRight();
       StartCoroutine(DelaySelect(select));
     }
     else if (Input.GetAxis("Horizontal") > 0)
     {
       Selectable select = m_Selectable.FindSelectableOnRight();
       if (select == null || !select.gameObject.activeInHierarchy)
         select = m_Selectable.FindSelectableOnLeft();
       StartCoroutine(DelaySelect(select));
     }
     else if (Input.GetAxis("Vertical") < 0)
     {
       Selectable select = m_Selectable.FindSelectableOnDown();
       if (select == null || !select.gameObject.activeInHierarchy)
         select = m_Selectable.FindSelectableOnUp();
       StartCoroutine(DelaySelect(select));
     }
     else if (Input.GetAxis("Vertical") > 0)
     {
       Selectable select = m_Selectable.FindSelectableOnUp();
       if (select == null || !select.gameObject.activeInHierarchy)
         select = m_Selectable.FindSelectableOnDown();
       StartCoroutine(DelaySelect(select));
     }
   }
 
   // Delay the select until the end of the frame.
   // If we do not, the current object will be selected instead.
   private IEnumerator DelaySelect(Selectable select)
   {
     yield return new WaitForEndOfFrame();
 
     if (select != null || !select.gameObject.activeInHierarchy)
       select.Select();
     else
       Debug.LogWarning("Please make sure your explicit navigation is configured correctly.");
   }
}
    
    
     */
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
            // if (onClick != null) {
            //     onClick.Invoke(data);
            // }

            BroadcastSubmitEvent(data, customData, submitAction);

            // trigger other ui holder active (page switching...)
            // if (destination != null) {
            //     destination.gameObject.SetActive(true);

            //     if (!destination.isBase) {
            //         destination.parentElement = parentHolder;
            //     }
            //     parentHolder.gameObject.SetActive(false);
            // }
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
                        // Debug.LogError("checking for submit");
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

        List<Action<GameObject[], object[]>> onSelectdelegates = new List<Action<GameObject[], object[]>>();
        event Action<GameObject[], object[]> _onSelect;
        
        // SelectableElement currentSelected;


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


        public void BroadcastSelectEvent (GameObject[] data, object[] customData) {
            
            if (RequiresInput()) {

            if (parentElement != null) {
                parentElement.BroadcastSelectEvent(data, customData);
                return;
            }
            if (_onSelect != null) _onSelect(data, customData);
                        }
        }
        public void SubscribeToSelectEvent (Action<GameObject[], object[]> callback) {
                        if (RequiresInput()) {

            if (parentElement != null) {
                parentElement.SubscribeToSelectEvent(callback);
                return;
            }
            _onSelect += callback;
            onSelectdelegates.Add(callback);
                        }
        }

        List<Action<GameObject[], object[], Vector2Int>> onSubmitdelegates = new List<Action<GameObject[], object[], Vector2Int>>();
        event Action<GameObject[], object[], Vector2Int> _onSubmit;
        public void BroadcastSubmitEvent (GameObject[] data, object[] customData, Vector2Int submit) {
                        if (RequiresInput()) {

            if (parentElement != null) {
                parentElement.BroadcastSubmitEvent(data, customData, submit);
                return;
            }
            if (_onSubmit != null) _onSubmit(data, customData, submit);
                        }
        }
        public void SubscribeToSubmitEvent (Action<GameObject[], object[], Vector2Int> callback) {
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
