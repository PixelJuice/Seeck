using Unity.Mathematics;

namespace dev.jonasjohansson.PathFinding
{
    public struct EuclideanHeuristicCalculator
    {        
        static readonly int DIAGONAL_MOVE_COST = 14;

        public int CalculateHCost(int2 p_aPostiontion, int2 p_bPosition)
        {
            int xDistance = math.abs(p_aPostiontion.x - p_bPosition.x);
            int yDistance = math.abs(p_aPostiontion.y - p_bPosition.y);
            return (int)(DIAGONAL_MOVE_COST * math.sqrt(xDistance * xDistance + yDistance + yDistance));
        }
        public int CalculateFCost(int p_gCost, int p_hCost)
        {
            return p_gCost + p_hCost;
        }
        public float CalculateGCost(int p_from, Edge p_to)
        {
            return p_from + p_to.Cost;
        }
    }
}