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

public enum BehaviourState {
    EXPLORING,
    RESTING,
    COMBAT,
    FLEEING,
    SOCIAL
}

public enum HealthState {
    CONCSCIOUS,
    STABLE,
    DYING,
    DEAD
}

public class Character {
    // Tracks variables and their values from components
    public Dictionary<string, object> variables;
    // Tracks functions to run on Update() for each component
    Action<Character, float> onUpdate;

    // AI logic
    // Tracks the many AI actions that can be taken
    public Dictionary<string, Action<Character, float>> AIBehaviours;
    // Store the results of each Weigh function
    public Dictionary<string, int> AIWeights;
    // All functions that weigh the probability of selecting a given AI option
    public Dictionary<string, Action<Character>> AIWeighOptions;
    // The AI behaviour which is currently active
    public string currentBehaviour;
    // The general attitude of the character
    private BehaviourState _behaviourState;
    public BehaviourState behaviourState {
        get {
            return _behaviourState;
        }
        set {
            if (_behaviourState != value) {
                _behaviourState = value;
                timeSinceLastBehaviourChange = 0;
                Debug.Log(name + " changing to behaviour - " + _behaviourState.ToString());
            }
        }
    }
    public float timeSinceLastBehaviourChange;

    public Dictionary<Character, float> noticedCharacters;
    public List<Treasure> noticedTreasure;
    public List<Character> noticedBy;

    // These base stats will be shared by all Characters, so they are not in a component.
    // TODO: Should stats be saved some other way? A data structure perchance?
    // TODO: Comment a brief mention of what each stat is used for when design is more solid
    public int strength; // damage
    public int precision; // accuracy
    public int constitution; // HP
    public int dexterity; // move speed
    public int perception; // Noticing distance
    public int intelligence; // How long before forgetting someone
    public int bravery; // How quickly a character returns from running
    public int greed;

    // TODO: Make more sophisticated
    int _HP;
    public int HP {
        get {
            return _HP;
        }
        set {
            if (_HP != value) {
                if (_HP > 0 && value <= 0) {
                    GoDown();
                }
                if (_HP <= 0 && value > 0) {
                    GetUp();
                }

                _HP = value;

                // Update the health bar
                if (TacticalController.instance.map.onCharacterGraphicChanged != null) {
                    TacticalController.instance.map.onCharacterGraphicChanged(this);
                }
            }
        }
    }

    public HealthState healthState {
        get;
        private set;
    }

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
    public bool isMoving;

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

    // All characters of the same allegiance are on the same side in a conflict
    public int allegiance;

    // TODO: Make more generic to encompass all carried gear
    public List<Treasure> treasure;

    // Sample components:
    // Exploration AI
    // Combat
    // Awareness (stealth, noticing)
    // HP and dying
    // Social encounters
    // Speech bubbles and voice acting

    public Character(string name, Tile startTile, int allegiance) {
        variables = new Dictionary<string, object>();
        currentTile = startTile;
        this.name = name;
        x = startTile.x;
        y = startTile.y;
        currentBehaviour = "deciding";
        behaviourState = BehaviourState.EXPLORING;
        this.allegiance = allegiance;

        AIBehaviours = new Dictionary<string, Action<Character, float>>();
        AIWeights = new Dictionary<string, int>();
        AIWeighOptions = new Dictionary<string, Action<Character>>();

        noticedCharacters = new Dictionary<Character, float>();
        noticedTreasure = new List<Treasure>();
        noticedBy = new List<Character>();

        treasure = new List<Treasure>();

        isMoving = false;
        healthState = HealthState.CONCSCIOUS;

        RegisterOnUpdate(UpdateMove);
        RegisterOnUpdate(UpdateNotice);
        RegisterOnUpdate(UpdateBehaviourState);
        RegisterOnUpdate(UpdateAI);
    }

