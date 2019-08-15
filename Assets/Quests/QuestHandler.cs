using System.Collections;
using System.Collections.Generic;


using System.Reflection;
using UnityEngine;
using ActorSystem;
using GameUI;
namespace QuestSystem {
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

            Actor.playerActor.ShowMessage( selectedQuest.GetHint(), UIColorScheme.Normal );
        }


        float hintTimer;
        void UpdateActiveQuests(float deltaTime) {
            for (int i = 0; i < activeQuests.Count; i++) {
                activeQuests[i].OnUpdateQuest(deltaTime);
            }
        }

        [HideInInspector] public List<Quest> activeQuests = new List<Quest>(), completedQuests = new List<Quest>();
        [HideInInspector] public Quest selectedQuest;
        public float selectedQuestHintsFrequency = 5;

        bool QuestActive (int id, out int index) {
            for (int i = 0; i< activeQuests.Count; i++) {
                if (activeQuests[i].questID == id) {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
        bool QuestCompleted (int id) {
            for (int i = 0; i< completedQuests.Count; i++) {
                if (completedQuests[i].questID == id) {
                    return true;
                }
            }
            return false;
        }

        public void AddQuestToActiveQuests (Quest questPrefab) {
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


            Quest newQuest = CopyComponent<Quest>(questPrefab, questHolderObj);
            // Quest newQuest = Instantiate(questPrefab);
            // newQuest.transform.SetParent(transform);

            activeQuests.Add(newQuest);
            newQuest.OnQuestInitialize();
        }


        // TODO: test out if this actually copies all nested class values...
        static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {

            // Debug.Log("copying " + original.name);
            System.Type type = original.GetType();
            
            var copy = destination.GetComponent(type) as T;
            if (!copy) copy = destination.AddComponent(type) as T;

            BindingFlags flags = 
                BindingFlags.Public 
                | BindingFlags.NonPublic
                | BindingFlags.Instance
                | BindingFlags.Default
                // | BindingFlags.DeclaredOnly
                // | BindingFlags.FlattenHierarchy //to get fields from inherited types.
            ;
         
            var fields = type.GetFields(flags);
            foreach (FieldInfo field in fields)
            {
                if (field.IsStatic) continue;
                field.SetValue(copy, field.GetValue(original));
                // Debug.Log("copying field " + field.Name);
            }

            var props = type.GetProperties(flags);
            foreach (var prop in props)
            {
                bool obsolete = false;
                IEnumerable attrData = prop.CustomAttributes;
                foreach (CustomAttributeData data in attrData)
                {
                    if (data.AttributeType == typeof(System.ObsoleteAttribute))
                    {
                        obsolete = true;
                        break;
                    }
                }
                if (obsolete) continue;
                if (!prop.CanWrite || prop.Name == "name" || prop.PropertyType.Equals(typeof(Material)) || prop.PropertyType.Equals(typeof(Material[]))) continue;
                prop.SetValue(copy, prop.GetValue(original, null), null);
                // Debug.Log("copying property " + prop.Name);
                    
            }
            return copy as T;
        }

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
        [HideInInspector] public int questID;
        public bool isPublic;
        public string displayName;
        public bool infinite;

        protected void CompleteQuest () {
            QuestHandler.instance.CompleteQuest(questID);
        }

        public abstract string GetHint ();

        //maybe have a seperate one for first time quest initializes, 
        //and another for when game is loaded...
        public abstract void OnQuestInitialize ();
        public abstract void OnUpdateQuest (float deltaTime);
        public abstract void OnQuestComplete ();

    }









}
