using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatBehaviour {
    // See WanderBehaviour for a description of how these Behaviour classes are meant to work
    // Characters with this behaviour will engage in combat with their enemies
    // Important note: All distance calculations should be based on character x and y position, not their tile's x and y

    // Actions:
    //  Attack
    //  Reposition
    //  Flee

    static float DEFAULT_RANGE = 1.5f;

    public static void Register(Character chara) {
        chara.RegisterAIBehaviour("target", CombatBehaviour.Target, CombatBehaviour.WeighTarget);
        chara.RegisterAIBehaviour("reposition", CombatBehaviour.Reposition, CombatBehaviour.WeighReposition);
        chara.RegisterAIBehaviour("attack", CombatBehaviour.Attack, CombatBehaviour.WeighAttack);
        chara.RegisterOnUpdate(CombatBehaviour.UpdateCombatAwareness);
    }

    public static void WeighTarget(Character chara) {
        if (chara.AIWeights.ContainsKey("target")) {
            // Should we allow this, and overwrite the previous value?
            // Until we have a use case, just error out
            Debug.LogError("Trying to weigh the Target AI twice for character: " + chara.name);
            return;
        }

        // Only target enemies in Combat mode
        if (chara.behaviourState == BehaviourState.COMBAT) {
            // If we have a healthy target within reach, do not seek a new one
            if (chara.variables.TryGetValue("combat_target", out object targetobj)) {
                Character target = TacticalController.instance.map.characters[(int) targetobj];
                if (target.healthState == HealthState.CONCSCIOUS ) {
                    return;
                }
                // Remove our invalid target
                chara.variables.Remove("combat_target");
            }
            // We have no valid target, so find someone new

            // Now that we know TO seek, WHOM do we seek?
            Dictionary<int, int> seekWeights = new Dictionary<int, int>();
            List<Character> noticed = new List<Character>(chara.noticedCharacters.Keys);

            foreach (Character target in noticed) {
                if (target.allegiance == chara.allegiance) {
                    // Do not target your own allies
                    continue;
                }
                float workingWeight = 100;
                // TODO: If the character has a trait like "no mercy", do not de-prioritise
                if (target.behaviourState == BehaviourState.FLEEING) {
                    // De-prioritise fleeing enemies
                    workingWeight = 20;
                }
                if (target.healthState == HealthState.DYING || target.healthState == HealthState.STABLE) {
                    // REALLY De-prioritise downed enemies
                    workingWeight = 1;
                }
                if (target.healthState == HealthState.DEAD) {
                    // Don't attack dead people
                    continue;
                }

                float distance = chara.GetDistanceToTarget(target);

                // Prioritise close-by enemies
                if (distance > 1) {
                    workingWeight /= distance;
                }

                if (distance > DEFAULT_RANGE) {
                    // De-prioritise out of range enemies again
                    workingWeight /= distance;
                }

                // TODO: Add measures based on the enemy's current health, threat, nearby wounded allies, race/class bias...
                int weight = (int)Math.Ceiling(workingWeight);
                if (weight > 0) {
                    seekWeights.Add(TacticalController.instance.map.characters.IndexOf(target), weight);
                }
            }

            if (seekWeights.Count > 0) {
                chara.AIWeights.Add("target", 10);
                chara.variables["combat_seekWeights"] = seekWeights;
            }
        }
    }

    public static void Target(Character chara, float deltaTime) {
        object seekWeightsObj;
        if (chara.variables.TryGetValue("combat_seekWeights", out seekWeightsObj) == false) {
            Debug.LogError(chara.name + " is trying to seek, but no seekWeights has been defined");
            chara.currentBehaviour = "deciding";
            return;
        }
        Dictionary<int, int> seekWeights = (Dictionary<int, int>) seekWeightsObj;

        if (seekWeights.Count == 0) {
            Debug.LogError(chara.name + " is trying to seek, but seekWeights is empty");
            chara.currentBehaviour = "deciding";
            return;
        }
        chara.variables.Remove("combat_target");

        List<int> weights = new List<int>();
        Dictionary<int, int> indices = new Dictionary<int, int>();
        int index = 0;
        foreach (int target in seekWeights.Keys) {
            weights.Add(seekWeights[target]);
            indices.Add(index, target);
            index++;
        }
        int toSeek = indices[TacticalController.MakeDecision(weights)];
        chara.variables.Add("combat_target", toSeek);
        Debug.Log(chara.name + " is now seeking " + TacticalController.instance.map.characters[toSeek].name);
        chara.currentBehaviour = "deciding";
    }

    public static void WeighReposition(Character chara) {
        if (chara.AIWeights.ContainsKey("reposition")) {
            // Should we allow this, and overwrite the previous value?
            // Until we have a use case, just error out
            Debug.LogError("Trying to weigh the Reposition AI twice for character: " + chara.name);
            return;
        }

        if (chara.behaviourState == BehaviourState.COMBAT) {
            if (chara.variables.TryGetValue("combat_target", out object targetObj)) {
                Character target = TacticalController.instance.map.characters[(int) targetObj];
                float distance = chara.GetDistanceToTarget(target);
                if (distance > DEFAULT_RANGE) {
                    chara.AIWeights.Add("reposition", 10);
                }
                if (DEFAULT_RANGE - distance >= 2) {
                    chara.AIWeights.Add("reposition", 3);
                }
            }
        }
    }

    public static void Reposition(Character chara, float deltaTime) {
        if (chara.variables.TryGetValue("combat_target", out object targetObj)) {
            Character target = TacticalController.instance.map.characters[(int) targetObj];
            // If we are within range, stop seeking
            if (chara.GetDistanceToTarget(target) <= DEFAULT_RANGE) {
                // TODO: Back up if within range, with wiggle room
                Debug.Log(chara.name + " has reached their target, " + target.name);
                chara.currentBehaviour = "deciding";
                return;
            }
            
            // Otherwise, approach the target
            TacticalController.BasicPathfindToCoordinates(chara, target.x, target.y);
        }
    }

    public static void WeighAttack(Character chara) {
        if (chara.AIWeights.ContainsKey("attack")) {
            // Should we allow this, and overwrite the previous value?
            // Until we have a use case, just error out
            Debug.LogError("Trying to weigh the Seek AI twice for character: " + chara.name);
            return;
        }

        // If we have a target, smack it
        if (chara.behaviourState == BehaviourState.COMBAT && chara.variables.TryGetValue("combat_target", out object targetObj)) {
            Character target = TacticalController.instance.map.characters[(int) targetObj];
            if (chara.GetDistanceToTarget(target) <= DEFAULT_RANGE) {
                chara.AIWeights.Add("attack", 10);
            }
        }
    }

    public static void Attack (Character chara, float deltaTime) {
        if (chara.variables.TryGetValue("combat_target", out object targetObj)) {
            if (chara.variables.TryGetValue("combat_attackCooldown", out object cooldownObj)) {
                float cooldown = (float) cooldownObj;
                cooldown -= deltaTime;
                if (cooldown > 0) {
                    chara.variables["combat_attackCooldown"] = cooldown;
                } else {
                    chara.variables.Remove("combat_attackCooldown");
                    chara.currentBehaviour = "deciding";
                }
            } else {
                Character target = TacticalController.instance.map.characters[(int) targetObj];
                // TODO: Make more sophisticated
                target.HP -= chara.strength;
                chara.variables.Add("combat_attackCooldown", 50f / (float)chara.dexterity);
                Debug.Log(chara.name + " attacks " + target.name + " for " + chara.strength + " damage!");
            }
        } else {
            Debug.LogError(chara.name + " is attacking without a target!");
        }
    }

    public static List<Character> FindTargetsInRange(Character chara, bool includeAllies = false) {
        List<Character> targets = new List<Character>();

        foreach (Character target in chara.noticedCharacters.Keys) {
            if ((includeAllies == false && target.allegiance != chara.allegiance) && chara.GetDistanceToTarget(target) <= DEFAULT_RANGE) {    // TODO: Implement map-based hostility system and gear-based range system
                targets.Add(target);
            }
        }

        return targets;
    }

    public static void UpdateCombatAwareness(Character chara, float deltaTime) {
        if (chara.currentBehaviour == "deciding") {
            if (chara.variables.ContainsKey("combat_target") && chara.behaviourState != BehaviourState.COMBAT) {
                chara.variables.Remove("combat_target");
            }
        }
    }
}