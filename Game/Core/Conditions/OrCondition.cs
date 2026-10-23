namespace Game.Core.Conditions
{
    public sealed class OrCondition : ICondition
    {
        public required IReadOnlyList<ICondition> Conditions { get; init; }

        public bool Evaluate(EffectContext context)
        {
            return Conditions.Any(condition => condition.Evaluate(context));
        }
    }
}
