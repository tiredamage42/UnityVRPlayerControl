// using System.Collections.Generic;
using UnityEngine;

using Game.PerkSystem;
using Game.InventorySystem;
using Game.QuestSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game {






    /*
    
    public enum QuestCompletionLevel {
            Inactive, Active, Completed 
        };
        
    
     */

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


        public enum ConditionType { GameValue = 0, Inventory = 1, Perk = 2, Quest = 3, Distance = 4, Angle = 5 };
        public enum InventoryCheckType { StashCount = 0, IsEquipped = 1, NotEquipped = 2 };
        public enum ConditionCheck { Equals = 0, NotEquals = 1, LessThan = 2, GreaterThan = 3, LessThanEqualTo = 4, GreaterThanEqualTo = 5 };
        public enum ConditionLink { And=0, Or=1 };
        public enum QuestConditionType { CompletionLevel=0, InternalKey=1 };
        

        
        public enum TransformCheckType { 
            SelfTransform = 0, SuppliedTransform = 1, AnySceneItemTransform = 2, AllSceneItemTransform = 3, SceneTransform = 4 
        };
        

        public enum AngleDirectionCheck {
            Fwd = 0, Back = 1, Left = 2, Right = 3, Up = 4, Down = 5, 
            WorldFwd = 6, WorldBack = 7, WorldLeft = 8, WorldRight = 9, WorldUp = 10, WorldDown = 11, 
            Position = 12
        };

        public TransformCheckType aTransformCheckType, bTransformCheckType;
        public Transform transformA, transformB;

        public AngleDirectionCheck aAngleCheckType, bAngleCheckType;


        Vector3 GetDirectionFromTransform (Vector3 aPos, Transform transform, AngleDirectionCheck dirCheckType) {
            switch (dirCheckType) {
                case AngleDirectionCheck.Fwd:   return transform.forward; 
                case AngleDirectionCheck.Back:  return -transform.forward; 
                case AngleDirectionCheck.Left:  return -transform.right; 
                case AngleDirectionCheck.Right: return transform.right; 
                case AngleDirectionCheck.Up:    return transform.up; 
                case AngleDirectionCheck.Down:  return -transform.up; 
                case AngleDirectionCheck.Position: return transform.position - aPos; // maybe normalize            
                
                case AngleDirectionCheck.WorldFwd:      return Vector3.forward;
                case AngleDirectionCheck.WorldBack:     return -Vector3.forward; 
                case AngleDirectionCheck.WorldLeft:     return -Vector3.right; 
                case AngleDirectionCheck.WorldRight:    return Vector3.right; 
                case AngleDirectionCheck.WorldUp :      return Vector3.up; 
                case AngleDirectionCheck.WorldDown :    return -Vector3.up; 
            }

            return Vector3.up;
        }

        Vector3 GetDirectionFromActor (Vector3 aPos, Actor actor, AngleDirectionCheck dirCheckType) {
            switch (dirCheckType) {
                case AngleDirectionCheck.Fwd: return actor.GetForward(); 
                case AngleDirectionCheck.Back: return -actor.GetForward(); 
                case AngleDirectionCheck.Left: return -actor.GetRight(); 
                case AngleDirectionCheck.Right: return actor.GetRight(); 
                case AngleDirectionCheck.Up: return actor.GetUp(); 
                case AngleDirectionCheck.Down: return -actor.GetUp(); 
                case AngleDirectionCheck.Position: return actor.GetPosition() - aPos; // maybe normalize            
            }
            return GetDirectionFromTransform (aPos, null, dirCheckType);
        }


        bool CheckAngleAgainstAnySceneItems (ItemBehavior itemCheck, Vector3 aAngle, Vector3 aPos) {
            for (int i = 0; i < SceneItem.allSceneItems.Count; i++) {
                if (SceneItem.allSceneItems[i].itemBehavior == itemCheck) {
                    if (CheckFloat(Vector3.Angle(aAngle, GetDirectionFromTransform(aPos, SceneItem.allSceneItems[i].transform, bAngleCheckType)))) {
                        return true;
                    }
                }
            }
            return false;
        }
        bool CheckAngleAgainstAllSceneItems (ItemBehavior itemCheck, Vector3 aAngle, Vector3 aPos) {
            bool anyFound = false;
            for (int i = 0; i < SceneItem.allSceneItems.Count; i++) {
                if (SceneItem.allSceneItems[i].itemBehavior == itemCheck) {
                    if (!CheckFloat(Vector3.Angle(aAngle, GetDirectionFromTransform(aPos, SceneItem.allSceneItems[i].transform, bAngleCheckType)))) {
                        return false;
                    }
                    anyFound = true;
                }
            }
            return anyFound;
        }

        bool Check_A_Angle (Actor selfActor, Actor suppliedActor, Vector3 aAngle, Vector3 aPos) {
            if (bTransformCheckType == TransformCheckType.SelfTransform) 
                return CheckFloat(Vector3.Angle(aAngle, GetDirectionFromActor(aPos, selfActor, bAngleCheckType)));
            else if (bTransformCheckType == TransformCheckType.SuppliedTransform) 
                return CheckFloat(Vector3.Angle(aAngle, GetDirectionFromActor(aPos, suppliedActor, bAngleCheckType)));
            else if (bTransformCheckType == TransformCheckType.SceneTransform) 
                return CheckFloat(Vector3.Angle(aAngle, GetDirectionFromTransform(aPos, transformB, bAngleCheckType)));
            else if (bTransformCheckType == TransformCheckType.AnySceneItemTransform)
                return CheckAngleAgainstAnySceneItems (itemCheckB, aAngle, aPos);
            else if (bTransformCheckType == TransformCheckType.AllSceneItemTransform) 
                return CheckAngleAgainstAllSceneItems (itemCheckB, aAngle, aPos);

            return false;
        }
        bool CheckAngleCondition (Actor selfActor, Actor suppliedActor) {
            if (aTransformCheckType == TransformCheckType.SelfTransform) {
                Check_A_Angle(selfActor, suppliedActor, GetDirectionFromActor(Vector3.zero, selfActor, aAngleCheckType), selfActor.GetPosition());   
            }
            else if (aTransformCheckType == TransformCheckType.SuppliedTransform) {
                Check_A_Angle(selfActor, suppliedActor, GetDirectionFromActor(Vector3.zero, suppliedActor, aAngleCheckType), suppliedActor.GetPosition());   
            }
            else if (aTransformCheckType == TransformCheckType.SceneTransform) {
                Check_A_Angle(selfActor, suppliedActor, GetDirectionFromTransform(Vector3.zero, transformA, aAngleCheckType), transformA.position);   
            }
            else if (aTransformCheckType == TransformCheckType.AnySceneItemTransform) {
                for (int i = 0; i < SceneItem.allSceneItems.Count; i++) {
                    if (SceneItem.allSceneItems[i].itemBehavior == itemCheckA) {
                        if (Check_A_Angle(selfActor, suppliedActor, GetDirectionFromTransform(Vector3.zero, SceneItem.allSceneItems[i].transform, aAngleCheckType), SceneItem.allSceneItems[i].transform.position)) {
                            return true;
                        }
                    }
                }
                return false;            
            }
            else if (aTransformCheckType == TransformCheckType.AllSceneItemTransform) {
                bool anyFound = false;
                for (int i = 0; i < SceneItem.allSceneItems.Count; i++) {
                    if (SceneItem.allSceneItems[i].itemBehavior == itemCheckA) {

                        if (!Check_A_Angle(selfActor, suppliedActor, GetDirectionFromTransform(Vector3.zero, SceneItem.allSceneItems[i].transform, aAngleCheckType), SceneItem.allSceneItems[i].transform.position)) {
                            return false;
                        }
                        anyFound = true;
                    }
                }
                return anyFound;
            }
            return false;
        }




        bool CheckDistanceAgainstAnySceneItems (ItemBehavior itemCheck, Vector3 checkPosition) {
            for (int i = 0; i < SceneItem.allSceneItems.Count; i++) {
                if (SceneItem.allSceneItems[i].itemBehavior == itemCheck) {
                    if (CheckFloatSqr(Vector3.SqrMagnitude(checkPosition - SceneItem.allSceneItems[i].transform.position))) {
                        return true;
                    }
                }
            }
            return false;
        }

        bool CheckDistanceAgainstAllSceneItems (ItemBehavior itemCheck, Vector3 checkPosition) {
            bool anyFound = false;
            for (int i = 0; i < SceneItem.allSceneItems.Count; i++) {
                if (SceneItem.allSceneItems[i].itemBehavior == itemCheck) {
                    if (!CheckFloatSqr(Vector3.SqrMagnitude(checkPosition - SceneItem.allSceneItems[i].transform.position))) {
                        return false;
                    }
                    anyFound = true;
                }
            }
            return anyFound;
        }

        bool Check_A_Position (Actor selfActor, Actor suppliedActor, Vector3 aPosition) {
            if (bTransformCheckType == TransformCheckType.SelfTransform) 
                return CheckFloatSqr(Vector3.SqrMagnitude(aPosition - selfActor.GetPosition ()));
            else if (bTransformCheckType == TransformCheckType.SuppliedTransform) 
                return CheckFloatSqr(Vector3.SqrMagnitude(aPosition - suppliedActor.GetPosition ()));
            else if (bTransformCheckType == TransformCheckType.SceneTransform) 
                return CheckFloatSqr(Vector3.SqrMagnitude(aPosition - transformB.position));
            else if (bTransformCheckType == TransformCheckType.AnySceneItemTransform) 
                return CheckDistanceAgainstAnySceneItems(itemCheckB, aPosition);
            else if (bTransformCheckType == TransformCheckType.AllSceneItemTransform) 
                return CheckDistanceAgainstAllSceneItems(itemCheckB, aPosition);
            return false;
        }
        

        bool CheckDistanceCondition (Actor selfActor, Actor suppliedActor) {
            
            if (aTransformCheckType == TransformCheckType.SelfTransform) {
                Check_A_Position(selfActor, suppliedActor, selfActor.GetPosition());   
            }
            else if (aTransformCheckType == TransformCheckType.SuppliedTransform) {
                Check_A_Position(selfActor, suppliedActor, suppliedActor.GetPosition());
            }
            else if (aTransformCheckType == TransformCheckType.SceneTransform) {
                Check_A_Position(selfActor, suppliedActor, transformA.position);
            }
            else if (aTransformCheckType == TransformCheckType.AnySceneItemTransform) {
                for (int i = 0; i < SceneItem.allSceneItems.Count; i++) {
                    if (SceneItem.allSceneItems[i].itemBehavior == itemCheckA) {
                        if (Check_A_Position(selfActor, suppliedActor, SceneItem.allSceneItems[i].transform.position)) {                
                            return true;
                        }
                    }
                }
                return false;            
            }
            else if (aTransformCheckType == TransformCheckType.AllSceneItemTransform) {
                bool anyFound = false;
                for (int i = 0; i < SceneItem.allSceneItems.Count; i++) {
                    if (SceneItem.allSceneItems[i].itemBehavior == itemCheckA) {
                        if (!Check_A_Position(selfActor, suppliedActor, SceneItem.allSceneItems[i].transform.position)) {                
                            return false;
                        }
                        anyFound = true;
                    }
                }
                return anyFound;
            }
            return false;
        }





        public bool useSuppliedValues;
        public ConditionType conditionType;
        public InventoryCheckType inventoryCheckType;
        public ItemBehavior itemCheckA, itemCheckB;
        
        public Perk perkCheck;
        public string valueName;
        public bool trueIfNoValue = true;
        public GameValue.GameValueComponent component;
        public ConditionCheck condition;

        public QuestConditionType questCheckType;
        public QuestHandler.QuestCompletionLevel questCompletionLevel;
        public bool questCompletionLevelNotEquals;


        public int intCheckLevel;        
        public float valueCheck;

        public ConditionLink link;

        bool CheckInteger (int checkValue) {
            if (condition == ConditionCheck.Equals) return checkValue == intCheckLevel;
            else if (condition == ConditionCheck.NotEquals) return checkValue != intCheckLevel;
            else if (condition == ConditionCheck.LessThan) return checkValue < intCheckLevel;
            else if (condition == ConditionCheck.GreaterThan) return checkValue > intCheckLevel;
            else if (condition == ConditionCheck.LessThanEqualTo) return checkValue <= intCheckLevel;
            else if (condition == ConditionCheck.GreaterThanEqualTo) return checkValue >= intCheckLevel;
            return false;
        }
        bool CheckFloat (float checkValue, float against) {
            if (condition == ConditionCheck.Equals) return checkValue == against;
            else if (condition == ConditionCheck.NotEquals) return checkValue != against;
            else if (condition == ConditionCheck.LessThan) return checkValue < against;
            else if (condition == ConditionCheck.GreaterThan) return checkValue > against;
            else if (condition == ConditionCheck.LessThanEqualTo) return checkValue <= against;
            else if (condition == ConditionCheck.GreaterThanEqualTo) return checkValue >= against;
            return false;
        }
        bool CheckFloat (float checkValue) {
            return CheckFloat(checkValue, valueCheck);
        }
        bool CheckFloatSqr (float checkValue) {
            return CheckFloat(checkValue, valueCheck * valueCheck);
        }

        public bool IsMet (Actor selfActor, Actor suppliedActor) {
            
            Actor actorToUse = useSuppliedValues ? suppliedActor : selfActor;
            
            if (conditionType == ConditionType.GameValue) {

                GameValue gameValue;
                if (!actorToUse.actorValues.TryGetValue(valueName, out gameValue)) {
                    Debug.LogError("Cant find game value: " + valueName);
                    return trueIfNoValue;
                }

                float value = gameValue.GetValue(component);

                return CheckFloat(value);
            }
            else if (conditionType == ConditionType.Inventory) {

                if (inventoryCheckType == InventoryCheckType.IsEquipped) {
                    if (actorToUse.inventory == null) return false;
                    if (actorToUse.inventory.equipper == null) return false;

                    return actorToUse.inventory.equipper.ItemIsEquipped(intCheckLevel, itemCheckA);
                }
                else if (inventoryCheckType == InventoryCheckType.NotEquipped) {
                    if (actorToUse.inventory == null) return true;
                    if (actorToUse.inventory.equipper == null) return true;
                    
                    return !actorToUse.inventory.equipper.ItemIsEquipped(intCheckLevel, itemCheckA);
                }
                else if (inventoryCheckType == InventoryCheckType.StashCount) {
                    if (actorToUse.inventory == null) return CheckInteger( 0 ); // if no perk handler, "item count" == 0
                    
                    return CheckInteger(actorToUse.inventory.GetItemCount(itemCheckA));
                }
            }
            else if (conditionType == ConditionType.Perk) {
                if (perkCheck == null) return true;
                if (actorToUse.perkHandler == null) { // if no perk handler, "perk level" == 0
                    return CheckInteger(0);
                    // return intCheckLevel <= 0;
                }
                return CheckInteger(actorToUse.perkHandler.GetPerkLevel(perkCheck));
            }
            else if (conditionType == ConditionType.Quest) {
                if (questCheckType == QuestConditionType.CompletionLevel) {
                    if (questCompletionLevelNotEquals) {
                        return questCompletionLevel != QuestHandler.GetQuestCompletionLevel(valueName);
                    }
                    else {
                        return questCompletionLevel == QuestHandler.GetQuestCompletionLevel(valueName);
                    }
                }
                // internal key
                else {
                    return CheckInteger(QuestHandler.GetInternalKey (valueName));
                }
            }
            else if (conditionType == ConditionType.Distance) {
                return CheckDistanceCondition ( selfActor, suppliedActor);
            }
            else if (conditionType == ConditionType.Angle) {
                return CheckAngleCondition ( selfActor, suppliedActor);
            }

            return false;       
        }
    }

    [System.Serializable] public class ActorValueConditionArray : NeatArrayWrapper<ActorValueCondition> { }
        
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ActorValueCondition))] public class ActorValueConditionDrawer : PropertyDrawer
    {
        static readonly string[] selfSuppliedOptions = new string[] { "Self", "Supplied" };
        static readonly string[] conditionTypeOptions = new string[] { "Game Value", "Inventory", "Perk Level", "Quest", "Distance", "Angle" };
        static readonly string[] conditionsOptions = new string[] { " == ", " != ", " < ", " > ", " <= ", " >= " };
        static readonly string[] inventoryCheckOptions = new string[] { "Stash Count", "Equipped To Slot", "Not Equipped To Slot" };
        

        static readonly string[] transformTypeOptions = new string[] { 
            "Self Actor" , "Supplied Actor" , "Any Scene Item" , "All Scene Items" , "Transform" 
        };
        
        

        void DrawNameValue (Rect position, SerializedProperty property, ref float x) {
            SerializedProperty p = property.FindPropertyRelative("valueName");
            string s = p.stringValue;
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)) p.stringValue = "Name";
            DrawProperty(position, p, 125, ref x);
        }


        void DrawProperty (Rect position, SerializedProperty property, float width, ref float x) {
            EditorGUI.PropertyField(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property, GUIContent.none);
            x+= width;
        }
        void DrawProperty (Rect position, SerializedProperty property, string propName, float width, ref float x) {
            DrawProperty(position, property.FindPropertyRelative(propName), width, ref x);
        }

        void DrawEnumWithCustomOptions (Rect position, SerializedProperty property, string propName, string[] customOptions, float width, ref float x) {
            SerializedProperty enumProp = property.FindPropertyRelative(propName);
            enumProp.enumValueIndex = EditorGUI.Popup (new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), enumProp.enumValueIndex, customOptions);
            x+= width;                
        }
        
        void DrawConditionCheck (Rect position, SerializedProperty property, ref float x) {
            DrawEnumWithCustomOptions(position, property, "condition", conditionsOptions, 40, ref x);
        }

        void DrawLabel (Rect position, ref float x, string txt, float width) {
            EditorGUI.LabelField(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), txt);
            x+= width;
        }

        void DrawItemCheck (Rect position, SerializedProperty property, int index, ref float x) {
            float width = 175;
            InventorySystemEditor.itemSelector.Draw(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("itemCheck" + (index == 0 ? "A" : "B")), GUIContent.none);
            x+= width;
        }

        
        void DrawFloatValueForCheck (Rect position, SerializedProperty property, ref float x) {
            DrawProperty(position, property, "valueCheck", 60, ref x);
        }
        void DrawIntValueForCheck (Rect position, SerializedProperty property, ref float x) {
            DrawProperty(position, property, "intCheckLevel", 60, ref x);
        }


        void DrawTransformSelection (Rect position, SerializedProperty property, int index, ref float x) {
            

            string propName = index == 0 ? "aTransformCheckType" : "bTransformCheckType";

            DrawEnumWithCustomOptions ( position, property, propName, transformTypeOptions, 90, ref x);

            SerializedProperty transformType = property.FindPropertyRelative(propName);

            // DrawProperty(position, transformType, 120, ref x);
            
            if (transformType.enumValueIndex == 4) {
                DrawProperty(position, property, index == 0 ? "transformA" : "transformB", 150, ref x);
            }
            else if (transformType.enumValueIndex == 2 || transformType.enumValueIndex == 3) {
                DrawItemCheck ( position, property, 0, ref x);
            }
        }

            
        
            


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float x = position.x;

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            x = EditorTools.DrawIndent (oldIndent, x);

            float width = 0;

            SerializedProperty conditionType = property.FindPropertyRelative("conditionType");


            if (conditionType.enumValueIndex != 3 && conditionType.enumValueIndex != 4 && conditionType.enumValueIndex != 5) {
                width = 60;
                SerializedProperty selfOrSuppliedProp = property.FindPropertyRelative("useSuppliedValues");
                selfOrSuppliedProp.boolValue = EditorGUI.Popup (new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), "", selfOrSuppliedProp.boolValue ? 1 : 0, selfSuppliedOptions) == 1;
                x += width;
            }

            width = 80;
            conditionType.enumValueIndex = EditorGUI.Popup (new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), conditionType.enumValueIndex, conditionTypeOptions);
            x+= width;

            

            // game value check
            if (conditionType.enumValueIndex == 0) {
                
                DrawNameValue ( position, property, ref x);
                
                DrawProperty(position, property, "component", 90, ref x);

                DrawConditionCheck ( position, property, ref x);
                DrawFloatValueForCheck ( position, property, ref x);
    
                DrawLabel ( position, ref x,  "True If Null:", 70);
        
                DrawProperty(position, property, "trueIfNoValue", 20, ref x);

            }
            // inventory check
            else if (conditionType.enumValueIndex == 1) {
                
                DrawItemCheck ( position, property, 0, ref x);

                DrawEnumWithCustomOptions(position, property, "inventoryCheckType", inventoryCheckOptions, 110, ref x);


                //stash count check
                if (property.FindPropertyRelative("inventoryCheckType").enumValueIndex == 0) {                    
                    DrawConditionCheck ( position, property, ref x);
                }

                DrawIntValueForCheck ( position, property, ref x);
            }
            //perk check
            else if (conditionType.enumValueIndex == 2) {

                width = 175;
                PerkSystemEditor.itemSelector.Draw(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("perkCheck"), GUIContent.none);
                x+= width;

                DrawConditionCheck ( position, property, ref x);
                DrawIntValueForCheck ( position, property, ref x);

            }
            // quest check
            else if (conditionType.enumValueIndex == 3) {

                DrawNameValue ( position, property, ref x);

                DrawProperty(position, property, "questCheckType", 110, ref x);

                //complettion level
                if (property.FindPropertyRelative("questCheckType").enumValueIndex == 0) {

                    width = 40;
                    SerializedProperty notEqualTo = property.FindPropertyRelative("questCompletionLevelNotEquals");
                    notEqualTo.boolValue = EditorGUI.Popup (new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), "", notEqualTo.boolValue ? 1 : 0, new string[] { " == ", " != " }) == 1;
                    x += width;

                    DrawProperty(position, property, "questCompletionLevel", 80, ref x);

                }
                else{
                // internal key
                // else if (questCheckType.enumValueIndex == 1) {
                    DrawConditionCheck ( position, property, ref x);  
                    DrawIntValueForCheck ( position, property, ref x);
                }
            }
            // distance transform checks
            else if (conditionType.enumValueIndex == 4) {
                
                Rect newPos = new Rect(position.x, position.y+EditorGUIUtility.singleLineHeight, position.width, position.height);
                float newX = x;
                
                DrawTransformSelection ( newPos, property, 0, ref newX);
                DrawLabel ( newPos, ref newX, "And", 40);
                DrawTransformSelection ( newPos, property, 1, ref newX);

                DrawConditionCheck ( position, property, ref x);  
                DrawFloatValueForCheck ( position, property, ref x);            
            }
            // angle transform checks
            else if (conditionType.enumValueIndex == 5) {

                Rect newPos = new Rect(position.x, position.y+EditorGUIUtility.singleLineHeight, position.width, position.height);

                float newX = x;
                DrawTransformSelection ( newPos, property, 0, ref newX);
                DrawProperty ( newPos, property, "aAngleCheckType", 50, ref newX);

                DrawLabel ( newPos, ref newX, "And", 40);
                DrawTransformSelection ( newPos, property, 1, ref newX);
                DrawProperty ( newPos, property, "bAngleCheckType", 50, ref newX);

                DrawConditionCheck ( position, property, ref x);  
                DrawFloatValueForCheck ( position, property, ref x);            
            }



            width = 50;

            x = EditorTools.DrawIndent (oldIndent, position.x) + 280+125+80+60;
            EditorGUI.PropertyField(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("link"), GUIContent.none);
            
            EditorGUI.EndProperty();

            EditorGUI.indentLevel = oldIndent;   
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty conditionType = property.FindPropertyRelative("conditionType");


            if (conditionType.enumValueIndex == 4 || conditionType.enumValueIndex == 5){
            return EditorGUIUtility.singleLineHeight * 2;
           
            }
return EditorGUIUtility.singleLineHeight;
           
        }
    }
#endif

}
