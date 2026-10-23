namespace Game.Core.Abilities
{
    public sealed class Synergy : BaseAbility
    {
        public required int RequiredSynergyBars { get; init; }

        protected override AbilityValidationResult ValidateSpecific(AbilityContext context)
        {
            if (!context.Initiator.Team.SynergyGauge.HasEnoughBars(RequiredSynergyBars)) return AbilityValidationResult.Invalid(AbilityValidationError.NotEnoughSynergy);

            return AbilityValidationResult.Valid();
        }

        public override void PayCost(AbilityContext context)
        {
            context.Initiator.Team.SynergyGauge.ConsumeBars(RequiredSynergyBars);
        }
    }
}
