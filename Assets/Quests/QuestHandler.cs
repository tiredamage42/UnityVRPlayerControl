﻿// using System.Collections;
using System.Collections.Generic;


// using System.Reflection;
using UnityEngine;
// using Game.GameUI;
using SimpleUI;
namespace Game.QuestSystem {
    /*
        make script quest as monobehaviour on a prefab

        supply that to quest handler, which then instantiates an instance of the quest
        adds as child to quest handler, and adds instance to active quests
    */

    public class QuestHandler : MonoBehaviour {

        public Quest[] autoQuests;

        // static List<System.Type> GetAllQuestTypes () {
        //     List<System.Type> r = new List<System.Type>();
        //     foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
        //     {
        //         foreach (var type in asm.GetTypes())
        //         {
        //             if (type.IsSubclassOf(typeof(Quest))) {
        //                 // Debug.Log(type.Name);
        //                 r.Add(type);
        //             }
        //         }
        //     }
        //     return r;
        // }


        static QuestHandler _i;
        public static QuestHandler instance {
            get {
                if (_i == null) _i = GameObject.FindObjectOfType<QuestHandler>();
                if (_i == null) Debug.LogError("No Quest Handler Instance in the scene");
                return _i;
            }
        }

        GameObject questHolderObj;

        void Awake () {
            questHolderObj = new GameObject("Quest Holder");
            questHolderObj.transform.SetParent(transform);
        }

        void Start () {
            for (int i = 0; i < autoQuests.Length; i++) {
                AddQuestToActiveQuests(autoQuests[i]);
            }
        }
        
        void Update () {
            UpdateActiveQuests (Time.deltaTime) ;

            hintTimer += Time.deltaTime;
            if (hintTimer >= selectedQuestHintsFrequency) {
                ShowSelectedQuestHint();
                hintTimer = 0;
            }
        }

        void ShowSelectedQuestHint() {
            if (selectedQuest == null)
                return;

            string hint = selectedQuest.GetCurrentTextHint();
            if (hint != null) {
                UIManager.ShowInGameMessage( hint, false, UIColorScheme.Normal );
            }
        }


        float hintTimer;
        void UpdateActiveQuests(float deltaTime) {
            for (int i = 0; i < activeQuests.Count; i++) {
                activeQuests[i].OnUpdateQuest(deltaTime);
            }
        }

        [HideInInspector] public List<Quest> activeQuests = new List<Quest>(), completedQuests = new List<Quest>();
        [HideInInspector] public Quest selectedQuest;

        public static Quest currentSelectedQuest {
            get {
                return instance == null ? null : instance.selectedQuest;
            }
            set {
                if (instance != null) instance.selectedQuest = value;
            }
        }
        public float selectedQuestHintsFrequency = 5;

        static bool QuestActive (int id, out int index) {
            index = -1;
            if (instance == null) {
                Debug.LogError(" Cant check quest active, quest handler instance is null ");
                return false;
            }
            for (int i = 0; i< instance.activeQuests.Count; i++) {
                if (instance.activeQuests[i].questID == id) {
                    index = i;
                    return true;
                }
            }
            return false;
        }
        static bool QuestCompleted (int id) {
            if (instance == null) {
                Debug.LogError(" Cant check quest completion, quest handler instance is null ");
                return false;
            }
            
            for (int i = 0; i< instance.completedQuests.Count; i++) {
                if (instance.completedQuests[i].questID == id) {
                    return true;
                }
            }
            return false;
        }

        public static void AddQuestToActiveQuests (Quest questPrefab) {
            if (questPrefab == null) {
                Debug.LogError(" Cant add quest to active quests... it is null ");
                return;
            }
            if (instance == null) {
                Debug.LogError(" Cant add quest " + questPrefab.displayName + " to active quests... quest handler instance is null ");
                return;
            }
            int id = questPrefab.GetInstanceID();

            if (QuestCompleted(id)) {
                Debug.Log(questPrefab.name + " is already completed!");
                return;
            }
            int questIndex;
            if (QuestActive(id, out questIndex)) {
                Debug.Log(questPrefab.name + " is already active!");
                return;
            }

            // Quest newQuest = CopyComponent<Quest>(questPrefab, questHolderObj);

            Quest newQuest = questPrefab;
            if (questPrefab.instantiateCopy) {
                newQuest = Instantiate(questPrefab);
                newQuest.transform.SetParent(instance.transform);
            }
            instance.activeQuests.Add(newQuest);
            newQuest.OnQuestInitialize();
        }


        // TODO: test out if this actually copies all nested class values...
        // static T CopyComponent<T>(T original, GameObject destination) where T : Component
        // {

        //     // Debug.Log("copying " + original.name);
        //     System.Type type = original.GetType();
            
        //     var copy = destination.GetComponent(type) as T;
        //     if (!copy) copy = destination.AddComponent(type) as T;

