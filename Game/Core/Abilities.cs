namespace Game.Core
{
    public interface IAbility
    {
        string Id { get; }
        string Name { get; }
        IReadOnlyList<ITargetsFilter> TargetsFilters { get; }
        int RequiredTargetsCount { get; }
        int RequiredParticipantsCount { get; }
        IReadOnlyList<ISequence> Sequences { get; }

        IReadOnlyList<Character> GetAvailableTargets(AbilityContext context);
        AbilityValidationResult Validate(AbilityContext context);
        void PayCost(AbilityContext context);
        void Use(AbilityContext context);
    }

    public abstract class BaseAbility : IAbility
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public int RequiredTargetsCount { get; init; } = 1;
        public IReadOnlyList<ITargetsFilter> TargetsFilters { get; init; } = [];
        public int RequiredParticipantsCount { get; init; } = 0;
        public required IReadOnlyList<ISequence> Sequences { get; init; }

        public IReadOnlyList<Character> GetAvailableTargets(AbilityContext context)
        {
            IReadOnlyList<Character> availableTargets = context.Battle.Teams.SelectMany(team => team.Members).ToList();

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

    public sealed class Skill : BaseAbility;

    public sealed class Spell : BaseAbility
    {
        public required int ManaCost { get; init; }

        protected override AbilityValidationResult ValidateSpecific(AbilityContext context)
        {
            if (context.Initiator.CurrentMp < ManaCost) return AbilityValidationResult.Invalid(AbilityValidationError.NotEnoughMana);

            return AbilityValidationResult.Valid();
        }

        public override void PayCost(AbilityContext context)
        {
            context.Initiator.ConsumeMp(ManaCost);
        }
    }

    public sealed class Synergy : BaseAbility
    {
        public required int RequiredSynergyBars { get; init; }

        protected override AbilityValidationResult ValidateSpecific(AbilityContext context)
        {
            var team = context.Battle.Teams.First(t => t.Members.Contains(context.Initiator));

            if (!team.SynergyGauge.HasEnoughBars(RequiredSynergyBars)) return AbilityValidationResult.Invalid(AbilityValidationError.NotEnoughSynergy);

            return AbilityValidationResult.Valid();
        }

        public override void PayCost(AbilityContext context)
        {
            var team = context.Battle.Teams.First(t => t.Members.Contains(context.Initiator));

            team.SynergyGauge.ConsumeBars(RequiredSynergyBars);
        }
    }
}
