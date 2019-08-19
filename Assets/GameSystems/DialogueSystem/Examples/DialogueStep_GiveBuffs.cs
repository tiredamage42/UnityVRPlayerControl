using UnityEngine;
using Game;
using Game.DialogueSystem;
public class DialogueStep_GiveBuffs : MonoBehaviour, IDialogueStepScript
{
    [Header("Self values = player values. Supplied values = speaker values")]
    bool _;

    [Header("Buffs should be permanent")]
    [NeatArray] public GameValueModifierArray playerBuffs;
    [NeatArray] public GameValueModifierArray speakerBuffs;

    public bool ClosesConversation () { 
        return false; 
    }
    public bool StepAvailable(Actor dialoguePlayer, Actor speaker) { 
        return true; 
    }

    public void OnDialogueStep (Actor dialoguePlayer, Actor speaker, float stepTime) {
        Debug.LogError("adding buffs confo");
        // add any buffs associated with this step to the player
        dialoguePlayer.AddBuffs(playerBuffs, 1, 0, 0, true, dialoguePlayer, speaker);

        // add any buffs associated with this step to the speaker
        speaker.AddBuffs(speakerBuffs, 1, 0, 0, true, dialoguePlayer, speaker);
    }
}
