using System.Collections.Generic;
using UnityEngine;
using Game.InventorySystem;
using Game.InventorySystem.CraftingSystem;
using Game.DialogueSystem;
using Game;
public class DialogueStep_TradeItems : MonoBehaviour, IDialogueStepScript
{


    [Header("Self values = player values. Supplied values = speaker values")]
    public bool autoScrapForTransferToSpeaker;
    // [ToolTip("If any specified,\nresponses leading to this dialogue step will not be available if the player inventory doesnt have the requred items...")]
    [NeatArray] public ItemCompositionArray transferToSpeaker;
    [NeatArray] public ItemCompositionArray transferToPlayer;

    public bool ClosesConversation () {
        return false;
    }

    public bool StepAvailable(Actor dialoguePlayer, Actor speaker) {
        if (autoScrapForTransferToSpeaker) {
            return transferToSpeaker.list.Length <= 0 || dialoguePlayer.inventory.ItemCompositionAvailableInInventoryAfterAutoScrap(transferToSpeaker, dialoguePlayer, speaker);
        }
        return transferToSpeaker.list.Length <= 0 || dialoguePlayer.inventory.ItemCompositionAvailableInInventory(transferToSpeaker, dialoguePlayer, speaker);

    }
            
    public void OnDialogueStep (Actor dialoguePlayer, Actor speaker, float stepTime) {
        
        // give items to player
        if (transferToPlayer.list.Length > 0) {
            dialoguePlayer.inventory.AddItemComposition(transferToPlayer, sendMessage: true, dialoguePlayer, speaker);
        }

        // player gives items to speaker
        if (transferToSpeaker.list.Length > 0) {
            if (autoScrapForTransferToSpeaker) {
                ItemComposition[] removedItemsFromPlayer = dialoguePlayer.inventory.RemoveItemCompositionWithAutoScrap(transferToSpeaker, sendMessage: true, dialoguePlayer, speaker);
                // null game values = no condition check
                speaker.inventory.AddItemComposition(removedItemsFromPlayer, false, null, null);
            }
            else {
                ItemComposition[] removedItemsFromPlayer = dialoguePlayer.inventory.RemoveItemComposition(transferToSpeaker, sendMessage: true, dialoguePlayer, speaker);
                // null game values = no condition check
                speaker.inventory.AddItemComposition(removedItemsFromPlayer, false, null, null);
            }
            
            
        }
    }
}