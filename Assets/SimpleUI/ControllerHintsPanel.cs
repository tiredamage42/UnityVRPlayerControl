using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SimpleUI {


    
    [System.Serializable] public class ControllerUIHintPrefab {

        public ControllerHintUIElement elementPrefab;

        List<ControllerHintUIElement> allInstances = new List<ControllerHintUIElement>();
        
        ControllerHintUIElement CreateNewElement () {
            ControllerHintUIElement newElement = GameObject.Instantiate(elementPrefab);
            allInstances.Add(newElement);
            newElement.gameObject.SetActive(true);
            return newElement;
        }

        ControllerHintUIElement _GetAvailableElement () {
            for (int i = 0; i < allInstances.Count; i++) {
                if (!allInstances[i].gameObject.activeSelf) {
                    allInstances[i].gameObject.SetActive(true);
                    return allInstances[i];
                }
            }
            return null;
        }

        public ControllerHintUIElement GetAvailableElement () {
            ControllerHintUIElement r = _GetAvailableElement();
            if (r == null) 
                r = CreateNewElement () ;
            return r;
        }
    }

    [System.Serializable] public class ControllerHintsPanelParameters {
        public Vector2 panelSize = new Vector2(3, .25f);
        public float textScale = .005f;
        public float hintY = 0;
        public float textY = 0;
        public float hintsScale = .1f;
        public float hintsSpacing = .25f;
        public float hintsTextScale = .005f;
        public Vector3 textOffset = new Vector3(0,-.7f,0);
        public TextAnchor textAnchor = TextAnchor.MiddleCenter;

        public ControllerHintsPanelParameters () {
            hintsScale = .1f;
            hintsSpacing = .25f;
            hintsTextScale = .005f;
            textScale = .005f;
            textOffset = new Vector3(0,-.7f,0);
            textAnchor = TextAnchor.MiddleCenter;
        }

    }
    [ExecuteInEditMode] public class ControllerHintsPanel : BaseUIElement
    {
        public override void SetSelectableActive(bool active) { }
        public override bool RequiresInput() { return false; }
        protected override bool CurrentSelectedIsOurs (GameObject currentSelected) { return true; }
        #if UNITY_EDITOR
        [TextArea] public string debugMessage;
        #endif
        public UIText textUI;
        public RectTransform backPanel;
        public ControllerHintsPanelParameters parameters = new ControllerHintsPanelParameters();
        public List<ControllerHintUIElement> allHints = new List<ControllerHintUIElement>();

        public void SetText (string text) {
            // if (textUI == null) textUI = GetComponentInChildren<UIText>();
            Debug.Log("set text" + textUI);
            textUI.SetText(text, -1);
        }

#if UNITY_EDITOR
        protected override void Update () {
            base.Update();
            UpdateHintsLayouts();
            if (!Application.isPlaying) {
                // debugMessage = "<size=64><b>\u2022</b></size> Hey <size=64><b>\u2665</b></size>";
                // debugMessage = "<size=64><b>\u25A3</b></size> Hey <size=64><b>\u2665</b></size>";

                
                // debugMessage = "<font size='6'><b>\u2022</b></font> buller \u2023 arraow hyphen \u2665";
                SetText ( debugMessage);
            }
        }
#endif
        void UpdateHintsLayouts () {

            /*
                    0   1    2    3
            
                    0   1,2  3,4  5,6
             */

            bool countEven = allHints.Count % 2 == 0;

            for (int i = 0; i < allHints.Count; i++) {
                allHints[i].SetLayoutValues (parameters.hintsTextScale, parameters.textOffset, parameters.textAnchor);
                allHints[i].transform.localScale = Vector3.one * parameters.hintsScale;
                
                bool indexEven = i % 2 == 0;

                int x = ((i / 2) + (countEven || indexEven ? 1 : 0)) * (indexEven ? 1 : -1);
                allHints[i].transform.localPosition = new Vector3(x * parameters.hintsSpacing, parameters.hintY, 0);
            }

            // if (textUI == null) textUI = GetComponentInChildren<UIText>();
            textUI.transform.localScale = Vector3.one * parameters.textScale;
            textUI.transform.localPosition = new Vector3(0, parameters.textY, 0);

            if (backPanel != null) {
                backPanel.sizeDelta = parameters.panelSize;
            }

            
        }

        public void RemoveAllHintElements () {
            for (int i = 0; i < allHints.Count; i++) {
                allHints[i].transform.SetParent(null);
                allHints[i].gameObject.SetActive(false);
            }
            allHints.Clear();
        }

        public void AddHintElements (List<int> actionHints, List<string> hintNames) {
            for (int i = 0; i < actionHints.Count; i++) {
                int hint = actionHints[i];
                if (hint < 0 || hint >= UIManager.instance.controllerHints.Length) {
                    Debug.LogError("Action hint index " + hint + " is not defined");
                    continue;
                }
                AddHintElement(UIManager.instance.controllerHints[actionHints[i]].GetAvailableElement(), hintNames[i]);
            }
        }

        public void AddHintElement (ControllerHintUIElement hint, string txt) {
            if (!allHints.Contains(hint)) {
                hint.transform.SetParent(transform);
                hint.transform.localRotation = Quaternion.identity;
                hint.gameObject.SetActive(true);
                hint.SetText ( txt );
                allHints.Add(hint);
                UpdateHintsLayouts();
            }
        }





    }
}
