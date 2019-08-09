using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;


namespace SimpleUI {

    public class UIMessageCenter : MonoBehaviour
    {
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

        List<UIMessageElement> shownElements = new List<UIMessageElement>();
        
        public TextAnchor textAnchor;
        public float elementMoveSpeed = 5;

        public float width = 128;
        public float textScale = 0.0015f;
        public float textMargin = .05f;
        
        public float duration = 1, fadeIn = .25f, fadeOut = .25f;

        [Header("negative values flip directions")]
        public float startXOffset = .1f;
        public float lineHeight = .5f;

        public void ShowMessage (string message) {
            
            UIMessageElement newMessage = GetAvailableElement();
            shownElements.Add(newMessage);
            newMessage.ShowMessage ( duration, fadeIn, fadeOut );

            newMessage.transform.localPosition = new Vector3(startXOffset, (shownElements.Count - 1) * lineHeight, 0);
            newMessage.SetSizeDelta(new Vector2(width, Mathf.Abs(lineHeight)));

            newMessage.text.SetText(message);
            newMessage.text.SetAnchor(textAnchor);
            newMessage.text.AdjustAnchorSet(new Vector2(textMargin, 0));
            newMessage.text.transform.localScale = Vector3.one * textScale;  
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