using System;
using System.Collections.Generic;
using UnityEngine;

public class Tile {
    public Map map {
        get;
        protected set;
    }
    public int x {
        get;
        protected set;
    }
    public int y {
        get;
        protected set;
    }

    // Movement cost multipliers
    // Cost of 0.5 = characters move at double speed when entering/leaving the tile
    // Cost of 2 = characters move at half speed when entering/leaving the tile
    // Cost To Enter of 0 = tile is impassible
    // These costs will not be used for flying and teleporting logic
    public int costToEnter {
        get;
        protected set;
    }
    public int costToLeave {
        get;
        protected set;
    }
    // Denotes which sprite on the spritesheet (determined by Map) will represent this tile
    private int _sprite;
    public int sprite {
        get {
            return _sprite;
        }
        set {
            if (_sprite != value) {
                _sprite = value;

                if (map.onTileGraphicChanged != null) {
                    map.onTileGraphicChanged(this);
                }
            }
        }
    }

    public Character character;

    // TODO: Some extra functionality like Cover, Secret, etc etc etc we'll worry about that later

    public Tile(Map m, int x, int y, int cte = 1, int ctl = 1, int s = 0) {
        map = m;
        costToEnter = cte;
        costToLeave = ctl;
        sprite = s;
        this.x = x;
        this.y = y;
    }

    public float CostToEnterTile(Tile otherTile, bool isAffectedByTerrain = true) {
        if (this.x == otherTile.x && this.y == otherTile.y) {
            Debug.LogError("Tile::CostToEnterTile - otherTile is this tile!");
            return 1;
        }

        // Check if the destination is orthogonal or diagonal to the source
        int movementX = Math.Abs(this.x - otherTile.x);
        int movementY = Math.Abs(this.y - otherTile.y);

        if (movementX > 1 || movementY > 1) {
            Debug.LogError("Tile::CostToEnterTile - otherTile is not adjacent to this tile!");
        }

        float diagonalModifier = 1;
        if (movementX + movementY != 1) {
            // Move more slowly on diagonals
            diagonalModifier = 0.71f; // 1 / Sqrt(2), because Sqrt() is expensive
        }

        if (isAffectedByTerrain) {
            return diagonalModifier * costToLeave * otherTile.costToEnter;
        } else {
            return diagonalModifier;
        }
    }

    public List<Tile> GetAdjacentTiles() {
        List<Tile> adjacents = new List<Tile>();

        Tile tile;

        tile = map.GetTileAt(x, y + 1);
        if (tile != null) {
            adjacents.Add(tile);
        }
        tile = map.GetTileAt(x + 1, y + 1);
        if (tile != null) {
            adjacents.Add(tile);
        }
        tile = map.GetTileAt(x + 1, y);
        if (tile != null) {
            adjacents.Add(tile);
        }
        tile = map.GetTileAt(x + 1, y - 1);
        if (tile != null) {
            adjacents.Add(tile);
        }
        tile = map.GetTileAt(x, y - 1);
        if (tile != null) {
            adjacents.Add(tile);
        }
        tile = map.GetTileAt(x - 1, y - 1);
        if (tile != null) {
            adjacents.Add(tile);
        }
        tile = map.GetTileAt(x - 1, y);
        if (tile != null) {
            adjacents.Add(tile);
        }
        tile = map.GetTileAt(x - 1, y + 1);
        if (tile != null) {
            adjacents.Add(tile);
        }

        return adjacents;
    }
}