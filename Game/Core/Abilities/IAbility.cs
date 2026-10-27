using Game.Core.Entities;

namespace Game.Core.Abilities
{
    public interface IAbility
    {
        string Id { get; }
        string Name { get; }
        IReadOnlyList<ITargetsFilter> TargetsFilters { get; }
        int RequiredTargetsCount { get; }
        int RequiredParticipantsCount { get; }
        IReadOnlyList<ISequence> Sequences { get; }

        IReadOnlyList<Combatant> GetAvailableTargets(AbilityContext context);
        AbilityValidationResult Validate(AbilityContext context);
        void PayCost(AbilityContext context);
        void Use(AbilityContext context);
    }
}
