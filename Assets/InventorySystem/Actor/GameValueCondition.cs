// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace ActorSystem {

    [System.Serializable] public class GameValueCondition {

        public static bool ConditionsMet (GameValueCondition[] conditions, Dictionary<string, GameValue> gameValues){//GameValueHolder holder) {
            
            if (conditions == null || conditions.Length == 0) {
                return true;
            }

            bool met = false;
            ConditionLink lastLink = ConditionLink.Or;
            bool falseUntilNextOr = false;

            for (int i =0 ; i < conditions.Length; i++) {
                
                bool conditionMet = falseUntilNextOr ? false : conditions[i].IsMet(gameValues);

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
        public bool trueIfNoValue = true;
        public string gameValueName;
        public GameValue.GameValueComponent component;
        public ConditionCheck condition;
        public float valueCheck;
        public ConditionLink link;

        public bool IsMet (Dictionary<string, GameValue> gameValues) {
            
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
    [CustomPropertyDrawer(typeof(GameValueCondition))]
    public class GameValueConditionDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            float offset = 0;//30;


            float[] widths = new float[] {
                125,
                90,
                40,
                80, 
                70, 
                20, 
                50
            };

            float x = position.x;

            int i = 0;
                
            var amountRect = new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight);
            x += widths[i]-offset;
            i++;
            SerializedProperty p = property.FindPropertyRelative("gameValueName");
            string s = p.stringValue;
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)) {
                p.stringValue = "Game Value Name";
            }
            EditorGUI.PropertyField(amountRect, p, GUIContent.none);
            
            
            amountRect = new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight);
            x+= widths[i]-offset;
            i++;
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("component"), GUIContent.none);


            SerializedProperty condition = property.FindPropertyRelative("condition");
            amountRect = new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight);
            x+= widths[i]-offset;
            i++;
            condition.enumValueIndex = EditorGUI.Popup (amountRect, condition.enumValueIndex, new string[] { " = ", " < ", " > ", " <= ", " >= " });

            amountRect = new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight);
            x+= widths[i]-offset;
            i++;
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("valueCheck"), GUIContent.none);

            amountRect = new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight);
            x+= widths[i]-offset;
            i++;
            EditorGUI.LabelField(amountRect, "True If Null:");
            
            amountRect = new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight);
            x += widths[i];//-offset;
            i++;
            
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("trueIfNoValue"), GUIContent.none);

            amountRect = new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("link"), GUIContent.none);
            
            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;// * 3;// * 6;
            // return base.GetPropertyHeight(property, label);
            // return EditorGUI.GetPropertyHeight(property, label);
        }
    }
#endif

}
