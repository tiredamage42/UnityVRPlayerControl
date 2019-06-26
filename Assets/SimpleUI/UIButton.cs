using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI {

    [ExecuteInEditMode]
    public class UIButton : SelectableElement
    {
        Image _mainImage;
        Image buttonImage {
            get {
                if (_mainImage == null) {
                    _mainImage = GetComponent<Image>();
                }
                return _mainImage;
            }
        }

        protected override void UpdateElement () {
            buttonImage.color = selected ? UIManager.instance.mainLightColor : (Color32)Color.clear;// UIManager.instance.mainDarkColor;
            
            uiText.invert = selected;
            uiText.UpdateColors();
        }


        protected override void OnSubmit () {
        
        }
        protected override void OnSelect() {
        
        }
        protected override void OnDeselect() {
        
        }
    }
}