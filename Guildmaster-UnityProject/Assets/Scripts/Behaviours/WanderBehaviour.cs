using System;
using System.Collections.Generic;
using UnityEngine;

public class WanderBehaviour
{

    /* Characters with Wander Behaviour will wander aimlessly around the map.
     * Each behaviour comes with its own "weigh" function, which determines the probability that a character
     * will choose this behaviour, based on their current status. For example, a character with low health and
     * many potions on-hand might be very inclined to quaff a healing potion, and therefore the weigh function
     * will return a higher value.
     *
     * Every behaviour must have a "continue" clause, where the next tick is expected to be the same function,
     * and an "end" clause, which wraps up the current behaviour and prepares for the next. During the "end" clause,
     * the function MUST reset the character's currentBehaviour to "deciding", so the AI will pick a new behaviour
     *
     * Eventually, this entire class will be ported to a scripting system for modability.
     */

    // TODO: Consider making behaviours an interface with a Register(character, variables) function, for modding
    public static void Register(Character chara, bool teleport = false) {
        chara.RegisterAIBehaviour("wander", WanderBehaviour.Wander, WanderBehaviour.WeighWander);
        chara.RegisterAIBehaviour("rest", WanderBehaviour.Rest, WanderBehaviour.WeighRest);
        if (teleport) {
            chara.RegisterAIBehaviour("teleport", WanderBehaviour.Teleport, WanderBehaviour.WeighTeleport);
        }
    }


    // Weigh the probability of selecting the Wander option based on the current context
    public static void WeighWander(Character chara)
    {
        if (chara.AIWeights.ContainsKey("wander"))
        {
            // Should we allow this, and overwrite the previous value?
            // Until we have a use case, just error out
            Debug.LogError("Trying to weigh the Wander AI twice for character: " + chara.name);
            return;
        }
        int weight = 0;
        if (chara.behaviourState == BehaviourState.EXPLORING)
        {
            weight = GameController.MAX_STAT_SCORE;
        }

        chara.AIWeights.Add("wander", weight);    // TODO: Base on some personality trait
    }

    // Move a few tiles
    public static void Wander(Character chara, float deltaTime)
    {
        if (chara.variables.TryGetValue("wander_wandering", out object wanderingObj) && (bool)wanderingObj == true)
        {
            // If a wander is ongoing and we have not arrived at the new tile, there is nothing more to do
            if (chara.variables.TryGetValue("sourceTile", out object sourceObj) == false || sourceObj == null)
            {
                // End case: If we have arrived at our destination, remove the wandering flag and reset the AI
                chara.variables.Remove("wander_wandering");
                chara.currentBehaviour = "deciding";
            }
        }
        else
        {
            // First, check for available adjacent tiles to move to
            Map map = chara.currentTile.map;
            Tile checkTile;
            List<Tile> directions = new List<Tile>();
            // N
            checkTile = map.GetTileAt(chara.currentTile.x, chara.currentTile.y + 1);
            if (checkTile != null && checkTile.character == null)
            {
                directions.Add(checkTile);
            }
            // NE
            checkTile = map.GetTileAt(chara.currentTile.x + 1, chara.currentTile.y + 1);
            if (checkTile != null && checkTile.character == null)
            {
                directions.Add(checkTile);
            }
            // E
            checkTile = map.GetTileAt(chara.currentTile.x + 1, chara.currentTile.y);
            if (checkTile != null && checkTile.character == null)
            {
                directions.Add(checkTile);
            }
            // SE
            checkTile = map.GetTileAt(chara.currentTile.x + 1, chara.currentTile.y - 1);
            if (checkTile != null && checkTile.character == null)
            {
                directions.Add(checkTile);
            }
            // S
            checkTile = map.GetTileAt(chara.currentTile.x, chara.currentTile.y - 1);
            if (checkTile != null && checkTile.character == null)
            {
                directions.Add(checkTile);
            }
            // SW
            checkTile = map.GetTileAt(chara.currentTile.x - 1, chara.currentTile.y - 1);
            if (checkTile != null && checkTile.character == null)
            {
                directions.Add(checkTile);
            }
            // W
            checkTile = map.GetTileAt(chara.currentTile.x - 1, chara.currentTile.y);
            if (checkTile != null && checkTile.character == null)
            {
                directions.Add(checkTile);
            }
            // NW
            checkTile = map.GetTileAt(chara.currentTile.x - 1, chara.currentTile.y + 1);
            if (checkTile != null && checkTile.character == null)
            {
                directions.Add(checkTile);
            }
            // Then, randomly choose one
            if (directions.Count <= 0)
            {
                // If no directions are possible, bail out
                chara.currentBehaviour = "deciding";
                return;
            }
            // TODO: Weighted choice to make more realistic wandering
            int choice = UnityEngine.Random.Range(0, directions.Count);
            Tile destination = directions[choice];
            TacticalController.BasicPathfindToCoordinates(chara, destination.x, destination.y);
            chara.variables.Add("wander_wandering", true);
        }
    }

