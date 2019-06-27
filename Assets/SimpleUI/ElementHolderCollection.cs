using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SimpleUI{

    public class ElementHolderCollection : UIElementHolder
    {

        protected override Image Background() {
            return null;
        }
        protected override Image BackgroundOverlay() {
            return null;
        }
        protected override SelectableElement ElementPrefab() {
            return null;
        }
        
        
    }
}
