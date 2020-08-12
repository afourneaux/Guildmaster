using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This controller contains Tactical layer logic
public class TacticalController : MonoBehaviour
{
    public static TacticalController instance { get; protected set; }
    public Map map
    {
        get;
        protected set;
    }

    // OnEnable runs before Start, so this ensures this controller initialises before others
    void OnEnable()
    {
        if (instance != null)
        {
            Debug.LogError("TacticalController has been initialised twice!");
        }
        instance = this;
        map = new Map(10, 10);  // TODO: Feed in some data structure to generate the map from JSON

        // Generate some sample characters with sample data (TODO: This data should come from the strategic layer)
        Character chara1 = new Character("Crimble Nottsworth", map.GetTileAt(0, 0), 1);
        Character chara2 = new Character("Zachary Nottingham", map.GetTileAt(0, map.height / 2), 1);
        Character chara3 = new Character("Dwayne \"The Rock\" Johnson", map.GetTileAt(0, map.height - 1), 1);
        chara1.SetStats(10, 10, 10, 10, 5, 10, 10, 10);
        chara2.SetStats(20, 5, 20, 3, 5, 5, 5, 30);
        chara3.SetStats(3, 15, 3, 20, 5, 10, 10, 50);
        chara1.RegisterAIBehaviour("wander", WanderBehaviour.Wander, WanderBehaviour.WeighWander);
        chara1.RegisterAIBehaviour("rest", WanderBehaviour.Rest, WanderBehaviour.WeighRest);
        chara1.RegisterAIBehaviour("teleport", WanderBehaviour.Teleport, WanderBehaviour.WeighTeleport);
        chara1.RegisterAIBehaviour("target", CombatBehaviour.Target, CombatBehaviour.WeighTarget);
        chara1.RegisterAIBehaviour("attack", CombatBehaviour.Attack, CombatBehaviour.WeighAttack);
        chara1.RegisterAIBehaviour("reposition", CombatBehaviour.Reposition, CombatBehaviour.WeighReposition);
        chara1.RegisterAIBehaviour("loot", LootBehaviour.Loot, LootBehaviour.WeighLoot);
        chara2.RegisterAIBehaviour("wander", WanderBehaviour.Wander, WanderBehaviour.WeighWander);
        chara2.RegisterAIBehaviour("rest", WanderBehaviour.Rest, WanderBehaviour.WeighRest);
        chara2.RegisterAIBehaviour("target", CombatBehaviour.Target, CombatBehaviour.WeighTarget);
        chara2.RegisterAIBehaviour("attack", CombatBehaviour.Attack, CombatBehaviour.WeighAttack);
        chara2.RegisterAIBehaviour("reposition", CombatBehaviour.Reposition, CombatBehaviour.WeighReposition);
        chara2.RegisterAIBehaviour("loot", LootBehaviour.Loot, LootBehaviour.WeighLoot);
        chara3.RegisterAIBehaviour("wander", WanderBehaviour.Wander, WanderBehaviour.WeighWander);
        chara3.RegisterAIBehaviour("rest", WanderBehaviour.Rest, WanderBehaviour.WeighRest);
        chara3.RegisterAIBehaviour("teleport", WanderBehaviour.Teleport, WanderBehaviour.WeighTeleport);
        chara3.RegisterAIBehaviour("attack", CombatBehaviour.Attack, CombatBehaviour.WeighAttack);
        chara3.UnregisterAIBehaviour("teleport"); // Test: Only chara1 should teleport
        chara3.RegisterAIBehaviour("target", CombatBehaviour.Target, CombatBehaviour.WeighTarget);
        chara3.RegisterAIBehaviour("reposition", CombatBehaviour.Reposition, CombatBehaviour.WeighReposition);
        chara3.RegisterAIBehaviour("loot", LootBehaviour.Loot, LootBehaviour.WeighLoot);
        chara1.sprite = chara2.sprite = chara3.sprite = "knight";
        chara1.allegiance = chara2.allegiance = chara3.allegiance = 1;
        map.PlaceCharacter(chara1);
        map.PlaceCharacter(chara2);
        map.PlaceCharacter(chara3);

        // Generate enemies. This data should come from the strategic layer, placing enemies into spawn points determined
        // by the map generation file
        Character knifey = new Character("Knifey Knifesworth", map.GetTileAt(map.width - 1, map.height / 2), 2);
        knifey.SetStats(5, 20, 10, 30, 2, 2, 10, 0);
        knifey.sprite = "knifer";
        knifey.allegiance = 2;
        knifey.RegisterAIBehaviour("reposition", CombatBehaviour.Reposition, CombatBehaviour.WeighReposition);
        knifey.RegisterAIBehaviour("target", CombatBehaviour.Target, CombatBehaviour.WeighTarget);
        knifey.RegisterAIBehaviour("attack", CombatBehaviour.Attack, CombatBehaviour.WeighAttack);
        map.PlaceCharacter(knifey);

        // Generate some sample colours (Should eventually come from whatever file generates the map)
        map.allegianceColours.Add(1, Colour.GREEN);
        map.allegianceColours.Add(2, Colour.RED);

        map.PlaceTreasure(20, 1, 1);
        map.PlaceTreasure(100, map.height - 1, map.width - 3);
        map.PlaceTreasure(10, 1, 1);
    }

