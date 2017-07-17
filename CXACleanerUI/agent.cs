/**
 * @Author: Joey Teng <Toujour>
 * @Date:   15-Jul-2017
 * @Email:  joey.teng.dev@gmail.com
 * @Filename: agent.cs
 * @Last modified by:   Toujour
 * @Last modified time: 17-Jul-2017
 */
using Constants;

namespace AgentApplication {
    using MapNode = System.Int32;

    class Agent {
        private int tentativeDirection;
        private int oldDirection;
        private int currentDirection;
        private RoutingApplication.Coordinate currentPosition;
        private RoutingApplication.Coordinate tentativePosition;
        private RoutingApplication.RouteNode[] _route;

        public int _serialNumber;
        public int chargerDirection;
        public bool initialized;
        public RoutingApplication.Coordinate chargerPosition;

        public Agent() {
            _serialNumber = -1;
            _route = null;
            chargerPosition = null;
        }

        public Agent(int serialNumber, RoutingApplication.Coordinate _chargerPosition = null, int _currentDirection = -1) {
            this._serialNumber = serialNumber;
            this.currentDirection = _currentDirection;
            this.tentativeDirection = this.currentDirection;
            this.oldDirection = -1;
            this.chargerDirection = -1;

            if (_chargerPosition != null) {
                this.chargerPosition = new RoutingApplication.Coordinate(_chargerPosition);
            } else {
                this.chargerPosition = new RoutingApplication.Coordinate(-1, -1);
            }
            this.currentPosition = new RoutingApplication.Coordinate(this.chargerPosition);
        }
        private void ClearRoute() {
            /// route & tentativeDirection & tentativePosition will be reset.
            this._route = null;
            this.tentativeDirection = this.currentDirection;
            this.tentativePosition = this.currentPosition;
        }
        private int Encode(int direction) {
            return Constants.AgentConstants.CODE[this.currentDirection, direction];
        }

/// Public
        public int facingDirection {
            get {
                return this.currentDirection;
            }
        }
        public RoutingApplication.RouteNode[] Decode(string commands, int finalDirection) {
            System.Collections.Generic.Stack<RoutingApplication.RouteNode> route = new System.Collections.Generic.Stack<RoutingApplication.RouteNode>();

            this.tentativeDirection = finalDirection;
            int _currentDirection = finalDirection;
            for (int j = commands.Length; j > 0; --j) {
                int i = j - 1;
                int value = commands[i];
                if (commands[i] == '\n') {
                    break;
                }
                if (value == 40) {
                    _currentDirection = Constants.AgentConstants.DECODE[_currentDirection, 0];
                    route.Push(new RoutingApplication.RouteNode(_currentDirection, 1));
                } else if (value == 41) {
                    _currentDirection = Constants.AgentConstants.DECODE[_currentDirection, 1];
                    route.Push(new RoutingApplication.RouteNode(_currentDirection, 1));
                } else {
                    int note = (int)commands[i];
                    int steps = 0;

                    if (note > 96) {
                        route.Push(new RoutingApplication.RouteNode(_currentDirection ^ 2, note - 96));
                        continue;
                    } else if (i != 0) {
                        /// TODO: Predict Forward (Backward involved)
                        steps = 1;
                    }
                    if (route.Count != 0) {
                        route.Pop();
                    }
                    route.Push(new RoutingApplication.RouteNode(_currentDirection, note - 64 + steps));
                }
            }

            RoutingApplication.RouteNode[] route_ = new RoutingApplication.RouteNode[route.Count];
            for (int i = 0; i < route_.Length; ++i) {
                route_[i] = route.Pop();
            }

            this.oldDirection = _currentDirection;

            this.currentDirection = _currentDirection;

            return route_;
        }

        public RoutingApplication.RouteNode[] route {
            get {
                return _route;
            }
        }

        public void UpdateRoute(RoutingApplication.RouteNode[] route) {
            /// tentativeDirection & tentativePosition will be updated.
            this._route = route;
            this.oldDirection = this.currentDirection;
            this.currentDirection = route[0].direction;
            if (!this.initialized) {
                this.chargerPosition = new RoutingApplication.Coordinate(this.currentPosition);
                this.chargerDirection = this.currentDirection;
                this.initialized = true;
            }

            this.tentativePosition = new RoutingApplication.Coordinate(this.currentPosition);
            foreach (RoutingApplication.RouteNode node in this._route) {
                this.tentativePosition = this.tentativePosition + Constants.RoutingConstants.MOVE_INCREMENT[node.direction] * node.steps;
            }
        }

