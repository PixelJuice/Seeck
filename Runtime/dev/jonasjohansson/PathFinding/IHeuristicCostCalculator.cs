namespace dev.jonasjohansson.PathFinding
{
    public interface IHeuristicCostCalculator<T>
    {
        int CalculateHCost(T p_nodeA, T p_nodeB);
        int CalculateFCost(int p_gCost, int p_hCost);
        float CalculateGCost(int p_currentCost, IEdge p_edge, INavigationCapabilities p_capabilities);
    }
}