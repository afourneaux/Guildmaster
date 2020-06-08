using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This controller exists to manage the visual/object layer
// TODO: Do we want a TacticalSpriteController vs StrategicSpriteController?
public class SpriteController : MonoBehaviour
{
    Dictionary<string, Sprite> spritesMap;
    Dictionary<Tile, GameObject> tileGOMap;

    void Start() {
        tileGOMap = new Dictionary<Tile, GameObject>();
        Map map = TacticalController.instance.map;

        LoadSprites();
        map.RegisterTileGraphicChangedCallback(OnTileGraphicChanged);
        Camera.main.transform.position = new Vector3(map.width / 2, map.height / 2, Camera.main.transform.position.z);

        // Create and draw game objects for each tile in the map
        for (int x = 0; x < map.width; x++) {
            for (int y = 0; y < map.height; y++) {
                GameObject tileGO = new GameObject();
                tileGO.name = "Tile_" + x + "_" + y;
                tileGO.transform.position = new Vector3(x, y, 0);
                tileGO.transform.SetParent(this.transform, true);
                tileGO.AddComponent<SpriteRenderer>();

                tileGOMap.Add(map.GetTileAt(x, y), tileGO);

                OnTileGraphicChanged(map.GetTileAt(x, y));
            }
        }
    }

    void LoadSprites() {
        spritesMap = new Dictionary<string, Sprite>();
        Sprite[] allSprites = Resources.LoadAll<Sprite>("/"); // TODO: Break this into categories
        Debug.Log("Loading Sprites...");
        foreach (Sprite sprite in allSprites) {
            Debug.Log("Loaded sprite: " + sprite.name);
            spritesMap.Add(sprite.name, sprite);
        }
    }

    void OnTileGraphicChanged(Tile tile) {
        Debug.Log("SpriteController::OnTileGraphicChanged");

        GameObject tileGO;
        if (tileGOMap.TryGetValue(tile, out tileGO) == false) {
            Debug.LogError("SpriteController::OnTileGraphicChanged - Tile not found");
            return;
        }

        // TODO: Get sprite based on spritesheet and index
        string spriteName = tile.sprite == 0 ? "BlankTile" : "EmptyTile"; // test
        Sprite tileSprite;
        if (spritesMap.TryGetValue(spriteName, out tileSprite) == false) {
            Debug.LogError("SpriteController::OnTileGraphicChanged - Sprite not found: " + spriteName);
        }
        tileGO.GetComponent<SpriteRenderer>().sprite = tileSprite;
    }
}