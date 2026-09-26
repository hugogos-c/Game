namespace Game.Core
{
    public enum TargetMode
    {
        Self,
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies
    }

    public interface IEffectResult { }

    public class GenericResult : IEffectResult { }

    public class AbilityContext
    {
        public required IReadOnlyList<Character> Casters { get; init; }
        public required IReadOnlyList<Character> Targets { get; init; }
        public required Battle Battle { get; init; }

        public bool IsAborted { get; set; }
        public List<IEffectResult> Results { get; } = new();
        public Dictionary<string, object> CustomData { get; } = new();
    }

    public class EffectContext
    {
        public required AbilityContext AbilityContext { get; init; }
        public required Character Target { get; init; }
    }
}
