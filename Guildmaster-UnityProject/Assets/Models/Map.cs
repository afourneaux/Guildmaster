using System;
using System.Collections.Generic;

public class Map {
    public List<Character> characters;  // The index of this list is used for logic, DO NOT REMOVE FROM THIS LIST (TODO: Convert to Dictionary<int, Character> so we can preserve indices on remove)
    public Dictionary<int, List<Character>> charactersByAllegiance;
    public Dictionary<int, Colour> allegianceColours;
    public List<Treasure> treasure;

    public Action<Tile> onTileGraphicChanged {
        get; 
        protected set;
    }
    public Action<Treasure> onTreasureGraphicChanged {
        get; 
        protected set;
    }
    public Action<Character> onCharacterGraphicChanged {
        get; 
        protected set;
    }
    public int width {
        get; 
        protected set;
    }
    public int height {
        get; 
        protected set;
    }
    // TODO: Determines which spritesheet to use. This will map to a file by name of something like "Resources/Visuals/Spritesheet_{spritesheet}"
    public string spritesheet {
        get; 
        protected set;
    }
    Tile[,] tiles;

    // TODO: import a string or other datastructure defining the map's layout
    //      Ex: 3x3 map might take the string "111101111" for a ring of walls (1) surrounding a single floor (0)
    //      Remember to validate that string length = width x length and throw otherwise
    // TODO: In this case, make a map of some hard-coded early values to definite tile types. If 0 = floor and 1 = wall, we'll need
    //      a few such hard-mapped types so we can properly calculate movement costs. Maybe map "a","b","c" to generic "furniture" objects
    //      and tie them to sprite names
    // Sounds complicated and full of edge cases... maybe just run with a datastructure and define each map in JSON/XML
    public Map(int w, int h, string ss = "default") {
        width = w;
        height = h;
        tiles = new Tile[w,h];
        spritesheet = ss;
        characters = new List<Character>();
        allegianceColours = new Dictionary<int, Colour>();
        charactersByAllegiance = new Dictionary<int, List<Character>>();
        treasure = new List<Treasure>();

        Random rand = new Random(); // Temporary

        for (int x = 0; x < w; x++) {
            for (int y = 0; y < h; y++) {
                tiles[x,y] = new Tile(this, x, y, 1, 1, rand.Next(0, 2));
                // TODO: Set up tile data
            }
        }
    }

    public Tile GetTileAt(int x, int y) {
        if (x > tiles.GetUpperBound(0) || x < tiles.GetLowerBound(0) || y > tiles.GetUpperBound(1) || y < tiles.GetLowerBound(1)) {
            // Index out of bounds
            return null;
        }
        return tiles[x,y];
    }

    public bool PlaceCharacter(Character chara) {
        if (chara.currentTile.character != null) {
            // Tile is occupied!
            return false;
        }
        characters.Add(chara);
        chara.currentTile.character = chara;

        List<Character> allies;
        if (charactersByAllegiance.TryGetValue(chara.allegiance, out allies)) {
            allies.Add(chara);
        } else {
            allies = new List<Character>();
            charactersByAllegiance.Add(chara.allegiance, allies);
        }

        return true;
    }

    // TODO: Will become more complicated with more varied treasure (items, weapons, key items, etc)
    public Treasure PlaceTreasure(int gp, int x, int y) {
        Tile tile = GetTileAt(x, y);
        if (tile == null) {
            return null;
        }

        Treasure treas = new Treasure(gp, "Loot", tile);
        tile.AddTreasure(treas);
        treasure.Add(treas);
        return treas;
    }

    public Treasure PlaceTreasure(Treasure treas, int x, int y) {
        Tile tile = GetTileAt(x, y);
        if (tile == null) {
            return null;
        }
        treas.tile = tile;
        tile.AddTreasure(treas);
        treasure.Add(treas);
        return treas;
    }

    public bool RemoveTreasure(Treasure treas) {
        if (treasure.Contains(treas) == false) {
            return false;
        }
        treasure.Remove(treas);
        return treas.RemoveFromTile();
    }

    public void RegisterTileGraphicChangedCallback(Action<Tile> callback) {
        onTileGraphicChanged += callback;
    }

    public void UnregisterTileGraphicChangedCallback(Action<Tile> callback) {
        onTileGraphicChanged -= callback;
    }
    public void RegisterTreasureGraphicChangedCallback(Action<Treasure> callback) {
        onTreasureGraphicChanged += callback;
    }

    public void UnregisterTreasureGraphicChangedCallback(Action<Treasure> callback) {
        onTreasureGraphicChanged -= callback;
    }
    public void RegisterCharacterGraphicChangedCallback(Action<Character> callback) {
        onCharacterGraphicChanged += callback;
    }

    public void UnregisterCharacterGraphicChangedCallback(Action<Character> callback) {
        onCharacterGraphicChanged -= callback;
    }
}