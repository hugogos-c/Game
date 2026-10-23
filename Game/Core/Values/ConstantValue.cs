namespace Game.Core.Values
{
    public sealed class ConstantValue : IValue
    {
        public required int Value { get; init; }

        public int Resolve(EffectContext context)
        {
            return Value;
        }
    }
}
