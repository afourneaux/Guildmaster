
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

    public Character(string name, Tile startTile) {
        variables = new Dictionary<string, object>();
        components = new Dictionary<string, string[]>();
        currentTile = startTile;
        this.name = name;
        sprite = "knight"; // TODO: Set sprite name
    }

    public void Update(float deltaTime) {
        if (onUpdate != null) {
            onUpdate(this, deltaTime); // Currently unused, eventually all logic will be moved here
        }

        AI_Wander(this, deltaTime);
    }

    public void AI_Wander(Character chara, float deltaTime) {
        // Periodically move tile to tile
    }

/*  TODO: Fix up in a clean way such that we can easily unregister simply by passing a component name. Not used for now.
    public bool RegisterComponent(string componentName, Dictionary<string, object> variables, Action<Character> actions) {
        if (components.ContainsKey(componentName)) {
            Debug.LogError("Character::RegisterComponent - Component \"" + componentName + "\" already registered!");
            return false;
        }

        string[] variableNames = new string[variables.Count];
        variables.Keys.CopyTo(variableNames, 0);

        foreach (string key in variableNames) {
            if ()
        }

        return true;
    }
    */
}