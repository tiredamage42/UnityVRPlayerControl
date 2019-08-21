using UnityEngine;

using Game.UI;

namespace Game.QuestSystem {

    /*
        objective 1:
            build a radio tower close to the radio (scene item distance objective)

        2:
            talk to debug person (subscribe to interactor interacted with)

        3:
            completion objective (gives xp buffs and items and perks) (values always player) (hint is quest cmplete message, needs original quest name)

    */



    public abstract class ObjectiveScript : MonoBehaviour {

        public abstract bool UpdateObjective (float deltaTime);
        public abstract string GetCurrentTextHint();
        public abstract void OnObjectiveIntroduced ();
        public abstract void OnQuestInitialized ();
        public abstract void OnQuestCompleted () ;
        public abstract void OnDisableActiveState() ;
        public abstract void OnEnableActiveState() ;
        

        Quest questObject;
        int objectiveIndex;

        public bool isActiveObjective { get { return questObject.internalKey == objectiveIndex; } }

        public void OnQuestInitialized(Quest questObject, int objectiveIndex) {
            this.questObject = questObject;
            this.objectiveIndex = objectiveIndex;
            OnQuestInitialized();
        }
    }
        

    public class StandardQuest : Quest
    {

        [Header("In Order")]
        public ObjectiveScript[] objectives;

        // ObjectiveScript currentObjective;

        int introducedLast = -1;

        void UpdateObjectives (float deltaTime) {
            // Debug.LogError("updating objective");
            int lastActive = -1;
            for (int i = 0; i < objectives.Length; i++) {
                if (i > introducedLast) {
                    introducedLast = i;
                    objectives[i].OnObjectiveIntroduced ();

                    string hint = objectives[i].GetCurrentTextHint();
                    if (hint != null) {
                        GameUI.ShowInGameMessage( hint, false, UIColorScheme.Normal );
                    }
                }

                bool isComplete = objectives[i].UpdateObjective(deltaTime);

                if (!isComplete) {
                    lastActive = i;
                    break;
                }
            }

            if (lastActive != -1) {
                // currentObjective = objectives[lastActive];
                if (internalKey != lastActive) {
                    objectives[internalKey].OnDisableActiveState();
                    internalKey = lastActive;
                    objectives[internalKey].OnEnableActiveState();
                }
            }
            else {
                CompleteQuest ();
            }
        }

        public override void OnQuestInitialize () { 
            for (int i = 0; i < objectives.Length; i++) {
                objectives[i].OnQuestInitialized(this, i);
            }
        }

        public override void OnUpdateQuest (float deltaTime) { 
            UpdateObjectives(deltaTime);
        }

        public override string GetCurrentTextHint () { 
            return objectives[internalKey].GetCurrentTextHint(); 
        }
        
        public override void OnQuestComplete () { 
            for (int i = 0; i < objectives.Length; i++) {
                objectives[i].OnQuestCompleted();
            }

        }
        
    }
}
