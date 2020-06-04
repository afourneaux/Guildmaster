public class Tile {

    // Movement cost multipliers
    // Cost of 0.5 = characters move at double speed when entering/leaving the tile
    // Cost of 2 = characters move at half speed when entering/leaving the tile
    // Cost To Enter of 0 = tile is impassible
    // These costs will not be used for flying and teleporting logic
    public int costToEnter;
    public int costToLeave;
    // Denotes which sprite on the spritesheet (determined by Map) will represent this tile
    public int sprite;

    // TODO: Some extra functionality like Cover, Secret, etc etc etc we'll worry about that later
}