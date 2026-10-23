using Game.Core.Conditions;
using Game.Core.Values;

namespace Game.Core.ValuesCollections
{
    public sealed class ChanceValuesCollection : IValuesCollection
    {
        public required IValue Chance { get; init; }
        public required IValuesCollection Success { get; init; }
        public required IValuesCollection Failure { get; init; }
        public IReadOnlyList<int> Resolve(EffectContext context)
        {
            return new ConditionalValuesCollection
            {
                Condition = new ComparisonCondition
                {
                    Left = new RandomValue { Min = new ConstantValue { Value = 1 }, Max = new ConstantValue { Value = 100 }, },
                    Operator = ComparisonOperator.LessThanOrEqual,
                    Right = Chance,
                },
                Then = Success,
                Else = Failure,
            }.Resolve(context);
        }
    }
}
