using UnityEngine;

using Game.DialogueSystem;

namespace Game.QuestSystem {

    public class Objective_SpeakTo : ObjectiveScript {

        public DialogueTemplateHolder dialogueInteractable;
        public DialogueTemplate templateToGive;
        public bool waitUntilEndDialogue = true;
        bool dialogueConditionMet, dialogueStarted;
        
        [TextArea] public string hint = "Speak to so and so";

        void CompleteObjective () {
            dialogueConditionMet = true;
            Uninitialize();
        }

        void OnDialogueStarted () {
            if (isActiveObjective) {

                dialogueStarted = true;
                dialogueInteractable.onDialogueStarted -= OnDialogueStarted;
                if (!waitUntilEndDialogue) CompleteObjective();
            }
        }

        void OnDialogueEnded () {
            if (dialogueStarted) CompleteObjective();
        }

        void Uninitialize () {
            dialogueInteractable.SetOverrideTemplate (null);
            dialogueInteractable.onDialogueEnded -= OnDialogueStarted;
            dialogueInteractable.onDialogueStarted -= OnDialogueStarted;
        }
        void InitializeDialogueObjective () {
            dialogueStarted = false;
            dialogueInteractable.SetOverrideTemplate (templateToGive);
            dialogueInteractable.onDialogueStarted += OnDialogueStarted;
            if (waitUntilEndDialogue) {
                dialogueInteractable.onDialogueEnded += OnDialogueEnded;
            }
        }

        public override bool UpdateObjective (float deltaTime) {
            return dialogueConditionMet;
        }

        public override string GetCurrentTextHint() {
            return hint;
        }

        public override void OnDisableActiveState() {
            Uninitialize();
        }

        public override void OnEnableActiveState() {
            InitializeDialogueObjective();
        }
        
        public override void OnObjectiveIntroduced () {

        }

        public override void OnQuestInitialized () {

        }

        public override void OnQuestCompleted () {
            Uninitialize();
        }
    }
}