using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