        //     BindingFlags flags = 
        //         BindingFlags.Public 
        //         | BindingFlags.NonPublic
        //         | BindingFlags.Instance
        //         | BindingFlags.Default
        //         // | BindingFlags.DeclaredOnly
        //         // | BindingFlags.FlattenHierarchy //to get fields from inherited types.
        //     ;
         
        //     var fields = type.GetFields(flags);
        //     foreach (FieldInfo field in fields)
        //     {
        //         if (field.IsStatic) continue;
        //         field.SetValue(copy, field.GetValue(original));
        //         // Debug.Log("copying field " + field.Name);
        //     }

        //     var props = type.GetProperties(flags);
        //     foreach (var prop in props)
        //     {
        //         bool obsolete = false;
        //         IEnumerable attrData = prop.CustomAttributes;
        //         foreach (CustomAttributeData data in attrData)
        //         {
        //             if (data.AttributeType == typeof(System.ObsoleteAttribute))
        //             {
        //                 obsolete = true;
        //                 break;
        //             }
        //         }
        //         if (obsolete) continue;
        //         if (!prop.CanWrite || prop.Name == "name" || prop.PropertyType.Equals(typeof(Material)) || prop.PropertyType.Equals(typeof(Material[]))) continue;
        //         prop.SetValue(copy, prop.GetValue(original, null), null);
        //         // Debug.Log("copying property " + prop.Name);
                    
        //     }
        //     return copy as T;
        // }

        // public static T GetCopyOf<T>(T original, GameObject destination) where T : MonoBehaviour
        // {
        //     //Type check
        //     System.Type type = original.GetType();

        //     var copy = destination.GetComponent(type) as T;
        //     if (!copy) copy = destination.AddComponent(type) as T;
        
        //     //Declare Binding Flags
        //     BindingFlags flags = 
        //         BindingFlags.Public 
        //         | BindingFlags.NonPublic 
        //         | BindingFlags.Instance 
        //         | BindingFlags.Default 
        //         | BindingFlags.DeclaredOnly
        //     ;
                    
        //     //Iterate through all types until monobehaviour is reached
        //     while (type != typeof(MonoBehaviour))
        //     {
        //         //Apply Fields
        //         FieldInfo[] fields = type.GetFields(flags);
        //         foreach (FieldInfo field in fields)
        //         {
        //             field.SetValue(copy, field.GetValue(original));
        //         }
        
        //         //Move to base class
        //         type = type.BaseType;
        //     }
        //     return copy as T;
        // }

        public enum QuestCompletionLevel {
            Inactive, Active, Completed 
        };
        
        public static QuestCompletionLevel GetQuestCompletionLevel (string displayName) {
            for (int i =0 ; i < instance.activeQuests.Count; i++) {
                if (instance.activeQuests[i].displayName == displayName) return QuestCompletionLevel.Active;
            }
            for (int i =0 ; i < instance.completedQuests.Count; i++) {
                if (instance.completedQuests[i].displayName == displayName) return QuestCompletionLevel.Completed;
            }
            return QuestCompletionLevel.Inactive;
        }
        public static int GetInternalKey (string displayName) {
            for (int i =0 ; i < instance.activeQuests.Count; i++) {
                if (instance.activeQuests[i].displayName == displayName) return instance.activeQuests[i].internalKey;
            }
            for (int i =0 ; i < instance.completedQuests.Count; i++) {
                if (instance.completedQuests[i].displayName == displayName) return instance.completedQuests[i].internalKey;
            }
            Debug.LogError("Quest " + displayName + " not found in active or completed quests");
            return 0;
        }


        public void CompleteQuest (int questID) {
            int questIndex;
            if (!QuestActive(questID, out questIndex)) {
                return;
            }
            Quest activeQuest = activeQuests[questIndex];

            if (activeQuest.infinite) {
                Debug.LogWarning("Cant complete " + activeQuest.name + " :: infinite quest");
                return;
            }

            activeQuests.Remove(activeQuest);
            completedQuests.Add(activeQuest);

            activeQuest.OnQuestComplete();
        }
    }

    public abstract class Quest : MonoBehaviour
    {
        [Header("Should be true if using prefab")]
        public bool instantiateCopy = true;
        [HideInInspector] public int questID;
        public bool isPublic;
        public string displayName;
        public bool infinite;

        [HideInInspector] public int internalKey;

        protected void CompleteQuest () {
            QuestHandler.instance.CompleteQuest(questID);
        }

        public abstract string GetCurrentTextHint ();

        //maybe have a seperate one for first time quest initializes, 
        //and another for when game is loaded...
        public abstract void OnQuestInitialize ();

        public abstract void OnUpdateQuest (float deltaTime);
        
        public abstract void OnQuestComplete ();

    }









}
