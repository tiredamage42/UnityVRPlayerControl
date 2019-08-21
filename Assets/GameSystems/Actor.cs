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

using SimpleUI;
using Game.InventorySystem;
using Game.DialogueSystem;
using Game.PerkSystem;
using InteractionSystem;
using System;

namespace Game {
    public class Actor : MonoBehaviour {

        // TODO: override with player camera or ai head


        public Vector3 GetPosition () {
            return transform.position;
        }
        public Vector3 GetForward () {
            return transform.forward;
        }
        public Vector3 GetUp () {
            return transform.up;
        }
        public Vector3 GetRight () {
            return transform.right;
        }
        




        [HideInInspector] public Interactor interactor;
        [HideInInspector] public Inventory inventory;
        DialoguePlayer dialoguePlayer;
        [HideInInspector] public PerkHandler perkHandler;


        public event System.Action<int, int> onActionStart, onActionEnd, onActionUpdate;


        // player only
        public void BroadcastActionStart (int controllerIndex, int action) {
            if (onActionStart != null) {
                onActionStart(controllerIndex, action);
            }
        }
        public void BroadcastActionEnd (int controllerIndex, int action) {
            if (onActionEnd != null) {
                onActionEnd(controllerIndex, action);
            }
        }
        public void BroadcastActionUpdate (int controllerIndex, int action) {
            if (onActionUpdate != null) {
                onActionUpdate(controllerIndex, action);
            }
        }


        void InitializeComponents () {
            //player only
            dialoguePlayer = GetComponent<DialoguePlayer>();
            interactor = GetComponent<Interactor>();
            
            
            inventory = GetComponent<Inventory>();
            perkHandler = GetComponent<PerkHandler>();
        }

        public void StartDialogue (DialogueTemplate template, Actor otherActor, AudioSource actorSource, System.Action onDialogueEnded) {
            dialoguePlayer.StartDialogue(otherActor, template, actorSource, onDialogueEnded);
        }

        public static Actor playerActor;
        public string actorName;
        public bool isPlayer;
        public GameValueTemplate actorValuesTemplate;

        // so we can serialize later...
        [HideInInspector] public List<GameValue> savedActorValues;
        public Dictionary<string, GameValue> actorValues = new Dictionary<string, GameValue>();

        void Awake () {
            InitializeComponents();
            if (isPlayer) playerActor = this;
            if (actorValuesTemplate != null) {
                AddGameValues(actorValuesTemplate.gameValueTemplates);
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
            GameValue value = new GameValue (template);
            actorValues[template.name] = value;
            savedActorValues.Add(value);
            return value;
        }

        public GameValue GetGameValue (string name) {
            if (actorValues.ContainsKey(name)) return actorValues[name];
            Debug.LogWarning("Couldnt find game value: " + name + " on actor " + this.name);
            return null;
        }

        public void AddBuffs (GameValueModifier[] buffs, int count, int senderKey, int buffKey, bool assertPermanent, Actor selfActorForConditionChecks, Actor suppliedActorForConditionChecks) {
            for (int i =0 ; i < buffs.Length; i++) {
                if (assertPermanent && !buffs[i].isPermanent) continue;
                if (ActorValueCondition.ConditionsMet (buffs[i].conditions, selfActorForConditionChecks, suppliedActorForConditionChecks)) {
                    GameValue gameValue = GetGameValue(buffs[i].gameValueName);
                    if (gameValue != null) {
                        gameValue.AddModifier(buffs[i], count, new Vector3Int(senderKey, buffKey, i));
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