    // TODO: Base on some grand data structure of character stats
    public void SetStats(int strength, int dexterity, int precision, int constitution, int perception, int intelligence, int bravery, int greed) {
        this.strength = strength;
        this.dexterity = dexterity;
        this.precision = precision;
        this.constitution = constitution;
        this.perception = perception;
        this.intelligence = intelligence;
        this.bravery = bravery;
        this.greed = greed;

        // TODO: Base on level? Some other calculation?
        this.HP = constitution;
    }

    public void Update(float deltaTime) {
        switch (healthState) {
            case HealthState.DEAD:
                // Do dead things, like lie still
                break;
            case HealthState.DYING:
                // Do dying things, like bleeding
                break;
            case HealthState.STABLE:
                // Do unconscious things, like lie still but not in a worrying way
                break;
            case HealthState.CONCSCIOUS:
                // Do alive things, like explore
                if (onUpdate != null) {
                    onUpdate(this, deltaTime);
                }
                break;
            default:
                Debug.LogError("Character " + name + " is in invalid health state: " + healthState);
                break;
        }
    }

    public void BeginMove(Tile destination) {
        if (currentTile.x == destination.x && currentTile.y == destination.y) {
            Debug.LogError("Character::BeginMove - " + name + " has been assigned to move to its current tile!");
            return;
        }

        if (Math.Abs(currentTile.x - destination.x) > 1 || Math.Abs(currentTile.y - destination.y) > 1) {
            Debug.LogError("Character::BeginMove - " + name + " has been assigned to move to a non-adjacent tile!");
            return;
        }
        if (destination.character != null) {
            Debug.LogError("Character::BeginMove - " + name + " has been assigned an occupied tile! Allowing the move");
        }
        if (variables.ContainsKey("sourceTile")) {
            Debug.LogError("Character::BeginMove - " + name + " already has a source tile!");
            return;
        }

        currentTile.character = null;
        variables.Add("sourceTile", currentTile);
        currentTile = destination;
        currentTile.character = this;
        isMoving = true;
    }

    void UpdateMove(Character chara, float deltaTime) {
        if (variables.TryGetValue("sourceTile", out object sourceObj)) {
            Tile source = (Tile) sourceObj;
            if ((source == currentTile) || (x == currentTile.x && y == currentTile.y)) {
                // We have reached our destination
                variables.Remove("sourceTile");
                isMoving = false;
                return;
            }

            int movementX = currentTile.x - source.x;
            int movementY = currentTile.y - source.y;
            float tileCost = source.CostToEnterTile(currentTile);
            float newX = x + (movementX * (dexterity / 10f) * deltaTime * tileCost);
            float newY = y + (movementY * (dexterity / 10f) * deltaTime * tileCost);

            newX = Mathf.Clamp(newX, Mathf.Min(source.x, currentTile.x), Mathf.Max(source.x, currentTile.x) );
            newY = Mathf.Clamp(newY, Mathf.Min(source.y, currentTile.y), Mathf.Max(source.y, currentTile.y) );

            SetPosition(newX, newY);
        }
    }

    public void SetPosition(float newX, float newY) {
        if (x != newX || y != newY) {
            x = newX;
            y = newY;
            TacticalController.instance.map.onCharacterGraphicChanged(this);
        }
    }

    public void SetTile(Tile destination) {
        if (destination.character != null) {
            Debug.LogError("Character::SetTile - " + name + " has been assigned an occupied tile! Allowing the move");
        }

        currentTile.character = null;
        currentTile = destination;
        currentTile.character = this;
    }

