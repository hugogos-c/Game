namespace Game.Core.Conditions
{
    public sealed class NotCondition : ICondition
    {
        public required ICondition Condition { get; init; }

        public bool Evaluate(EffectContext context)
        {
            return !Condition.Evaluate(context);
        }
    }
}
