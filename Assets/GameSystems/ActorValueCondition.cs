// using System.Collections.Generic;
using UnityEngine;

using Game.PerkSystem;
using Game.InventorySystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game {

    [System.Serializable] public class ActorValueCondition {
        public static bool ConditionsMet (ActorValueCondition[] conditions, Actor selfActor, Actor suppliedActor) {
            
            if (conditions == null || conditions.Length == 0) return true;
            
            bool met = false;
            ConditionLink lastLink = ConditionLink.Or;
            bool falseUntilNextOr = false;

            for (int i =0 ; i < conditions.Length; i++) {
                
                bool conditionMet = falseUntilNextOr ? false : conditions[i].IsMet(selfActor, suppliedActor);

                if (lastLink == ConditionLink.Or) met = met || conditionMet;
                else if (lastLink == ConditionLink.And) met = met && conditionMet;
                
                lastLink = conditions[i].link;

                if (lastLink == ConditionLink.Or) {
                    if (met) return true;
                    falseUntilNextOr = false;
                }
                else if (lastLink == ConditionLink.And) {
                    if (!met) falseUntilNextOr = true;
                }
            }
            return met;
        }


        public enum ConditionType { GameValue = 0, Inventory = 1, Perk = 2 }
        public enum InventoryCheckType { StashCount = 0, IsEquipped = 1, NotEquipped = 2 }
        public enum ConditionCheck { Equals = 0, LessThan = 1, GreaterThan = 2, LessThanEqualTo = 3, GreaterThanEqualTo = 4 };
        public enum ConditionLink { And, Or }
        
        public bool useSuppliedValues;
        public ConditionType conditionType;
        public InventoryCheckType inventoryCheckType;
        public ItemBehavior itemCheck;
        
        public Perk perkCheck;
        public int intCheckLevel;        
        public string gameValueName;
        public bool trueIfNoValue = true;
        public GameValue.GameValueComponent component;
        public ConditionCheck condition;
        public float valueCheck;

        public ConditionLink link;

        bool CheckInteger (int checkValue) {
            if (condition == ConditionCheck.Equals)
                return checkValue == intCheckLevel;
            else if (condition == ConditionCheck.LessThan)
                return checkValue < intCheckLevel;
            else if (condition == ConditionCheck.GreaterThan)
                return checkValue > intCheckLevel;
            else if (condition == ConditionCheck.LessThanEqualTo)
                return checkValue <= intCheckLevel;
            else if (condition == ConditionCheck.GreaterThanEqualTo)
                return checkValue >= intCheckLevel;
            return false;
        }

        public bool IsMet (Actor selfActor, Actor suppliedActor) {
            
            Actor actorToUse = useSuppliedValues ? suppliedActor : selfActor;
            
            if (conditionType == ConditionType.GameValue) {

                GameValue gameValue;
                if (!actorToUse.actorValues.TryGetValue(gameValueName, out gameValue)) {
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
            else if (conditionType == ConditionType.Inventory) {

                if (inventoryCheckType == InventoryCheckType.IsEquipped) {
                    if (actorToUse.inventory == null) {
                        return false;
                    }
                    if (actorToUse.inventory.equipper == null) {
                        return false;
                    }
                    return actorToUse.inventory.equipper.ItemIsEquipped(intCheckLevel, itemCheck);
                }
                else if (inventoryCheckType == InventoryCheckType.NotEquipped) {
                    if (actorToUse.inventory == null) 
                        return true;
                    
                    if (actorToUse.inventory.equipper == null) 
                        return true;
                    
                    return !actorToUse.inventory.equipper.ItemIsEquipped(intCheckLevel, itemCheck);
                
                }
                else if (inventoryCheckType == InventoryCheckType.StashCount) {
                    
                    if (actorToUse.inventory == null) 
                        return intCheckLevel <= 0;
                    
                    return CheckInteger(actorToUse.inventory.GetItemCount(itemCheck));
                }
            }
            else if (conditionType == ConditionType.Perk) {
                if (perkCheck == null)
                    return true;
                
                if (actorToUse.perkHandler == null)
                    return intCheckLevel <= 0;
                
                return CheckInteger(actorToUse.perkHandler.GetPerkLevel(perkCheck));
            }
            return false;       
        }
    }

    [System.Serializable] public class ActorValueConditionArray : NeatArrayWrapper<ActorValueCondition> { }
        
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ActorValueCondition))] public class ActorValueConditionDrawer : PropertyDrawer
    {
        static readonly string[] selfSuppliedOptions = new string[] { "Self", "Supplied" };
        static readonly string[] conditionTypeOptions = new string[] { "Game Value", "Inventory", "Perk Level" };
        static readonly string[] conditionsOptions = new string[] { " = ", " < ", " > ", " <= ", " >= " };
        static readonly string[] inventoryCheckOptions = new string[] { "Stash Count", "Equipped To Slot", "Not Equipped To Slot" };
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float x = position.x;

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            x = EditorTools.DrawIndent (oldIndent, x);

            float width = 60;
            SerializedProperty selfOrSuppliedProp = property.FindPropertyRelative("useSuppliedValues");
            selfOrSuppliedProp.boolValue = EditorGUI.Popup (new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), "", selfOrSuppliedProp.boolValue ? 1 : 0, selfSuppliedOptions) == 1;
            x += width;

            width = 80;
            SerializedProperty conditionType = property.FindPropertyRelative("conditionType");
            conditionType.enumValueIndex = EditorGUI.Popup (new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), conditionType.enumValueIndex, conditionTypeOptions);
            x+= width;

            // game value check
            if (conditionType.enumValueIndex == 0) {
                
                width = 125;
                SerializedProperty p = property.FindPropertyRelative("gameValueName");
                string s = p.stringValue;
                if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)) p.stringValue = "Game Value Name";
                EditorGUI.PropertyField(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), p, GUIContent.none);
                x+= width;

                width = 90;    
                EditorGUI.PropertyField(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("component"), GUIContent.none);
                x+= width;

                width = 40;
                SerializedProperty condition = property.FindPropertyRelative("condition");
                condition.enumValueIndex = EditorGUI.Popup (new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), condition.enumValueIndex, conditionsOptions);
                x+= width;

                width = 60;
                EditorGUI.PropertyField(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("valueCheck"), GUIContent.none);
                x+= width;
            
                width = 70;
                EditorGUI.LabelField(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), "True If Null:");
                x+= width;

                width = 20;
                EditorGUI.PropertyField(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("trueIfNoValue"), GUIContent.none);
                x+= width;
            }
            // inventory check
            else if (conditionType.enumValueIndex == 1) {

                width = 175;
                InventorySystemEditor.itemSelector.Draw(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("itemCheck"), GUIContent.none);
                x+= width;

                width = 110;
                SerializedProperty inventoryCheckType = property.FindPropertyRelative("inventoryCheckType");
                inventoryCheckType.enumValueIndex = EditorGUI.Popup (new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), inventoryCheckType.enumValueIndex, inventoryCheckOptions);
                x+= width;

                //stash count check
                if (inventoryCheckType.enumValueIndex == 0) {

                    width = 40;
                    SerializedProperty condition = property.FindPropertyRelative("condition");
                    condition.enumValueIndex = EditorGUI.Popup (new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), condition.enumValueIndex, conditionsOptions);
                    x+= width;
                }

                width = 60;
                EditorGUI.PropertyField(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("intCheckLevel"), GUIContent.none);
                x+= width;
            }
            //perk check
            else if (conditionType.enumValueIndex == 2) {

                width = 175;
                PerkSystemEditor.itemSelector.Draw(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("perkCheck"), GUIContent.none);
                x+= width;

                width = 40;
                SerializedProperty condition = property.FindPropertyRelative("condition");
                condition.enumValueIndex = EditorGUI.Popup (new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), condition.enumValueIndex, conditionsOptions);
                x+= width;

                width = 60;
                EditorGUI.PropertyField(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("intCheckLevel"), GUIContent.none);
                x+= width;
            }

            width = 50;

            x = EditorTools.DrawIndent (oldIndent, position.x) + 280+125+80+60;
            EditorGUI.PropertyField(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("link"), GUIContent.none);
            
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
