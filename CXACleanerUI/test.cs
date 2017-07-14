/**
 * @Author: Joey Teng <Toujour>
 * @Date:   15-Jul-2017
 * @Email:  joey.teng.dev@gmail.com
 * @Filename: test.cs
 * @Last modified by:   Toujour
 * @Last modified time: 15-Jul-2017
 */



class Test {
    public static void main() {
        int[,] node = new int[,] {{Constants.MappingConstants.DEFAULT}};
        RoutingApplication.Coordinate c = new RoutingApplication.Coordinate(0, 0);

        Constants.MappingConstants.Block(node, c);
        System.Console.WriteLine("{0} {1}", Constants.MappingConstants.Blocked(node, c), Constants.MappingConstants.Unblocked(node, c));

        Constants.MappingConstants.Unblock(node, c);
        System.Console.WriteLine("{0} {1}", Constants.MappingConstants.Blocked(node, c), Constants.MappingConstants.Unblocked(node, c));

        Constants.MappingConstants.Select(node, c);
        System.Console.WriteLine("{0} {1} {2}", Constants.MappingConstants.Selected(node, c), Constants.MappingConstants.Deselected(node, c), node[0, 0]);
        System.Console.WriteLine("{0} {1}", Constants.MappingConstants.SELECTED << Constants.MappingConstants.SELECTED_SHIFT, Constants.MappingConstants.SELECTED_MASK);

        Constants.MappingConstants.Deselect(node, c);
        System.Console.WriteLine("{0} {1}", Constants.MappingConstants.Selected(node, c), Constants.MappingConstants.Deselected(node, c));
    }
}
