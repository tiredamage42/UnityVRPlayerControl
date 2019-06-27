using System.Collections.Generic;
using UnityEngine;

namespace ActorSystem {
    public class Actor : MonoBehaviour {
        public static Actor playerActor;
        public bool isPlayer;
        public GameValueTemplate template;
        public GameValue[] gameValues;

        void Awake () {
            if (isPlayer) {
                playerActor = this;
            }
        }
        public Dictionary<string, GameValue> GetValueDictionary() {
            Dictionary<string, GameValue> toReturn = new Dictionary<string, GameValue>();
            for (int i = 0; i < gameValues.Length; i++) {
                toReturn.Add(gameValues[i].name, gameValues[i]);
            }
            return toReturn;
        }

        public void ResetActor () {
            gameValues = null;
        }
        
        void CheckForTemplate () {
            if (gameValues == null || gameValues.Length == 0) {
                if (template != null) {
                    gameValues = template.GetTemplateInstance();
                }
            }
        }

        void Update () {
            CheckForTemplate();
        }

        public GameValue GetGameValueByName (string name) {
            for (int i = 0; i< gameValues.Length; i++) {
                if (gameValues[i].name == name) {
                    return gameValues[i];
                }
            }
            Debug.LogWarning("Couldnt find game value: " + name + " on actor " + this.name);
            return null;
        }

        public void AddBuffs (ActorBuff buff, int count, int senderKey) {
            for (int i =0 ; i < buff.buffs.Length; i++) {
                GameValue gameValue = GetGameValueByName(buff.buffs[i].gameValueName);
                if (gameValue != null) {
                    gameValue.AddModifier(buff.buffs[i], count, senderKey, i);
                }
            }
        }
        public void RemoveBuffs (ActorBuff buff, int count, int senderKey) {
            for (int i =0 ; i < buff.buffs.Length; i++) {
                GameValue gameValue = GetGameValueByName(buff.buffs[i].gameValueName);
                if (gameValue != null) {
                    gameValue.RemoveModifier(buff.buffs[i], count, senderKey, i);
                }
            }
        }
    }
}