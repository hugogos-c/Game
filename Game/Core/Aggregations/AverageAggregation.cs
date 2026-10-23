namespace Game.Core.Aggregations
{
    public sealed class AverageAggregation : IAggregation
    {
        public int Aggregate(IReadOnlyList<int> values)
        {
            return (int)values.Average();
        }
    }
}
