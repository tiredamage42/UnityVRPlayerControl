using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace SimpleUI {

    // TODO: unselect on disable

    [ExecuteInEditMode] public abstract class UIElementHolder : BaseUIElement
    {

        bool initialized;

        public override bool RequiresInput() { return true; }

        protected abstract float TextScale();
     
        public UITextPanel textPanel;

        public UIElementHolder[] subHolders;
        
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
        public List<SelectableElement> allElements = new List<SelectableElement>();

        protected override bool CurrentSelectedIsOurs(GameObject currentSelected) {
            if (isHoldersCollection) {
                for (int i = 0; i< subHolders.Length; i++) {
                    if (subHolders[i].CurrentSelectedIsOurs(currentSelected)) {
                        return true;
                    }
                }
            }
            else {
                for (int i = 0; i < allElements.Count; i++) {
                    if (allElements[i].gameObject == currentSelected) {
                        return true;
                    }
                }
            }
            return false;
        }

        void GetSelectableElementReferences () {
                
            if (isHoldersCollection) return;

            if (Application.isPlaying) {
                initialized = false;
                allElements.Clear();
            }

            if (!initialized) {
                SelectableElement[] _allElements = GetComponentsInChildren<SelectableElement>();
                for (int i = 0; i < _allElements.Length; i++) {
                    _allElements[i].parentHolder = this;
                    allElements.Add(_allElements[i]);                
                }
                initialized = true;
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
                subHolders[i].parentElement = this;
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
               
        // protected virtual void Update () {
        protected override void Update () {
#if UNITY_EDITOR 
            if (!Application.isPlaying) {
                InitializeSelectableElements();
            }
#endif
            base.Update();
        }


        public override void SetSelectableActive(bool active) {
            if (!isPopup) {
                if (isHoldersCollection) {
                    for (int i =0 ; i< subHolders.Length; i++) {
                        subHolders[i].SetSelectableActive(active);
                    }
                    return;
                }
                for (int i = 0; i < allElements.Count; i++) {
                    Button button = allElements[i].GetComponent<Button>();
                    if (button) {
                        Navigation customNav = button.navigation;
                        customNav.mode = active ? Navigation.Mode.Automatic : Navigation.Mode.None;
                        button.navigation = customNav;
                    }
                }
            }
        }


        protected abstract bool ShouldWiggleLayoutChanges ();

        IEnumerator WiggleActive(int count) {

            // because im using a mix of layout groups and content size fitters, 
            // layout groups need to be enabled and disabled a few times to show correctly
            // (unity editor warnings be damned, it's the only way to get the expexted layout behavior unfortunately)
            
            // if (count%2 != 0) // make sure it's even last set active = true
            //     count++;

            // for (int i = 0; i < count; i++) {
            //     yield return null; baseObject.SetActive(i%2!=0); // false, true, false....
            //     Debug.LogError("setting active " + (i%2!=0));
            // }


            yield return null; (parentElement != null ? parentElement.baseObject : baseObject).SetActive(false); // false, true, false....
            yield return null; (parentElement != null ? parentElement.baseObject : baseObject).SetActive(true); // false, true, false....
            yield return null; (parentElement != null ? parentElement.baseObject : baseObject).SetActive(false); // false, true, false....
            yield return null; (parentElement != null ? parentElement.baseObject : baseObject).SetActive(true); // false, true, false....
                // Debug.LogError("setting active " );

        }
        public void WiggleActive () {
            if (ShouldWiggleLayoutChanges()) {
                UIManager.instance.StartCoroutine(WiggleActive(4));
            }
        }


        public SelectableElement[] GetAllSelectableElements (int targetCount) {
            if (isHoldersCollection) return null;
            
            int c = allElements.Count;
            for (int i =0 ; i < c; i++) {
                allElements[i].gameObject.SetActive(true);
            }
            if (c < targetCount) {

                int cnt = targetCount - c;
                for (int i = 0 ; i < cnt; i++) {
                    AddNewSelectableElement("Adding new", i == (cnt - 1));
                }
            }
            else if (c > targetCount) {
                for (int i = targetCount; i < c; i++) {
                    allElements[i].gameObject.SetActive(false);
                }
                WiggleActive();

                List<SelectableElement> r = new List<SelectableElement>();
                for (int i = 0; i < targetCount; i++) {
                    r.Add(allElements[i]);
                }
                return r.ToArray();
            }

            WiggleActive();
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