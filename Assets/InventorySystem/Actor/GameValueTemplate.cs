using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ActorSystem {

    [CreateAssetMenu()]
    public class GameValueTemplate : ScriptableObject {
        [Header("Pairs of two per value,\nset up as min/max range\n\n[0] 'Health' min ranges\n[1] 'Health max ranges")]
        public GameValue[] gameValueTemplates;

        public GameValue[] GetTemplateInstance () {
            int l = gameValueTemplates.Length;

            if (l % 2 != 0) {
                return null;
            }

            int c = l/2;
            GameValue[] newValues = new GameValue[c];

            int x = 0;
            for (int i =0; i < l; i+=2) {

                GameValue minValue = gameValueTemplates[i];
                GameValue maxValue = gameValueTemplates[i+1];

                newValues[x] = new GameValue (
                    minValue.name, 
                    Random.Range( minValue.baseValue, maxValue.baseValue ),
                    Random.Range( minValue.baseMinValue, maxValue.baseMinValue ),
                    Random.Range( minValue.baseMaxValue, maxValue.baseMaxValue )
                );
                
                x++;
            }
            return newValues;
        }
    }
}
