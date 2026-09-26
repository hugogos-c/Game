namespace Game.Core
{
    public interface IEffect
    {
        IEffectResult Apply(EffectContext context);
    }

    public sealed class DamageEffect : IEffect
    {
        public required int Power { get; init; }

        public IEffectResult Apply(EffectContext context)
        {
            int combinedAttack = context.AbilityContext.Initiator.BaseStats.Attack + context.AbilityContext.Participants.Sum(c => c.BaseStats.Attack);
            string castersName = string.Join(" + ", [context.AbilityContext.Initiator.Name, ..context.AbilityContext.Participants.Select(c => c.Name)]);

            int rawDamage = Power + combinedAttack;
            int finalDamage = Math.Max(1, rawDamage - context.Target.BaseStats.Defense);

            context.Target.TakeDamage(finalDamage);
            Console.WriteLine($"      ⚡ {castersName} inflige {finalDamage} dégâts à {context.Target.Name} ! ({context.Target.CurrentHp}/{context.Target.BaseStats.MaxHp} PV)");

            return new GenericResult();
        }
    }

    public sealed class HealEffect : IEffect
    {
        public required int Amount { get; init; }

        public IEffectResult Apply(EffectContext context)
        {
            context.Target.Heal(Amount);
            Console.WriteLine($"      💚 {context.Target.Name} récupère {Amount} PV ! ({context.Target.CurrentHp}/{context.Target.BaseStats.MaxHp} PV)");

            return new GenericResult();
        }
    }

    public sealed class ConditionalEffect : IEffect
    {
        public required ICondition Condition { get; init; }
        public required IEffect Then { get; init; }
        public IEffect? Else { get; init; }

        public IEffectResult Apply(EffectContext context)
        {
            if (Condition.Evaluate(context)) return Then.Apply(context);

            return Else?.Apply(context) ?? new GenericResult();
        }
    }

    public sealed class CompositeEffect : IEffect
    {
        public required IReadOnlyList<IEffect> Effects { get; init; }

        public IEffectResult Apply(EffectContext context)
        {
            foreach (var effect in Effects)
            {
                if (context.AbilityContext.IsAborted) break;

                effect.Apply(context);
            }

            return new GenericResult();
        }
    }
}
