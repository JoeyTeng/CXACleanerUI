/**
 * @Author: Joey Teng <Toujour>
 * @Date:   11-Jul-2017
 * @Email:  joey.teng.dev@gmail.com
 * @Filename: routing.cs
 * @Last modified by:   Toujour
 * @Last modified time: 15-Jul-2017
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

        public int Hash {
            get {
                return ((this.x << 16) | (this.y & (1 << 16) - 1));
            }
        }

        public static Coordinate operator+(Coordinate lhs, Coordinate rhs) {
            Coordinate result = new Coordinate();
            result.x = lhs.x + rhs.x;
            result.y = lhs.y + rhs.y;

            return result;
        }

        public static Coordinate operator-(Coordinate lhs, Coordinate rhs) {
            Coordinate result = new Coordinate();
            result.x = lhs.x - rhs.x;
            result.y = lhs.y - rhs.y;

            return result;
        }

        public static Coordinate operator*(Coordinate lhs, int rhs) {
            Coordinate result = new Coordinate();
            result.x = lhs.x * rhs;
            result.y = lhs.y * rhs;

            return result;
        }

        public override bool Equals(object obj) {
            if (obj == null || obj.GetType() != this.GetType()) {
                return false;
            }
            return ((Coordinate)obj).Hash.Equals(this.Hash);
        }

        public override int GetHashCode() {
            return this.Hash.GetHashCode();
        }

        public static bool operator==(Coordinate lhs, Coordinate rhs) {
            return (lhs.x == rhs.x) && (lhs.y == rhs.y);
        }

        public static bool operator!=(Coordinate lhs, Coordinate rhs) {
            return (lhs.x != rhs.x) || (lhs.y != rhs.y);
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
        private static void Initialize2DArray<Tout, Tin>(out Tout[,] obj, Tin[,] arg) {
            obj = new Tout[arg.GetLength(0), arg.GetLength(1)];
        }

        private static void Initialize2DArray<Tout, Tin>(out Tout[,] obj, Tin[,] arg, Tout initValue) {
            obj = new Tout[arg.GetLength(0), arg.GetLength(1)];
            for (int i = 0; i < obj.GetLength(0); ++i) {
                for (int j = 0; j < obj.GetLength(1); ++j) {
                    obj[i, j] = initValue;
                }
            }
        }

        private static int HeuristicEstimateOfDistance(Coordinate start, Coordinate destination) {
            /// Manhattan Distance
            return start.ManhattanDistance(destination);
        }

        private static int Dist(Coordinate point, int[,] realDistance, int[,] estimatedDistance) {
            return (realDistance[point.x, point.y] + estimatedDistance[point.x, point.y]);
        }

        private static RouteNode[] AStarRebuildRoute(MapNode[,] map, Coordinate destination, int[,] history) {
            System.Console.WriteLine("A* Rebuild Route\n"); /// DEBUG

            System.Collections.Generic.Queue<int> record = new System.Collections.Generic.Queue<int>();
            System.Collections.Generic.Stack<int> stack = new System.Collections.Generic.Stack<int>();

            Coordinate current = new Coordinate(destination);
            ClearPlan(map);
            while (history[current.x, current.y] != RoutingConstants.DIR_INIT_POINT) {
                int direction = history[current.x, current.y];

                Constants.MappingConstants.Plan(map, current);
                System.Console.WriteLine("{0}", direction);

                stack.Push(direction);
                current = current + RoutingConstants.MOVE_INCREMENT[direction ^ 2];
            }

            while (stack.Count != 0) {
                record.Enqueue(stack.Pop());
            }

            return RebuildRoute(record);
        }

        private static bool CheckNextStep(MapNode[,] map, Coordinate current, int direction, out Coordinate nextPoint, bool ignoreFlags = false, bool selectedOnly = false) {
            Coordinate next;
            next = current + RoutingConstants.MOVE_INCREMENT[direction];

            if (MappingConstants.Unblocked(map, next) && ((!selectedOnly || MappingConstants.Selected(map, next)) && MappingConstants.Unplanned(map, next)) && (selectedOnly || ignoreFlags || (MappingConstants.Unplanned(map, next) && MappingConstants.Unclean(map, next)))) {
                nextPoint = next;
                return true;
            } else {
                nextPoint = current;
                return false;
            }
        }

        private static RouteNode[] RebuildRoute(System.Collections.Generic.Queue<int> history) {
            if (history.Count == 0) {
                return null;
            }

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
/// Public

        public static void ClearPlan(MapNode[,] map) {
            for (int i = 0; i < map.GetLength(0); ++i) {
                for (int j = 0; j < map.GetLength(0); ++j) {
                    Constants.MappingConstants.Unplan(map, new Coordinate(i, j));
                    if (MappingConstants.Planned(map, new Coordinate(i, j))) {
                        System.Console.WriteLine("WARNING!!!!")
                    }
                }
            }
        }
        public static RouteNode[] AStar(MapNode[,] map, Coordinate initPoint, Coordinate destination) {
            System.Console.WriteLine("{0} {1} => {2} {3}", initPoint.x, initPoint.y, destination.x, destination.y);

            int estimatedCount = 0;
            System.Collections.Hashtable estimatedPoints = new System.Collections.Hashtable();
            int estimatingCount = 0;
            System.Collections.Hashtable estimatingSet = new System.Collections.Hashtable();
            FastPriorityQueue<Coordinate> estimatingPoints = new FastPriorityQueue<Coordinate>(map.Length);

            int[,] record;
            Initialize2DArray(out record, map, -1);
            int[,] realDistance;
            Initialize2DArray(out realDistance, map, System.Int32.MaxValue);
            int[,] estimatedDistance;
            Initialize2DArray(out estimatedDistance, map, 0);

            record[initPoint.x, initPoint.y] = RoutingConstants.DIR_INIT_POINT;
            realDistance[initPoint.x, initPoint.y] = 0;
            estimatedDistance[initPoint.x, initPoint.y] = HeuristicEstimateOfDistance(initPoint, destination);

            estimatingPoints.Enqueue(initPoint, Dist(initPoint, realDistance, estimatedDistance));
            estimatingSet.Add(initPoint.Hash, estimatingCount++);


            while (estimatingPoints.Count != 0) {
                Coordinate current = estimatingPoints.Dequeue();
                estimatingSet.Remove(current.Hash);

                if (current == destination) {
                    return AStarRebuildRoute(map, destination, record);
                }

                estimatedPoints.Add(current.Hash, estimatedCount++);
                Coordinate next;
                for (int i = 0; i < RoutingConstants.MOVE_INCREMENT.Length; ++i) {
                    if (!CheckNextStep(map, current, i, out next, ignoreFlags: true)) {
                        continue;
                    }
                    if (estimatedPoints.Contains(next.Hash)) {
                        continue;
                    }
                    int tentativeRealDistance = realDistance[next.x, next.y] + 1;

                    if (!estimatingSet.Contains(next.Hash)) {
                        record[next.x, next.y] = i;
                        realDistance[next.x, next.y] = tentativeRealDistance;
                        estimatedDistance[next.x, next.y] = HeuristicEstimateOfDistance(next, destination);

                        estimatingPoints.Enqueue(next, realDistance[next.x, next.y] + estimatedDistance[next.x, next.y]);
                        estimatingSet.Add(next.Hash, estimatingCount++);
                    }
                }
            }

            return null;
        }

        public static void RouteSnakeShape(MapNode[,] map, Coordinate initPoint, out Coordinate endPoint, out RouteNode[] route, bool ignoreFlags = false, bool selectedOnly = false) {
            System.Collections.Generic.Queue<int> record = new System.Collections.Generic.Queue<int>(map.Length);
            Coordinate current = new Coordinate(initPoint);
            int direction = 0;

            while (true) {
                /// TODO: How do detect better init direction?

                while (CheckNextStep(map, current, direction, out current, ignoreFlags: ignoreFlags, selectedOnly: selectedOnly)) {
                    record.Enqueue(direction);
                    MappingConstants.Plan(map, current);
                }
                if (CheckNextStep(map, current, 1, out current, ignoreFlags: ignoreFlags, selectedOnly: selectedOnly)) {
                    record.Enqueue(1);
                    MappingConstants.Plan(map, current);
                } else if (CheckNextStep(map, current, 3, out current, ignoreFlags: ignoreFlags, selectedOnly: selectedOnly)) {
                    record.Enqueue(3);
                    MappingConstants.Plan(map, current);
                } else {
                    break;
                }
                direction ^= 2;
            }

            endPoint = current;
            route = RebuildRoute(record);
        }
    }
}
