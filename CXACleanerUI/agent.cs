/**
 * @Author: Joey Teng <Toujour>
 * @Date:   15-Jul-2017
 * @Email:  joey.teng.dev@gmail.com
 * @Filename: agent.cs
 * @Last modified by:   Toujour
 * @Last modified time: 15-Jul-2017
 */
using Constants;

namespace AgentApplication {
    using MapNode = System.Int32;

    class Agent {
        public int _serialNumber;
        private int facingDirection;
        private int oldDirection;
        private RoutingApplication.Coordinate currectPosition;
        public RoutingApplication.RouteNode[] route;

        public RoutingApplication.Coordinate chargerPosition;

        public Agent() {
            _serialNumber = -1;
            route = null;
            chargerPosition = null;
        }

        public Agent(int serialNumber, RoutingApplication.Coordinate _chargerPosition = null, int _facingDirection = -1) {
            this._serialNumber = serialNumber;
            this.facingDirection = _facingDirection;

            if (_chargerPosition != null) {
                this.chargerPosition = new RoutingApplication.Coordinate(_chargerPosition);
            }
        }

        private int Encode(int direction) {
            return Constants.AgentConstants.CODE[facingDirection, direction];
        }

        public RoutingApplication.RouteNode[] Decode(string commands, int finalDirection) {
            System.Collections.Generic.Stack<RoutingApplication.RouteNode> route = new System.Collections.Generic.Stack<RoutingApplication.RouteNode>();

            this.facingDirection = finalDirection;
            int currentDirection = finalDirection;
            for (int j = commands.Length; j > 0; --j) {
                int i = j - 1;
                int value = commands[i];
                if (commands[i] == '\n') {
                    break;
                }
                if (value == 40) {
                    currentDirection = Constants.AgentConstants.DECODE[currentDirection, 0];
                    route.Push(new RoutingApplication.RouteNode(currentDirection, 1));
                } else if (value == 41) {
                    currentDirection = Constants.AgentConstants.DECODE[currentDirection, 1];
                    route.Push(new RoutingApplication.RouteNode(currentDirection, 1));
                } else {
                    int note = (int)commands[i];
                    int steps = 0;

                    if (note > 96) {
                        route.Push(new RoutingApplication.RouteNode(currentDirection ^ 2, note - 96));
                        continue;
                    } else if (i != 0) {
                        /// TODO: Predict Forward (Backward involved)
                        steps = 1;
                    }
                    if (route.Count != 0) {
                        route.Pop();
                    }
                    route.Push(new RoutingApplication.RouteNode(currentDirection, note - 64 + steps));
                }
            }

            RoutingApplication.RouteNode[] route_ = new RoutingApplication.RouteNode[route.Count];
            for (int i = 0; i < route_.Length; ++i) {
                route_[i] = route.Pop();
            }
            return route_;
        }

/// Public
        public void UpdateRoute(RoutingApplication.RouteNode[] _route) {
            route = _route;
            this.facingDirection = route[0].direction;
        }

        public string Transport() {
            string commands = "";
            oldDirection = facingDirection;

            foreach (RoutingApplication.RouteNode i in route) {
                System.Console.WriteLine("C: {0} {1}", i.direction, i.steps);
                if (((i.direction ^ this.facingDirection) & 1) == 0) {
                    commands += (char)(this.Encode(i.direction) + i.steps);
                } else {
                    commands += (char)this.Encode(i.direction);
                    this.facingDirection = i.direction;
                    if (i.steps > 1) {
                        commands += (char)(this.Encode(i.direction) + i.steps - 1);
                    }
                }
            }
            System.Console.WriteLine(commands);

            foreach (RoutingApplication.RouteNode i in Decode(commands, this.facingDirection)) {
                System.Console.WriteLine("D: {0} {1}", i.direction, i.steps);
            }

            return commands + '\n';
        }

        public void Commit(MapNode[,] map) {}
    }
}
