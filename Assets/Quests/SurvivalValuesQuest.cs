using UnityEngine;

using Game.QuestSystem;
using Game.UI;
using Game;
public class SurvivalValuesQuest : Quest {

    [System.Serializable] public class SurviveValue {

        
        [HideInInspector] public int survivalValueID, questInstanceID;
        [Header("Value Template")] public GameValue valueTemplate;
        [NeatArray] public GameValueModifierArray degredations;
        [Header("Realtime Seconds")] public float degredationTime = 1;
        public string hintMessage = "Drink some Water...";
        
        [Header("Count = value template max")] [NeatArray] public GameValueModifierArray2D thresholdPenalties;

        [NeatArray] public UIColorSchemeArray thresholdMessageColorSchemes;

        float timer;
        Actor playerActor;
        GameValue runtimeValue;
        int lastThresholdValue;

        public void InitializeValue (Actor playerActor, int survivalValueID, int questInstanceID) {
            this.survivalValueID = survivalValueID;
            this.questInstanceID = questInstanceID;

            this.playerActor = playerActor;    
            runtimeValue = playerActor.AddGameValue(valueTemplate);
            runtimeValue.AddChangeListener(OnValueChange);
            GameValueModifier[] mods = degredations;
            for (int i = 0; i < mods.Length; i++) {
                mods[i].gameValueName = valueTemplate.name;
            }
        }

        void OnValueChange(float delta, float current, float min, float max) {
            //value relief
            if (delta < 0) {
                GameUI.ShowInGameMessage (runtimeValue.name + " " + delta, false, UIColorScheme.Normal);
            }
            
            int v = (int)current;
            if (lastThresholdValue != v) {

                GameUI.ShowInGameMessage (runtimeValue.name + " Level :: " + v + (v == 0 ? "" : hintMessage), false, thresholdMessageColorSchemes.list[v]);
                playerActor.RemoveBuffs(thresholdPenalties.list[lastThresholdValue], 1, questInstanceID, survivalValueID);
                playerActor.AddBuffs(thresholdPenalties.list[v], 1, questInstanceID, survivalValueID, false, playerActor, playerActor);
                
                lastThresholdValue = v;
            }                
        }

        public void Update (float deltaTime) {

            timer += deltaTime;
            if (timer >= degredationTime) {
                playerActor.AddBuffs(degredations, 1, 0, 0, true, playerActor, playerActor);
                timer = 0;
            }
        }
    }

    public SurviveValue[] survivalValues;

    public override void OnQuestInitialize () {
        for (int i = 0; i < survivalValues.Length; i++) {
            survivalValues[i].InitializeValue(Actor.playerActor, i, GetInstanceID());
        }
    }
    public override void OnUpdateQuest (float deltaTime){
        for (int i = 0; i < survivalValues.Length; i++) {
            survivalValues[i].Update(deltaTime);
        }
    }

    public override string GetCurrentTextHint () { return null; }
    public override void OnQuestComplete () { }
}