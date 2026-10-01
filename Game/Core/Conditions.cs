namespace Game.Core
{
    public interface ICondition
    {
        bool Evaluate(EffectContext context);
    }

    #region And
    public sealed class AndCondition : ICondition
    {
        public required IReadOnlyList<ICondition> Conditions { get; init; }

        public bool Evaluate(EffectContext context)
        {
            return Conditions.All(condition => condition.Evaluate(context));
        }
    }
    #endregion

    #region Or
    public sealed class OrCondition : ICondition
    {
        public required IReadOnlyList<ICondition> Conditions { get; init; }

        public bool Evaluate(EffectContext context)
        {
            return Conditions.Any(condition => condition.Evaluate(context));
        }
    }
    #endregion

    #region Not
    public sealed class NotCondition : ICondition
    {
        public required ICondition Condition { get; init; }

        public bool Evaluate(EffectContext context)
        {
            return !Condition.Evaluate(context);
        }
    }
    #endregion

    #region Comparison
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
    #endregion
}
