using Unity.Mathematics;

namespace dev.jonasjohansson.PathFinding
{
    public struct ManhattanHeuristicCalculator
    {
        static readonly int STRAIGHT_MOVE_COST = 10;
        static readonly int DIAGONAL_MOVE_COST = 14;

        public int CalculateHCost(int2 p_aPostiontion, int2 p_bPosition)
        {
            int xDistance = math.abs(p_aPostiontion.x - p_bPosition.x);
            int yDistance = math.abs(p_aPostiontion.y - p_bPosition.y);
            int remaining = math.abs(xDistance - yDistance);
            return DIAGONAL_MOVE_COST * math.min(xDistance, yDistance) + STRAIGHT_MOVE_COST * remaining;
        }

        public int CalculateFCost(int p_gCost, int p_hCost)
        {
            return p_gCost + p_hCost;
        }

        #pragma warning disable 0219
        public float CalculateGCost(int p_from, Edge p_to, NavigationCapabilities p_capabilities)
        {
            return p_from + p_to.Cost;
        }
        #pragma warning restore 0219
    }
}