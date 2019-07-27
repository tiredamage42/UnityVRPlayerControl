using System.Collections.Generic;
using UnityEngine;

namespace ActorSystem {
    [System.Serializable] public class GameValue
    {
        public enum GameValueComponent { BaseValue, BaseMinValue, BaseMaxValue, Value, MinValue, MaxValue };
        
        public string name;
        public float baseValue, baseMinValue=0, baseMaxValue=500;
        public Vector2 initializationRange;

        public GameValue(string name, float baseValue, float baseMinValue, float baseMaxValue) {
            this.name = name;
            this.baseValue = baseValue;
            this.baseMinValue = baseMinValue;
            this.baseMaxValue = baseMaxValue;
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
            return GetValue(GameValueComponent.MinValue, baseMinValue);
        }
        public float GetMaxValue () {
            return GetValue(GameValueComponent.MaxValue, baseMaxValue);
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
                    return baseMinValue;
                case GameValueComponent.BaseMaxValue:
                    return baseMaxValue;
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

        public void AddModifier (GameValueModifier modifier, int count, int senderKey, int buffKey, int modifierKey) {
            //permanent modifiers
            if (modifier.modifyValueComponent == GameValueComponent.BaseValue) {
                baseValue = modifier.Modify(baseValue);

                //clamp the value
                baseValue = Mathf.Clamp(baseValue, GetMinValue(), GetMaxValue());
                return;
            }
            if (modifier.modifyValueComponent == GameValueComponent.BaseMinValue) {
                baseMinValue = modifier.Modify(baseMinValue);
                return;
            }
            if (modifier.modifyValueComponent == GameValueComponent.BaseMaxValue) {
                baseMaxValue = modifier.Modify(baseMaxValue);
                return;
            }
            Vector3Int key = new Vector3Int(senderKey, buffKey, modifierKey);
            GameValueModifier existingModifier = GetModifier ( key );//senderKey, buffKey, modifierKey );
            if (existingModifier != null) {
                existingModifier.count += count;
            }
            else {
                modifiers.Add(new GameValueModifier(modifier, count, key));//senderKey, modifierKey));
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
                    existingModifier.count -= count;
                    if (existingModifier.count <= 0) {
                        modifiers.Remove(existingModifier);
                    }
                }
            // }
        }
    }
    [System.Serializable] public class GameValueModifier {

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
        public string gameValueName;

        [Header("Base Value modifiers are permanent")]
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

}