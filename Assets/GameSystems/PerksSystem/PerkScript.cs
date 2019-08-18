using UnityEngine;

namespace Game.PerkSystem {

        /*
        perk example outlines:

            simple permanent mod perk:
                Adamantium skeleton:
                    level 1 receive 10 % less damage
                    level 2 receive 20 % less damage
                    level 3 receive 30 % less damage

                    perk behavior give buffs {
                        on perk level change (perk holder, actor, perk) {
                            remove all perk behavior buffs

                            actor add buffs (behavior buffs[level] ) 
                        }
                    }

            scripted mod perk:
                Last Stand:
                    you wont go down easy without a fight!

                    level 1: when your hp is below 10, you do 2x damage
                    level 2: your hp regenerates when below 30 hp
                    level 3: while your hp is regenerating time slows


                    perk script base {
                        perk level { get { return perk holder element. level }}
                    }

                    perk script {

                        on perk value changed (new level) {
                            if new level == 0:
                                unsubscribe from hp events
                        }

                        subscribe to hp value change

                        bool wasBelowThreshold;
                        bool isBelowperk2Threhsold;
                        bool wasBelowThreshold3;

                        
                        on health value change () {
                            if (perklevel >= 1) {

                                if (perk1LevelConditions.met(perkHolderActor.values)) {
                                    if (!wasBelowThreshold) {
                                        perkholderActor.AddBuffs(perk1buffs);
                                        wasBelowThreshold = true;
                                    }
                                }
                                else {
                                    if (wasBelowThreshold) {
                                        perkholderActor.RemoveBuffs(perk1buffs);
                                        wasBelowThreshold = false;
                                    }
                                }
                            }
                            if (perlevel >= 2) {
                                isBelowperk2Threhsold = (perk2LevelConditions.met(perkHolderActor.values));
                            }
                        }

                        void OnPerkUpdate (float deltaTime) {
                            if perk level >= 2:
                                if (isBelowperk2Threhsold) {
                                    perkholderActor.addBuffs(perk2buffs, deltaTime); // add per second modifier property for adding
                                }

                            if perk level >= 3 && isplayer perk holder:


                                if (isBelowperk2Threhsold) {
                                    if !wasBelowThreshold3:

                                        add tiem mod perk to perk holder actor
                                        wasBelowThreshold3 = true;
                                        

                                }
                                else {
                                    if wasBelowThreshold3:
                                        remove time mod perk

                                        wasBelowThreshold3 = false;
                                    
                                }


                        }                    
                    }


            
            quest controlled perk:
                insomnia damage:
                    the less you sleep you sleep the more damage you dish out
                    level 1: damage + 10%
                    level 2: damage + 20%
                    level 3: damage + 30% 
                    
                    same behavior as adamntium skeleton, perma buffs, but controlled by quest (eg. survival values quest)

            just like game values modifiers in editor, have a set perk level option in editor stuff
    */


    /*
        perk behaviors that dont need to be instantiated into the scene to keep track of values during runtime...

        such as buffs that are only given or taken away during perk level changes
    */
    [System.Serializable] public class PerkBehaviorsArray : NeatArrayWrapper<PerkBehavior> { }
    public abstract class PerkBehavior : ScriptableObject {
        public abstract void OnPerkLevelChange (int level, Actor actor);
        public abstract void OnPerkGiven(Actor actor);
    }

    /*
        copies of each perk script object are instantiated and attached to the actor they're given to

        on perk update is only called if the perk level > 0

        attach IPerkScript behaviours to the main perk script GameObject and save as a prefab
    */
        
    [System.Serializable] public class PerkScriptsArray : NeatArrayWrapper<PerkScript> { }
    public interface IPerkScript {
        void OnPerkLevelChange (int level, Actor actor);
        void OnPerkGiven(Actor actor);
        void OnPerkUpdate (int level, Actor actor, float deltaTime);
    }
    public class PerkScript : MonoBehaviour {
        IPerkScript[] attachedScripts;
        public void OnPerkLevelChange (int level, Actor actor) {
            if (attachedScripts == null) attachedScripts = GetComponents<IPerkScript>();
            for (int i = 0; i < attachedScripts.Length; i++) attachedScripts[i].OnPerkLevelChange( level, actor);
        }
        public void OnPerkGiven(Actor actor) {
            if (attachedScripts == null) attachedScripts = GetComponents<IPerkScript>();
            for (int i = 0; i < attachedScripts.Length; i++) attachedScripts[i].OnPerkGiven(actor);
        }
        public void OnPerkUpdate(int level, Actor actor, float deltaTime) {
            if (attachedScripts == null) attachedScripts = GetComponents<IPerkScript>();
            for (int i = 0; i < attachedScripts.Length; i++) attachedScripts[i].OnPerkUpdate(level, actor, deltaTime);
        }
    }
}
