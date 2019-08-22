using UnityEngine;

// using SimpleUI;

// TODO: switch over to game ui
namespace SimpleUI {
    [ExecuteInEditMode]
    public class ControllerHintUIElement : MonoBehaviour
    {
        [HideInInspector] public UIText text;
        public float textScale = .005f;
        public Vector3 textOffset = new Vector3(0,-.7f,0);
        public TextAnchor textAnchor = TextAnchor.MiddleCenter;

        void Awake () {
            UpdateTextValues();
        }
        void Start () {
            if (Application.isPlaying) {
                Hide();
            }
        }
        public void SetLayoutValues (float textScale, Vector3 textOffset, TextAnchor textAnchor) {
            this.textScale = textScale;
            this.textOffset = textOffset;
            this.textAnchor = textAnchor;
            UpdateTextValues();
        }
        public void SetText (string txt) {
            if (text == null) text = GetComponentInChildren<UIText>();
            text.SetText(txt, -1);
        }

        void UpdateTextValues () {
            if (text == null) text = GetComponentInChildren<UIText>();
    
            text.SetAnchor(textAnchor);
            text.transform.localPosition = textOffset;
            text.transform.localScale = Vector3.one * textScale; 
            // if (text != null) {
            // }
        }

#if UNITY_EDITOR
        void Update () {
            UpdateTextValues();
        }
#endif
        
        public void Hide () {
            gameObject.SetActive(false);
        }
        public void Show (string message) {
            UpdateTextValues();
            gameObject.SetActive(true);
            text.SetText(message, -1);
            // if (text != null) {
            // }
        }
    }
}
