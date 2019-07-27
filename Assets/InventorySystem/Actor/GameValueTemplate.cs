using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ActorSystem {

    [CreateAssetMenu()]
    public class GameValueTemplate : ScriptableObject {
        // [Header("Pairs of two per value,")]
        // [Header("set up as min/max range")]
        // [Space]
        // [Header("[0] 'Health' min ranges")]
        // [Header("[1] 'Health max ranges")]
        
        public GameValue[] gameValueTemplates;

        public GameValue[] GetTemplateInstance () {
            int l = gameValueTemplates.Length;
            GameValue[] newValues = new GameValue[l];

            for (int i = 0; i < l; i++) {

                GameValue value = gameValueTemplates[i];
                
                newValues[i] = new GameValue (
                    value.name, 
                    Random.Range( value.initializationRange.x, value.initializationRange.y ),
                    value.baseMinValue,
                    value.baseMaxValue
                );
            }
            return newValues;

            
            
            
            
            
            
            // int l = gameValueTemplates.Length;

            // if (l % 2 != 0) {
            //     return null;
            // }

            // int c = l/2;
            // GameValue[] newValues = new GameValue[c];

            // int x = 0;
            // for (int i =0; i < l; i+=2) {

            //     GameValue minValue = gameValueTemplates[i];
            //     GameValue maxValue = gameValueTemplates[i+1];

            //     newValues[x] = new GameValue (
            //         minValue.name, 
            //         Random.Range( minValue.baseValue, maxValue.baseValue ),
            //         Random.Range( minValue.baseMinValue, maxValue.baseMinValue ),
            //         Random.Range( minValue.baseMaxValue, maxValue.baseMaxValue )
            //     );
                
            //     x++;
            // }
            // return newValues;
        }
    }
}
