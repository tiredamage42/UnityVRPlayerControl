using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SimpleUI{


    public abstract class UIElementHolderEditor<T> : Editor where T : UIElementHolder {

        T holder;

        void OnEnable () {
            holder = target as T;
        }
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            DrawAddElementButton();
        }

        protected abstract string NewElementString();

        void DrawAddElementButton () {
            if (GUILayout.Button("Add " + NewElementString())) {
                holder.AddNewElement(NewElementString());
            }
        }
    }

    [CustomEditor(typeof(UIPage))]
    public class UIPageEditor : UIElementHolderEditor<UIPage>
    {
        protected override string NewElementString () {
            return "New Button";
        }
    }
    [CustomEditor(typeof(UIRadial))]
    public class UIRadialEditor : UIElementHolderEditor<UIRadial>
    {
        protected override string NewElementString () {
            return "New Radial";
        }
    }
}
