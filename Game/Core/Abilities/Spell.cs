namespace Game.Core.Abilities
{
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
}
