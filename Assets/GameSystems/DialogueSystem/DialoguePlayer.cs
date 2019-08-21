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

// using ActorSystem;
// using InventorySystem;
// using QuestSystem;

using Game.UI;
using SimpleUI;

#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace Game.DialogueSystem {
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
            if (onDialogueEnded != null) {
                onDialogueEnded();
                onDialogueEnded = null;
            }
        }
        
        
        void BuildStepIDDictionary() {
            stepIDtoStep.Clear();
            for (int i = 0; i < template.allSteps.Length; i++) {
                stepIDtoStep.Add(template.allSteps[i].stepID, template.allSteps[i]);
            }
        }

        System.Action onDialogueEnded;
        public void StartDialogue (Actor speakingTo, DialogueTemplate template, AudioSource speakerSource, System.Action onDialogueEnded) {
            this.onDialogueEnded = onDialogueEnded;
            this.speakingTo = speakingTo;
            this.template = template;
            this.speakerSource = speakerSource;

            inDialogue = true;
            currentStepID = 0;
            BuildStepIDDictionary();
            playNextInitialBark = true;
            PlayStep(stepIDtoStep[currentStepID]);
        }
        bool playNextInitialBark;
        void PlayStep (DialogueStep step) {
            
            // Speaking to begins first...

            closesConvo = false;
            bool leadsToStep = false;

            if (playNextInitialBark) {
                List<DialogueResponse> usedResponses = FilterResponses (step.initialBarks, true);

                if (usedResponses.Count == 0) {
                    Debug.LogError("no initial step bark for step on template " + template.name + " step id: " +currentStepID);
                    Stop();
                    return;
                }
                // chose random bark:
                DialogueResponse initialBark = usedResponses[Random.Range(0, usedResponses.Count)];
                
                UIManager.ShowSubtitles (speakingTo.actorName, initialBark.bark);// step.bark);
                
                // play audio
                if (speakerSource != null) {
                    if (initialBark.associatedAudio != null) {
                        speakerSource.clip = initialBark.associatedAudio;
                        audioSource.Play();
                    }
                }
                leadsToStep = stepIDtoStep.ContainsKey(initialBark.nextDialogueStepID);    
                
                for (int i = 0; i < step.stepScripts.list.Length; i++) {
                    step.stepScripts.list[i].OnDialogueStep (myActor, speakingTo, initialBark.barkTime);
                    if (step.stepScripts.list[i].ClosesConversation()) {
                        closesConvo = true;
                    }
                }
                if (leadsToStep && !closesConvo) {

                    // if we want to actually play the step bark:
                    SetPhase(2, initialBark.barkTime);
                    currentStepID = initialBark.nextDialogueStepID;
                    playNextInitialBark = initialBark.playBarkOnStep;

                    // else
                    // SetPhase(1, initialBark.barkTime);
                    // currentStepID = initialBark.nextDialogueStepID;

                    


                }

                else {
                    //set phase to 0, and let the speaker bark timer go
                    SetPhase(0, initialBark.barkTime);                
                    workingStep = step;
                }
            }
            else {
                    //set phase to 0, and let the speaker bark timer go
                    SetPhase(0, 1);                
                    workingStep = step;
                

            }
                    
            

        }
        bool closesConvo;


        void SetPhase (int newPhase, float phaseTime) {
            phaseTimer = 0;
            phase = newPhase;
            this.phaseTime = phaseTime;
            // Debug.LogError("set phase " + phase);
        }

        void Update () {
            if (inDialogue) {
                if (phase == 0 || phase == 2) {
                    phaseTimer += Time.deltaTime;
                    if (phaseTimer >= phaseTime) {
                        // Debug.LogError("phase end" + phase);
                        phaseTimer = 0;
                        phase++;
                        OnPhaseEnd();
                    }
                }
            }
        }

        DialogueStep workingStep;


        List<DialogueResponse> FilterResponses (DialogueResponse[] choices, bool checkingInitialBark) {
            List<DialogueResponse> usedResponses = new List<DialogueResponse>();
            for (int i = 0; i < choices.Length; i++) {

                //check the response for editor conditions chekcs...
                if (ActorValueCondition.ConditionsMet(choices[i].conditions, myActor, speakingTo)) {

                    int nextStepID = choices[i].nextDialogueStepID;

                    bool leadsToStep = stepIDtoStep.ContainsKey(nextStepID);    
                    // if the next next dialogue step id for the response exists...
                    // or we're checking the initial bark which doesnt have to lead to a nother step
                    
                    
                    if (leadsToStep || checkingInitialBark) {

                        // also check if the response leads to transferring form the player inventory to the speaker,
                        // that the player inventory has enough components

                        bool stepAvailable = true;
                        if (leadsToStep) {
                            DialogueStep stepResponseLeadsTo = stepIDtoStep[nextStepID];
                            for (int x = 0; x < stepResponseLeadsTo.stepScripts.list.Length; x++) {
                                if (!stepResponseLeadsTo.stepScripts.list[x].StepAvailable (myActor, speakingTo)) {
                                    stepAvailable = false;
                                    break;
                                }
                            }
                        }
                        if (stepAvailable) {
                            usedResponses.Add(choices[i]);
                        }
                    }
                }
            }
            return usedResponses;
        }

        
        
        void OnPhaseEnd () {
            // done waiting for speaking to to finish bark 
            // now we start to wait for the player response
            if (phase == 1) {

                bool hasResponses = workingStep.responses.list.Length > 0;
                if (closesConvo || !hasResponses) {
                    Stop();
                    return;
                }
                
                List<DialogueResponse> usedResponses = FilterResponses (workingStep.responses, false);

                if (usedResponses.Count == 0) {
                    Stop();
                    return;
                }

                //set up the ui to show potential responses (TODO: fade out instead of hard close on response...)
                waitingForResponse = true;
                
                GameUI.dialogueResponseUI.onUIClose += OnResponseCancelled;
                GameUI.dialogueResponseUI.OpenDialogueResponseUI(usedResponses, OnResponseChosen);
            }
                
            
            // we chose a response and let the audio play
            else if (phase == 3) {
                // Debug.LogError("playing new step");
                PlayStep(stepIDtoStep[currentStepID]);
                return;
            }
        }

        void OnResponseChosen (DialogueResponse chosenResponse) {
            waitingForResponse = false;
            // Debug.LogError("chose response");
            // player "speaks"
            UIManager.ShowSubtitles (myActor.actorName, chosenResponse.bark);
            
            if (audioSource != null && chosenResponse.associatedAudio != null) {
                audioSource.clip = chosenResponse.associatedAudio;
                audioSource.Play();
            }

            SetPhase(2, chosenResponse.barkTime);
            currentStepID = chosenResponse.nextDialogueStepID;

            playNextInitialBark = chosenResponse.playBarkOnStep;
        }

        void OnResponseCancelled (GameObject uiObject) {
            // uIHandler
            GameUI.dialogueResponseUI.onUIClose -= OnResponseCancelled;
            // if we closed the ui without choosing a response, just stop the convo
            if (waitingForResponse) {
                Debug.LogError("stopped because of cancel");
                Stop();
            }
        }
    }
}