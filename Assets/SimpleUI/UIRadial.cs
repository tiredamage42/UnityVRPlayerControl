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

        Transform _radialElementsParent;
        Transform radialElementsParent {
            get {
                if (_radialElementsParent == null) {
                    _radialElementsParent = transform.GetChild(0);
                }
                return _radialElementsParent;
            }
        }
        float sliceAngle {
            get {
                return 360.0f / allElements.Count;
            }
        }
        

        protected override void UpdateElementHolder () {
            base.UpdateElementHolder();
            backGroundRect.sizeDelta = new Vector2(backgroundSize, backgroundSize);
            backGroundOverlayRect.sizeDelta = new Vector2(backgroundSize, backgroundSize);
            float sliceAngle = this.sliceAngle;
            float radialAmount = 1f/allElements.Count;
            float radialAngle = sliceAngle*.5f;
            Quaternion elementInsidesLocalRotation = Quaternion.Euler(0, 0, radialAngle);
            

            Vector3 flairSize = Vector3.one * this.flairSize;
            Vector3 insideSize = Vector3.one * this.insideSize;
            Vector3 textLocalPos = new Vector3(0,textOffset,0);
            

            for (int i = 0; i < allElements.Count; i++) {

                UIRadialElement element = allElements[i] as UIRadialElement;
                float elementAngle = -i * sliceAngle;

                element.transform.localRotation = Quaternion.Euler(0,0,elementAngle);

                for (int x =0 ; x < element.images.Length; x++) {
                    element.images[x].fillAmount = radialAmount;
                }

                element.mainImageTransform.localRotation = elementInsidesLocalRotation;
                element.selectFlairTransform.localRotation = elementInsidesLocalRotation;

                element.text.transform.localRotation = Quaternion.Euler(0,0,-elementAngle);
                element.text.SetAnchor(elementAngle < -180f ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft);
                // element.UpdateLayout(1f/allElements.Count, sliceAngle, -i * sliceAngle);

                element.selectFlairTransform.localScale = flairSize;
                element.mainImageTransform.localScale = insideSize;

                element.text.transform.localPosition = textLocalPos;
            }
        }
        
        
        protected override SelectableElement ElementPrefab () {
            return UIManager.instance.radialElementPrefab;

        }

        
        protected override Image Background () {
            return GetComponent<Image>();
        }
        protected override Image BackgroundOverlay () {
            return radialElementsParent.GetComponent<Image>();
        }
        
        protected override void OnDisable () {
            base.OnDisable();
            currentSelected = -1;
        }

        protected override void Update () {
            base.Update ();
            if (Application.isPlaying) {
                if (manualSelection) {
                    SetSelection(UIManager.selectionAxis);
                }
            }

        }

        int currentSelected = -1;

        public void SetSelection(Vector2 selection) {
            int lastSelected = currentSelected;
            currentSelected = -1;

            if (selection != Vector2.zero && allElements.Count > 0) {
                currentSelected = 0;
                if (allElements.Count > 1) {
                    float sliceAngle = this.sliceAngle;
            
                    float a = (Mathf.Atan2(selection.x, selection.y) * Mathf.Rad2Deg) + (sliceAngle * .5f);
                    // a += sliceAngle / 2.0f;
                    if (a < 0) a = a + 360.0f;
                    currentSelected = (int)(a / sliceAngle);
                }
            }

            if (lastSelected != currentSelected) {
                eventSystem.SetSelectedGameObject( currentSelected != -1 ? allElements[currentSelected].gameObject : null );
            }
        }
    }



}
