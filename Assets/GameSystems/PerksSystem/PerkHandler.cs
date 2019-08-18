using System.Collections.Generic;
using UnityEngine;

namespace Game.PerkSystem {
    [System.Serializable] public class PerkHolder {
        public Perk perk;
        public int level;
        public List<PerkScript> perkScripts = new List<PerkScript>();
        public PerkHolder (Perk perk, Actor actor) {
            this.perk = perk;
            OnPerkGiven(actor);
        }
        void OnPerkGiven (Actor actor) {
            for (int i =0; i < perk.perkBehaviors.list.Length; i++) {
                perk.perkBehaviors.list[i].OnPerkGiven(actor);
            }
            for (int i =0; i < perk.perkScripts.list.Length; i++) {
                PerkScript scriptInstance = GameObject.Instantiate(perk.perkScripts.list[i]);
                scriptInstance.transform.SetParent(actor.transform);
                scriptInstance.OnPerkGiven(actor);
                perkScripts.Add(scriptInstance);
            }
        }
        public void SetLevel(int newLevel, Actor actor) {
            if (level != newLevel) {
                level = newLevel;
                OnPerkLevelChange(actor);
            }
        }
        void OnPerkLevelChange (Actor actor) {
            // tell the static behaviors
            for (int i =0; i < perk.perkBehaviors.list.Length; i++) perk.perkBehaviors.list[i].OnPerkLevelChange(level, actor);
            // tell the runtime scripts
            for (int i =0; i < perkScripts.Count; i++) perkScripts[i].OnPerkLevelChange(level, actor);
        }

        public void OnPerkUpdate (Actor actor, float deltaTime) {
            if (level > 0) {
                // update the runtime scripts attached to this perk
                for (int i =0; i < perkScripts.Count; i++) {
                    perkScripts[i].OnPerkUpdate(level, actor, deltaTime);
                }   
            }
        }
    }
    [RequireComponent(typeof(Actor))] public class PerkHandler : MonoBehaviour {
        Actor _actor;
        Actor actor {
            get {
                if (_actor == null) _actor = GetComponent<Actor>();
                return _actor;
            }
        }



        // MAYBE: make this a game value so we can use it in condition checks ?
        public int perkPoints; 
        public List<PerkHolder> allPerks = new List<PerkHolder>();

        void Update () {
            UpdatePerkScripts();
        }

        void UpdatePerkScripts () {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < allPerks.Count; i++) {
                allPerks[i].OnPerkUpdate (actor, deltaTime);
            }
        }

        bool HasPerk (Perk perk, out int index) {
            for (int i = 0; i < allPerks.Count; i++) {
                if (allPerks[i].perk == perk) {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }


        public int GetPerkLevel (Perk perk) {
            for (int i = 0; i < allPerks.Count; i++) {
                if (allPerks[i].perk == perk) {
                    return allPerks[i].level;
                }
            }    
            return 0;
        }

        public bool HasPerkMinLevel (string perkDisplayName, int minLevel) {
            for (int i = 0; i < allPerks.Count; i++) {
                if (allPerks[i].perk.displayName == perkDisplayName) {
                    return allPerks[i].level >= minLevel;
                }
            }
            return false;
        }
        public bool HasPerkLevel (string perkDisplayName, int level) {
            for (int i = 0; i < allPerks.Count; i++) {
                if (allPerks[i].perk.displayName == perkDisplayName) {
                    return allPerks[i].level == level;
                }
            }
            return false;
        }
        public bool HasPerk (string perkDisplayName, int minLevel) {
            return HasPerkMinLevel(perkDisplayName, 1);
        }

        public PerkHolder AddPerk (Perk perk) {
            int index;
            if (HasPerk(perk, out index)) return allPerks[index];
            allPerks.Add(new PerkHolder(perk, actor));
            return allPerks[allPerks.Count -1];
        }

        public void SetPerkLevel (Perk perk, int level) {
            int index;
            PerkHolder holder;
            if (HasPerk(perk, out index)) {
                holder = allPerks[index];   
            }
            else {
                holder = AddPerk(perk);
            }
            holder.SetLevel(level, actor);
        }

        // public void RemovePerk (Perk perk) {
        //     int index;
        //     if (HasPerk(perk, out index)) {
        //         allPerks.Remove(allPerks[index]);
        //     }
        // }        
    }         
}