    // Change the current behaviour state based on the current context
    public void UpdateBehaviourState(Character chara, float deltaTime) {
        bool enemiesNearby = false;
        bool threatsNearby = false;
        bool alliesNearby = false;
        // Take stock of surrounding characters
        foreach (Character other in noticedCharacters.Keys) {
            // TODO: Determine something like "hostile" or "neutral" relationships between allegiances on the Map level
            // For now, assume all other allegiances are hostile
            if (other.allegiance == allegiance) {
                alliesNearby = true;
            } else {
                if (other.healthState == HealthState.CONCSCIOUS) {
                    threatsNearby = true;
                }
                enemiesNearby = true;
            }
        }
        // If this character started fleeing, keep fleeing for a while
        // Speed up the flee timer if no enemies are seen
        if (behaviourState == BehaviourState.FLEEING) {
            float recoveryModifier = 1f;
            if (alliesNearby) {
                recoveryModifier += 0.5f;
            }
            if (threatsNearby) {
                recoveryModifier -= 0.5f;
            }
            timeSinceLastBehaviourChange += deltaTime * recoveryModifier;

            if (timeSinceLastBehaviourChange <= 100 / bravery) { // TODO: More sophisticated system
                behaviourState = BehaviourState.FLEEING;
                return;
            }
        }

        // If there are nearby enemies, go to Combat
        if (threatsNearby) {
            behaviourState = BehaviourState.COMBAT;
            return;
        }

        // If there are no noticed enemies, continue with Explore
        behaviourState = BehaviourState.EXPLORING;
        return;

        // If there are nearby social encounters, go to Social (not implemented)
        // If the Guildmaster has called for a rest, go to Resting (not implemented)
    }

    // Run the current AI behaviour
    public void UpdateAI(Character chara, float deltaTime) {
        // If the AI is deciding, get a new behaviour
        if (currentBehaviour == "deciding") {
            // Clear existing weights
            AIWeights = new Dictionary<string, int>();
            // Calculate the weight of each decision based on the current context
            foreach (Action<Character> weighOption in AIWeighOptions.Values) {
                weighOption(chara);
            }
            // Convert the weights into an indexed list
            List<int> options = new List<int>();
            Dictionary<int, string> optionNames = new Dictionary<int, string>();
            int index = 0;
            foreach (KeyValuePair<string, int> weight in AIWeights) {
                if (weight.Value <= 0) {
                    continue;
                }
                options.Add(weight.Value);
                optionNames.Add(index, weight.Key);
                index++;
            }
            if (index == 0) {
                // There are no registered behaviours!
                return;
            }
            int selection = TacticalController.MakeDecision(options);
            currentBehaviour = optionNames[selection];
        }

        AIBehaviours[currentBehaviour](chara, deltaTime);
    }


    // On each step, Notice will check how far away each other character is. If another character
    // is within a certain range (defined by both characters' stats and terrain conditions) and
    // line of sight is unobstructed, that other character is noticed. Characters will remember
    // other characters for a certain amount of time after they lose sight, then forget about them.
    public void UpdateNotice(Character chara, float deltaTime) {
        // Notice characters
        UpdateNoticeCharacters(chara, deltaTime);

        // Forget treasure no longer on the map
        if (chara.allegiance == 1) {    // TODO: Will non-party ever need to notice loot? For now, assume no
            UpdateNoticeTreasure(chara);
        }

        // Notice other map features...
    }

    private void UpdateNoticeCharacters(Character chara, float deltaTime) {
        foreach (Character other in TacticalController.instance.map.characters) {
            if (other == chara) {
                // Don't worry about noticing oneself
                continue;
            }
            double distance = GetDistanceToTarget(other);

            if (distance <= chara.perception) { // TODO: Sophistication
                if (chara.noticedCharacters.ContainsKey(other) == false) {
                    // Character was noticed when not noticed before
                    chara.noticedCharacters.Add(other, 0f);
                    other.noticedBy.Add(chara);
                    TacticalController.instance.map.onCharacterGraphicChanged(other);
                    //Debug.Log(chara.name + " noticed " + other.name + " - Perception: " + chara.perception + " Distance: " + distance + ".");
                } else {
                    // Character was noticed again
                    chara.noticedCharacters[other] = 0f;
                }
            } else {
                // We cannot see the other person. 
                if (chara.noticedCharacters.TryGetValue(other, out float timeSinceLost)) {
                    // We previously could see the other person
                    if (timeSinceLost >= chara.intelligence) { // TODO: More sophisticated calculation
                        chara.noticedCharacters.Remove(other);
                        other.noticedBy.Remove(chara);
                        TacticalController.instance.map.onCharacterGraphicChanged(other);
                        //Debug.Log(chara.name + " lost track of " + other.name + " - Intelligence: " + chara.intelligence + " time: " + timeSinceLost + ".");
                    } else {
                        chara.noticedCharacters[other] += deltaTime;
                    }
                }
            }
        }
    }

