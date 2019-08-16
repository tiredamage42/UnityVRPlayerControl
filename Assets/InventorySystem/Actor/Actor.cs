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
using DialogueSystem;
// using GameUI;
using SimpleUI;

namespace ActorSystem {
    public class Actor : MonoBehaviour {

        [HideInInspector] public Inventory inventory;
        DialoguePlayer dialoguePlayer;

        // GameMessageInbox messageInbox;
        // public UIMessageCenter messageCenterUI;

        
        void InitializeComponents () {
            inventory = GetComponent<Inventory>();
            dialoguePlayer = GetComponent<DialoguePlayer>();
            // messageInbox = GetComponent<GameMessageInbox>();
        }

        public event System.Action<string, bool, UIColorScheme> onShowMessage;
        public event System.Action<string, string> onShowSubtitles;
        
        public void ShowMessage (string msg, UIColorScheme scheme, bool immediate=false) {
            if (onShowMessage != null) {
                onShowMessage(msg, immediate, scheme);
            }

            // messageCenterUI.ShowMessage(msg, immediate, scheme);
            // messageInbox.ShowMessage( msg );
        }
        public void ShowSubtitles (string speaker, string subtitles) {
            // Debug.LogError("show sutitles player");
            if (onShowSubtitles != null) {
                // Debug.LogError(("show subtitls event"));
                onShowSubtitles(speaker, subtitles);
            }
        }


        public void StartDialogue (DialogueTemplate template, Actor otherActor, AudioSource actorSource) {
            dialoguePlayer.StartDialogue(otherActor, template, actorSource);
        }


        public static Actor playerActor;
        public string actorName;
        public bool isPlayer;
        public GameValueTemplate actorValuesTemplate;
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
                if (assertPermanent && !buffs[i].isPermanent) continue;
                if (GameValueCondition.ConditionsMet (buffs[i].conditions, selfValues, suppliedValues)) {
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