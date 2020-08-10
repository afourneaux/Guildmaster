using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// This controller exists to manage the visual/object layer
// TODO: Do we want a TacticalSpriteController vs StrategicSpriteController?
public enum Colour {
    CLEAR, WHITE, BLACK, GREY, RED, GREEN, BLUE, MAGENTA, YELLOW, CYAN
}

public class SpriteController : MonoBehaviour
{
    Dictionary<string, Sprite> spritesMap;
    Dictionary<Tile, GameObject> tileGOMap;
    Dictionary<Treasure, GameObject> treasureGOMap;
    Dictionary<Character, GameObject> characterGOMap;

    void Start() {
        tileGOMap = new Dictionary<Tile, GameObject>();
        treasureGOMap = new Dictionary<Treasure, GameObject>();
        characterGOMap = new Dictionary<Character, GameObject>();
        Map map = TacticalController.instance.map;

        LoadSprites();
        map.RegisterTileGraphicChangedCallback(OnTileGraphicChanged);
        map.RegisterTreasureGraphicChangedCallback(OnTreasureGraphicChanged);
        map.RegisterCharacterGraphicChangedCallback(OnCharacterGraphicChanged);
        Camera.main.transform.position = new Vector3(map.width / 2, map.height / 2, Camera.main.transform.position.z);

        // Create and draw game objects for each tile in the map
        for (int x = 0; x < map.width; x++) {
            for (int y = 0; y < map.height; y++) {
                Tile tile = map.GetTileAt(x, y);
                CreateTile(tile);
                foreach (Treasure treasure in tile.treasure) {
                    CreateTreasure(treasure);
                }
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
        GameObject tileGO;
        if (tileGOMap.TryGetValue(tile, out tileGO) == false) {
            Debug.LogError("SpriteController::OnTileGraphicChanged - Tile not found");
            return;
        }

        // TODO: Get sprite based on spritesheet and index
        string spriteName = tile.sprite == 0 ? "BlankTile" : "EmptyTile"; // TODO: Spritesheets
        Sprite sprite;
        if (spritesMap.TryGetValue(spriteName, out sprite) == false) {
            Debug.LogError("SpriteController::OnTileGraphicChanged - Sprite not found: " + spriteName);
        }
        tileGO.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    void OnTreasureGraphicChanged(Treasure treasure) {
        GameObject treasureGO;
        if (treasureGOMap.TryGetValue(treasure, out treasureGO) == false) {
            Debug.LogError("SpriteController::OnTreasureGraphicChanged - Treasure not found");
            return;
        }

        if (treasure.tile == null) {
            // The treasure is not on the ground. Do not display.
            treasureGO.GetComponent<SpriteRenderer>().enabled = false;
        } else {
            treasureGO.GetComponent<SpriteRenderer>().enabled = true;
            Sprite sprite;
            if (spritesMap.TryGetValue(treasure.sprite, out sprite) == false) {
                Debug.LogError("SpriteController::OnTreasureGraphicChanged - Sprite not found: " + treasure.sprite);
            }
            treasureGO.GetComponent<SpriteRenderer>().sprite = sprite;
        }
    }

    void OnCharacterGraphicChanged(Character chara) {
        GameObject charaGO;
        if (characterGOMap.TryGetValue(chara, out charaGO) == false) {
            Debug.LogError("SpriteController::OnCharacterGraphicChanged - Character not found");
            return;
        }

        // Object location
        charaGO.transform.position = new Vector3(chara.x, chara.y, 0);

        // TODO: Move to some "visible" property in Character, set by the HideBehaviour component
        bool isVisible = chara.allegiance == 1 || chara.noticedBy.Count > 0;
        foreach (Renderer child in charaGO.GetComponentsInChildren<Renderer>()) {
            child.enabled = isVisible;
        }

        Sprite sprite;
        if (spritesMap.TryGetValue(chara.sprite, out sprite) == false) {
            Debug.LogError("SpriteController::OnCharacterGraphicChanged - Sprite not found: " + chara.sprite);
        }
        charaGO.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    void CreateTile(Tile tile) {
        GameObject tileGO = new GameObject();
        tileGO.name = "Tile_" + tile.x + "_" + tile.y;
        tileGO.transform.position = new Vector3(tile.x, tile.y, 0);
        tileGO.transform.SetParent(this.transform, true);
        SpriteRenderer tileSR = tileGO.AddComponent<SpriteRenderer>();
        tileSR.sortingLayerName = "TileMap";

        tileGOMap.Add(tile, tileGO);

        OnTileGraphicChanged(tile);
    }

    void CreateTreasure(Treasure treasure) {
        GameObject treasureGO = new GameObject();
        treasureGO.name = "Treasure_gp_" + treasure.gp;
        treasureGO.transform.position = new Vector3(treasure.tile.x, treasure.tile.y, 0);
        treasureGO.transform.SetParent(this.transform, true);
        SpriteRenderer treasureSR = treasureGO.AddComponent<SpriteRenderer>();

        treasureGOMap.Add(treasure, treasureGO);

        OnTreasureGraphicChanged(treasure);
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

        // Colour the text based on allegiance
        Color TMPColor;
        if (chara.currentTile.map.allegianceColours.TryGetValue(chara.allegiance, out Colour colour)) {
            TMPColor = ColourToTMPColor(colour);
        } else {
            // Default to white if none found
            TMPColor = Color.white;
        }
        textMesh.color = TMPColor;

        // Allow text to bleed halfway into other tiles
        textGO.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 1);
        characterGOMap.Add(chara, charaGO);
        OnCharacterGraphicChanged(chara);
    }

    Color ColourToTMPColor(Colour colour) {
        switch (colour) {
            case Colour.CLEAR:
                return Color.clear;
            case Colour.BLACK:
                return Color.black;
            case Colour.WHITE:
                return Color.white;
            case Colour.GREY:
                return Color.grey;
            case Colour.RED:
                return Color.red;
            case Colour.GREEN:
                return Color.green;
            case Colour.BLUE:
                return Color.blue;
            case Colour.MAGENTA:
                return Color.magenta;
            case Colour.YELLOW:
                return Color.yellow;
            case Colour.CYAN:
                return Color.cyan;
            default:
                Debug.LogError("Unrecognised colour: " + colour.ToString());
                return Color.clear;
        }
    }
}