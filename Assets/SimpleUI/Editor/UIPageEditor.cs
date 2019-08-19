using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SimpleUI{
    public abstract class UIElementHolderEditor<T> : Editor where T : UIElementHolder {
        T holder;
        protected abstract string NewElementString();
        void OnEnable () {
            holder = target as T;
        }
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            DrawAddElementButton();
        }
        void DrawAddElementButton () {
            if (GUILayout.Button("Add " + NewElementString())) {
                holder.AddNewSelectableElement(NewElementString(), true);
            }
        }
    }

    [CustomEditor(typeof(UIPage))] public class UIPageEditor : UIElementHolderEditor<UIPage>
    {
        protected override string NewElementString () {
            return "New Button";
        }
    }
    [CustomEditor(typeof(UIRadial))] public class UIRadialEditor : UIElementHolderEditor<UIRadial>
    {
        protected override string NewElementString () {
            return "New Radial";
        }
    }
}
