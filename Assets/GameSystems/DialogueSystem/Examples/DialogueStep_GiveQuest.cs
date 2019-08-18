using UnityEngine;

using Game;
using Game.DialogueSystem;
using Game.QuestSystem;
public class DialogueStep_GiveQuest : MonoBehaviour, IDialogueStepScript
{
    public Quest startQuest;
    public bool StepAvailable(Actor dialoguePlayer, Actor speaker) { 
        return true; 
    }
    public bool ClosesConversation () { 
        return false; 
    }
    public void OnDialogueStep (Actor dialoguePlayer, Actor speaker, float stepTime) {
        QuestHandler.AddQuestToActiveQuests(startQuest);
    }
}
