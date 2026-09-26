namespace Game.Core
{
    public interface IEffect
    {
        IEffectResult Execute(EffectContext context);
    }

    public class DamageEffect : IEffect
    {
        public required int Power { get; init; }

        public IEffectResult Execute(EffectContext context)
        {
            int combinedAttack = context.AbilityContext.Casters.Sum(c => c.BaseStats.Attack);
            string castersName = string.Join(" + ", context.AbilityContext.Casters.Select(c => c.Name));

            int rawDamage = Power + combinedAttack;
            int finalDamage = Math.Max(1, rawDamage - context.Target.BaseStats.Defense);

            context.Target.TakeDamage(finalDamage);
            Console.WriteLine($"      ⚡ {castersName} inflige {finalDamage} dégâts à {context.Target.Name} ! ({context.Target.CurrentHp}/{context.Target.BaseStats.MaxHp} PV)");

            return new GenericResult();
        }
    }

    public class HealEffect : IEffect
    {
        public required int Amount { get; init; }

        public IEffectResult Execute(EffectContext context)
        {
            context.Target.Heal(Amount);
            Console.WriteLine($"      💚 {context.Target.Name} récupère {Amount} PV ! ({context.Target.CurrentHp}/{context.Target.BaseStats.MaxHp} PV)");

            return new GenericResult();
        }
    }
}
