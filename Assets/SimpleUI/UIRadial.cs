using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI{

    [ExecuteInEditMode]
    public class UIRadial : UIElementHolder
    {
        public bool manualSelection;

        public float textOffset = .8f;

        public float backgroundSize = 1;
        public float insideSize = .9f;
        public float flairSize = 1.1f;

        protected override void UpdateElementHolder () {
            base.UpdateElementHolder();
            backGroundRect.sizeDelta = new Vector2(backgroundSize, backgroundSize);
            backGroundOverlayRect.sizeDelta = new Vector2(backgroundSize, backgroundSize);
            
            
            for (int i = 0; i < allElements.Count; i++) {
                UIRadialElement element = allElements[i] as UIRadialElement;
                element.transform.localRotation = Quaternion.Euler(0,0,-i * sliceAngle);
                element.UpdateLayout(1f/allElements.Count, sliceAngle, -i * sliceAngle);

                element.selectFlairTransform.localScale = Vector3.one * flairSize;
                element.mainImageTransform.localScale = Vector3.one * insideSize;

                element.text.transform.localPosition = new Vector3(0,textOffset,0);
            
            
            }



        }
        
        // void Update()
        // {

        //     if (!Application.isPlaying) {
        //         GetSubButtonReferences();
        //     }
        //     UpdateRadialElements();

            
            
        // }

        // public int subDivisionCount = 4;

        float sliceAngle {
            get {
                return 360.0f / allElements.Count;
            }
        }


        // void UpdateRadialElements () {

        //     for (int i = 0; i < allSubButtons.Count; i++) {
        //         UIRadialElement element = allSubButtons[i];
        //         element.transform.localRotation = Quaternion.Euler(0,0,-i * sliceAngle);
        //         element.UpdateElement(1f/allSubButtons.Count, -i * sliceAngle);
        //     }
        // }

        
        protected override SelectableElement ElementPrefab () {
            return UIManager.instance.radialElementPrefab;

        }
        // protected override Transform ElementParent () {
        //     return radialElementsParent;
        // }

        Transform _radialElementsParent;
        Transform radialElementsParent {
            get {
                if (_radialElementsParent == null) {
                    _radialElementsParent = transform.GetChild(0);
                }
                return _radialElementsParent;
            }
        }

        protected override Image Background () {
            return GetComponent<Image>();
        }
        protected override Image BackgroundOverlay () {
            return radialElementsParent.GetComponent<Image>();
        }
        



        // Transform radialElementsParent;



        // void OnEnable () {
        //     radialElementsParent = transform.GetChild(0).GetChild(0);

        //     GetSubButtonReferences();

        // }

        // void GetSubButtonReferences () {
        //     allSubButtons.Clear();
        //     UIRadialElement[] subButtons = GetComponentsInChildren<UIRadialElement>();
        //     for (int i = 0; i < subButtons.Length; i++) {
        //         allSubButtons.Add(subButtons[i]);                
        //     }
        // }




        

        // List<UIRadialElement> allSubButtons = new List<UIRadialElement>();

        protected override void OnDisable () {
            base.OnDisable();
            // allSubButtons.Clear();
            currentSelected = -1;
        }

        protected override void Update () {
            base.Update ();

            if (Application.isPlaying) {

                if (manualSelection) {
                    
                    
                    // needs to change to vertical horizontal axis when not in vr
                    // Vector2 axis = new Vector2 (UIManager.input.GetAxis(UIManager.horizontalAxis), UIManager.input.GetAxis(UIManager.verticalAxis));
                    Vector2 axis = UIManager.input.mousePosition;
                    
                    SetSelection(axis);
                }
            }

        }


        // public UIRadialElement AddNewButton (string buttonName) {
        //     UIRadialElement newButton = Instantiate(UIManager.instance.radialElementPrefab);
        //     newButton.transform.SetParent( radialElementsParent.transform );
        //     newButton.transform.localScale = Vector3.one;
        //     allSubButtons.Add(newButton);
            
        //     return newButton;
        // }

        

        int currentSelected = -1;

        public void SetSelection(Vector2 selection) {
            int lastSelected = currentSelected;
            currentSelected = -1;

            if (selection != Vector2.zero && allElements.Count > 0) {
                currentSelected = 0;
                if (allElements.Count > 1) {
                    float a = Mathf.Atan2(selection.x, selection.y) * Mathf.Rad2Deg;
                    // Debug.LogError("first angel :: " + a);
                    
                    a += sliceAngle / 2.0f;
                    // Debug.LogError("second angel :: " + a);
                    
                    if (a < 0) a = a + 360.0f;
                    // Debug.LogError("final angel :: " + a);
                    
                    currentSelected = (int)(a / sliceAngle);
                }
            }

            if (lastSelected != currentSelected) {
                if (lastSelected != -1) {

                    // allElements[lastSelected].OnDeselect(null);
                }
                if (currentSelected != -1) {

                    eventSystem.SetSelectedGameObject( allElements[currentSelected].gameObject );
                    // allElements[currentSelected].OnSelect(null);
                }
                else {
                    eventSystem.SetSelectedGameObject( null );
                    
                }
            }
        }
    }



}
