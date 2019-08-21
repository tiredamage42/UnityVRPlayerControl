

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.UI;

using Game.RadioSystem;

namespace Game.QuestSystem {

    public class Objective_RadioWithinTowerRange : ObjectiveScript {

        public Radio radioToCheck;
        
        [Header("If null checks for any tower")]
        public RadioStation station;
        
        [TextArea] public string hint = "Build a radio tower within broadcasting range of the radio";

        public override bool UpdateObjective (float deltaTime) { 
            if (station == null) {
                return radioToCheck.WithinAnyRadioTower(); 
            }
            return radioToCheck.StationAvailable(station);
        }
        public override string GetCurrentTextHint() { return hint; }
        public override void OnObjectiveIntroduced () { }
        public override void OnQuestInitialized () { }
        public override void OnQuestCompleted () { }

        public override void OnDisableActiveState() {}
        public override void OnEnableActiveState() {}
        
    }

}