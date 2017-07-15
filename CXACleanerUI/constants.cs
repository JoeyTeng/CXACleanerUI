/**
 * @Author: Joey Teng <Toujour>
 * @Date:   11-Jul-2017
 * @Email:  joey.teng.dev@gmail.com
 * @Filename: constants.cs
 * @Last modified by:   Toujour
 * @Last modified time: 15-Jul-2017
 */
namespace Constants {
    using MapNode = System.Int32;

    class MappingConstants {
        public const int COLOR_SHIFT = 8;
        public const int MASK = (1 << COLOR_SHIFT) - 1;

    /// Element in map
    /// Map [0..GetLength(0)]
    ///     [.]
    ///     [GetLength(1)]
    /// BLOCK_FLAG * 1 + Selected * 1 + Planned * 1 + CLEAN * 1 + AgentSerialNumber * 10 + Timeout * 18
        public const int BLOCK = 0;
        public const int UNBLOCK = 1;
        public const int BLOCK_SHIFT = 0;
        public const int BLOCK_LENGTH = 1;
        public const int BLOCK_MASK = (1 << BLOCK_LENGTH) - 1;

        public const int SELECTED = 1;
        public const int DESELECTED = 0;
        public const int SELECTED_SHIFT = BLOCK_SHIFT + BLOCK_LENGTH;
        public const int SELECTED_LENGTH = 1;
        public const int SELECTED_MASK = (1 << SELECTED_LENGTH) - 1;

        public const int PLANNED = 1;
        public const int UNPLANNED = 0;
        public const int PLANNED_SHIFT = SELECTED_SHIFT + SELECTED_LENGTH;
        public const int PLANNED_LENGTH = 1;
        public const int PLANNED_MASK = (1 << PLANNED_LENGTH) - 1;

        public const int CLEAN = 1;
        public const int UNCLEAN = 0;
        public const int CLEAN_SHIFT = PLANNED_SHIFT + PLANNED_LENGTH;
        public const int CLEAN_LENGTH = 1;
        public const int CLEAN_MASK = (1 << CLEAN_LENGTH) - 1;

        public const int AGENT_SERIAL_NUMBER_SHIFT = CLEAN_SHIFT + CLEAN_LENGTH;
        public const int AGENT_SERIAL_NUMBER_LENGTH = 10;
        public const int AGENT_SERIAL_NUMBER_MASK = (1 << AGENT_SERIAL_NUMBER_LENGTH) - 1;

        public const int TIMEOUT_SHIFT = AGENT_SERIAL_NUMBER_SHIFT + AGENT_SERIAL_NUMBER_LENGTH;
        public const int TIMEOUT_LENGTH = 18;
        public const int TIMEOUT_MASK = (1 << TIMEOUT_LENGTH) - 1;

        public const int DEFAULT = (UNBLOCK << BLOCK_SHIFT) | (DESELECTED << SELECTED_SHIFT) | (UNPLANNED << PLANNED_SHIFT) | (UNCLEAN << CLEAN_SHIFT);

        public static bool Blocked(MapNode[,] map, RoutingApplication.Coordinate position) {
            return (map[position.x, position.y] & (BLOCK_MASK << BLOCK_SHIFT)) == (BLOCK << BLOCK_SHIFT);
        }

        public static void Block(MapNode[,] map, RoutingApplication.Coordinate position) {
            map[position.x, position.y] &= (~(BLOCK_MASK << BLOCK_SHIFT)) | (BLOCK << BLOCK_SHIFT);
        }

        public static bool Unblocked(MapNode[,] map, RoutingApplication.Coordinate position) {
            return (map[position.x, position.y] & (BLOCK_MASK << BLOCK_SHIFT)) == (UNBLOCK << BLOCK_SHIFT);
        }

        public static void Unblock(MapNode[,] map, RoutingApplication.Coordinate position) {
            map[position.x, position.y] |= (UNBLOCK << BLOCK_SHIFT);
        }

        public static bool Selected(MapNode[,] map, RoutingApplication.Coordinate position) {
            return (map[position.x, position.y] & (SELECTED_MASK << SELECTED_SHIFT)) == (SELECTED << SELECTED_SHIFT);
        }

        public static void Select(MapNode[,] map, RoutingApplication.Coordinate position) {
            map[position.x, position.y] |= (SELECTED << SELECTED_SHIFT);
        }

        public static bool Deselected(MapNode[,] map, RoutingApplication.Coordinate position) {
            return (map[position.x, position.y] & (SELECTED_MASK << SELECTED_SHIFT)) == (DESELECTED << SELECTED_SHIFT);
        }

        public static void Deselect(MapNode[,] map, RoutingApplication.Coordinate position) {
            map[position.x, position.y] &= (~(SELECTED_MASK << SELECTED_SHIFT)) | (DESELECTED << SELECTED_SHIFT);
        }

        public static bool Clean(MapNode[,] map, RoutingApplication.Coordinate position) {
            return (map[position.x, position.y] & (CLEAN_MASK << CLEAN_SHIFT)) == (CLEAN << CLEAN_SHIFT);
        }

        public static bool Dirty(MapNode[,] map, RoutingApplication.Coordinate position) {
            return (map[position.x, position.y] & (CLEAN_MASK << CLEAN_SHIFT)) == (UNCLEAN << CLEAN_SHIFT);
        }

        public static bool Unclean(MapNode[,] map, RoutingApplication.Coordinate position) {
            return Dirty(map, position);
    }

        public static bool Planned(MapNode[,] map, RoutingApplication.Coordinate position) {
            return (map[position.x, position.y] & (PLANNED_MASK << PLANNED_SHIFT)) == (PLANNED << PLANNED_SHIFT);
        }

        public static void Plan(MapNode[,] map, RoutingApplication.Coordinate position) {
            map[position.x, position.y] |= (PLANNED << PLANNED_SHIFT);
        }

        public static bool Unplanned(MapNode[,] map, RoutingApplication.Coordinate position) {
            return (map[position.x, position.y] & (PLANNED_MASK << PLANNED_SHIFT)) == (UNPLANNED << PLANNED_SHIFT);
        }

        public static void Unplan(MapNode[,] map, RoutingApplication.Coordinate position) {
            map[position.x, position.y] &= (~(PLANNED_MASK << PLANNED_SHIFT)) | (UNPLANNED << PLANNED_SHIFT);
        }
    }

    class RoutingConstants {
        public static RoutingApplication.Coordinate[] MOVE_INCREMENT = new RoutingApplication.Coordinate[] {
            new RoutingApplication.Coordinate(1, 0),
            new RoutingApplication.Coordinate(0, 1),
            new RoutingApplication.Coordinate(-1, 0),
            new RoutingApplication.Coordinate(0, -1)
        };

        public const int DIR_INIT_POINT = 4;
    }

    class AgentConstants {
        public static int[,] CODE = new int[,] {
            {64, 41, 96, 40}, /// Forward, Turn Right, Backward, Turn Left
            {40, 64, 41, 96},
            {96, 40, 64, 41},
            {41, 96, 40, 64}
        };
        public static int[,] DECODE = new int[,] {
            {1, 3, 2, 0}, ///0 <= 40, 41, 96, 64
            {2, 0, 3, 1},
            {3, 1, 0, 2},
            {0, 2, 1, 3}
        };
    }
}
