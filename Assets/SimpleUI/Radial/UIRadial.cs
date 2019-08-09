﻿using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI{

    [System.Serializable] public class UIRadialParameters {
        public float textOffset = .8f;
        [Range(0,1)] public float selectionSizeOffset = .1f;
        
        public UIRadialParameters (float textOffset, float selectionSizeOffset) {
            this.textOffset = textOffset;
            this.selectionSizeOffset = selectionSizeOffset;
        }

        public UIRadialParameters () {
            textOffset = .8f;
            selectionSizeOffset = .1f;
        }
    }

    [ExecuteInEditMode] public class UIRadial : UIElementHolder
    {
        public UIRadialParameters parameters = new UIRadialParameters();
        // public float textOffset = .8f;
        // [Range(0,1)] public float selectionSizeOffset = .1f;
        
        public override void UpdateElementHolder () {
            base.UpdateElementHolder();

            if (allElements.Count == 0)
                return;

            float sliceAngle = 360.0f / allElements.Count;
            float radialAmount = 1f / allElements.Count;
            float radialAngle = sliceAngle*.5f;

            Vector3 selectInsideSize = Vector3.one * (1+parameters.selectionSizeOffset);
            Vector3 textLocalPos = new Vector3(0,parameters.textOffset,0);
        
            for (int i = 0; i < allElements.Count; i++) {
                float elementAngle = -i * sliceAngle;

                SelectableElement element = allElements[i];

                element.transform.localScale = selectInsideSize;
                element.transform.localRotation = Quaternion.Euler(0,0,elementAngle + radialAngle);
                element.mainImage.fillAmount = radialAmount;
                
                if (element.hasText) {
                    element.uiText.transform.localPosition = textLocalPos;
                    element.uiText.transform.localRotation = Quaternion.Euler(0,0,-elementAngle);
                    if (elementAngle == 0 || elementAngle == -180) {
                        element.uiText.SetAnchor(TextAnchor.MiddleCenter);
                    }
                    else {
                        element.uiText.SetAnchor(elementAngle < -180f ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft);
                    }
                }
            }
        }
        
        
        protected override SelectableElement ElementPrefab () {
            return UIManager.instance.radialElementPrefab;
        }

        protected override Transform ElementsParent() {
            return transform.GetChild(0);
        }
        
        protected override void OnDisable () {
            base.OnDisable();
            currentSelected = -1;
        }

        protected override void Update () {
            base.Update ();
            if (Application.isPlaying) {
                SetSelection(UIManager.selectionAxis);
            }
        }

        int currentSelected = -1;

        public void SetSelection(Vector2 selection) {
            int lastSelected = currentSelected;
            currentSelected = -1;

            if (selection != Vector2.zero && allElements.Count > 0) {
                currentSelected = 0;
                if (allElements.Count > 1) {
                    float sliceAngle = 360.0f / allElements.Count;
                    float a = (Mathf.Atan2(selection.x, selection.y) * Mathf.Rad2Deg) + (sliceAngle * .5f);
                    if (a < 0) a = a + 360.0f;
                    currentSelected = (int)(a / sliceAngle);
                }
            }

            if (lastSelected != currentSelected) {
                UIManager.SetSelection(currentSelected != -1 ? allElements[currentSelected].gameObject : null);
                // eventSystem.SetSelectedGameObject( currentSelected != -1 ? allElements[currentSelected].gameObject : null );
            }
        }
    }
}
