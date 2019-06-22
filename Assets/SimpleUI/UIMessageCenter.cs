using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace SimpleUI {

    [ExecuteInEditMode]

    public class UIMessageCenter : MonoBehaviour
    {

        void OnEnable () {
            UIManager.onShowGameMessage += OnShowGameMesssage;

        }
        void OnDisable () {
            UIManager.onShowGameMessage -= OnShowGameMesssage;
        }



        
        
        static Queue<UIMessageElement> elementPool = new Queue<UIMessageElement>();
        UIMessageElement BuildNewElement () {
            UIMessageElement newElement = Instantiate(UIManager.instance.messagePrefab);
            newElement.transform.SetParent(transform);
            newElement.transform.localRotation = Quaternion.identity;
            newElement.transform.localScale = Vector3.one;
            newElement.DisableMessage();
            return newElement;
        }

        UIMessageElement GetAvailableElement () {
            if (elementPool.Count > 0) 
                return elementPool.Dequeue();
            return BuildNewElement();
        }

        public int key = 0;


        List<UIMessageElement> shownElements = new List<UIMessageElement>();
        
        // public TextAlignment textAlignment;
        public TextAnchor textAnchor;
        public float elementMoveSpeed = 5;

        public float width = 128;
        public float textScale = 0.0015f;
        public float textMargin = .05f;
        public float scale = 0.01f;

        public float duration = 1, fadeIn = .25f, fadeOut = .25f;




        [Header("negative values flip directions")]
        public float startXOffset = .1f;
        public float lineHeight = .5f;


        void OnShowGameMesssage (string message, int key) {
            if (key == this.key) {
                ShowMessage(message);
            }
        }

        public void ShowMessage (string message) {
            transform.localScale = Vector3.one * scale;
            
            UIMessageElement newMessage = GetAvailableElement();
            shownElements.Add(newMessage);
            newMessage.ShowMessage ( duration, fadeIn, fadeOut );

            newMessage.transform.localPosition = new Vector3(startXOffset, (shownElements.Count - 1) * lineHeight, 0);

            newMessage.text.SetAnchor(textAnchor);
            newMessage.text.SetText(message);
            newMessage.text.transform.localScale = Vector3.one * textScale;  
            // newMessage._UpdateElement();  
            // newMessage.rectTransform.sizeDelta = new Vector2(width, Mathf.Abs(lineHeight));
            newMessage.SetSizeDelta(new Vector2(width, Mathf.Abs(lineHeight)));
            
            UIText textC = newMessage.text;
            // textC.SetAnchor( textAlignment );

            RectTransform textRect = textC.rectTransform;
            if (textAnchor == TextAnchor.MiddleCenter || textAnchor == TextAnchor.LowerCenter || textAnchor == TextAnchor.UpperCenter ) {
                textRect.anchorMin = new Vector2(.5f, .5f);
                textRect.anchorMax = new Vector2(.5f, .5f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
            }
            else if (textAnchor == TextAnchor.MiddleRight || textAnchor == TextAnchor.LowerRight || textAnchor == TextAnchor.UpperRight  ) {
                textRect.anchorMin = new Vector2(1.0f-textMargin, 0.5f);
                textRect.anchorMax = new Vector2(1.0f-textMargin, 0.5f);
                textRect.pivot = new Vector2(1.0f-textMargin, 0.5f);
            }
            else if (textAnchor == TextAnchor.MiddleLeft || textAnchor == TextAnchor.LowerLeft || textAnchor == TextAnchor.UpperLeft  ) {
                textRect.anchorMin = new Vector2(textMargin, 0.5f);
                textRect.anchorMax = new Vector2(textMargin, 0.5f);
                textRect.pivot = new Vector2(textMargin, 0.5f);
            }
            
        }

        
        

        public void DisableAllMessages () {
            for (int i =0; i < shownElements.Count; i++) {
                shownElements[i].DisableMessage();
                elementPool.Enqueue(shownElements[i]);
            }
            shownElements.Clear();
        }

        
        void UpdateShownElements (float deltaTime) {
            

            float speed = deltaTime * elementMoveSpeed;
            for (int i = 0; i < shownElements.Count; i++) {
                UIMessageElement element = shownElements[i];
                element.transform.localPosition = Vector3.Lerp(element.transform.localPosition, new Vector3 (0, i * lineHeight, 0), speed);
                element.UpdateElement(deltaTime);
            }

            for (int i = shownElements.Count - 1; i >= 0; i--) {
                UIMessageElement element = shownElements[i];
                if (element.isAvailable) {
                    shownElements.Remove(element);
                    elementPool.Enqueue(element);
                }
            }
        }

        void Update()
        {
            UpdateShownElements(Time.deltaTime);
        }

        
        
        



        



    }

}
