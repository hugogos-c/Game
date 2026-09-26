namespace Game.Core
{
    public interface IEffectResult { }

    public class GenericResult : IEffectResult { }

    public class AbilityContext
    {
        public required Character Initiator { get; init; }
        public required IReadOnlyList<Character> Participants { get; init; }
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

    public enum AbilityValidationError
    {
        None,
        
        // BaseAbility
        InitiatorDead,
        ParticipantDead,
        NoAvailableTargets,

        // Skill
        
        // Spell
        NotEnoughMana,
        
        // Synergy
        NotEnoughSynergy
    }

    public class AbilityValidationResult
    {
        public AbilityValidationError Error { get; init; }
        public bool IsValid => Error == AbilityValidationError.None;

        public static AbilityValidationResult Valid() => new()
        {
            Error = AbilityValidationError.None,
        };

        public static AbilityValidationResult Invalid(AbilityValidationError error) => new()
        {
            Error = error
        };
    }
}
