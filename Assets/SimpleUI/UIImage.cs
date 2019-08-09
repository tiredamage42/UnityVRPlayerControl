using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI {

    /*
        used for keeping ui image color consistent
    */

    [ExecuteInEditMode][RequireComponent(typeof(Image))]
    public class UIImage : MonoBehaviour
    {
        public bool useDark;
        Image image;
        
        void GetImage () {
            if (image == null) image = GetComponent<Image>();
        }
        void SetImageColor () {
            GetImage();
            if (image != null) image.color = useDark ? UIManager.instance.mainDarkColor : UIManager.instance.mainLightColor;
        }

    #if UNITY_EDITOR
        void Update () {
            SetImageColor();
        }
    #endif

        void OnEnable () {
            SetImageColor();
        }    
    }
}
