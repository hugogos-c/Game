namespace Game.Core.Aggregations
{
    public interface IAggregation
    {
        int Aggregate(IReadOnlyList<int> values);
    }
}
