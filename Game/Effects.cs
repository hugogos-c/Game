namespace Game
{
    public interface IEffect
    {
        IEffectResult Execute(AbilityContext context);
    }

    // Degats generiques relies a l'attaque des lanceurs
    public class DealDamage : IEffect
    {
        public required int Power { get; init; }

        public IEffectResult Execute(AbilityContext context)
        {
            int combinedAttack = context.Casters.Sum(c => c.BaseStats.Attack);
            string castersName = string.Join(" + ", context.Casters.Select(c => c.Name));

            foreach (var target in context.Targets)
            {
                int rawDamage = Power + combinedAttack;
                int finalDamage = Math.Max(1, rawDamage - target.BaseStats.Defense);

                target.TakeDamage(finalDamage);
                Console.WriteLine($"      ⚡ {castersName} inflige {finalDamage} dégâts à {target.Name} ! ({target.CurrentHp}/{target.BaseStats.MaxHp} PV)");
            }

            return new GenericResult();
        }
    }

    // Soin generique sur la/les cibles
    public class Heal : IEffect
    {
        public required int Amount { get; init; }

        public IEffectResult Execute(AbilityContext context)
        {
            foreach (var target in context.Targets)
            {
                target.Heal(Amount);
                Console.WriteLine($"      💚 {target.Name} récupère {Amount} PV ! ({target.CurrentHp}/{target.BaseStats.MaxHp} PV)");
            }

            return new GenericResult();
        }
    }

    // Soin generique sur le/les lanceurs (utile pour les synergies)
    public class HealCasters : IEffect
    {
        public required int Amount { get; init; }

        public IEffectResult Execute(AbilityContext context)
        {
            foreach (var caster in context.Casters)
            {
                caster.Heal(Amount);
                Console.WriteLine($"      💚 {caster.Name} récupère {Amount} PV (Soin de zone) ! ({caster.CurrentHp}/{caster.BaseStats.MaxHp} PV)");
            }

            return new GenericResult();
        }
    }
}
