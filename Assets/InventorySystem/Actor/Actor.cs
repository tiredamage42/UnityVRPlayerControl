using System.Collections.Generic;
using UnityEngine;



/*

    TODO: add multiplier for incoming changes on game values (so we can for instance get 2x XP)
    
    xp levelling:
        via scripting :

            location discover:
            on kill:

            
                
                give script constructed game value modifier (+= XP amount)

        when crafting




    perks should be a game value (make seperate template than game values template)

    that way editor tools can use game value conditionals to check if perk is above certain level

    to add perk:

        add item with give buff on stash that adds 1 to the perk name

    

*/

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
            CheckForTemplate();

            gameValuesDictionary = GetValueDictionary();
        }

        public Dictionary<string, GameValue> gameValuesDictionary;

        public Dictionary<string, GameValue> GetValueDictionary() {
            Dictionary<string, GameValue> toReturn = new Dictionary<string, GameValue>();
            for (int i = 0; i < gameValues.Length; i++) {
                // Debug.LogError("adding " + gameValues[i].name);
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

        public GameValue GetGameValue (string name) {
            if (gameValuesDictionary.ContainsKey(name)) {
                return gameValuesDictionary[name];
            }
            // for (int i = 0; i< gameValues.Length; i++) {
            //     if (gameValues[i].name == name) {
            //         return gameValues[i];
            //     }
            // }
            Debug.LogWarning("Couldnt find game value: " + name + " on actor " + this.name);
            return null;
        }

        public void AddBuffs (GameValueModifier[] buffs, int count, int senderKey, int buffKey, bool assertPermanent) {
        
        // public void AddBuffs (ActorBuff buff, int count, int senderKey, int buffKey, bool assertPermanent) {
            //check Conditional 
            // if (GameValueCondition.ConditionsMet (buff.conditions, gameValuesDictionary)) 
            {
                // GameValueModifier[] buffs = buff.buffs;
                for (int i =0 ; i < buffs.Length; i++) {



                    GameValueModifier mod = buffs[i];

                    if (assertPermanent && (
                        mod.modifyValueComponent == GameValue.GameValueComponent.Value ||
                        mod.modifyValueComponent == GameValue.GameValueComponent.MinValue ||
                        mod.modifyValueComponent == GameValue.GameValueComponent.MaxValue
                    )) {
                        // Debug.LogError("non permanent stacked buff found. Consume Buff Index: " + i.ToString() + " on " + name);
                        // Debug.LogError("non permanent stacked buff found in OnItemConsumed. Consume Buff Index: " + consumeBuffs[i].name + " on " + name);
                        
                        continue;
                    }

                    if (GameValueCondition.ConditionsMet (mod.conditions, gameValuesDictionary)) {


                        GameValue gameValue = GetGameValue(mod.gameValueName);
                        if (gameValue != null) {
                            gameValue.AddModifier(mod, count, senderKey, buffKey, i);
                        }
                    }
                }
            }
        }
        // public void RemoveBuffs (ActorBuff buff, int count, int senderKey, int buffKey) {
        public void RemoveBuffs (GameValueModifier[] buffs, int count, int senderKey, int buffKey) {
        
            // GameValueModifier[] buffs = buff.buffs;
                
            for (int i =0 ; i < buffs.Length; i++) {
                GameValue gameValue = GetGameValue(buffs[i].gameValueName);
                if (gameValue != null) {
                    gameValue.RemoveModifier(buffs[i], count, senderKey, buffKey, i);
                }
            }
        }
    }
}