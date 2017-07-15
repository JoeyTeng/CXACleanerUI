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
        public int facingDirection;
        private int oldDirection;
        private RoutingApplication.Coordinate currentPosition;
        private RoutingApplication.Coordinate tentativePosition;
        private RoutingApplication.RouteNode[] _route;

        public RoutingApplication.Coordinate chargerPosition;

        public Agent() {
            _serialNumber = -1;
            _route = null;
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

            this.oldDirection = currentDirection;

            return route_;
        }

/// Public
        public RoutingApplication.RouteNode[] route {
            get {
                return _route;
            }
        }
        public void UpdateRoute(RoutingApplication.RouteNode[] route) {
            this._route = route;
            this.facingDirection = route[0].direction;
            this.oldDirection = this.facingDirection;

            this.tentativePosition = this.currentPosition;
            foreach (RoutingApplication.RouteNode node in this._route) {
                this.tentativePosition = this.tentativePosition + Constants.RoutingConstants.MOVE_INCREMENT[node.direction] * node.steps;
            }
        }

        public string Transport() {
            string commands = "";
            oldDirection = facingDirection;

            foreach (RoutingApplication.RouteNode i in route) {
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

            return commands + '\n';
        }

        public void UpdatePosition(RoutingApplication.Coordinate position) {
            this.currentPosition = position;
            this.chargerPosition = position;
        }

        public void UpdatePosition(MapNode[,] map, RoutingApplication.Coordinate position) {
            this.currentPosition = position;
            this.chargerPosition = position;
            for (int i = position.x + position.y; i < map.Length; ++i) {
                for (int j = position.x; i - j >= position.y; ++j) {
                    if (Constants.MappingConstants.Unblocked(map, new RoutingApplication.Coordinate(j, i - j)) && Constants.MappingConstants.Unplanned(map, new RoutingApplication.Coordinate(j, i - j))) {
                        this.chargerPosition = new RoutingApplication.Coordinate(j, i - j);
                        this.currentPosition = new RoutingApplication.Coordinate(this.chargerPosition);

                        return;
                    }
                }
            }
        }

        public void BackToCharger() {
        }

        public void Commit(MapNode[,] map) {}
    }
}
