using Game.Core.Aggregations;
using Game.Core.ValuesCollections;

namespace Game.Core.Values
{
    public sealed class AggregateValue : IValue
    {
        public required IValuesCollection ValuesCollection { get; init; }
        public required IAggregation Aggregation { get; init; }

        public int Resolve(EffectContext context)
        {
            return Aggregation.Aggregate(ValuesCollection.Resolve(context));
        }
    }
}
