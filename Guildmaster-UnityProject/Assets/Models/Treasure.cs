using System;
using UnityEngine;

public class Treasure {
    public int gp {
        get;
        private set;
    }

    private string _sprite;
    public string sprite {
        get {
            return _sprite;
        }
        private set {
            if (_sprite != value) {
                _sprite = value;
                if (tile != null) {
                    tile.map.onTreasureGraphicChanged(this);
                }
            }
        }
    }

    private Tile _tile;
    public Tile tile {
        get {
            return _tile;
        }
        private set {
            if (_tile != value) {
                _tile = value;
                if (_tile != null && _tile.map.onTreasureGraphicChanged != null) {
                    _tile.map.onTreasureGraphicChanged(this);
                }
            }
        }
    }

    public Treasure(int gp, string sprite, Tile tile) {
        this.gp = gp;
        this.sprite = sprite;
        this.tile = tile;
    }
}