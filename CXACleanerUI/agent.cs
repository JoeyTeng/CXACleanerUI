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

        private char encode(int direction) {
            return Constants.AgentConstants.CODE[facingDirection, direction];
        }

/// Public
        public void UpdateRoute(RoutingApplication.RouteNode[] _route) {
            route = _route;
        }

        public string Transport() {
            string commands = "";
            oldDirection = facingDirection;

            foreach (RoutingApplication.RouteNode i in route) {
                if ((i.direction ^ this.facingDirection & 1) == 0) {
                    commands += this.encode(i.direction) + i.steps;
                } else {
                    commands += this.encode(i.direction);
                    this.facingDirection = i.direction;
                }
            }

            return commands;
        }

        public void Commit(MapNode[,] map) {}
    }
}
