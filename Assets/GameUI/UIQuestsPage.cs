﻿using System.Collections.Generic;
using UnityEngine;

using Game.QuestSystem;
using SimpleUI;

namespace Game.UI {

    public class UIQuestsPage : UISelectableElementHandler
    {
        static UIQuestsPage _instance;
        public static UIQuestsPage instance {
            get {
                if (_instance == null) _instance = GameObject.FindObjectOfType<UIQuestsPage>();
                return _instance;
            }
        }
        public void OpenQuestManagementUI () {
            OpenUI ( 0, new object[] { } );
        }

        protected override void OnOpenUI() { 
            BuildButtons("Active Quests", true, 0);
            BuildButtons("Completed Quests", false, 1);
        }

        protected override void OnUISelect (GameObject selectedObject, GameObject[] data, object[] customData) { 
            string txt = "";
            if (customData != null) {
                Quest quest = customData[0] as Quest;
                if (quest != null) {
                    txt = quest.displayName;
                    int panelIndex = (int)customData[1];
                    if (panelIndex == 0) {
                        txt += (quest.infinite ? " [ Infinite ]" : "") + ":\n\n" + quest.GetCurrentTextHint();
                    }
                    else {
                        txt += " [ Completed ]";
                    }
                }   
            }
            (uiObject as SimpleUI.ElementHolderCollection).textPanel.SetText(txt);
        }
        
        protected override object[] GetDefaultColdOpenParams() { return new object[] { }; }
        protected override List<object> BuildButtonObjectsListForDisplay (int panelIndex){
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

        protected override void OnUIInput (GameObject selectedObject, GameObject[] data, object[] customData, Vector2Int input, int actionOffset) {
            
            if (input.x == selectQuestAction+actionOffset) {
                if (customData != null) {
                    Quest quest = customData[0] as Quest;
                    if (quest != null) {
                        int panelIndex = (int)customData[1];
                        if (panelIndex == 0) {
                            QuestHandler.currentSelectedQuest = QuestHandler.currentSelectedQuest == quest ? null : quest;
                            UpdateUIButtons( );
                        }
                    }    
                }    
            }
		}
    }        
}
