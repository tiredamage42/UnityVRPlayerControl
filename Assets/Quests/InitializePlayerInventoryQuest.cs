using Game;
using Game.QuestSystem;
using Game.InventorySystem;

public class InitializePlayerInventoryQuest : Quest
{
    public LevelledList levelledList;

    void InitializePlayerInventory () {
        Actor.playerActor.inventory.AddInventory(levelledList.SpawnItems(Actor.playerActor, Actor.playerActor), sendMessage: false);
    }
    public override void OnQuestInitialize () {
        InitializePlayerInventory();
        CompleteQuest();
    }
    public override void OnUpdateQuest (float deltaTime) { }
    public override string GetCurrentTextHint () { return null; }
    public override void OnQuestComplete () { }
    
}
