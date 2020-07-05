using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatBehaviour {
    // See WanderBehaviour for a description of how these Behaviour classes are meant to work
    // Important note: All distance calculations should be based on character x and y position, not their tile's x and y

    // Actions:
    //  Attack
    //  Seek
    //  Flee

    static float DEFAULT_RANGE = 1.5f;

    public static void WeighSeek(Character chara) {
        if (chara.AIWeights.ContainsKey("seek")) {
            // Should we allow this, and overwrite the previous value?
            // Until we have a use case, just error out
            Debug.LogError("Trying to weigh the Seek AI twice for character: " + chara.name);
            return;
        }

        // Only seek enemies in Combat mode
        if (chara.behaviourState == BehaviourState.COMBAT) {
            // If we have a healthy, non-fleeing target within reach, do not seek a new one
            if (chara.variables.TryGetValue("combat_target", out object targetobj)) {
                Character target = TacticalController.instance.map.characters[(int) targetobj];
                if (target.HP > 0 && target.behaviourState != BehaviourState.FLEEING && chara.GetDistanceToTarget(target) <= DEFAULT_RANGE) {
                    return;
                }
            }

            // We are not currently engaged. Check if there are others within reach
            List<Character> targets = FindTargetsInRange(chara);
            // If there are adjacent healthy, non-fleeing enemies, do not seek a new target
            foreach (Character target in targets) {
                if (target.HP > 0 && target.behaviourState != BehaviourState.FLEEING) {
                    return;
                }
            }

            // There is nobody in reach, so seek someone new
            chara.AIWeights.Add("seek", 10);

            // Now that we know TO seek, WHOM do we seek?
            Dictionary<int, int> seekWeights = new Dictionary<int, int>();
            List<Character> noticed = new List<Character>(chara.noticedCharacters.Keys);

            foreach (Character target in noticed) {
                if (target.allegiance == chara.allegiance) {
                    continue;
                }
                int weight = 100;
                // TODO: If the character has a trait like "no mercy", do not de-prioritise
                if (target.behaviourState == BehaviourState.FLEEING) {
                    // De-prioritise fleeing enemies
                    weight = 20;
                }
                if (target.HP <= 0) {
                    // REALLY De-prioritise downed enemies
                    weight = 1;
                }
                // Prioritise close-by enemies
                weight = (int) Math.Ceiling(weight / chara.GetDistanceToTarget(target));

                // TODO: Add measures based on the enemy's current health, threat, nearby wounded allies, race/class bias...
                seekWeights.Add(TacticalController.instance.map.characters.IndexOf(target), weight);
            }
            chara.variables["combat_seekWeights"] = seekWeights;
        }
    }

    public static void Seek(Character chara, float deltaTime) {
        // We already have a target
        if (chara.variables.TryGetValue("combat_target", out object targetObj)) {
            Character target = TacticalController.instance.map.characters[(int) targetObj];
            // If we are within range, stop seeking
            if (chara.GetDistanceToTarget(target) <= DEFAULT_RANGE) {
                Debug.Log(chara.name + " has reached his target, " + target.name);
                chara.currentBehaviour = "deciding";
                return;
            }
            
            // Otherwise, approach the target
            // SUPER BASIC PATHFINDING, replace with A* when implemented
            if (chara.isMoving == true) {
                return;
            }

            int deltaX = 0;
            int deltaY = 0;
            if (target.x > chara.x) {
                deltaX = 1;
            }
            if (target.x < chara.x) {
                deltaX = -1;
            }
            if (target.y > chara.y) {
                deltaY = 1;
            }
            if (target.y < chara.y) {
                deltaY = -1;
            }
            Tile destination = TacticalController.instance.map.GetTileAt(chara.currentTile.x + deltaX, chara.currentTile.y + deltaY);
            if (destination.character != null && deltaY != 0) {
                destination = TacticalController.instance.map.GetTileAt(chara.currentTile.x, chara.currentTile.y + deltaY);
            }
            if (destination.character != null && deltaX != 0) {
                destination = TacticalController.instance.map.GetTileAt(chara.currentTile.x + deltaX, chara.currentTile.y);
            }
            if (destination.character != null) {
                // Just wait for now
                return;
            }
            chara.BeginMove(destination);
        } else {
            // We need a target
            object seekWeightsObj;
            if (chara.variables.TryGetValue("combat_seekWeights", out seekWeightsObj) == false) {
                Debug.LogError(chara.name + " is trying to seek, but no seekWeights has been defined");
                return;
            }
            Dictionary<int, int> seekWeights = (Dictionary<int, int>) seekWeightsObj;

            if (seekWeights.Count == 0) {
                // No nearby enemies, hold position
                return;
            }

            List<int> options = new List<int>();
            Dictionary<int, int> indices = new Dictionary<int, int>();
            int index = 0;
            foreach (int target in seekWeights.Keys) {
                options.Add(seekWeights[target]);
                indices.Add(index, target);
                index++;
            }
            int toSeek = indices[TacticalController.MakeDecision(options)];
            chara.variables.Add("combat_target", toSeek);
            Debug.Log(chara.name + " is now seeking " + TacticalController.instance.map.characters[toSeek].name);
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
}