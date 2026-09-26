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

    public class ConditionalEffect : IEffect
    {
        public required ICondition Condition { get; init; }
        public required IEffect Then { get; init; }
        public IEffect? Else { get; init; }

        public IEffectResult Execute(EffectContext context)
        {
            if (Condition.Evaluate(context))
            {
                return Then.Execute(context);
            }

            return Else?.Execute(context) ?? new GenericResult();
        }
    }

    public class CompositeEffect : IEffect
    {
        public required IReadOnlyList<IEffect> Effects { get; init; }

        public IEffectResult Execute(EffectContext context)
        {
            foreach (var effect in Effects)
            {
                if (context.AbilityContext.IsAborted) break;

                effect.Execute(context);
            }

            return new GenericResult();
        }
    }
}
