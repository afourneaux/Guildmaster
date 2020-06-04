using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalController : MonoBehaviour
{
    public Sprite tileSprite; // TODO: Temporary
    Map map;

    void Start() {
        // TODO: For now, we are auto-running a tactical map on game start. In the future, we'll start the game in Strategic, and finally on in a main menu
        map = new Map(10, 10);
        for (int x = 0; x < map.width; x++) {
            for (int y = 0; y < map.height; y++) {
                GameObject tileGO = new GameObject();
                tileGO.name = "Tile_" + x + "_" + y;
                tileGO.transform.position = new Vector3(x, y, 0);

                // TODO: Move to SpriteController
                SpriteRenderer tileSR = tileGO.AddComponent<SpriteRenderer>();
                tileSR.sprite = tileSprite;

                
            }
        }
    }
}
