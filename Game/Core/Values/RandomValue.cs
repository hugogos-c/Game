namespace Game.Core.Values
{
    public sealed class RandomValue : IValue
    {
        public required IValue Min { get; init; }
        public required IValue Max { get; init; }

        public int Resolve(EffectContext context)
        {
            return Random.Shared.Next(Min.Resolve(context), Max.Resolve(context) + 1);
        }
    }
}
