/**
 * @Author: Joey Teng <Toujour>
 * @Date:   11-Jul-2017
 * @Email:  joey.teng.dev@gmail.com
 * @Filename: routing.cs
 * @Last modified by:   Toujour
 * @Last modified time: 14-Jul-2017
 */
using Priority_Queue;
using Constants;

namespace RoutingApplication {
    using MapNode = System.Int32;

    class Coordinate : FastPriorityQueueNode {
        public int x;
        public int y;

        public Coordinate() {
            x = System.Int32.MinValue;
            y = System.Int32.MinValue;
        }

        public Coordinate(Coordinate arg) {
            x = arg.x;
            y = arg.y;
        }

        public Coordinate(int _x, int _y) {
            x = _x;
            y = _y;
        }

        public static Coordinate operator+(Coordinate lhs, Coordinate rhs) {
            Coordinate result = new Coordinate();
            result.x = lhs.x + rhs.x;
            result.y = lhs.y + rhs.y;

            return result;
        }

        public int ManhattanDistance(Coordinate destination) {
            return System.Math.Abs(x - destination.x) + System.Math.Abs(y - destination.y);
        }
    }

    class RouteNode {
        public int direction;
        public int steps;

        public RouteNode() {
            direction = -1;
            steps = 0;
        }

        public RouteNode(RouteNode arg) {
            this.direction = arg.direction;
            this.steps = arg.steps;
        }

        public RouteNode(int _direction, int _steps) {
            this.direction = _direction;
            this.steps = _steps;
        }
    }

    class Routing {
        static private void Initialize2DArray<Tout, Tin>(out Tout[,] obj, Tin[,] arg) {
            obj = new Tout[arg.GetLength(0), arg.GetLength(1)];
        }

        static private int HeuristicEstimateOfDistance(Coordinate start, Coordinate destination) {
            /// Manhattan Distance
            return start.ManhattanDistance(destination);
        }

        static private void AStar(MapNode[,] map, Coordinate initPoint, Coordinate destination) {
            FastPriorityQueue<Coordinate> estimatedPoints = new FastPriorityQueue<Coordinate>(map.Length);
            System.Collections.Queue estimatingPoints = new System.Collections.Queue();

            Coordinate[,] previousPoint;
            Initialize2DArray(out previousPoint, map);
            int[,] realDistance;
            Initialize2DArray(out realDistance, map);
            int[,] estimatedDistance;
            Initialize2DArray(out estimatedDistance, map);
        }

        static private bool CheckNextStep(MapNode[,] map, Coordinate current, int direction, out Coordinate nextPoint) {
            Coordinate next;
            next = current + RoutingConstants.MOVE_INCREMENT[direction];

            if (MappingConstants.Unblocked(map, next) && MappingConstants.Unplanned(map, next) && MappingConstants.Unclean(map, next)) {
                nextPoint = next;
                return true;
            } else {
                nextPoint = current;
                return false;
            }
        }

        static public void RouteSnakeShape(MapNode[,] map, Coordinate initPoint, out Coordinate endPoint, out RouteNode[] route) {
            System.Collections.Generic.Queue<int> record = new System.Collections.Generic.Queue<int>();
            Coordinate current = new Coordinate(initPoint);
            int direction = 0;

            while (true) {
                /// TODO: How do detect better init direction?

                while (CheckNextStep(map, current, direction, out current)) {
                    record.Enqueue(direction);
                    MappingConstants.PlannedSet(map, current);
                }
                if (CheckNextStep(map, current, 1, out current)) {
                    record.Enqueue(1);
                    MappingConstants.PlannedSet(map, current);
                } else if (CheckNextStep(map, current, 3, out current)) {
                    record.Enqueue(3);
                    MappingConstants.PlannedSet(map, current);
                } else {
                    break;
                }
                direction ^= 2;
            }

            endPoint = current;
            route = RebuildRoute(record);
        }

        static private RouteNode[] RebuildRoute(System.Collections.Generic.Queue<int> history) {
            RouteNode current = new RouteNode(history.Dequeue(), 1);
            System.Collections.Generic.Queue<RouteNode> route = new System.Collections.Generic.Queue<RouteNode>();

            while (history.Count != 0) {
                if (current.direction == history.Peek()) {
                    ++current.steps;
                    history.Dequeue();
                } else {
                    route.Enqueue(current);
                    current = new RouteNode(history.Dequeue(), 1);
                }
            }
            route.Enqueue(current);

            return route.ToArray();
        }
    }
}
