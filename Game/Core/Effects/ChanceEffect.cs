using Game.Core.Conditions;
using Game.Core.Values;

namespace Game.Core.Effects
{
    public sealed class ChanceEffect : IEffect
    {
        public required IValue Chance { get; init; }
        public required IEffect Success { get; init; }
        public IEffect? Failure { get; init; }

        public IEffectResult Apply(EffectContext context)
        {
            return new ConditionalEffect
            {
                Condition = new ComparisonCondition
                {
                    Left = new RandomValue { Min = new ConstantValue { Value = 1 }, Max = new ConstantValue { Value = 100 }, },
                    Operator = ComparisonOperator.LessThanOrEqual,
                    Right = Chance,
                },
                Then = Success,
                Else = Failure,
            }.Apply(context);
        }
    }
}
