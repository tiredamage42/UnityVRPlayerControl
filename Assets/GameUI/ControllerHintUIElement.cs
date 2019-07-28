using UnityEngine;

using SimpleUI;
namespace VRPlayer.UI {
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

        void UpdateTextValues () {
            if (text == null) {
                text = GetComponentInChildren<UIText>();
            }

            if (text != null) {
                text.SetAnchor(textAnchor);
                text.transform.localPosition = textOffset;
                text.transform.localScale = Vector3.one * textScale; 
            }
        }

        void Update () {
#if UNITY_EDITOR
            UpdateTextValues();
#endif
        }
        
        public void Hide () {
            // Debug.Log("hiding");
            gameObject.SetActive(false);
        }
        public void Show (string message) {
            UpdateTextValues();
            if (text != null) {
                gameObject.SetActive(true);
                text.SetText(message);
            }
        }
    }
}
