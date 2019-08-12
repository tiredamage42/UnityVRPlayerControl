// using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace ActorSystem {

    [System.Serializable] public class GameValueModifierArray : NeatArrayWrapper<GameValueModifier> { }
    [System.Serializable] public class GameValueModifierArray2D { [NeatArray] public GameValueModifierArray[] list; }

    [System.Serializable] public class GameValueModifier {
        [NeatArray] public GameValueConditionArray conditions;
        
        public GameValueModifier () {
            count = 1;
        }
        public GameValueModifier (GameValueModifier template, int count, Vector3Int key) {
            this.key = key;
            this.count = count;

            gameValueName = template.gameValueName;
            modifyValueComponent = template.modifyValueComponent;
            modifyBehavior = template.modifyBehavior;
            modifyValue = template.modifyValue;
        }
            
        [HideInInspector] public Vector3Int key; //sender, buff, modifier
        [HideInInspector] public int count = 1;
        int getCount { get { return isStackable ? count : 1; } }


        public bool isStackable;
        public string gameValueName = "Game Value Name";

        // [Header("Base Value modifiers are permanent")]
        public GameValue.GameValueComponent modifyValueComponent;
        
        public enum ModifyBehavior { Set, Add, Multiply };
        public ModifyBehavior modifyBehavior;
        public float modifyValue = 0;

        public float Modify(float baseValue) {
            if (modifyBehavior == ModifyBehavior.Set)
                return modifyValue;
            else if (modifyBehavior == ModifyBehavior.Add)
                return baseValue + (modifyValue * getCount);
            else if (modifyBehavior == ModifyBehavior.Multiply)
                return baseValue * (modifyValue * getCount);
            return baseValue;
        }


        string modifyBehaviorString {
            get {
                if (modifyBehavior == ModifyBehavior.Set)
                    return "=";
                else if (modifyBehavior == ModifyBehavior.Add)
                    return "+";
                else if (modifyBehavior == ModifyBehavior.Multiply) 
                    return "x";
                return "UHHH";
            }
        }
        public string gameMessageToShow {
            get {
                return gameValueName + " " + modifyBehaviorString + modifyValue.ToString(); 
            }
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GameValueModifier))] public class GameValueModifierDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            GUIContent noContent = GUIContent.none;
            EditorGUI.BeginProperty(position, label, property);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
    

            int i = 0;
            float[] widths = new float[] { 60, 125, 90, 80, 60, 15, };

            float x = EditorTools.DrawIndent (oldIndent, position.x);
            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], singleLineHeight), property.FindPropertyRelative("modifyBehavior"), noContent);
            x += widths[i++];
            
            SerializedProperty nameProp = property.FindPropertyRelative("gameValueName");
            string s = nameProp.stringValue;
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)) nameProp.stringValue = "Game Value Name";
            
            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], singleLineHeight), nameProp, noContent);
            x+= widths[i++];
            
            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], singleLineHeight), property.FindPropertyRelative("modifyValueComponent"), noContent);
            x+= widths[i++];
            
            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], singleLineHeight), property.FindPropertyRelative("modifyValue"), noContent);
            x+= widths[i++];
            
            EditorGUI.LabelField(new Rect(x, position.y, widths[i], singleLineHeight), "Stackable:");
            x+= widths[i++];
            
            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], singleLineHeight), property.FindPropertyRelative("isStackable"), noContent);
            
            EditorGUI.indentLevel = oldIndent + 1;
            SerializedProperty conditionsProp = property.FindPropertyRelative("conditions");
            EditorGUI.PropertyField(new Rect(position.x, position.y + singleLineHeight, position.width, (EditorGUI.GetPropertyHeight(conditionsProp, true))), conditionsProp, new GUIContent("Conditions"));
            EditorGUI.indentLevel = oldIndent;
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + (EditorGUI.GetPropertyHeight(property.FindPropertyRelative("conditions"), true));
        }
    }
#endif

}