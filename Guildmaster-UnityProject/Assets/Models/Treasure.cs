using System.Collections.Generic;
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
                if (TacticalController.instance.map.onTreasureGraphicChanged != null) {
                    TacticalController.instance.map.onTreasureGraphicChanged(this);
                }
            }
        }
    }

    private List<Character> noticedBy;

    private Tile _tile;
    public Tile tile {
        get {
            return _tile;
        }
        private set {
            if (_tile != value) {
                _tile = value;
                if (TacticalController.instance.map.onTreasureGraphicChanged != null) {
                    TacticalController.instance.map.onTreasureGraphicChanged(this);
                }
            }
        }
    }

    public Treasure(int gp, string sprite, Tile tile) {
        this.gp = gp;
        this.sprite = sprite;
        this.tile = tile;
        noticedBy = new List<Character>();
    }

    public bool RemoveFromTile() {
        if (tile == null) {
            Debug.LogError("Trying to remove treasure from a null tile!");
            return false;
        }
        tile.treasure.Remove(this);
        tile = null;
        noticedBy.Clear();
        return true;
    }

    public void NoticeBy(Character other) {
        if (tile == null) {
            Debug.LogError(other.name + " has noticed an invalid treasure!");
            return;
        }
        if (noticedBy.Contains(other)) {
            Debug.LogError("This treasure is already noticed by " + other.name + "!");
            return;
        }
        noticedBy.Add(other);
        tile.map.onTreasureGraphicChanged(this);
    }

    public List<Character> getNoticedBy() {
        return new List<Character>(noticedBy);
    }
}