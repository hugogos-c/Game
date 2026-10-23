using Game.Core.Conditions;

namespace Game.Core.Effects
{
    public sealed class ConditionalEffect : IEffect
    {
        public required ICondition Condition { get; init; }
        public required IEffect Then { get; init; }
        public IEffect? Else { get; init; }

        public IEffectResult Apply(EffectContext context)
        {
            if (Condition.Evaluate(context)) return Then.Apply(context);

            return Else?.Apply(context) ?? new GenericResult();
        }
    }
}