    // Weigh the probability of selecting the Rest option based on the current context
    public static void WeighRest(Character chara)
    {
        if (chara.AIWeights.ContainsKey("rest"))
        {
            // Should we allow this, and overwrite the previous value?
            // Until we have a use case, just error out
            Debug.LogError("Trying to weigh the Rest AI twice for character: " + chara.name);
            return;
        }
        int weight = 0;
        if (chara.behaviourState == BehaviourState.EXPLORING)
        {
            weight = GameController.MAX_STAT_SCORE / chara.constitution;
        }
        if (chara.behaviourState == BehaviourState.RESTING)
        {
            weight = GameController.MAX_STAT_SCORE;
        }

        chara.AIWeights.Add("rest", weight);
    }

    // Stand in place
    public static void Rest(Character chara, float deltaTime)
    {
        if (chara.variables.TryGetValue("wander_timeResting", out object timeObj))
        {
            float time = (float)timeObj;
            time -= deltaTime;
            if (time <= 0)
            {
                //Debug.Log("ACTION: " + name + " finishes their rest" + " - " + Time.time);
                chara.variables.Remove("wander_timeResting");
                chara.currentBehaviour = "deciding";
            }
            else
            {
                chara.variables["wander_timeResting"] = time;
            }
        }
        else
        {
            chara.variables.Add("wander_timeResting", 1f);  // Rest for 1 second
            //Debug.Log("ACTION: " + name + " begins resting" + " - " + Time.time);
        }
    }

    // DEBUG: Just a sample
    public static void WeighTeleport(Character chara)
    {
        if (chara.AIWeights.ContainsKey("teleport"))
        {
            // Should we allow this, and overwrite the previous value?
            // Until we have a use case, just error out
            Debug.LogError("Trying to weigh the Teleport AI twice for character: " + chara.name);
            return;
        }

        chara.AIWeights.Add("teleport", 1);
    }

    // DEBUG: Just a sample
    // Teleport to a random space on the grid, then wait for 2 seconds
    public static void Teleport(Character chara, float deltaTime)
    {
        if (chara.variables.TryGetValue("wander_teleportCooldown", out object timeObj))
        {
            float time = (float)timeObj;
            time -= deltaTime;
            if (time <= 0)
            {
                chara.variables.Remove("wander_teleportCooldown");
                chara.currentBehaviour = "deciding";
            }
            else
            {
                chara.variables["wander_teleportCooldown"] = time;
            }
        }
        else
        {
            int x;
            int y;
            Tile tile;
            do
            {
                // Terrible implementation due to highly random run time, and possibility of infinite loops
                // but it serves as a debug
                x = UnityEngine.Random.Range(0, TacticalController.instance.map.width);
                y = UnityEngine.Random.Range(0, TacticalController.instance.map.height);
                tile = TacticalController.instance.map.GetTileAt(x, y);
            } while (tile.character != null);
            chara.SetPosition(x, y);
            chara.SetTile(tile);
            chara.variables.Add("wander_teleportCooldown", 2f);  // Rest for 2 seconds
        }
    }
}