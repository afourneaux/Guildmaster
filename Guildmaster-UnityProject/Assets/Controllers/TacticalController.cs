using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This controller contains Tactical layer logic
public class TacticalController : MonoBehaviour
{
    public static TacticalController instance {get; protected set; }
    public Map map {
        get;
        protected set;
    }

    // OnEnable runs before Start, so this ensures this controller initialises before others
    void OnEnable() {
        if (instance != null) {
            Debug.LogError("TacticalController has been initialised twice!");
        }
        instance = this;
        map = new Map(10, 20);  // TODO: Feed in some data structure to generate the map from JSON
        
        // Generate some sample characters with sample data
        Character chara1 = new Character("Crimble Nottsworth", map.GetTileAt(map.width / 2, map.height / 2));
        Character chara2 = new Character("Zachary Nottingham", map.GetTileAt((map.width / 2) + 2, map.height / 2));
        Character chara3 = new Character("Dwayne \"The Rock\" Johnson", map.GetTileAt(map.width / 2, (map.height / 2) + 2));
        chara1.dexterity = 10;
        chara2.dexterity = 5;
        chara3.dexterity = 20;
        chara1.RegisterAIBehaviour("wander", AIBehaviour.AI_Wander, AIBehaviour.AI_WeighWander);
        chara1.RegisterAIBehaviour("rest", AIBehaviour.AI_Rest, AIBehaviour.AI_WeighRest);
        chara1.RegisterAIBehaviour("teleport", AIBehaviour.AI_Teleport, AIBehaviour.AI_WeighTeleport);
        chara2.RegisterAIBehaviour("wander", AIBehaviour.AI_Wander, AIBehaviour.AI_WeighWander);
        chara2.RegisterAIBehaviour("rest", AIBehaviour.AI_Rest, AIBehaviour.AI_WeighRest);
        chara3.RegisterAIBehaviour("wander", AIBehaviour.AI_Wander, AIBehaviour.AI_WeighWander);
        chara3.RegisterAIBehaviour("rest", AIBehaviour.AI_Rest, AIBehaviour.AI_WeighRest);
        chara3.RegisterAIBehaviour("teleport", AIBehaviour.AI_Teleport, AIBehaviour.AI_WeighTeleport);
        chara3.UnregisterAIBehaviour("teleport"); // Test: Only chara1 should teleport
        map.PlaceCharacter(chara1);
        map.PlaceCharacter(chara2);
        map.PlaceCharacter(chara3);
    }

    float randomDelay = 2f;
    float randomCountdown = 2f;

    void Update() {
        // test updating tile sprites
        randomCountdown -= Time.deltaTime;
        if (false && randomCountdown <= 0) {
            for (int x = 0; x < map.width; x++) {
                for (int y = 0; y < map.height; y++) {
                    map.GetTileAt(x, y).sprite = Random.Range(0, 2);
                }
            }
            randomCountdown = randomDelay;
        }

        foreach(Character chara in map.characters) {
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
    public static int MakeDecision(List<int> options) {
        if (options == null || options.Count <= 0) {
            Debug.LogError("TacticalController::MakeDecision - No options provided!");
            return -1;
        }
        int total = 0;
        foreach (int weight in options) {
            total += weight;
        }
        int selection = Random.Range(0, total + 1);
        int returnIndex = 0;
        foreach (int weight in options) {
            selection -= weight;
            if (selection <= 0) {
                return returnIndex;
            }
            returnIndex++;
        }

        // Selection is somehow greater than the sum of the weights in Options on the second pass
        Debug.LogError("Something went horribly wrong!");
        return -1;
    }
}
