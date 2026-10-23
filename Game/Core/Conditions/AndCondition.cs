namespace Game.Core.Conditions
{
    public sealed class AndCondition : ICondition
    {
        public required IReadOnlyList<ICondition> Conditions { get; init; }

        public bool Evaluate(EffectContext context)
        {
            return Conditions.All(condition => condition.Evaluate(context));
        }
    }
}
