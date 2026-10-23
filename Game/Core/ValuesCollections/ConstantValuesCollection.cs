namespace Game.Core.ValuesCollections
{
    public sealed class ConstantValuesCollection : IValuesCollection
    {
        public required IReadOnlyList<int> ValuesCollection { get; init; }

        public IReadOnlyList<int> Resolve(EffectContext context)
        {
            return ValuesCollection;
        }
    }
}
