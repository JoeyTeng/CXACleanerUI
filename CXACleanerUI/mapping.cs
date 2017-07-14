/**
 * @Author: Joey Teng <Toujour>
 * @Date:   06-Jul-2017
 * @Email:  joey.teng.dev@gmail.com
 * @Filename: Flood-Filled.cs
 * @Last modified by:   Toujour
 * @Last modified time: 14-Jul-2017
 */
using System;
using System.Drawing;
using Constants;

namespace CXACleanerUI {
    using MapNode = System.Int32;

    class Mapping {
        private Bitmap image;
        private MapNode[,] map;

        private void LoadImage(string path) {
            image = new Bitmap(path, true);
        }

        private void InitMap(MapNode[,] map) {
            int max_x = map.GetLength(0);
            int max_y = map.GetLength(1);

            for (int i = 0; i < max_x; ++i) {
                map[i, 0] = MappingConstants.BLOCK;
                map[i, max_y - 1] = MappingConstants.BLOCK;
            }

            for (int i = 0; i < max_y; ++i) {
                map[0, i] = MappingConstants.BLOCK;
                map[max_x - 1, i] = MappingConstants.BLOCK;
            }
        }

        private int ColorToInt(Color color) {
            /// Combine 3 8bits into an int (32bits)
            return (((color.R & MappingConstants.MASK) << (MappingConstants.COLOR_SHIFT * 2)) | ((color.G & MappingConstants.MASK) << MappingConstants.COLOR_SHIFT) | (color.B & MappingConstants.MASK));
        }

        private int SumColor(int color) {
            /// Sum up the R G B components
            return ((color & MappingConstants.MASK) + ((color >> MappingConstants.COLOR_SHIFT) & MappingConstants.MASK) + ((color >> MappingConstants.COLOR_SHIFT * 2) & MappingConstants.MASK));
        }

        private int Transfer(int color, int threshold = 400) {
            /// Default number is a magic number
            int sum = SumColor(color);

            if (sum < threshold) {
                return MappingConstants.BLOCK;
            } else {
                return MappingConstants.UNBLOCK;
            }
        }

        private void Fill(int threshold) {
            map = new int[image.Width + 2, image.Height + 2];
            InitMap(map);

            for (int i = 0; i < image.Width; ++i) {
                for (int j = 0; j < image.Height; ++j) {
                    Color pixelColor = image.GetPixel(i, j);
                    map[i + 1, j + 1] = Transfer(ColorToInt(pixelColor), threshold);
                }
            }
        }

        private int CheckBlock(int s_i, int s_j, int x, int y) {
            for (int i = s_i; i < s_i + x; ++i) {
                for (int j = s_j; j < s_j + y; ++j) {
                    if (map[i, j] == MappingConstants.BLOCK) {
                        return MappingConstants.BLOCK;
                    }
                }
            }
            return MappingConstants.UNBLOCK;
        }

        private void Compress(int x, int y = 0) {
            y = (y != 0) ? y : x;
            int max_x = image.Width / x;
            int max_y = image.Height / y;
            MapNode[,] new_map = new int[(image.Width / x) + 2, (image.Height / y) + 2];
            InitMap(new_map);

            for (int i = 0; i < max_x; ++i) {
                for (int j = 0; j < max_y; ++j) {
                    new_map[i + 1, j + 1] = CheckBlock((i * x) + 1, (j * y) + 1, x, y);
                }
            }
            map = new_map;
        }

        public void Print() {
            for (int i = 0; i < map.GetLength(1); ++i) {
                for (int j = 0; j < map.GetLength(0); ++j) {
                    Console.Write(" {0}", map[j, i]);
                }
                Console.WriteLine();
            }
        }

        static private void Execute(string imageName, int resolution, int threshold) {
            Mapping mapping = new Mapping();

            mapping.LoadImage(imageName);
            mapping.Fill(threshold);
            mapping.Compress(resolution);
            mapping.Print();

            RoutingApplication.Coordinate initPoint = new RoutingApplication.Coordinate(1, 1);
            RoutingApplication.Coordinate endPoint;
            RoutingApplication.RouteNode[] route;
            RoutingApplication.Routing.RouteSnakeShape(mapping.map, initPoint, out endPoint, out route);

            System.Console.WriteLine("{0} {1}\n", endPoint.x, endPoint.y);

            foreach(RoutingApplication.RouteNode node in route) {
                System.Console.WriteLine("{0} {1}", node.direction, node.steps);
            }
        }

        static void Main(string[] args) {
            Execute(args[0], 15, 500);
        }
    }
}
