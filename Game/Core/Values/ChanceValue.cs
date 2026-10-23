using Game.Core.Conditions;

namespace Game.Core.Values
{
    public sealed class ChanceValue : IValue
    {
        public required IValue Chance { get; init; }
        public required IValue Success { get; init; }
        public required IValue Failure { get; init; }

        public int Resolve(EffectContext context)
        {
            return new ConditionalValue
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
