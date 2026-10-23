namespace Game.Core.Effects
{
    public sealed class DamageEffect : IEffect
    {
        public required int Power { get; init; }

        public IEffectResult Apply(EffectContext context)
        {
            int combinedAttack = context.AbilityContext.Initiator.BaseStats.Attack + context.AbilityContext.Participants.Sum(c => c.BaseStats.Attack);
            string castersName = string.Join(" + ", [context.AbilityContext.Initiator.Name, .. context.AbilityContext.Participants.Select(c => c.Name)]);

            int rawDamage = Power + combinedAttack;
            int finalDamage = Math.Max(1, rawDamage - context.CurrentTarget.BaseStats.Defense);

            context.CurrentTarget.TakeDamage(finalDamage);
            Console.WriteLine($"      ⚡ {castersName} inflige {finalDamage} dégâts à {context.CurrentTarget.Name} ! ({context.CurrentTarget.CurrentHp}/{context.CurrentTarget.BaseStats.MaxHp} PV)");

            return new GenericResult();
        }
    }
}
