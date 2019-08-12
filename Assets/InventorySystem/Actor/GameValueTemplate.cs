using UnityEngine;

namespace ActorSystem {

    [CreateAssetMenu()]
    public class GameValueTemplate : ScriptableObject {

        [NeatArray] public GameValueArray gameValueTemplates;
        // public GameValue[] gameValueTemplates;

        // public GameValue[] GetTemplateInstance () {
        //     int l = gameValueTemplates.Length;
        //     GameValue[] newValues = new GameValue[l];

        //     for (int i = 0; i < l; i++) {

        //         GameValue value = gameValueTemplates[i];
                
        //         newValues[i] = new GameValue (
        //             value.name, 
        //             Random.Range( value.initializationRange.x, value.initializationRange.y ), value.baseMinMax
        //             // value.baseMinValue,
        //             // value.baseMaxValue
        //         );
        //     }
        //     return newValues;
        // }
    }
}
