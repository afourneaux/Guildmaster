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
    public Dictionary<string, object> variables;
    // Tracks functions to run on Update() for each component
    Action<Character, float> onUpdate;

    // These base stats will be shared by all Characters, so they are not in a component.
    // TODO: Should stats be saved some other way? A data structure perchance?
    // TODO: Comment a brief mention of what each stat is used for when design is more solid
    int strength;
    int precision;
    int constitution;
    public int dexterity;
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

                if (TacticalController.instance.map.onCharacterGraphicChanged != null) {
                    TacticalController.instance.map.onCharacterGraphicChanged(this);
                }
            }
        }
    }

    public float x {
        get;
        protected set;
    }
    public float y {
        get;
        protected set;
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
        currentTile = startTile;
        this.name = name;
        sprite = "knight"; // TODO: Set sprite name
        x = startTile.x;
        y = startTile.y;

        onUpdate += Move;

        // DEBUG: Add basic AI behaviour
        AIBehaviour AICore = new AIBehaviour();
        onUpdate += AICore.AI_Core;
    }

    public void Update(float deltaTime) {
        if (onUpdate != null) {
            onUpdate(this, deltaTime);
        }
    }

    public void BeginMove(Tile destination) {
        destination.character = this;
        variables.Add("sourceTile", currentTile);
        currentTile.character = null;
        currentTile = destination;
    }

    void Move(Character chara, float deltaTime) {
        if (chara.variables.TryGetValue("sourceTile", out object sourceObj)) {
            Tile source = (Tile) sourceObj;
            if ((source == chara.currentTile) || (chara.x == chara.currentTile.x && chara.y == chara.currentTile.y)) {
                // We have reached our destination
                chara.variables.Remove("sourceTile");
                return;
            }

            // Check if the destination is orthogonal or diagonal to the source
            int movementX = chara.currentTile.x - source.x;
            int movementY = chara.currentTile.y - source.y;
            bool isOrthogonal = Math.Abs(movementX) + Math.Abs(movementY) == 1;
            float speedModifier = 1;
            if (isOrthogonal == false) {
                // Move more slowly on diagonals
                speedModifier = 0.71f;
            }
            float newX = chara.x;
            float newY = chara.y;
            newX += movementX * speedModifier * (chara.dexterity / 10f) * deltaTime * source.costToLeave * currentTile.costToEnter;
            newY += movementY * speedModifier * (chara.dexterity / 10f) * deltaTime * source.costToLeave * currentTile.costToEnter;

            newX = Mathf.Clamp(newX, Mathf.Min(source.x, chara.currentTile.x), Mathf.Max(source.x, chara.currentTile.x) );
            newY = Mathf.Clamp(newY, Mathf.Min(source.y, chara.currentTile.y), Mathf.Max(source.y, chara.currentTile.y) );

            chara.UpdatePosition(newX, newY);
        }
    }

    public void UpdatePosition(float newX, float newY) {
        if (x != newX || y != newY) {
            x = newX;
            y = newY;
            TacticalController.instance.map.onCharacterGraphicChanged(this);
        }
    }
}