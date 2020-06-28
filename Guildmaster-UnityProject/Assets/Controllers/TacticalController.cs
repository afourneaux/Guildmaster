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
        
        // Generate a sample character
        Character chara1 = new Character("Crimble Nottsworth", map.GetTileAt(map.width / 2, map.height / 2));
        Character chara2 = new Character("Zachary Nottingham", map.GetTileAt((map.width / 2) + 2, map.height / 2));
        Character chara3 = new Character("Dwayne \"The Rock\" Johnson", map.GetTileAt(map.width / 2, (map.height / 2) + 2));
        map.characters.Add(chara1);
        map.characters.Add(chara2);
        map.characters.Add(chara3);
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
    }
}
