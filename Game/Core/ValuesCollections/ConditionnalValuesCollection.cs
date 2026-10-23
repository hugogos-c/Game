using Game.Core.Conditions;

namespace Game.Core.ValuesCollections
{
    public sealed class ConditionalValuesCollection : IValuesCollection
    {
        public required ICondition Condition { get; init; }
        public required IValuesCollection Then { get; init; }
        public required IValuesCollection Else { get; init; }

        public IReadOnlyList<int> Resolve(EffectContext context)
        {
            return Condition.Evaluate(context) ? Then.Resolve(context) : Else.Resolve(context);
        }
    }
}
