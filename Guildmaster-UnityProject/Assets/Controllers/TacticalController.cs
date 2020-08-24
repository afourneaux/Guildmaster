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
        Character chara3 = new Character("Duane \"The Jock\" Ronson", map.GetTileAt(0, map.height - 1), 1);
        chara1.SetStats(10, 10, 10, 10, 10, 10, 10, 100);
        chara2.SetStats(50, 5, 100, 30, 15, 5, 5, 30);
        chara3.SetStats(30, 15, 30, 20, 5, 10, 10, 50);
        WanderBehaviour.Register(chara1);
        WanderBehaviour.Register(chara2);
        WanderBehaviour.Register(chara3, true);
        chara3.UnregisterAIBehaviour("teleport"); // Test removing a registered behaviour
        LootBehaviour.Register(chara1);
        LootBehaviour.Register(chara2);
        LootBehaviour.Register(chara3);
        CombatBehaviour.Register(chara1);
        CombatBehaviour.Register(chara2);
        CombatBehaviour.Register(chara3);
        chara1.sprite = chara2.sprite = chara3.sprite = "knight";
        chara1.allegiance = chara2.allegiance = chara3.allegiance = 1;
        map.PlaceCharacter(chara1);
        map.PlaceCharacter(chara2);
        map.PlaceCharacter(chara3);

        // Generate enemies. This data should come from the strategic layer, placing enemies into spawn points determined
        // by the map generation file
        Character knifey = new Character("Knifey Knifesworth", map.GetTileAt(map.width - 1, map.height / 2), 2);
        knifey.SetStats(10, 20, 10, 100, 2, 2, 10, 0);
        knifey.sprite = "knifer";
        knifey.allegiance = 2;
        CombatBehaviour.Register(knifey);
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


    bool TEMP_gameover = false;
    void Update()
    {
        // test updating tile sprites
        DebugTileFlashing();

        // Update each character
        foreach (Character chara in map.characters)
        {
            chara.Update(Time.deltaTime);
        }

        // Check mission success/failure (TODO: Optimise)
        if (TEMP_gameover == false)
        {
            if (isMissionSuccess())
            {
                Debug.Log("Mission success!");
                TEMP_gameover = true;
            }
            if (isMissionFailure())
            {
                Debug.Log("Mission failed!");
                TEMP_gameover = true;
            }
        }
    }

    void DebugTileFlashing()
    {
        randomCountdown -= Time.deltaTime;
        if (randomCountdown <= 0)
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

    // TODO: Make a dynamic mission success determiner
    // For now, assume all missions require one side to be unconscious
    private bool isMissionSuccess()
    {
        foreach (Character chara in map.charactersByAllegiance[2])
        {    // Assume allegiance 2 are the baddies for now
            if (chara.healthState == HealthState.CONCSCIOUS)
            {
                return false;
            }
        }

        return true;
    }

    // TODO: Make a dynamic mission success determiner
    // For now, assume all missions require good guys to survive
    private bool isMissionFailure()
    {
        foreach (Character chara in map.charactersByAllegiance[1])
        {    // Assume allegiance 1 are the goodies for now
            if (chara.healthState == HealthState.CONCSCIOUS)
            {
                return false;
            }
        }

        return true;
    }
}
