using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Dialogue templates will be held on gameobjects/prefabs

    in order to give them the capability to reference objects in the scene

    in case dialogue triggers :
        enabling disabling
        quest only in scene

    current system assumes that only player will have template player component...
        who else could be choosing responses?

    so ::
        self game value checks refer to player actor values
        supplied game value checks refer to the actor the player is talking to
    

*/

using ActorSystem;
using InventorySystem;
using QuestSystem;


#if UNITY_EDITOR 
using UnityEditor;
#endif







// using System.Collections;

using SimpleUI;
using GameUI;
// using DialogueSystem;

namespace DialogueSystem {

    public class DialoguePlayer : MonoBehaviour {
        
        // public DialogueTemplate template;
        public AudioSource audioSource;
        Actor myActor;
        Inventory myInventory;
        public bool inDialogue;

        int currentStepID;


        DialogueTemplateHolder speakingTo;
        void Awake () {
            // if (myActor == null) 
            myActor = GetComponent<Actor>();
            // if (myInventory == null) 
            myInventory = GetComponent<Inventory>();
        }

        public void Stop () {
            currentStepID = 0;
            waitingForResponse = false;
            if (inDialogue) {
                if (audioSource != null) audioSource.Stop();
                if (speakingTo != null) {
                    if (speakingTo.audioSource != null) speakingTo.audioSource.Stop();
                    speakingTo = null;
                }
            }
            inDialogue = false;
        }


        
        Dictionary<int, DialogueStep> stepIDtoStep = new Dictionary<int, DialogueStep>();


        void BuildStepIDDictionary() {
            stepIDtoStep.Clear();
            for (int i = 0; i < speakingTo.template.allSteps.Length; i++) {
                stepIDtoStep.Add(speakingTo.template.allSteps[i].stepID, speakingTo.template.allSteps[i]);
            }
        }
        public void StartDialogueWith (DialogueTemplateHolder speakingTo) {
            this.speakingTo = speakingTo;
            BuildStepIDDictionary();
            inDialogue = true;
            currentStepID = 0;
            PlayStep(stepIDtoStep[currentStepID]);
        }
        void PlayStep (DialogueStep step) {
            
            // Speaking to begins first...
            
            // show subtitles
            UISubtitles.instance.ShowSubtitles (speakingTo.actor.actorName, step.bark);
            
            // play audio
            if (speakingTo.audioSource != null) {
                if (step.associatedAudio != null) {
                    speakingTo.audioSource.clip = step.associatedAudio;
                    audioSource.Play();
                }
            }


            Dictionary<string, GameValue> selfValues = myActor.GetValueDictionary();
            Dictionary<string, GameValue> suppliedValues = speakingTo.actor.GetValueDictionary();
            

            // add any buffs associated with this step to the player
            myActor.AddBuffs(step.playerBuffs, 1, 0, 0, true, selfValues, suppliedValues);

            // add any buffs associated with this step to the speaker
            speakingTo.actor.AddBuffs(step.speakerBuffs, 1, 0, 0, true, selfValues, suppliedValues);


            // give items to player
            if (step.transferToPlayer.list.Length > 0) {
                myInventory.GetComponent<InventoryCrafter>().AddItemComposition(step.transferToPlayer, true, selfValues, suppliedValues);
            }

            // player gives items to speaker
            if (step.transferToSpeaker.list.Length > 0) {
                Item_Composition[] removedItemsFromPlayer = myInventory.GetComponent<InventoryCrafter>().RemoveItemComposition(step.transferToSpeaker, checkScrap: false, selfValues, suppliedValues);
                
                speakingTo.inventory.GetComponent<InventoryCrafter>().AddItemComposition(removedItemsFromPlayer, checkConditions: false, selfValues, suppliedValues);
            }

            // quest given to the player
            if (step.startQuest != null) {
                QuestHandler.instance.AddQuestToActiveQuests(step.startQuest);
            }


            // speakingInventory = playerInventory;
            workingStep = step;
            // stepHasQuestToGive = step.startQuest != null;
            // stepHasContextToOpen = !(string.IsNullOrEmpty(step.contextName) || string.IsNullOrWhiteSpace(step.contextName));
            

            // phase = 0;
            // phaseTimer = 0;
            // phaseTime = step.barkTime;

            //set phase to 0, and let the speaker bark timer go
            SetPhase(0, step.barkTime);
        }