    private void UpdateNoticeTreasure(Character chara) {
        List<Treasure> toRemove = new List<Treasure>();
        foreach (Treasure treas in chara.noticedTreasure) {
            if (treas.tile == null) {
                toRemove.Add(treas);
            }
        }
        chara.noticedTreasure.RemoveAll((t) => toRemove.Contains(t));
        // Notice treasure
        List<Tile> tilesSeen = new List<Tile>();
        List<Tile> tilesUnseen = new List<Tile>();
        foreach (Treasure treas in TacticalController.instance.map.treasure) {
            if (chara.noticedTreasure.Contains(treas)) {
                // Character has already noticed this treasure
                continue;
            }
            if (tilesSeen.Contains(treas.tile)) {
                // We have already spotted treasure on this tile
                chara.noticedTreasure.Add(treas);
                treas.NoticeBy(chara);
                continue;
            }
            if (tilesUnseen.Contains(treas.tile)) {
                // We have already determined that we cannot see this tile
                continue;
            }

            double distance = GetDistanceToTile(treas.tile);

            if (distance <= chara.perception) {
                tilesSeen.Add(treas.tile);
                chara.noticedTreasure.Add(treas);
                treas.NoticeBy(chara);
            } else {
                tilesUnseen.Add(treas.tile);
            }
        }
    }

    // TODO: The Sqrt function is expensive, can we cheapen it? Keep an eye on performance here
    // TODO: make an IsInRange function given a desired range, returns bool. That way we can square
    //       the desired range for comparison instead of Sqrt the actual distance
    public float GetDistanceToTarget(Character other) {
        return (float) Math.Sqrt(Math.Pow(x - other.x, 2) + Math.Pow(y - other.y, 2));
    }
    public float GetDistanceToTile(Tile other) {
        return (float) Math.Sqrt(Math.Pow(x - other.x, 2) + Math.Pow(y - other.y, 2));
    }

    // HP just hit 0
    public void GoDown() {
        // TODO: Implement dying and stable
        Debug.Log(name + " is dead!");
        healthState = HealthState.DEAD;

        // Drop all treasure on the ground
        // TODO: Generate treasure with more than just gp value
        foreach(Treasure treas in treasure) {
            TacticalController.instance.map.PlaceTreasure(treas, currentTile.x, currentTile.y);
        }

        noticedCharacters.Clear();
        noticedTreasure.Clear();

        currentTile.character = null;
    }

    // HP was 0, now restored to above 0
    public void GetUp() {
        healthState = HealthState.CONCSCIOUS;
    }

    public bool RegisterAIBehaviour(string name, Action<Character, float> behaviour, Action<Character> weight) {
        if (AIBehaviours.ContainsKey(name)) {
            Debug.LogError("Character::RegisterAIBehaviour - " + name + " already has a registered behaviour \"" + name + "\"!");
            return false;
        }
        AIBehaviours.Add(name, behaviour);
        AIWeighOptions.Add(name, weight);
        return true;
    }

    public bool UnregisterAIBehaviour(string name) {
        if (AIBehaviours.ContainsKey(name) == false) {
            Debug.LogError("Character::RegisterAIBehaviour - " + name + " does not have the behaviour \"" + name + "\"!");
            return false;
        }
        AIBehaviours.Remove(name);
        AIWeighOptions.Remove(name);
        return true;
    }

    public void RegisterOnUpdate(Action<Character, float> cbOnUpdate) {
        onUpdate += cbOnUpdate;
    }

    public void UnregisterOnUpdate(Action<Character, float> cbOnUpdate) {
        onUpdate -= cbOnUpdate;
    }
}