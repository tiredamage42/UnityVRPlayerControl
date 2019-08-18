using System.Collections;
using UnityEngine;
using Game.GameUI;
using Game.DialogueSystem;
using Game;
public class DialogueStep_OpenFullTradeUI : MonoBehaviour, IDialogueStepScript
{
    [NeatArray] public NeatIntList categoryFilter;
    public bool StepAvailable(Actor dialoguePlayer, Actor speaker) { 
        return true; 
    }
    public bool ClosesConversation () { 
        return true; 
    }

    IEnumerator OpenTrade (Actor dialoguePlayer, Actor speaker, float stepTime) {
        
        // give the player time to stop
        yield return new WaitForSeconds (stepTime+.1f); 

        // dialoguePlayer.GetComponent
        GameObject.FindObjectOfType<FullTradeUIHandler>().OpenUI(
            new object[] { dialoguePlayer.inventory, 0, speaker.inventory, categoryFilter }
        );
    }
            
    public void OnDialogueStep (Actor dialoguePlayer, Actor speaker, float stepTime) {
        StartCoroutine( OpenTrade ( dialoguePlayer, speaker, stepTime) );
    }
}
