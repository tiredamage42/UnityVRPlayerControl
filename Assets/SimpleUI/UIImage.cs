using UnityEngine;
using UnityEngine.UI;
namespace SimpleUI {
    /* used for keeping ui image color consistent */
    [ExecuteInEditMode] [RequireComponent(typeof(Image))]
    public class UIImage : UIGraphic<Image> { }
}
