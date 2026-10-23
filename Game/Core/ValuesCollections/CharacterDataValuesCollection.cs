namespace Game.Core.ValuesCollections
{
    public enum CharacterData
    {
        MaxHp,
        MaxMp,
        CurrentHp,
        CurrentMp,
        Attack,
        Defense,
        Speed,
    }

    public sealed class CharacterDataValuesCollection : IValuesCollection
    {
        public required ITargetsSelector TargetsSelector { get; init; }
        public required CharacterData CharacterData { get; init; }

        public IReadOnlyList<int> Resolve(EffectContext context)
        {
            return TargetsSelector.Select(context.AbilityContext).Select(t => CharacterData switch
            {
                CharacterData.MaxHp => t.BaseStats.MaxHp,
                CharacterData.MaxMp => t.BaseStats.MaxMp,
                CharacterData.CurrentHp => t.CurrentHp,
                CharacterData.CurrentMp => t.CurrentMp,
                CharacterData.Attack => t.BaseStats.Attack,
                CharacterData.Defense => t.BaseStats.Defense,
                CharacterData.Speed => t.BaseStats.Speed,
                _ => throw new InvalidOperationException(),
            }).ToList();
        }
    }
}
