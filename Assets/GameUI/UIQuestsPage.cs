using System.Collections.Generic;
using UnityEngine;

using Game.QuestSystem;

namespace Game.GameUI {

    public class UIQuestsPage : UISelectableElementHandler
    {
        protected override int ParamsLength () { 
            return 0; 
        }
        
        protected override bool CheckParameters (object[] parameters) {
            return true;
        }

        protected override void OnOpenUI(object[] parameters) { 
            BuildButtons("Active Quests", true, 0, null);
            BuildButtons("Completed Quests", false, 1, null);
        }

        protected override void OnUISelect (GameObject[] data, object[] customData) { 

            if (customData != null) {
                Quest quest = customData[0] as Quest;   
                int panelIndex = (int)customData[1];

                string textToShow = quest.displayName;
                if (panelIndex == 0) {
                    textToShow += (quest.infinite ? " [ Infinite ]" : "") + ":\n\n" + quest.GetHint();
                }
                else {
                    textToShow += " [ Completed ]";
                }

                (uiObject as SimpleUI.ElementHolderCollection).textPanel.SetText(textToShow);
            }
        }
        
        protected override object[] GetDefaultColdOpenParams() { 
            return new object[] { }; 
        }

        protected override List<object> BuildButtonObjectsListForDisplay (int panelIndex, object[] updateButtonsParams) { 
            List<object> r = new List<object>();
            List<Quest> listToUse = panelIndex == 0 ? QuestHandler.instance.activeQuests : QuestHandler.instance.completedQuests;
            
            for (int i = 0; i < listToUse.Count; i++) {
                if (listToUse[i].isPublic) {
                    r.Add(listToUse[i]);
                }
            }
            return r;
        }

        protected override string GetDisplayForButtonObject(object obj) { 
            return (obj as Quest).displayName; 
        }

        const int selectQuestAction = 0;

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input, int actionOffset) {
            
            if (customData != null) {
                if (input.x == selectQuestAction+actionOffset) {
                    
                    Quest quest = customData[0] as Quest;
                    int panelIndex = (int)customData[1];
                    
                    if (panelIndex == 0) {
                        
                        if (quest != null) {
                            QuestHandler.currentSelectedQuest = QuestHandler.currentSelectedQuest == quest ? null : quest;
                            UpdateUIButtons( 0, null );
                        }
                    }
                }    
            }
		}
    }        
}
