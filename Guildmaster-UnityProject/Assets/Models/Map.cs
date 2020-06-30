using System;
using System.Collections.Generic;

public class Map {
    public List<Character> characters;
    public Action<Tile> onTileGraphicChanged {
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

        return true;
    }

    public void RegisterTileGraphicChangedCallback(Action<Tile> callback) {
        onTileGraphicChanged += callback;
    }

    public void UnregisterTileGraphicChangedCallback(Action<Tile> callback) {
        onTileGraphicChanged -= callback;
    }
    public void RegisterCharacterGraphicChangedCallback(Action<Character> callback) {
        onCharacterGraphicChanged += callback;
    }

    public void UnregisterCharacterGraphicChangedCallback(Action<Character> callback) {
        onCharacterGraphicChanged -= callback;
    }
}