using UnityEngine;

using UnityEngine.UI;

namespace VRPlayer.UI {
    [ExecuteInEditMode] public class TrackpadGhostUI : MonoBehaviour
    {
        public Vector2 axis;
        public float middleZone = .1f;
        public bool inMiddle;
        public float showSize = .2f;
        public RectTransform axisShowRect;
        public RectTransform middleShowRect;
        public RectTransform inMiddleRect;




        
        void OnEnable () {
            UpdateAxisShow();
        }
    
        void UpdateAxisShow () {
            if (axisShowRect == null) axisShowRect = transform.GetChild(1).GetComponent<RectTransform>();
            
            
            if (inMiddleRect != null) inMiddleRect.gameObject.SetActive(inMiddle);
            if (axisShowRect != null) axisShowRect.gameObject.SetActive(!inMiddle);
            
            
            if (axisShowRect != null) {
                float halfShow = showSize * .5f;
                Vector2 a = new Vector2(
                    Mathf.Clamp((axis.x * .5f + .5f), halfShow, 1-halfShow), 
                    Mathf.Clamp((axis.y * .5f + .5f), halfShow, 1-halfShow)
                );
                axisShowRect.anchorMin = a;
                axisShowRect.anchorMax = a;
// #if UNITY_EDITOR
                axisShowRect.localScale = Vector3.one * showSize;
// #endif
            }

            if (middleShowRect != null) middleShowRect.localScale = Vector3.one * middleZone;
            if (inMiddleRect != null) inMiddleRect.localScale = Vector3.one * middleZone;// * .9f;
            
        }

        void Update()
        {
            UpdateAxisShow();
        }
    }
}

