using Game.Core.Values;

namespace Game.Core.Conditions
{
    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
    }

    public sealed class ComparisonCondition : ICondition
    {
        public required IValue Left { get; init; }
        public required ComparisonOperator Operator { get; init; }
        public required IValue Right { get; init; }

        public bool Evaluate(EffectContext context)
        {
            var left = Left.Resolve(context);
            var right = Right.Resolve(context);

            return Operator switch
            {
                ComparisonOperator.Equal => left == right,
                ComparisonOperator.NotEqual => left != right,
                ComparisonOperator.LessThan => left < right,
                ComparisonOperator.LessThanOrEqual => left <= right,
                ComparisonOperator.GreaterThan => left > right,
                ComparisonOperator.GreaterThanOrEqual => left >= right,
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
