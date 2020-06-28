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

    // TODO: Some extra functionality like Cover, Secret, etc etc etc we'll worry about that later

    public Tile(Map m, int x, int y, int cte = 1, int ctl = 1, int s = 0) {
        map = m;
        costToEnter = cte;
        costToLeave = ctl;
        sprite = s;
        this.x = x;
        this.y = y;
    }
}