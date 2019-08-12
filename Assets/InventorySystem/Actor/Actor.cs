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

using InventorySystem;


namespace ActorSystem {
    public class Actor : MonoBehaviour {

        public Inventory inventory;
        void InitializeComponents () {
            inventory = GetComponent<Inventory>();
        }


        public string actorName;
        public static Actor playerActor;
        public bool isPlayer;
        public GameValueTemplate template;
        public List<GameValue> savedActorValues;
        public Dictionary<string, GameValue> actorValues = new Dictionary<string, GameValue>();

        void Awake () {
            InitializeComponents();
            if (isPlayer) playerActor = this;
            CheckForTemplate();
        }

        void CheckForTemplate () {
            if (template != null) {
                AddGameValues(template.gameValueTemplates);
            }
        }

        public void AddGameValues(GameValue[] template) {
            for (int i = 0; i < template.Length; i++ ){
                AddGameValue(template[i]);
            }
        }

        public GameValue AddGameValue (GameValue template)
        {
            if (actorValues.ContainsKey(template.name)) {
                Debug.LogError("Already added " + template.name + " game value to actor " + this.name + "!");
    
                //maybe return null to avoid confusion on additive "mod" quests...
                return actorValues[template.name]; 
            }
            GameValue value = new GameValue (template.name, Random.Range( template.initializationRange.x, template.initializationRange.y ), template.baseMinMax);
            Debug.LogWarning("adding " + template.name);
            actorValues[template.name] = value;
            savedActorValues.Add(value);
            return value;
        }

        public GameValue GetGameValue (string name) {
            if (actorValues.ContainsKey(name)) return actorValues[name];
            Debug.LogWarning("Couldnt find game value: " + name + " on actor " + this.name);
            return null;
        }

        public void AddBuffs (GameValueModifier[] buffs, int count, int senderKey, int buffKey, bool assertPermanent, Dictionary<string, GameValue> selfValues, Dictionary<string, GameValue> suppliedValues) {
        
            for (int i =0 ; i < buffs.Length; i++) {
                GameValueModifier mod = buffs[i];

                if (assertPermanent && (
                    mod.modifyValueComponent == GameValue.GameValueComponent.Value ||
                    mod.modifyValueComponent == GameValue.GameValueComponent.MinValue ||
                    mod.modifyValueComponent == GameValue.GameValueComponent.MaxValue
                )) {                    
                    continue;
                }

                if (GameValueCondition.ConditionsMet (mod.conditions, selfValues, suppliedValues)) {
                    GameValue gameValue = GetGameValue(mod.gameValueName);
                    if (gameValue != null) {
                        gameValue.AddModifier(mod, count, new Vector3Int(senderKey, buffKey, i));
                    }
                }
            }
        }
        public void RemoveBuffs (GameValueModifier[] buffs, int count, int senderKey, int buffKey) {
            for (int i =0 ; i < buffs.Length; i++) {
                GameValue gameValue = GetGameValue(buffs[i].gameValueName);
                if (gameValue != null) {
                    gameValue.RemoveModifier(buffs[i], count, new Vector3Int(senderKey, buffKey, i));
                }
            }
        }
    }
}