        public string Transport() {
            /// No side effect if no error occurs.
            string commands = "";
            this.oldDirection = this.currentDirection;

            foreach (RoutingApplication.RouteNode i in route) {
                if (((i.direction ^ this.currentDirection) & 1) == 0) {
                    commands += (char)(this.Encode(i.direction) + i.steps);
                } else {
                    commands += (char)this.Encode(i.direction);
                    this.currentDirection = i.direction;
                    if (i.steps > 1) {
                        commands += (char)(this.Encode(i.direction) + i.steps - 1);
                    }
                }
            }
            this.tentativeDirection = this.currentDirection;
            this.currentDirection = this.oldDirection;

            return commands;
        }

        public void UpdateChargerPosition(RoutingApplication.Coordinate position = null, int direction = -1) {
            if (position == null) {
                position = new RoutingApplication.Coordinate(-1, -1);
            }
            if (position.x != -1) {
                this.chargerPosition = position;
            }
            if (direction != -1) {
                this.chargerDirection = direction;
            }
            this.initialized = true;
        }

        public void UpdatePosition(RoutingApplication.Coordinate position) {
            this.currentPosition = position;
            if (this.tentativePosition == null) {
                this.currentPosition = this.tentativePosition;
            }
        }

        public void UpdatePosition(MapNode[,] map, RoutingApplication.Coordinate position) {
            this.currentPosition = position;
            for (int i = position.x + position.y; i < map.Length; ++i) {
                for (int j = position.x; i - j >= position.y; ++j) {
                    if (Constants.MappingConstants.Unblocked(map, new RoutingApplication.Coordinate(j, i - j)) && Constants.MappingConstants.Unplanned(map, new RoutingApplication.Coordinate(j, i - j))) {
                        this.currentPosition = new RoutingApplication.Coordinate(this.chargerPosition);
                        if (this.tentativePosition == null) {
                            this.tentativePosition = this.currentPosition;
                        }

                        return;
                    }
                }
            }
        }

        public RoutingApplication.RouteNode[] Rotate(int currentDirection, int targetDirection) {
            if (currentDirection == targetDirection) {
                return new RoutingApplication.RouteNode[0];
            }
            if (((currentDirection ^ targetDirection) & 1) == 0) {
                /// Opposite direction
                RoutingApplication.RouteNode[] route = new RoutingApplication.RouteNode[] {new RoutingApplication.RouteNode(currentDirection ^ 3, 1), new RoutingApplication.RouteNode(currentDirection ^ 2, 1), new RoutingApplication.RouteNode(currentDirection ^ 1, 1), new RoutingApplication.RouteNode(currentDirection ^ 2, 1), new RoutingApplication.RouteNode(currentDirection, 2)};
                /// Check route
                return route;
            } else {
                RoutingApplication.RouteNode[] route = new RoutingApplication.RouteNode[] {new RoutingApplication.RouteNode(targetDirection, 1), new RoutingApplication.RouteNode(targetDirection ^ 1, 1)};

                return route;
            }
        }

        public void AddRoute(RoutingApplication.RouteNode[] route_) {
            RoutingApplication.RouteNode[] route = new RoutingApplication.RouteNode[_route.Length + route_.Length];
            for (int i = 0; i < this._route.Length; ++i) {
                route[i] = this._route[i];
            }
            for (int i = 0; i < route_.Length; ++i) {
                route[i + this._route.Length] = route_[i];
            }

            this.UpdateRoute(route);
        }

        public void NavigateTo(MapNode[,] map, RoutingApplication.Coordinate destination) {
            if (this.tentativePosition != this.currentPosition) {
                System.Console.WriteLine("Warning: Last route have not completed yet.");
            }

            this.AddRoute(RoutingApplication.Routing.AStar(map, this.currentPosition, destination));
        }

        public void BackToCharger(MapNode[,] map) {
            this.NavigateTo(map, this.chargerPosition);
            this.AddRoute(this.Rotate(this.currentDirection, this.chargerDirection));
        }

        public void Commit(MapNode[,] map) {
            RoutingApplication.Coordinate position = new RoutingApplication.Coordinate(this.currentPosition);

            foreach (RoutingApplication.RouteNode node in this.route) {
                for (int i = 0; i < node.steps; ++i) {
                    Constants.MappingConstants.Cleaning(map, currentPosition);
                    currentPosition = currentPosition + Constants.RoutingConstants.MOVE_INCREMENT[node.direction];
                }
            }
            if (position == this.tentativePosition) {
                this.currentPosition = position;
            } else {
                System.Console.WriteLine("Warning: Different tentativePosition with calculated position.");
                this.currentPosition = this.tentativePosition;
            }
            this.currentDirection = this.tentativeDirection;
            this.oldDirection = this.currentDirection;
            this.ClearRoute();
        }
    }
}
