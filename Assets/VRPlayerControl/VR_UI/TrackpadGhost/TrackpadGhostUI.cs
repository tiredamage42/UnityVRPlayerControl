using UnityEngine;

using UnityEngine.UI;

namespace VRPlayer.UI {
    public class TrackpadGhostUI : MonoBehaviour
    {
        public Vector2 axis;
        public float showSize = .2f;
        RectTransform axisShowRect;
        
        void OnEnable () {
            UpdateAxisShow();
        }
    
        void UpdateAxisShow () {
            if (axisShowRect == null) axisShowRect = transform.GetChild(1).GetComponent<RectTransform>();
            
            if (axisShowRect != null) {
                float halfShow = showSize * .5f;
                Vector2 a = new Vector2(
                    Mathf.Clamp((axis.x * .5f + .5f), halfShow, 1-halfShow), 
                    Mathf.Clamp((axis.y * .5f + .5f), halfShow, 1-halfShow)
                );
                axisShowRect.anchorMin = a;
                axisShowRect.anchorMax = a;
#if UNITY_EDITOR
                axisShowRect.localScale = Vector3.one * showSize;
#endif
            }
        }

        void Update()
        {
            UpdateAxisShow();
        }
    }
}

