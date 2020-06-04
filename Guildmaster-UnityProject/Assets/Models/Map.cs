using System;

public class Map {
    public Action<Tile> onTileGraphicChanged {
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
    // Determines which spritesheet to use. This will map to a file by name of "map_spritesheet_{spritesheet}.png"
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

        Random rand = new Random(); // Temporary

        for (int x = 0; x < w; x++) {
            for (int y = 0; y < h; y++) {
                tiles[x,y] = new Tile(this, 1, 1, rand.Next(0, 2));
                // TODO: Set up tile data
            }
        }
    }

    public Tile GetTileAt(int x, int y) {
        return tiles[x,y];
    }

    public void RegisterTileGraphicChangedCallback(Action<Tile> callback) {
        onTileGraphicChanged += callback;
    }

    public void UnregisterTileGraphicChangedCallback(Action<Tile> callback) {
        onTileGraphicChanged -= callback;
    }
}