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

namespace DialogueSystem {
    public class DialoguePlayer : MonoBehaviour {
        public AudioSource audioSource;
        public bool inDialogue;

        Actor myActor, speakingTo;
        DialogueTemplate template; 
        AudioSource speakerSource;
        Dictionary<int, DialogueStep> stepIDtoStep = new Dictionary<int, DialogueStep>();
        bool waitingForResponse;
        int phase, currentStepID;
        float phaseTimer, phaseTime;
        

        void Awake () {
            myActor = GetComponent<Actor>();
        }

        public void Stop () {
            currentStepID = 0;
            waitingForResponse = false;
            if (inDialogue) {
                if (audioSource != null) audioSource.Stop();
                if (speakingTo != null) {
                    if (speakerSource != null) speakerSource.Stop();
                    speakingTo = null;
                }
            }
            inDialogue = false;
        }
        
        
        void BuildStepIDDictionary() {
            stepIDtoStep.Clear();
            for (int i = 0; i < template.allSteps.Length; i++) {
                stepIDtoStep.Add(template.allSteps[i].stepID, template.allSteps[i]);
            }
        }
        public void StartDialogue (Actor speakingTo, DialogueTemplate template, AudioSource speakerSource) {

            if (onResponseRequested == null)
            {
                Debug.LogError(name + " / " + myActor.actorName + " doesnt have a callback for on response requested, hook player up with dialogue ui handler, or other method of choosing a response");
            }
            this.speakingTo = speakingTo;
            this.template = template;
            this.speakerSource = speakerSource;

            inDialogue = true;
            currentStepID = 0;
            BuildStepIDDictionary();
            PlayStep(stepIDtoStep[currentStepID]);
        }
        void PlayStep (DialogueStep step) {
            
            // Speaking to begins first...
            
            // show subtitles
            myActor.ShowSubtitles (speakingTo.actorName, step.bark);
            
            // play audio
            if (speakerSource != null) {
                if (step.associatedAudio != null) {
                    speakerSource.clip = step.associatedAudio;
                    audioSource.Play();
                }
            }


            Dictionary<string, GameValue> selfValues = myActor.actorValues;
            Dictionary<string, GameValue> suppliedValues = speakingTo.actorValues;
            

            // add any buffs associated with this step to the player
            myActor.AddBuffs(step.playerBuffs, 1, 0, 0, true, selfValues, suppliedValues);

            // add any buffs associated with this step to the speaker
            speakingTo.AddBuffs(step.speakerBuffs, 1, 0, 0, true, selfValues, suppliedValues);

            // give items to player
            if (step.transferToPlayer.list.Length > 0) {
                myActor.inventory.AddItemComposition(step.transferToPlayer, sendMessage: true, selfValues, suppliedValues);
            }

            // player gives items to speaker
            if (step.transferToSpeaker.list.Length > 0) {
                Item_Composition[] removedItemsFromPlayer = myActor.inventory.RemoveItemComposition(step.transferToSpeaker, checkScrap: false, sendMessage: true, selfValues, suppliedValues);
                
                // null game values = no condition check
                speakingTo.inventory.AddItemComposition(removedItemsFromPlayer, false, null, null);
            }

            // quest given to the player
            if (step.startQuest != null) {
                QuestHandler.instance.AddQuestToActiveQuests(step.startQuest);
            }

            //set phase to 0, and let the speaker bark timer go
            SetPhase(0, step.barkTime);
            
            workingStep = step;
        }


        void SetPhase (int newPhase, float phaseTime) {
            phaseTimer = 0;
            phase = newPhase;
            this.phaseTime = phaseTime;
        }

        void Update () {
            if (inDialogue) {

                if (phase == 0 || phase == 2) {
                    phaseTimer += Time.deltaTime;
                    if (phaseTimer >= phaseTime) {
                        phaseTimer = 0;
                        phase++;
                        OnPhaseEnd();
                    }
                }
            }
        }

        DialogueStep workingStep;
        
        void OnPhaseEnd () {
            // done waiting for speaking to to finish bark 
            // now we start to wait for the player response
            if (phase == 1) {
                // or we open the ui menu (e.g. trading)
                bool stepHasUIContextToOpen = !(string.IsNullOrEmpty(workingStep.contextName) || string.IsNullOrWhiteSpace(workingStep.contextName));
                if (stepHasUIContextToOpen) {
                    myActor.inventory.InitiateInventoryManagement(workingStep.contextName, equipID: 0, speakingTo.inventory, workingStep.categoryFilter);
                    Stop();
                    return;
                }
                
                // start to set up the player response ui
                bool hasResponses = workingStep.responses.list.Length > 0;
                if (!hasResponses) {
                    Stop();
                    return;
                }

                Dictionary<string, GameValue> selfValues = myActor.actorValues;
                Dictionary<string, GameValue> suppliedValues = speakingTo.actorValues;
            
                DialogueResponse[] responses = workingStep.responses;
                List<DialogueResponse> usedResponses = new List<DialogueResponse>();

                for (int i = 0; i < responses.Length; i++) {

                    //check the response for editor conditions chekcs...
                    if (GameValueCondition.ConditionsMet(responses[i].conditions, selfValues, suppliedValues)) {
                        
                        int nextStepID = responses[i].nextDialogueStepID;

                        // if the next next dialogue step id for the response exists...
                        if (stepIDtoStep.ContainsKey(nextStepID)) {

                            // also check if the response leads to transferring form the player inventory to the speaker,
                            // that the player inventory has enough components
                            DialogueStep leadsToStep = stepIDtoStep[nextStepID];
                            bool hasTransferFromPlayer = leadsToStep.transferToSpeaker.list.Length > 0;
                            if (!hasTransferFromPlayer || myActor.inventory.ItemCompositionAvailableInInventory(leadsToStep.transferToSpeaker, checkScrap: false, selfValues, suppliedValues)) {
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

                onResponseRequested(usedResponses, OnResponseChosen, OnResponseCancelled);
                
            }
            
            // we chose a response and let the audio play
            else if (phase == 3) {
                PlayStep(stepIDtoStep[currentStepID]);
                return;
            }
        }




        public event System.Action<List<DialogueResponse>, System.Action<DialogueResponse>, System.Action> onResponseRequested;

        void OnResponseChosen (DialogueResponse chosenResponse) {
            waitingForResponse = false;

            // player "speaks"
            myActor.ShowSubtitles (myActor.actorName, chosenResponse.bark);
            
            if (audioSource != null && chosenResponse.associatedAudio != null) {
                audioSource.clip = chosenResponse.associatedAudio;
                audioSource.Play();
            }

            SetPhase(2, chosenResponse.barkTime);
            currentStepID = chosenResponse.nextDialogueStepID;
        }

        void OnResponseCancelled () {
            // if we closed the ui without choosing a response, just stop the convo
            if (waitingForResponse) {
                Stop();
            }
        }
    }
}