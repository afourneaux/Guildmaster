using System;
using System.Collections.Generic;
using UnityEngine;

public class LootBehaviour {
    // Characters with this behaviour will seek and collect loot
    public static void Register(Character chara) {
        chara.RegisterAIBehaviour("loot", LootBehaviour.Loot, LootBehaviour.WeighLoot);

    }

    public static void WeighLoot(Character chara) {
        if (chara.AIWeights.ContainsKey("loot")) {
            // Should we allow this, and overwrite the previous value?
            // Until we have a use case, just error out
            Debug.LogError("Trying to weigh the Loot AI twice for character: " + chara.name);
            return;
        }

        // If there is no nearby loot, do not loot
        if (chara.noticedTreasure.Count == 0) {
            return;
        }

        // If we are actively in a fight, do not loot
        if (chara.variables.ContainsKey("combat_target")) {
            return;
        }

        Dictionary<Treasure, int> weightByTreasure = new Dictionary<Treasure, int>();
        foreach (Treasure option in chara.noticedTreasure) {
            // Set baseline
            float workingWeight = 100f;

            // For each ally already seeking this loot, lessen the weight
            foreach (Character ally in chara.noticedCharacters.Keys) {
                if (ally.allegiance != chara.allegiance) {
                    continue;
                }
                if (chara.variables.TryGetValue("loot_target", out object targetObj)) {
                    if (((Treasure) targetObj).tile == option.tile) {
                        workingWeight -= ((GameController.MAX_STAT_SCORE * 0.7f) - chara.greed);   // A character with greed over 70% of the max will actually want it more
                    }
                }
            }
            float distance = chara.GetDistanceToTile(option.tile);
            if (distance > 1) {
                workingWeight /= distance;
            }
            if (workingWeight > 0) {
                weightByTreasure.Add(option, Mathf.CeilToInt(workingWeight));
            }
        }
        if (weightByTreasure.Count > 0) {
            float workingTotalWeight = weightByTreasure.Count * chara.greed;

            // TODO: If already carrying a lot, lessen the value

            // If in combat mode, drastically reduce weight
            if (chara.behaviourState == BehaviourState.COMBAT) {
                int divisor = GameController.MAX_STAT_SCORE - chara.greed;
                if (divisor > 1) {
                    workingTotalWeight /= divisor;
                }
            }
            chara.AIWeights.Add("loot", Mathf.CeilToInt(workingTotalWeight));
            chara.variables["loot_weightByTreasure"] = weightByTreasure;
        }
    }

    public static void Loot(Character chara, float deltaTime) {
        if (chara.variables.TryGetValue("loot_target", out object targetObj)) {
            Treasure treasure = (Treasure) targetObj;

            // Is the treasure still valid?
            if (treasure.tile == null) {
                chara.variables.Remove("loot_target");
                chara.currentBehaviour = "deciding";
                return;
            }

            if (treasure.tile == chara.currentTile && chara.isMoving == false) {
                // We have arrived, grab everything and exit behaviour
                Debug.Log(chara.name + " has arrived to loot at x: " + treasure.tile.x + " y:" + treasure.tile.y);
                while (chara.currentTile.treasure.Count > 0) {  // TODO: Also check for character's carrying capacity
                    Treasure topOfPile = chara.currentTile.treasure[0]; 
                    TacticalController.instance.map.RemoveTreasure(topOfPile);
                    chara.treasure.Add(topOfPile);
                }
                chara.variables.Remove("loot_target");
                chara.currentBehaviour = "deciding";
                return;
            }
            // We have a target but are not there yet. Pathfind!
            // TODO: Replace this pathfinding with a centralised A* model
            TacticalController.BasicPathfindToCoordinates(chara, treasure.tile.x, treasure.tile.y);
        } else {
            object weightByTreasureObj;
            if (chara.variables.TryGetValue("loot_weightByTreasure", out weightByTreasureObj) == false) {
                Debug.LogError(chara.name + " is trying to loot, but no weightByTreasure has been defined");
                chara.currentBehaviour = "deciding";
                return;
            }

            Dictionary<Treasure, int> weightByTreasure = (Dictionary<Treasure, int>) weightByTreasureObj;
            if (weightByTreasure.Count <= 0) {
                Debug.LogError(chara.name + " is trying to loot, but weightByTreasure is empty");
                chara.currentBehaviour = "deciding";
                return;
            }

            List<int> weights = new List<int>();
            Dictionary<int, Treasure> indices = new Dictionary<int, Treasure>();
            int index = 0;
            foreach (Treasure treas in weightByTreasure.Keys) {
                weights.Add(weightByTreasure[treas]);
                indices.Add(index, treas);
                index++;
            }

            Treasure toSeek = indices[TacticalController.MakeDecision(weights)];
            chara.variables.Add("loot_target", toSeek);
            Debug.Log(chara.name + " is heading to loot at x: " + toSeek.tile.x + " y:" + toSeek.tile.y);
        }
    }
}