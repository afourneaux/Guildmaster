using System;
using UnityEngine;

public class Treasure {
    public int gp {
        get;
        private set;
    }
    public string sprite {
        get;
        private set;
    }

    public Tile tile;

    public Treasure(int gp, string sprite, Tile tile) {
        this.gp = gp;
        this.sprite = sprite;
        this.tile = tile;
    }
}