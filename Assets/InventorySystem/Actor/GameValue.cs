using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace ActorSystem {
    [System.Serializable] public class GameValue
    {
        public enum GameValueComponent { BaseValue, BaseMinValue, BaseMaxValue, Value, MinValue, MaxValue };


        
        
        public string name;
        [HideInInspector] public float baseValue;
        // public float baseMinValue=0, baseMaxValue=500;
        public Vector2 baseMinMax = new Vector2(0,500);
        
        public Vector2 initializationRange;

        public GameValue(string name, float baseValue, Vector2 baseMinMax){// float baseMinValue, float baseMaxValue) {
            this.name = name;
            this.baseValue = baseValue;
            this.baseMinMax = baseMinMax;
            // this.baseMinValue = baseMinValue;
            // this.baseMaxValue = baseMaxValue;
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
            return GetValue(GameValueComponent.MinValue, baseMinMax.x);// baseMinValue);
        }
        public float GetMaxValue () {
            return GetValue(GameValueComponent.MaxValue, baseMinMax.y);// baseMaxValue);
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

                // case GameValueComponent.BaseMinValue:
                //     return baseMinValue;
                // case GameValueComponent.BaseMaxValue:
                //     return baseMaxValue;
            }
            return 0;
        }

        GameValueModifier GetModifier (Vector3Int key) {// int senderKey, int buffKey, int modifierKey) {
            for (int i = 0; i < modifiers.Count; i++) {
                if (modifiers[i].key == key) {
                    return modifiers[i];
                }

                // //coming from the same buff holder
                // if (modifiers[i].senderKey == senderKey) { 
                //     //coming from the same buff
                //     if (modifiers[i].modifierKey == modifierKey) { 
                //         return modifiers[i];
                //     }
                // }
            }

            return null;
        }



        // delta, current, min, max
        event System.Action<float, float, float, float> onValueChange;
        public void AddChangeListener (System.Action<float, float, float, float> listener) {
            onValueChange += listener;
        }
        public void RemoveChangeListener (System.Action<float, float, float, float> listener) {
            onValueChange -= listener;
        }


        void ClampBaseValue (float minVal, float maxVal) {

        }

        public void AddModifier (GameValueModifier modifier, int count, int senderKey, int buffKey, int modifierKey) {
            
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

                Vector3Int key = new Vector3Int(senderKey, buffKey, modifierKey);
                GameValueModifier existingModifier = GetModifier ( key );//senderKey, buffKey, modifierKey );
                if (existingModifier != null) {
                    existingModifier.count += count;
                }
                else {
                    modifiers.Add(new GameValueModifier(modifier, count, key));//senderKey, modifierKey));
                }

                if (onValueChange != null) {
                    // delta, current, min, max
                    float newVal = GetValue();
                    onValueChange(newVal - origValue, newVal, GetMinValue(), GetMaxValue());
                }
            }
        }



        public void RemoveModifier (GameValueModifier modifier, int count, int senderKey, int buffKey, int modifierKey) {
            if (modifier.modifyValueComponent == GameValueComponent.BaseValue) return;
            if (modifier.modifyValueComponent == GameValueComponent.BaseMinValue) return;
            if (modifier.modifyValueComponent == GameValueComponent.BaseMaxValue) return;
            
            // if (modifier.isPermanent) {

            // }
            // else {




                Vector3Int key = new Vector3Int(senderKey, buffKey, modifierKey);
            
                GameValueModifier existingModifier = GetModifier ( key );//senderKey, buffKey, modifierKey );
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
            // }
        }
    }
    [System.Serializable] public class GameValueModifier {
        [DisplayedArray(new float[] {0,0,0,.1f}, false)] 
        public GameValueConditionArray conditions;
        

        public GameValueModifier () {
            count = 1;
        }
        public GameValueModifier (GameValueModifier template, int count, 
            Vector3Int key
            // int senderKey, int modifierKey
        
        ) {
            this.key = key;
            // this.senderKey = senderKey;
            // this.modifierKey = modifierKey;
            this.count = count;

            gameValueName = template.gameValueName;
            modifyValueComponent = template.modifyValueComponent;
            modifyBehavior = template.modifyBehavior;
            modifyValue = template.modifyValue;
        }
            
        // [HideInInspector] public int senderKey, modifierKey;
        [HideInInspector] public Vector3Int key; //sender, buff, modifier
        [HideInInspector] public int count = 1;
        int getCount { get { return isStackable ? count : 1; } }


        public bool isStackable;
        public string gameValueName = "Game Value Name";

        // [Header("Base Value modifiers are permanent")]
        public GameValue.GameValueComponent modifyValueComponent;
        
        public enum ModifyBehavior { Set, Add, Multiply };
        //game value modifiers that SET value are permanenet
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

        // Set | Add | Multiply

        // Base | Max     

        // Variable Name

        // Value

        // isOneOff  
        //     (modifier cant be removed, and is permanent 
        //         i.e level up adds 100 to max health, 
        //         or health pack adds health but then is let go (so cant remove modifier)
        //     )

        // gameMessage 
    }
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(GameValueModifier))]
    public class GameValueModifierDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            float offset = 0;

            float x = position.x;

            int i = 0;

            float[] widths = new float[] {
                60,
                125,
                90,
                80, 
                60, 
                15,
            };

                
            var amountRect = new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight);
            x += widths[i]-offset;
            i++;

            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("modifyBehavior"), GUIContent.none);
            
            amountRect = new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight);
            x+= widths[i]-offset;
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
            
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("modifyValueComponent"), GUIContent.none);
            
            amountRect = new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight);
            x+= widths[i]-offset;
            i++;
            
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("modifyValue"), GUIContent.none);
            
            amountRect = new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight);
            x+= widths[i]-offset;
            i++;
            
            EditorGUI.LabelField(amountRect, "Stackable:");
            
            amountRect = new Rect(x, position.y, widths[i], EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("isStackable"), GUIContent.none);
            

            // amountRect = new Rect(position.x + 16, position.y + EditorGUIUtility.singleLineHeight, position.width, (EditorGUI.GetPropertyHeight(property.FindPropertyRelative("conditions"), true)));
            amountRect = new Rect(position.x + 16, position.y + EditorGUIUtility.singleLineHeight, position.width, (EditorGUI.GetPropertyHeight(property.FindPropertyRelative("conditions"), true)));
            
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("conditions"), new GUIContent("Conditions"));
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + (EditorGUI.GetPropertyHeight(property.FindPropertyRelative("conditions"), true));
        }
    }
#endif

}