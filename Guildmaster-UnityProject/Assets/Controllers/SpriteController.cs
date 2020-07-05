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
    Dictionary<Character, GameObject> characterGOMap;

    Vector2 HP_POS = new Vector2(0, 0.5f);
    Vector2 HP_SIZE = new Vector2(1f, 0.1f);

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

        Sprite CharacterSprite;
        if (spritesMap.TryGetValue(chara.sprite, out CharacterSprite) == false) {
            Debug.LogError("SpriteController::OnCharacterGraphicChanged - Sprite not found: " + chara.sprite);
        }
        GameObject spriteGO = charaGO.transform.Find(charaGO.name + "_Sprite").gameObject;
        spriteGO.GetComponent<SpriteRenderer>().sprite = CharacterSprite;

        if (chara.HP > 0) {
            Sprite hpGreenSprite;
            Sprite hpRedSprite;
            if (spritesMap.TryGetValue("hp_green", out hpGreenSprite) == false) {
                Debug.LogError("SpriteController::OnCharacterGraphicChanged - Sprite not found: hp_green");
            }
            if (spritesMap.TryGetValue("hp_red", out hpRedSprite) == false) {
                Debug.LogError("SpriteController::OnCharacterGraphicChanged - Sprite not found: hp_red");
            }
            GameObject hpGO = charaGO.transform.Find(charaGO.name + "_HP").gameObject;
            GameObject hpGreenGO = hpGO.transform.Find(charaGO.name + "_HP_Green").gameObject;
            hpGreenGO.GetComponent<SpriteRenderer>().sprite = hpGreenSprite;
            hpGreenGO.transform.localScale = new Vector3(HP_SIZE.x * ((float)chara.HP / (float)chara.constitution), HP_SIZE.y, 0);
            GameObject hpRedGO = hpGO.transform.Find(charaGO.name + "_HP_Red").gameObject;
            hpRedGO.GetComponent<SpriteRenderer>().sprite = hpRedSprite;
        }
    }

    // This effectively replaces grabbing a prefab
    void CreateCharacter(Character chara) {
        // Create an empty container object
        string objectName = "Character_" + chara.name.Replace(' ', '_');
        GameObject charaGO = new GameObject();
        charaGO.name = objectName;
        charaGO.transform.SetParent(this.transform, true);
        // Create character sprite
        GameObject spriteGO = new GameObject(objectName + "_Sprite");
        spriteGO.transform.SetParent(charaGO.transform, false);
        SpriteRenderer charaSR = spriteGO.AddComponent<SpriteRenderer>();
        charaSR.sortingLayerName = "Characters";
        // Create nameplate
        GameObject textGO = new GameObject(objectName + "_Text");
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

        Texture2D greenBar = Resources.Load<Texture2D>("hp_green");
        Texture2D redBar = Resources.Load<Texture2D>("hp_red");
        // Create health bar
        if (chara.HP != 0) {
            GameObject hpGO = new GameObject(objectName + "_HP");
            hpGO.transform.SetParent(charaGO.transform, false);
            GameObject hpGreenGO = new GameObject(objectName + "_HP_Green");
            hpGreenGO.transform.SetParent(hpGO.transform, false);
            hpGreenGO.transform.localPosition = new Vector3(-0.5f, HP_POS.y, 0);
            hpGreenGO.transform.localScale = new Vector3(HP_SIZE.x, HP_SIZE.y, 0);
            SpriteRenderer hpGreenSR = hpGreenGO.AddComponent<SpriteRenderer>();
            hpGreenSR.sortingLayerName = "UI";
            hpGreenSR.sortingOrder = 1;
            RectTransform hpGreenRT = hpGreenGO.AddComponent<RectTransform>();
            hpGreenRT.pivot = new Vector2(0, 0.5f); // Anchor left
            GameObject hpRedGO = new GameObject(objectName + "_HP_Red");
            hpRedGO.transform.SetParent(hpGO.transform, false);
            hpRedGO.transform.localPosition = new Vector3(HP_POS.x, HP_POS.y, 0);
            hpRedGO.transform.localScale = new Vector3(HP_SIZE.x, HP_SIZE.y, 0);
            SpriteRenderer hpRedSR = hpRedGO.AddComponent<SpriteRenderer>();
            hpRedSR.sortingLayerName = "UI";
            hpRedSR.sortingOrder = 0;
        }

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