using Unity.Mathematics.FixedPoint;

namespace SepM.Physics {
    public static class Constants{
        public enum coll_layers {
            normal = 0,
            ground = 1,
            wall = 2,
            player = 3,
            noPlayer = 4, // Collides with ground and such, but no players,
            danger = 5,
            win = 6,
            hitbox = 7,
        };
        public enum coll_types {
            none = 0,
            sphere = 1,
            capsule = 2,
            aabb = 3
        };
        // Bitwise for collision layers
        public static long layer_none = 0;
        public static long layer_all = ~0;
        public static long layer_normal = 1 << ((int)coll_layers.normal);
        public static long layer_ground = 1 << ((int)coll_layers.ground);
        public static long layer_wall = 1 << ((int)coll_layers.wall);
        public static long layer_player = 1 << ((int)coll_layers.player);
        public static long layer_noPlayer = 1 << ((int)coll_layers.noPlayer);
        public static fp3 GRAVITY = new fp3(0,-9.81m, 0);
    }

}