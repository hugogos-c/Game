using Game.Core.Conditions;

namespace Game.Core.Values
{
    public sealed class ConditionalValue : IValue
    {
        public required ICondition Condition { get; init; }
        public required IValue Then { get; init; }
        public required IValue Else { get; init; }

        public int Resolve(EffectContext context)
        {
            return Condition.Evaluate(context) ? Then.Resolve(context) : Else.Resolve(context);
        }
    }
}
