namespace Game.Core
{
    public interface IEffect
    {
        IEffectResult Apply(EffectContext context);
    }

    #region Damage
    public sealed class DamageEffect : IEffect
    {
        public required int Power { get; init; }

        public IEffectResult Apply(EffectContext context)
        {
            int combinedAttack = context.AbilityContext.Initiator.BaseStats.Attack + context.AbilityContext.Participants.Sum(c => c.BaseStats.Attack);
            string castersName = string.Join(" + ", [context.AbilityContext.Initiator.Name, ..context.AbilityContext.Participants.Select(c => c.Name)]);

            int rawDamage = Power + combinedAttack;
            int finalDamage = Math.Max(1, rawDamage - context.CurrentTarget.BaseStats.Defense);

            context.CurrentTarget.TakeDamage(finalDamage);
            Console.WriteLine($"      ⚡ {castersName} inflige {finalDamage} dégâts à {context.CurrentTarget.Name} ! ({context.CurrentTarget.CurrentHp}/{context.CurrentTarget.BaseStats.MaxHp} PV)");

            return new GenericResult();
        }
    }
    #endregion

    #region Heal
    public sealed class HealEffect : IEffect
    {
        public required IValue Amount { get; init; }

        public IEffectResult Apply(EffectContext context)
        {
            var amount = Amount.Resolve(context);

            context.CurrentTarget.Heal(amount);
            Console.WriteLine($"      💚 {context.CurrentTarget.Name} récupère {amount} PV ! ({context.CurrentTarget.CurrentHp}/{context.CurrentTarget.BaseStats.MaxHp} PV)");

            return new GenericResult();
        }
    }
    #endregion

    #region Composite
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
    #endregion

    #region Conditional
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
    #endregion

    #region Chance - Sucre syntaxique
    public sealed class ChanceEffect : IEffect
    {
        public required IValue Chance { get; init; }
        public required IEffect Success { get; init; }
        public IEffect? Failure { get; init; }

        public IEffectResult Apply(EffectContext context)
        {
            return new ConditionalEffect
            {
                Condition = new ComparisonCondition
                {
                    Left = new RandomValue { Min = new ConstantValue { Value = 1 }, Max = new ConstantValue { Value = 100 }, },
                    Operator = ComparisonOperator.LessThanOrEqual,
                    Right = Chance,
                },
                Then = Success,
                Else = Failure,
            }.Apply(context);
        }
    }
    #endregion
}
