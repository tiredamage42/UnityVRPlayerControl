using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Game {

    [System.Serializable] public class GameValueArray : NeatArrayWrapper<GameValue> { }
    
    [System.Serializable] public class GameValue
    {
        public enum GameValueComponent { BaseValue, BaseMinValue, BaseMaxValue, Value, MinValue, MaxValue };
        public string name;
        [TextArea] public string description;
        [HideInInspector] public float baseValue;
        public Vector2 baseMinMax = new Vector2(0,500);
        public Vector2 initializationRange;
        public bool showInStats;

        public GameValue(string name, float baseValue, Vector2 baseMinMax, bool showInStats, string description){
            this.name = name;
            this.baseValue = baseValue;
            this.baseMinMax = baseMinMax;
            this.showInStats = showInStats;
            this.description = description;
        }

        public GameValue (GameValue template) {
            this.name = template.name;
            this.baseValue = UnityEngine.Random.Range(template.initializationRange.x, template.initializationRange.y);
            this.baseMinMax = template.baseMinMax;
            this.showInStats = template.showInStats;
            this.description = template.description;
        }

        //not using a dictionary in order to keep thses serializable by unity
        [HideInInspector] public List<GameValueModifier> modifiers = new List<GameValueModifier>();

        float GetValue (GameValueComponent checkType, float value, float min = float.MinValue, float max = float.MaxValue) {
            for (int i = 0; i < modifiers.Count; i++) {
                if (modifiers[i].modifyValueComponent == checkType) {
                    value = modifiers[i].Modify(value);
                }
            }
            return Mathf.Clamp(value, min, max);
        }
        public float GetValue () {
            return GetValue (GameValueComponent.Value, baseValue, GetMinValue(), GetMaxValue());
        }
        public float GetMinValue () {
            return GetValue(GameValueComponent.MinValue, baseMinMax.x);
        }
        public float GetMaxValue () {
            return GetValue(GameValueComponent.MaxValue, baseMinMax.y);
        }
        public float GetValue (GameValueComponent checkType) {
            switch (checkType) {
                case GameValueComponent.Value:
                    return GetValue();
                case GameValueComponent.MinValue:
                    return GetMinValue();
                case GameValueComponent.MaxValue:
                    return GetMaxValue();
                case GameValueComponent.BaseValue:
                    return baseValue;
                case GameValueComponent.BaseMinValue:
                    return baseMinMax.x;
                case GameValueComponent.BaseMaxValue:
                    return baseMinMax.y;
            }
            return 0;
        }

        GameValueModifier GetModifier (Vector3Int key) {// int senderKey, int buffKey, int modifierKey) {
            for (int i = 0; i < modifiers.Count; i++) {
                if (modifiers[i].key == key) {
                    return modifiers[i];
                }
            }
            return null;
        }



        // delta, current, min, max
        event System.Action<float, float, float, float> onValueChange;
        public void AddChangeListener (Action<float, float, float, float> listener) {
            onValueChange += listener;
        }
        public void RemoveChangeListener (Action<float, float, float, float> listener) {
            onValueChange -= listener;
        }

        public void AddModifier (GameValueModifier modifier, int count, Vector3Int key){
            
            //permanent modifiers
            if (modifier.modifyValueComponent == GameValueComponent.BaseValue) {
                float origValue = GetValue();
                float minVal = GetMinValue();
                float maxVal = GetMaxValue();
                                
                baseValue = modifier.Modify(baseValue);
                
                //clamp the base value
                baseValue = Mathf.Clamp(baseValue, minVal, maxVal);

                if (onValueChange != null) {
                    // delta, current, min, max
                    float newVal = GetValue();
                    onValueChange(newVal - origValue, newVal, minVal, maxVal);
                }
                return;
            }


            if (modifier.modifyValueComponent == GameValueComponent.BaseMinValue) {
                float origValue = GetValue();
                float maxVal = GetMaxValue();
                                
                baseMinMax.x = modifier.Modify(baseMinMax.x);
                float minVal = GetMinValue();
                
                //clamp the base value
                baseValue = Mathf.Clamp(baseValue, minVal, maxVal);

                if (onValueChange != null) {
                    // delta, current, min, max
                    float newVal = GetValue();
                    onValueChange(newVal - origValue, newVal, minVal, maxVal);
                }
                
                return;
            }
            if (modifier.modifyValueComponent == GameValueComponent.BaseMaxValue) {

                float origValue = GetValue();
                float minVal = GetMinValue();
                baseMinMax.y = modifier.Modify(baseMinMax.y);
                float maxVal = GetMaxValue();


                //clamp the base value
                baseValue = Mathf.Clamp(baseValue, minVal, maxVal);

                if (onValueChange != null) {
                    // delta, current, min, max
                    float newVal = GetValue();
                    onValueChange(newVal - origValue, newVal, minVal, maxVal);
                }
                
                
                return;
            }


            {
                float origValue = GetValue();

                GameValueModifier existingModifier = GetModifier ( key );
                if (existingModifier != null) {
                    existingModifier.count += count;
                }
                else {
                    modifiers.Add(new GameValueModifier(modifier, count, key));
                }

                if (onValueChange != null) {
                    // delta, current, min, max
                    float newVal = GetValue();
                    onValueChange(newVal - origValue, newVal, GetMinValue(), GetMaxValue());
                }
            }
        }

        public void RemoveModifier (GameValueModifier modifier, int count, Vector3Int key){
            if (modifier.isPermanent) return;
            
            GameValueModifier existingModifier = GetModifier ( key );
            if (existingModifier != null) {
                float origValue = GetValue();
                
                existingModifier.count -= count;
                if (existingModifier.count <= 0) {
                    modifiers.Remove(existingModifier);
                }

                if (onValueChange != null) {
                    // delta, current, min, max
                    float newVal = GetValue();
                    onValueChange(newVal - origValue, newVal, GetMinValue(), GetMaxValue());
                }
            }
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(GameValue))] public class GameValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            GUIContent noContent = GUIContent.none;
            EditorGUI.BeginProperty(position, label, property);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float x = EditorTools.DrawIndent (oldIndent, position.x);

            float origX = x;
    
            SerializedProperty nameProp = property.FindPropertyRelative("name");
            string s = nameProp.stringValue;
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)) nameProp.stringValue = "Game Value Name";
            
            EditorGUI.PropertyField(new Rect(x, position.y, 125, singleLineHeight), nameProp, noContent);
            x += 125;

            EditorGUI.LabelField(new Rect(x, position.y, 70, singleLineHeight), new GUIContent("Base Range:"));
            EditorGUI.PropertyField(new Rect(x + 70, position.y, 90, singleLineHeight), property.FindPropertyRelative("baseMinMax"), GUIContent.none);
            
            x += 90 + 70 + 10;

            EditorGUI.LabelField(new Rect(x, position.y, 64, singleLineHeight), new GUIContent("Init Range:"));
            EditorGUI.PropertyField(new Rect(x + 64, position.y, 90, singleLineHeight), property.FindPropertyRelative("initializationRange"), GUIContent.none);
            
            //showInStats
            
            EditorGUI.PropertyField(new Rect(origX, position.y + singleLineHeight, position.width, singleLineHeight * 3), property.FindPropertyRelative("description"));//, GUIContent.none);
            

            EditorGUI.LabelField(new Rect(origX+125, position.y + singleLineHeight, 80, singleLineHeight), new GUIContent("Show In Stats:"));
            
            EditorGUI.PropertyField(new Rect(origX+125+80, position.y + singleLineHeight, position.width, singleLineHeight), property.FindPropertyRelative("showInStats"), GUIContent.none);
            

            EditorGUI.indentLevel = oldIndent;
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 4;
        }
    }
    
#endif

}