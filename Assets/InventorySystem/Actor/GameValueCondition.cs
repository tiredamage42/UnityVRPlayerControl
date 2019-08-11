// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace ActorSystem {

    [System.Serializable] public class GameValueCondition {

        public static bool ConditionsMet (GameValueCondition[] conditions, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues){//GameValueHolder holder) {
            
            if (conditions == null || conditions.Length == 0) {
                return true;
            }

            bool met = false;
            ConditionLink lastLink = ConditionLink.Or;
            bool falseUntilNextOr = false;

            for (int i =0 ; i < conditions.Length; i++) {
                
                bool conditionMet = falseUntilNextOr ? false : conditions[i].IsMet(selfGameValues, suppliedGameValues);

                if (lastLink == ConditionLink.Or) {
                    met = met || conditionMet;
                }
                else if (lastLink == ConditionLink.And) {
                    met = met && conditionMet;
                }

                lastLink = conditions[i].link;

                if (lastLink == ConditionLink.Or) {
                    if (met) {
                        return true;
                    }
                    falseUntilNextOr = false;
                }
                else if (lastLink == ConditionLink.And) {
                    if (!met) {
                        falseUntilNextOr = true;
                    }
                }
            }
            return met;
        }

        public enum ConditionCheck { 
            Equals = 0, LessThan = 1, GreaterThan = 2, LessThanEqualTo = 3, GreaterThanEqualTo = 4 
        };
        
        public enum ConditionLink { And, Or }

        public bool useSuppliedValues;
        public bool trueIfNoValue = true;
        public string gameValueName;
        public GameValue.GameValueComponent component;
        public ConditionCheck condition;
        public float valueCheck;
        public ConditionLink link;

        public bool IsMet (Dictionary<string, GameValue> selfValues, Dictionary<string, GameValue> suppliedValues) {
            
            Dictionary<string, GameValue> gameValues = useSuppliedValues ? suppliedValues : selfValues;
            GameValue gameValue;
            if (!gameValues.TryGetValue(gameValueName, out gameValue)) {
                Debug.LogError("Cant find game value: " + gameValueName);
                return trueIfNoValue;
            }

            float value = gameValue.GetValue(component);

            if (condition == ConditionCheck.Equals)
                return value == valueCheck;
            else if (condition == ConditionCheck.LessThan)
                return value < valueCheck;
            else if (condition == ConditionCheck.GreaterThan)
                return value > valueCheck;
            else if (condition == ConditionCheck.LessThanEqualTo)
                return value <= valueCheck;
            else if (condition == ConditionCheck.GreaterThanEqualTo)
                return value >= valueCheck;
            return trueIfNoValue;
        }
    }

    [System.Serializable] public class GameValueConditionArray : NeatArrayWrapper<GameValueCondition> { }
        


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GameValueCondition))] public class GameValueConditionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float[] widths = new float[] {
                125, 90, 40, 80, 70, 20, 50
            };

            float x = position.x;

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            x = EditorTools.DrawIndent (oldIndent, x);





            SerializedProperty selfOrSuppliedProp = property.FindPropertyRelative("useSuppliedValues");
            selfOrSuppliedProp.boolValue = EditorGUI.Popup (new Rect(x, position.y, 60, EditorGUIUtility.singleLineHeight), "", selfOrSuppliedProp.boolValue ? 1 : 0, new string[] { "Self", "Supplied" }) == 1;
            x += 60;


            int i = 0;
                
            SerializedProperty p = property.FindPropertyRelative("gameValueName");
            string s = p.stringValue;
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)) p.stringValue = "Game Value Name";
            
            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight), p, GUIContent.none);
            x += widths[i];
            i++;
            
            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("component"), GUIContent.none);
            x+= widths[i];
            i++;

            SerializedProperty condition = property.FindPropertyRelative("condition");
            condition.enumValueIndex = EditorGUI.Popup (new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight), condition.enumValueIndex, new string[] { " = ", " < ", " > ", " <= ", " >= " });
            x+= widths[i];
            i++;

            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("valueCheck"), GUIContent.none);
            x+= widths[i];
            i++;

            EditorGUI.LabelField(new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight), "True If Null:");
            x+= widths[i];
            i++;
            
            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("trueIfNoValue"), GUIContent.none);
            x += widths[i];
            i++;
            
            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("link"), GUIContent.none);
            
            EditorGUI.EndProperty();

            EditorGUI.indentLevel = oldIndent;   
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
#endif

}
