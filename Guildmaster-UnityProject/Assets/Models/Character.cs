
using System;
using System.Collections.Generic;
using UnityEngine;

// COMPONENT BASED PROGRAMMING CRASH COURSE:
// Classes like this shall serve as base versions of Characters, ie "dudes on a map".
// This class will be shared by allied adventurers, enemies, monsters of all kinds,
// even possibly neutral NPCs. To become more specific, we will NOT use subclassing.
// The Game Design Way (tm) is to use Components, so essentially we will have parameters
// akin to a Dictionary<string, object> and an Action<Character> set to dynamically assign
// different variables and behaviours to each Character. Our AI for example will be a component,
// comprised of variables and functions which will be tagged onto characters that require it.

// As a super cool bonus, this also makes it really easy to implement modding, since we can
// parse more components from text files. That's a stretch goal though

public class Character {
    // Tracks variables and their values from components
    Dictionary<string, object> variables;
    // Tracks which variable belongs to which component
    Dictionary<string, string[]> components;
    // Tracks functions to run on Update() for each component
    Action<Character, float> onUpdate;

    // These base stats will be shared by all Characters, so they are not in a component.
    // TODO: Should stats be saved some other way? A data structure perchance?
    // TODO: Comment a brief mention of what each stat is used for when design is more solid
    int strength;
    int precision;
    int constitution;
    int dexterity;
    public string name {
        get;
        protected set;
    }
    private string _sprite;
    public string sprite {
        get {
            return _sprite;
        }
        set {
            if (_sprite != value) {
                _sprite = value;

                // TODO: Callback to change graphic when sprite's value changes
                /*if (map.onTileGraphicChanged != null) {
                    map.onTileGraphicChanged(this);
                }*/
            }
        }
    }
    public Tile currentTile {
        get;
        protected set;
    }

    // Sample components:
    // Exploration AI
    // Combat
    // Awareness (stealth, noticing)
    // HP and dying
    // Social encounters
    // Speech

    public Character(string name, Tile startTile) {
        variables = new Dictionary<string, object>();
        components = new Dictionary<string, string[]>();
        currentTile = startTile;
        this.name = name;
        sprite = "knight"; // TODO: Set sprite name

        // DEBUG: Add basic AI behaviour
        onUpdate += AI_Core;
    }

    public void Update(float deltaTime) {
        if (onUpdate != null) {
            onUpdate(this, deltaTime);
        }
    }

    // TODO: Move everything beginning with AI_ to an AI component

    // Tracks the many AI actions that can be taken
    Dictionary<string, Action<Character, float>> AI_Behaviours;
    // All functions that weigh the probability of selecting a given AI option
    Action<Character> AI_WeighOptions;
    // Store the results of each Weigh function
    Dictionary<string, int> AI_Weights;

    public void AI_Configure() {
        // Register callbacks and set default values
        AI_WeighOptions += AI_ClearWeights;
        AI_WeighOptions += AI_WeighWander;
        AI_WeighOptions += AI_WeighRest;

        AI_Behaviours = new Dictionary<string, Action<Character, float>>();
        AI_Behaviours.Add("wander", AI_Wander);
        AI_Behaviours.Add("rest", AI_Rest);

        variables.Add("AI_ConfigurationSet", true);
    }

    public void AI_Core(Character chara, float deltaTime) {
        // Choose an action and perform it
        if (variables.TryGetValue("AI_ConfigurationSet", out object configuredObj) == false || (bool) configuredObj == false) {
            AI_Configure();
        }

        // If the AI is already running something which has not yet completed, continue it
        if (variables.TryGetValue("AI_runningFunction", out object functionObj)) {
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
            Debug.LogError("Trying to weigh the Wander AI twice for character: " + name);
            return;
        }

        AI_Weights.Add("wander", 2);    // TODO: Base on some personality trait
    }

    // Move a few tiles
    public void AI_Wander(Character chara, float deltaTime) {
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
        }
    }

    // Weigh the probability of selecting the Rest option based on the current context
    public void AI_WeighRest(Character chara) {
        if (AI_Weights.ContainsKey("rest")) {
            // Should we allow this, and overwrite the previous value?
            // Until we have a use case, just error out
            Debug.LogError("Trying to weigh the Rest AI twice for character: " + name);
            return;
        }

        AI_Weights.Add("rest", 8);    // TODO: Base on some personality trait
    }

    // Stand in place
    public void AI_Rest(Character character, float deltaTime) {
        if (variables.TryGetValue("AI_timeResting", out object timeObj)) {
            float time = (float) timeObj;
            time -= deltaTime;
            if (time <= 0) {
                Debug.Log("ACTION: " + name + " finishes their rest" + " - " + Time.time);
                variables.Remove("AI_timeResting");
                variables.Remove("AI_runningFunction");
            } else {
                variables["AI_timeResting"] = time;
            }
        } else {
            variables.Add("AI_timeResting", 5 - deltaTime);  // Rest for 5 seconds
            variables.Add("AI_runningFunction", (Action<Character, float>) AI_Rest);
            Debug.Log("ACTION: " + name + " begins resting" + " - " + Time.time);
        }
    }
}