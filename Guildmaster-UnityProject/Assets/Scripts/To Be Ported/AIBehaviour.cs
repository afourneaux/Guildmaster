using System;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviour {
    // Tracks the many AI actions that can be taken
    Dictionary<string, Action<Character, float>> AI_Behaviours;
    // All functions that weigh the probability of selecting a given AI option
    Action<Character> AI_WeighOptions;
    // Store the results of each Weigh function
    Dictionary<string, int> AI_Weights;

    public void AI_Configure(Character chara) {
        // Register callbacks and set default values
        AI_WeighOptions += AI_ClearWeights;
        AI_WeighOptions += AI_WeighWander;
        AI_WeighOptions += AI_WeighRest;

        AI_Behaviours = new Dictionary<string, Action<Character, float>>();
        AI_Behaviours.Add("wander", AI_Wander);
        AI_Behaviours.Add("rest", AI_Rest);

        chara.variables.Add("AI_ConfigurationSet", true);
    }

    public void AI_Core(Character chara, float deltaTime) {
        // Choose an action and perform it
        if (chara.variables.TryGetValue("AI_ConfigurationSet", out object configuredObj) == false || (bool) configuredObj == false) {
            AI_Configure(chara);
        }

        // If the AI is already running something which has not yet completed, continue it
        if (chara.variables.TryGetValue("AI_runningFunction", out object functionObj)) {
            Action<Character, float> runningFunction = (Action<Character, float>) functionObj;
            runningFunction(chara, deltaTime);
        } else {
            AI_WeighOptions(chara);
            List<int> options = new List<int>();
            Dictionary<int, string> optionNames = new Dictionary<int, string>();
            int index = 0;
            foreach (KeyValuePair<string, int> weight in AI_Weights) {
                options.Add(weight.Value);
                optionNames.Add(index, weight.Key);
                index++;
            }
            int selected = TacticalController.MakeDecision(options);
            Action<Character, float> optionToRun = AI_Behaviours[optionNames[selected]];
            optionToRun(chara, deltaTime);
        }
    }

    public void AI_ClearWeights(Character chara) {
        AI_Weights = new Dictionary<string, int>();
    }


    // Weigh the probability of selecting the Wander option based on the current context
    public void AI_WeighWander(Character chara) {
        if (AI_Weights.ContainsKey("wander")) {
            // Should we allow this, and overwrite the previous value?
            // Until we have a use case, just error out
            Debug.LogError("Trying to weigh the Wander AI twice for character: " + chara.name);
            return;
        }

        AI_Weights.Add("wander", 8);    // TODO: Base on some personality trait
    }

    // Move a few tiles
    public void AI_Wander(Character chara, float deltaTime) {
        if (chara.variables.TryGetValue("AI_wandering", out object wanderingObj) && (bool) wanderingObj == true) {
            // If a wander is ongoing, continue it
            if (chara.variables.TryGetValue("sourceTile", out object sourceObj) == false || sourceObj == null) {
                chara.variables.Remove("AI_wandering");
                chara.variables.Remove("AI_runningFunction");
            }
        } else {
            // First, check for available adjacent tiles to move to
            Map map = chara.currentTile.map;
            Tile checkTile;
            List<Tile> directions = new List<Tile>();
            // N
            checkTile = map.GetTileAt(chara.currentTile.x, chara.currentTile.y + 1);
            if (checkTile != null && checkTile.character == null) {
                directions.Add(checkTile);
            }
            // NE
            checkTile = map.GetTileAt(chara.currentTile.x + 1, chara.currentTile.y + 1);
            if (checkTile != null && checkTile.character == null) {
                directions.Add(checkTile);
            }
            // E
            checkTile = map.GetTileAt(chara.currentTile.x + 1, chara.currentTile.y);
            if (checkTile != null && checkTile.character == null) {
                directions.Add(checkTile);
            }
            // SE
            checkTile = map.GetTileAt(chara.currentTile.x + 1, chara.currentTile.y - 1);
            if (checkTile != null && checkTile.character == null) {
                directions.Add(checkTile);
            }
            // S
            checkTile = map.GetTileAt(chara.currentTile.x, chara.currentTile.y - 1);
            if (checkTile != null && checkTile.character == null) {
                directions.Add(checkTile);
            }
            // SW
            checkTile = map.GetTileAt(chara.currentTile.x - 1, chara.currentTile.y - 1);
            if (checkTile != null && checkTile.character == null) {
                directions.Add(checkTile);
            }
            // W
            checkTile = map.GetTileAt(chara.currentTile.x - 1, chara.currentTile.y);
            if (checkTile != null && checkTile.character == null) {
                directions.Add(checkTile);
            }
            // NW
            checkTile = map.GetTileAt(chara.currentTile.x - 1, chara.currentTile.y + 1);
            if (checkTile != null && checkTile.character == null) {
                directions.Add(checkTile);
            }
            // Then, randomly choose one
            if (directions.Count <= 0) {
                // If no directions are possible, bail out
                return;
            }
            int choice = UnityEngine.Random.Range(0, directions.Count);
            Tile destination = directions[choice];
            chara.BeginMove(destination);
            chara.variables.Add("AI_wandering", true);
            chara.variables.Add("AI_runningFunction", (Action<Character, float>) AI_Wander);
        }

        /*
        if (variables.TryGetValue("AI_timeWandering", out object timeObj)) {
            float time = (float) timeObj;
            time -= deltaTime;
            if (time <= 0) {
                Debug.Log("ACTION: " + name + " finishes their wander" + " - " + Time.time);
                variables.Remove("AI_timeWandering");
                variables.Remove("AI_runningFunction");
            } else {
                variables["AI_timeWandering"] = time;
            }
        } else {
            variables.Add("AI_timeWandering", 3 - deltaTime);  // Wander for 3 seconds
            variables.Add("AI_runningFunction", (Action<Character, float>) AI_Wander);
            Debug.Log("ACTION: " + name + " begins wandering" + " - " + Time.time);
        }*/
    }

    // Weigh the probability of selecting the Rest option based on the current context
    public void AI_WeighRest(Character chara) {
        if (AI_Weights.ContainsKey("rest")) {
            // Should we allow this, and overwrite the previous value?
            // Until we have a use case, just error out
            Debug.LogError("Trying to weigh the Rest AI twice for character: " + chara.name);
            return;
        }

        AI_Weights.Add("rest", 20);    // TODO: Base on some personality trait
    }

    // Stand in place
    public void AI_Rest(Character chara, float deltaTime) {
        if (chara.variables.TryGetValue("AI_timeResting", out object timeObj)) {
            float time = (float) timeObj;
            time -= deltaTime;
            if (time <= 0) {
                //Debug.Log("ACTION: " + name + " finishes their rest" + " - " + Time.time);
                chara.variables.Remove("AI_timeResting");
                chara.variables.Remove("AI_runningFunction");
            } else {
                chara.variables["AI_timeResting"] = time;
            }
        } else {
            chara.variables.Add("AI_timeResting", 1 - deltaTime);  // Rest for 1 second
            chara.variables.Add("AI_runningFunction", (Action<Character, float>) AI_Rest);
            //Debug.Log("ACTION: " + name + " begins resting" + " - " + Time.time);
        }
    }
}