        void SetPhase (int newPhase, float phaseTime) {
            phaseTimer = 0;
            phase = newPhase;
            this.phaseTime = phaseTime;
        }



            



        int phase;
        float phaseTimer, phaseTime;
        void Update () {
            if (phase == 0 || phase == 2) {
                phaseTimer += Time.deltaTime;
                if (phaseTimer >= phaseTime) {
                    phaseTimer = 0;
                    phase++;
                    OnPhaseEnd();
                }
            }
        }

        DialogueStep workingStep;
        // Inventory speakingInventory;
        // int speakingEquipID;
        // Actor playerActor;
        // bool stepHasContextToOpen;//, stepHasQuestToGive;//, hasGiftForSpeaker, speakerGivesToMe;

        void OnPhaseEnd () {
            // done waiting for speaking to to finish bark 
            // now we start to wait for the player response
            if (phase == 1) {

                // or we open the ui menu (e.g. trading)
                bool stepHasUIContextToOpen = !(string.IsNullOrEmpty(workingStep.contextName) || string.IsNullOrWhiteSpace(workingStep.contextName));
                if (stepHasUIContextToOpen) {
                    myInventory.InitiateInventoryManagement(workingStep.contextName, equipID: 0, speakingTo.inventory, workingStep.categoryFilter);
                    Stop();
                    return;
                }
                
                // start to set up the player response ui
                bool hasResponses = workingStep.responses.list.Length > 0;
                if (!hasResponses) {
                    Stop();
                    return;
                }

                Dictionary<string, GameValue> selfValues = myActor.GetValueDictionary();
                Dictionary<string, GameValue> suppliedValues = speakingTo.actor.GetValueDictionary();
            
                DialogueResponse[] responses = workingStep.responses;
                List<DialogueResponse> usedResponses = new List<DialogueResponse>();

                for (int i = 0; i < responses.Length; i++) {

                    //check the response for editor conditions chekcs...
                    if (GameValueCondition.ConditionsMet(responses[i].conditions, selfValues, suppliedValues)) {
                        
                        int nextStepID = responses[i].nextDialogueStepID;
                        if (stepIDtoStep.ContainsKey(nextStepID)) {

                            // also check if the response leads to transferring form the player inventory to the speaker,
                            // that the player inventory has enough components
                            DialogueStep leadsToStep = stepIDtoStep[nextStepID];
                            bool hasTransferFromPlayer = leadsToStep.transferToSpeaker.list.Length > 0;
                            if (!hasTransferFromPlayer || myInventory.GetComponent<InventoryCrafter>().ItemCompositionAvailableInInventory(leadsToStep.transferToSpeaker, checkScrap: false, selfValues, suppliedValues)) {
                                usedResponses.Add(responses[i]);
                            }
                        }
                    }
                }
                if (usedResponses.Count == 0) {
                    Stop();
                    return;
                }


                //set up the ui to show potential responses (TODO: fade out instead of hard close on response...)
                waitingForResponse = true;
                dialoguePlayerUIHandler.onUIClose += OnDialogueUIHandlerClose;
                dialoguePlayerUIHandler.ShowResponses(usedResponses, OnResponseChosen);
            }
            

            // we chose a response and let the audio play
            else if (phase == 3) {

                PlayStep(stepIDtoStep[currentStepID]);//, speaker, speakingInventory, speakingEquipID);
                return;
            }
        }

        bool waitingForResponse;

        void OnResponseChosen (DialogueResponse chosenResponse) {
            waitingForResponse = false;
            UISubtitles.instance.ShowSubtitles (myActor.actorName, chosenResponse.bark);
            if (audioSource != null) {
                if (chosenResponse.associatedAudio != null) {
                    audioSource.clip = chosenResponse.associatedAudio;
                    audioSource.Play();
                }
            }
            SetPhase(2, chosenResponse.barkTime);
            currentStepID = chosenResponse.nextDialogueStepID;
        }


        void OnDialogueUIHandlerClose (UIElementHolder uiObject) {
            dialoguePlayerUIHandler.onUIClose -= OnDialogueUIHandlerClose;
            
            // if we closed the ui without choosing a response, just stop the convo
            if (waitingForResponse) {
                Stop();
            }
        }

        public DialoguePlayerUIHandler dialoguePlayerUIHandler;

        
        
    }

}