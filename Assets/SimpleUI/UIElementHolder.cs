using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace SimpleUI {

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

    public System.Action onAnySelection;
    System.Action _onAnySelection {
        get {
            if (parentHolder != null) {
                return parentHolder._onAnySelection;
            }
            return onAnySelection;
        }
    }
    public System.Action<SelectableElement> onAnySubmit;
    System.Action<SelectableElement> _onAnySubmit {
        get {
            if (parentHolder != null) {
                return parentHolder._onAnySubmit;
            }
            return onAnySubmit;
        }
    }



    protected abstract SelectableElement ElementPrefab () ;
    // protected abstract Transform ElementParent () ;
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
            _allElements[i].onSelect = _onAnySelection;
            _allElements[i].onSubmit = _onAnySubmit;
            
            allElements.Add(_allElements[i]);                
        }
    }

    
    // void OnAnySelection () {

    //     if (onAnySelection != null) {
    //         onAnySelection;
    //     }

    // }


    protected virtual void UpdateElementHolder () {

        backGround.color = UIManager.instance.mainDarkColor;
        backGroundOverlay.color = UIManager.instance.mainLightColor;
        transform.localScale = Vector3.one * scale;

        for (int i = 0; i < allElements.Count; i++) {
            allElements[i].text.transform.localScale = Vector3.one * textScale;  
            allElements[i]._UpdateElement();  
        }

        if (Application.isPlaying) {
            if (needsDefaultSelection && allElements.Count > 0) {
                GetComponent<SelectOnEnable>().toSelect = allElements[0].gameObject;
            }
        }
    }

    protected virtual void OnEnable () {
        GetElementReferences();
        UpdateElementHolder();    
    }
    protected virtual void OnDisable () {
        allElements.Clear();
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
                    // if (baseObject != null) {
                        
                    // }
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
        newElement.transform.SetParent( backGroundOverlay.transform );//ElementParent() );
        newElement.transform.localScale = Vector3.one;
        newElement.transform.localPosition = Vector3.zero;
        newElement.transform.localRotation = Quaternion.identity;
        
        allElements.Add(newElement);
        newElement.text.SetText( elementText );
        UpdateElementHolder();
        return newElement;
    }



    



}

}