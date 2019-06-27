using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

        public enum ConditionCheck { Equals, LessThan, GreaterThan, LessThanEqualTo, GreaterThanEqualTo };
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

}
