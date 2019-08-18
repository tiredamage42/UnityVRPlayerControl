using System.Collections.Generic;
using UnityEngine;
namespace Game.GameUI {

    public class UIGameValuePage : UISelectableElementHandler
    {
        protected override int ParamsLength () { return 2; }
        
        protected override bool CheckParameters (object[] parameters) {
            
            if ((parameters[0] as string) == null) {
                Debug.LogError("Cant open " + GetType().ToString() + " ui handler, nameToShow null");
                return false;
            }
            if ((parameters[1] as Dictionary<string, GameValue>) == null) {
                Debug.LogError("Cant open " + GetType().ToString() + " ui handler, valuesToShow null");
                return false;
            }
            return true;
        }

        protected override object[] GetDefaultColdOpenParams() { 
            return new object[] { myActor.actorName, myActor.actorValues }; 
        }

        protected override void OnOpenUI(object[] parameters) { 
            BuildButtons((parameters[0] as string) + " Stats", true, 0, new object[] { parameters[1] });
        }

        protected override List<object> BuildButtonObjectsListForDisplay (int panelIndex, object[] updateButtonsParams) { 
            Dictionary<string, GameValue> valuesToShow = updateButtonsParams[0] as Dictionary<string, GameValue>;
            List<object> r = new List<object>();
            foreach (var k in valuesToShow.Keys) {
                if (valuesToShow[k].showInStats) {
                    r.Add(valuesToShow[k]);
                }
            }
            return r;
        }

        protected override string GetDisplayForButtonObject(object obj) { 
            return (obj as GameValue).name; 
        }

        protected override void OnUISelect (GameObject[] data, object[] customData) { 
                    (uiObject as SimpleUI.UIPage).textPanel.SetText("");

            if (customData != null) {
                GameValue g = customData[0] as GameValue;   
                if (g != null) {

                    string textToShow = g.description + "\n\n" + g.GetValue() + " / " + g.GetMaxValue();
                    
                    // TODO: add modifiers to show

                    (uiObject as SimpleUI.UIPage).textPanel.SetText(textToShow);
                }
            }
        }

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input, int actionOffset) { }
    }
}
