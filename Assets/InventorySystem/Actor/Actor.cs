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
        public List<GameValue> actorValues;
        Dictionary<string, GameValue> actorValues_Dict = new Dictionary<string, GameValue>();

        void Awake () {
            if (isPlayer) {
                playerActor = this;
            }
            CheckForTemplate();

            // gameValuesDictionary = GetValueDictionary();
        }


        public Dictionary<string, GameValue> GetValueDictionary() {
            return actorValues_Dict;
            // Dictionary<string, GameValue> toReturn = new Dictionary<string, GameValue>();
            // for (int i = 0; i < gameValues.Length; i++) {
            //     // Debug.LogError("adding " + gameValues[i].name);
            //     toReturn.Add(gameValues[i].name, gameValues[i]);
            // }
            // return toReturn;
        }

        // public void ResetActor () {
        //     gameValues = null;
        // }
        
        void CheckForTemplate () {

            
            // if (gameValues == null || gameValues.Length == 0) {
                if (template != null) {

                    AddGameValues(template.gameValueTemplates);
                    // gameValues = template.GetTemplateInstance();
                }
            // }
        }

        // void Update () {
        //     CheckForTemplate();
        // }


        public void AddGameValues(GameValue[] template) {
            for (int i = 0; i < template.Length; i++ ){
                AddGameValue(template[i]);
            }
        }

        public GameValue AddGameValue (GameValue template)
        {
            if (actorValues_Dict.ContainsKey(template.name)) {
                Debug.LogError("Already added " + template.name + " game value to actor " + this.name + "!");
                
                //maybe return null to avoid confusion on additive "mod" quests...
                return actorValues_Dict[template.name]; 
            }
            GameValue value = new GameValue (template.name, Random.Range( template.initializationRange.x, template.initializationRange.y ), template.baseMinMax);
            actorValues_Dict[template.name] = value;
            actorValues.Add(value);
            return value;
        }

        public GameValue GetGameValue (string name) {
            if (actorValues_Dict.ContainsKey(name)) {
                return actorValues_Dict[name];
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

                    if (GameValueCondition.ConditionsMet (mod.conditions, actorValues_Dict, actorValues_Dict)) {


                        GameValue gameValue = GetGameValue(mod.gameValueName);
                        if (gameValue != null) {
                            gameValue.AddModifier(mod, count, new Vector3Int(senderKey, buffKey, i));
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
                    gameValue.RemoveModifier(buffs[i], count, new Vector3Int(senderKey, buffKey, i));
                }
            }
        }
    }
}