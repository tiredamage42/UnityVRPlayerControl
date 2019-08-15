using System.Collections.Generic;
using UnityEngine;
using System.Collections;

using UnityEngine.UI;


namespace SimpleUI {

    [System.Serializable] public class UIMessageCenterParameters {

        public TextAnchor textAnchor;

        public float textScale = 0.0015f;
        public float textMargin = .05f;

        public Vector2 lineSize = new Vector2(4, .5f);
        
        [Header("negative values flip directions")]
        public float startXOffset = .1f;
        public float moveSpeed = 5;
        public float duration = 1;
        public float frequency = 1f;
        public Vector2 fadeInOut = new Vector2( .25f, .25f );

        public UIMessageCenterParameters () {
            textAnchor = TextAnchor.MiddleLeft;
            moveSpeed = 5;
            lineSize = new Vector2(4, .5f);
            textScale = 0.0015f;
            textMargin = .05f;
            duration = 1; 
            frequency = 1;
            fadeInOut = new Vector2( .25f, .25f );
            startXOffset = .1f;
        }
        public UIMessageCenterParameters (TextAnchor textAnchor, float moveSpeed , Vector2 lineSize , float textScale , float textMargin , float duration , Vector2 fadeInOut, float startXOffset, float frequency) {
            this.textAnchor = textAnchor;
            this.moveSpeed = moveSpeed;
            this.lineSize = lineSize;
            this.textScale = textScale;
            this.textMargin = textMargin;
            this.duration = duration; 
            this.frequency = frequency;
            this.fadeInOut = fadeInOut;
            this.startXOffset = startXOffset;
        }
    }

    public class UIMessageCenter : BaseUIElement
    {

        Queue<string> messageQ = new Queue<string>();
        Queue<UIColorScheme> schemesQ = new Queue<UIColorScheme>();
        
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

        public UIMessageCenterParameters parameters = new UIMessageCenterParameters();
        
        public event System.Action<string> onShowMessage;
        void Start () {
            StartCoroutine(HandleMessageShowing());
        }

        IEnumerator HandleMessageShowing () {

            while (true) {
                if (messageQ.Count > 0) {
                    ShowMessageImmediate(messageQ.Dequeue(), schemesQ.Dequeue());
                }
                yield return new WaitForSeconds(parameters.frequency);
            }
        }

        public void ShowMessage (string message, bool immediate, UIColorScheme scheme) {
            if (immediate || messageQ.Count == 0) {
                ShowMessageImmediate(message, scheme);
            }
            else {
                messageQ.Enqueue(message);
            }
        }


        public void ShowMessageImmediate (string message, UIColorScheme scheme) {
            if (onShowMessage != null) {
                onShowMessage(message);
            }

            UIMessageElement newMessage = GetAvailableElement();
            shownElements.Add(newMessage);
            newMessage.ShowMessage ( parameters.duration, parameters.fadeInOut.x, parameters.fadeInOut.y, scheme );
            
            newMessage.transform.localPosition = new Vector3(parameters.startXOffset, (shownElements.Count - 1) * parameters.lineSize.y, 0);

            newMessage.text.SetText(message, -1);
            newMessage.text.SetAnchor(parameters.textAnchor);
            newMessage.text.AdjustAnchorSet(new Vector2(parameters.textMargin, 0));
            newMessage.text.transform.localScale = Vector3.one * parameters.textScale;  
            
            newMessage.SetSizeDelta(new Vector2(parameters.lineSize.x, Mathf.Abs(parameters.lineSize.y)));
        }

        public void DisableAllMessages () {
            for (int i =0; i < shownElements.Count; i++) {
                shownElements[i].DisableMessage();
                elementPool.Enqueue(shownElements[i]);
            }
            shownElements.Clear();
        }

        void UpdateShownElements (float deltaTime) {
            
            float speed = deltaTime * parameters.moveSpeed;
            for (int i = 0; i < shownElements.Count; i++) {
                UIMessageElement element = shownElements[i];
                element.transform.localPosition = Vector3.Lerp(element.transform.localPosition, new Vector3 (0, i * parameters.lineSize.y, 0), speed);
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