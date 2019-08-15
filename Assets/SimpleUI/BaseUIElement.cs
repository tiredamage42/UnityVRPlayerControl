// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace SimpleUI {
    public abstract class BaseUIElement : MonoBehaviour
    {
        public GameObject baseObject;

        RectTransform _rectTransform;
        public RectTransform rectTransform {
            get {
                if (_rectTransform) _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }


        
    }

}
