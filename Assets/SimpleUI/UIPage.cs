using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI {

    [ExecuteInEditMode]
    public class UIPage : UIElementHolder
    {

        // [HideInInspector] public UIPage parentPage;
        // public static UIPage lastActivePage;

        // Start is called before the first frame update
        // void Start()
        // {
            
        // }

        // public int maxCharacters = 999;

        // public float textScale = 0.125f;
        // public float scale = 0.01f;


        protected override SelectableElement ElementPrefab () {
            return UIManager.instance.buttonPrefab;

        }
        // protected override Transform ElementParent () {
        //     return backGroundOverlay.transform;
        // }





        // Image backgroundPanel, overlayPanel;
        
        UIText _pageTitle;
        UIText pageTitle {
            get {
                if (_pageTitle == null) {
                    _pageTitle = transform.GetChild(0).GetComponent<UIText>();
                }
                return _pageTitle;
            }
        }


        protected override Image Background () {
            return transform.GetChild(1).GetComponent<Image>();

        }
        protected override Image BackgroundOverlay () {
            return backGround.transform.GetChild(0).GetComponent<Image>();
        }
        

       



        // Outline pageTitleOutline;

        // RectTransform pageTitleRect;

        // void Awake () {
        //     pageTitle = transform.GetChild(0).GetComponent<Text>();
        //     pageTitleOutline = pageTitle.GetComponent<Outline>();
        //     pageTitleRect = pageTitle.GetComponent<RectTransform>();

        //     backgroundPanel = transform.GetChild(1).GetComponent<Image>();
        //     overlayPanel = backgroundPanel.transform.GetChild(0).GetComponent<Image>();
        // }

        public string pageTitleText = "Window Title";

        public void SetTitle (string title) {
            pageTitleText = title;
            UpdateElementHolder();
            // pageTitle.text = title;
        }



        // List<UIButton> allSubButtons = new List<UIButton>();
        // void OnDisable () {
        //     allSubButtons.Clear();
        // }


        // public UIButton AddNewButton (string buttonName) {
        //     UIButton newButton = Instantiate(UIManager.instance.buttonPrefab);
        //     newButton.parentPage = this;
        //     newButton.transform.SetParent( overlayPanel.transform );
        //     newButton.transform.localScale = Vector3.one;
        //     allSubButtons.Add(newButton);

        //     newButton.text.text = buttonName;

        //     Canvas.ForceUpdateCanvases();

        //     return newButton;
        // }
        // public UIButton AddNewPageButton () {
        //     UIButton newButton = AddNewButton();
        //     UIPageButton pageButton = newButton.gameObject.AddComponent<UIPageButton>();
        //     pageButton.parentPage = this;
        //     return newButton;
        // }

        public int lineHeight = 10;
        public int width = 64;

        public TextAnchor textAlignment;

        

        public void SetSize (int width, int lineHeight) {
            this.width = width;
            this.lineHeight = lineHeight;
            UpdateElementHolder();
        }

        protected override void UpdateElementHolder () {

        // void SetSizes () {
            base.UpdateElementHolder();

            pageTitle.SetText(pageTitleText);

            // transform.localScale = Vector3.one * scale;

            // backgroundPanel.color = UIManager.instance.mainDarkColor;
            // overlayPanel.color = UIManager.instance.mainLightColor;


            pageTitle.rectTransform.sizeDelta = new Vector2(width, lineHeight);
            pageTitle.transform.localScale = Vector3.one * textScale;

            for (int i = 0; i < allElements.Count; i++) {
                allElements[i].rectTransform.sizeDelta = new Vector2(width, lineHeight);
                
                UIText textC = allElements[i].text;
                textC.SetAnchor( textAlignment );

                RectTransform textRect = allElements[i].textRectTransform;
                if (textAlignment == TextAnchor.MiddleCenter || textAlignment == TextAnchor.LowerCenter || textAlignment == TextAnchor.UpperCenter ) {
                    textRect.anchorMin = new Vector2(.5f, .5f);
                    textRect.anchorMax = new Vector2(.5f, .5f);
                    textRect.pivot = new Vector2(0.5f, 0.5f);
                }
                else if (textAlignment == TextAnchor.MiddleRight || textAlignment == TextAnchor.LowerRight || textAlignment == TextAnchor.UpperRight  ) {
                    textRect.anchorMin = new Vector2(1, 0.5f);
                    textRect.anchorMax = new Vector2(1, 0.5f);
                    textRect.pivot = new Vector2(1, 0.5f);
                }
                else if (textAlignment == TextAnchor.MiddleLeft || textAlignment == TextAnchor.LowerLeft || textAlignment == TextAnchor.UpperLeft  ) {
                    textRect.anchorMin = new Vector2(0, 0.5f);
                    textRect.anchorMax = new Vector2(0, 0.5f);
                    textRect.pivot = new Vector2(0, 0.5f);
                }
                    

                // if (textC != null) {


                        
                    // string textButton = allSubButtons[i].buttonText;
                    
                    // // string buttonText = textC.text;
                    // if (textButton.Length > maxCharacters) {
                    //     textButton = textButton.Substring(0, maxCharacters - 3) + "...";
                    // }
                    // textC.text = textButton;
                    // textC.transform.localScale = Vector3.one * textScale;
                // }
            }
        }



        // void SetColors () {
            
        //     backgroundPanel.color = UIManager.instance.mainDarkColor;
        //     overlayPanel.color = UIManager.instance.mainLightColor;
        // }

        // void UpdateValues () {
        //     // SetColors ();
        //     // SetSizes ();
        //     // pageTitle.text = pageTitleText;
            
        //     if (allSubButtons.Count > 0) {

        //         GetComponent<SelectOnEnable>().toSelect = allSubButtons[0].gameObject;
        //     }
        //     // Canvas.ForceUpdateCanvases();
        // }
            

        // void OnEnable () {

            // pageTitle = transform.GetChild(0).GetComponent<Text>();
            // pageTitleOutline = pageTitle.GetComponent<Outline>();
            // pageTitleRect = pageTitle.GetComponent<RectTransform>();

            // backgroundPanel = transform.GetChild(1).GetComponent<Image>();
            // overlayPanel = backgroundPanel.transform.GetChild(0).GetComponent<Image>();
            
            // GetSubButtonReferences();
            // UpdateValues();
        // }

        // void GetSubButtonReferences () {
        //     allSubButtons.Clear();
        //     UIButton[] subButtons = GetComponentsInChildren<UIButton>();
        //     for (int i = 0; i < subButtons.Length; i++) {
        //         if (Application.isPlaying) {
        //             subButtons[i].parentPage = this;
        //         }
        //         allSubButtons.Add(subButtons[i]);                
        //     }
        // }

        // Update is called once per frame
        // void Update()
        // {
            // if (!Application.isPlaying) {
            //     GetSubButtonReferences();
            //     UpdateValues();
            // }

            // if (Application.isPlaying) {

            //     if (parentPage != null) {
            //     if (UIManager.input.GetButtonDown(UIManager.cancelButton)) {
            //         gameObject.SetActive(false);
            //         parentPage.gameObject.SetActive(true);
            //     }

            //     }

            // }
        // }
    }
}
