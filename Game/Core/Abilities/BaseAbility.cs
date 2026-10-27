using Game.Core.Entities;

namespace Game.Core.Abilities
{
    public abstract class BaseAbility : IAbility
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public int RequiredTargetsCount { get; init; } = 1;
        public IReadOnlyList<ITargetsFilter> TargetsFilters { get; init; } = [];
        public int RequiredParticipantsCount { get; init; } = 0;
        public required IReadOnlyList<ISequence> Sequences { get; init; }

        public IReadOnlyList<Combatant> GetAvailableTargets(AbilityContext context)
        {
            IReadOnlyList<Combatant> availableTargets = context.Battle.Teams.SelectMany(team => team.Members).ToList();

            foreach (var filter in TargetsFilters)
            {
                availableTargets = filter.Filter(availableTargets, context);
            }

            return availableTargets;
        }

        public AbilityValidationResult Validate(AbilityContext context)
        {
            if (!context.Initiator.IsAlive) return AbilityValidationResult.Invalid(AbilityValidationError.InitiatorDead);

            if (context.Participants.Any(p => !p.IsAlive)) return AbilityValidationResult.Invalid(AbilityValidationError.ParticipantDead);

            return ValidateSpecific(context);
        }

        protected virtual AbilityValidationResult ValidateSpecific(AbilityContext context) => AbilityValidationResult.Valid();

        public virtual void PayCost(AbilityContext context) { }

        public virtual void Use(AbilityContext context)
        {
            foreach (var sequence in Sequences)
            {
                if (context.IsAborted) break;

                sequence.Execute(context);
            }
        }
    }
}