    float randomDelay = 2f;
    float randomCountdown = 2f;

    void Update()
    {
        // test updating tile sprites
        randomCountdown -= Time.deltaTime;
        if (false && randomCountdown <= 0)
        {
            for (int x = 0; x < map.width; x++)
            {
                for (int y = 0; y < map.height; y++)
                {
                    map.GetTileAt(x, y).sprite = Random.Range(0, 2);
                }
            }
            randomCountdown = randomDelay;
        }

        // Update each character
        foreach (Character chara in map.characters)
        {
            chara.Update(Time.deltaTime);
        }
    }

    // Given a weighted list, randomly select one. For example, in a list of 5, 5, 10
    // the probability of returning 0 (for the first option) is 25%, the second is 25%,
    // and the third is 50%.
    // This will be used by AI deciding their actions based on weighted criteria. Perhaps
    // a brave unit will have a 5% chance to run away, a 10% chance to take a potion, and
    // a 85% chance to stand and fight.
    // What decision each index corresponds to will be tracked by the caller.
    public static int MakeDecision(List<int> options)
    {
        if (options == null || options.Count <= 0)
        {
            Debug.LogError("TacticalController::MakeDecision - No options provided!");
            return -1;
        }
        int total = 0;
        foreach (int weight in options)
        {
            total += weight;
        }
        int selection = Random.Range(1, total + 1);
        int returnIndex = 0;
        foreach (int weight in options)
        {
            selection -= weight;
            if (selection <= 0)
            {
                return returnIndex;
            }
            returnIndex++;
        }

        // Something went horribly wrong!
        Debug.LogError("TacticalController::MakeDecision - Selection is somehow greater than the sum of the weights in Options on the second pass");
        return -1;
    }

    // SUPER BASIC PATHFINDING, replace with A* when implemented
    public static void BasicPathfindToCoordinates(Character chara, float targetX, float targetY)
    {
        if (chara.isMoving == true)
        {
            return;
        }

        int deltaX = 0;
        int deltaY = 0;
        if (targetX > chara.x)
        {
            deltaX = 1;
        }
        if (targetX < chara.x)
        {
            deltaX = -1;
        }
        if (targetY > chara.y)
        {
            deltaY = 1;
        }
        if (targetY < chara.y)
        {
            deltaY = -1;
        }
        Tile destination = TacticalController.instance.map.GetTileAt(chara.currentTile.x + deltaX, chara.currentTile.y + deltaY);

        // If the next tile is blocked, go around
        if (destination.character != null)
        {
            // If the diagonal is blocked, try an orthogonal movement
            if (deltaY != 0 && deltaX != 0)
            {
                destination = TacticalController.instance.map.GetTileAt(chara.currentTile.x, chara.currentTile.y + deltaY);
                if (destination == null || destination.character != null)
                {
                    destination = TacticalController.instance.map.GetTileAt(chara.currentTile.x + deltaX, chara.currentTile.y);
                }
            }
            // If orthogonal movement is blocked, try diagonal
            if (destination == null || destination.character != null)
            {
                if (deltaY != 0)
                {
                    destination = TacticalController.instance.map.GetTileAt(chara.currentTile.x + 1, chara.currentTile.y + deltaY);
                    if (destination == null || destination.character != null)
                    {
                        destination = TacticalController.instance.map.GetTileAt(chara.currentTile.x - 1, chara.currentTile.y + deltaY);
                    }
                }
                else if (deltaX != 0)
                {
                    destination = TacticalController.instance.map.GetTileAt(chara.currentTile.x + deltaX, chara.currentTile.y + 1);
                    if (destination == null || destination.character != null)
                    {
                        destination = TacticalController.instance.map.GetTileAt(chara.currentTile.x + deltaX, chara.currentTile.y - 1);
                    }
                }
            }
        }
        // If all movement is blocked, just wait a bit
        // TODO: After waiting enough time, find another target
        if (destination == null || destination.character != null)
        {
            return;
        }
        chara.BeginMove(destination);
    }
}
