using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// This controller exists to manage the visual/object layer
// TODO: Do we want a TacticalSpriteController vs StrategicSpriteController?
public class SpriteController : MonoBehaviour
{
    Dictionary<string, Sprite> spritesMap;
    Dictionary<Tile, GameObject> tileGOMap;
    Dictionary<Character, GameObject> characterGOMap;

    void Start() {
        tileGOMap = new Dictionary<Tile, GameObject>();
        characterGOMap = new Dictionary<Character, GameObject>();
        Map map = TacticalController.instance.map;

        LoadSprites();
        map.RegisterTileGraphicChangedCallback(OnTileGraphicChanged);
        map.RegisterCharacterGraphicChangedCallback(OnCharacterGraphicChanged);
        Camera.main.transform.position = new Vector3(map.width / 2, map.height / 2, Camera.main.transform.position.z);

        // Create and draw game objects for each tile in the map
        for (int x = 0; x < map.width; x++) {
            for (int y = 0; y < map.height; y++) {
                GameObject tileGO = new GameObject();
                tileGO.name = "Tile_" + x + "_" + y;
                tileGO.transform.position = new Vector3(x, y, 0);
                tileGO.transform.SetParent(this.transform, true);
                SpriteRenderer tileSR = tileGO.AddComponent<SpriteRenderer>();
                tileSR.sortingLayerName = "TileMap";

                tileGOMap.Add(map.GetTileAt(x, y), tileGO);

                OnTileGraphicChanged(map.GetTileAt(x, y));
            }
        }

        foreach (Character chara in map.characters) {
            CreateCharacter(chara);
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
        Sprite sprite;
        if (spritesMap.TryGetValue(spriteName, out sprite) == false) {
            Debug.LogError("SpriteController::OnTileGraphicChanged - Sprite not found: " + spriteName);
        }
        tileGO.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    void OnCharacterGraphicChanged(Character chara) {
        Debug.Log("SpriteController::OnCharacterGraphicChanged");

        GameObject charaGO;
        if (characterGOMap.TryGetValue(chara, out charaGO) == false) {
            Debug.LogError("SpriteController::OnCharacterGraphicChanged - Character not found");
            return;
        }

        // Object location
        charaGO.transform.position = new Vector3(chara.x, chara.y, 0);

        Sprite sprite;
        if (spritesMap.TryGetValue(chara.sprite, out sprite) == false) {
            Debug.LogError("SpriteController::OnCharacterGraphicChanged - Sprite not found: " + chara.sprite);
        }
        charaGO.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    // This effectively replaces grabbing a prefab
    void CreateCharacter(Character chara) {
        string objectName = "Character_" + chara.name.Replace(' ', '_');
        GameObject charaGO = new GameObject();
        charaGO.name = objectName;
        charaGO.transform.SetParent(this.transform, true);
        SpriteRenderer charaSR = charaGO.AddComponent<SpriteRenderer>();
        charaSR.sortingLayerName = "Characters";
        // Create nameplate
        GameObject textGO = new GameObject();
        textGO.name = objectName + "_Text";
        textGO.transform.SetParent(charaGO.transform, false);
        TextMeshPro textMesh = textGO.AddComponent<TextMeshPro>();
        textMesh.text = chara.name;
        textMesh.fontSize = 2;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.alignment = TextAlignmentOptions.Bottom;
        textMesh.outlineWidth = 0.3f;
        textMesh.fontSharedMaterial.shaderKeywords = new string[] {"OUTLINE_ON"};
        textGO.GetComponent<MeshRenderer>().sortingLayerName = "UI";
        // Allow text to bleed halfway into other tiles
        textGO.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 1);
        characterGOMap.Add(chara, charaGO);
        OnCharacterGraphicChanged(chara);
    }
}