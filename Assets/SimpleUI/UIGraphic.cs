using UnityEngine;
using UnityEngine.UI;
namespace SimpleUI {
    [ExecuteInEditMode] public abstract class UIGraphic<T> : MonoBehaviour where T : Graphic
    {
        public bool overrideColors;
        public RectTransform rectTransform { get { return mainGraphic.rectTransform; } }
        
        T _mainGraphic;
        public T mainGraphic { 
            get { 
                if (_mainGraphic == null) _mainGraphic = GetComponent<T>();
                return _mainGraphic;
            } 
        }
        
        public UIColorScheme colorScheme;
        public bool useDark;
        protected virtual void OnEnable () {
            UpdateGraphicColors();
        }
        public void SetColorScheme (UIColorScheme colorScheme, bool useDark) {
            this.useDark = useDark;
            this.colorScheme = colorScheme;
            UpdateGraphicColors();
        }
        public virtual void UpdateGraphicColors () {
            mainGraphic.color = UIManager.GetColor(colorScheme, useDark );
        }
    }
}
