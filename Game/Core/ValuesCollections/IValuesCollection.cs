namespace Game.Core.ValuesCollections
{
    public interface IValuesCollection
    {
        IReadOnlyList<int> Resolve(EffectContext context);
    }
}
