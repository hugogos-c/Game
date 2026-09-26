using Game.Characters;

namespace Game
{
    public interface IEffectResult { }

    public class AbilityContext
    {
        public required IReadOnlyList<Character> Casters { get; init; }
        public required IReadOnlyList<Character> Targets { get; init; }
        public List<IEffectResult> Results { get; } = [];
    }

    public interface IEffect
    {
        IEffectResult Execute(AbilityContext context);
    }

    public abstract class Value
    {
        public abstract int Resolve(AbilityContext context);
    }
    public class ConstantValue : Value
    {
        public required int Amount { get; init; }
        public override int Resolve(AbilityContext context) => Amount;
    }

    public class Ability
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required List<IEffect> Effects { get; init; }
    }

    #region Damage
    public class DamageResult : IEffectResult
    {
        public required Character Target { get; init; }
        public required int DamageDealt { get; init; }
    }

    public class DamageEffect : IEffect
    {
        public required int Power { get; init; }

        public IEffectResult Execute(AbilityContext context)
        {
            var caster = context.Casters[0];
            var target = context.Targets[0];

            int rawDamage = Power + caster.BaseStats.Attack;
            int finalDamage = Math.Max(1, rawDamage - target.BaseStats.Defense);

            target.TakeDamage(finalDamage);

            Console.WriteLine($"[Effet] {caster.Name} inflige {finalDamage} dégâts à {target.Name}");

            return new DamageResult
            {
                Target = target,
                DamageDealt = finalDamage
            };
        }
    }
    #endregion

    #region Heal
    public class HealEffectResult : IEffectResult
    {
        public required Character Target { get; init; }
        public required int AmountHealed { get; init; }
    }

    public class HealEffect: IEffect
    {
        public required Value Amount { get; init; }

        public IEffectResult Execute(AbilityContext context)
        {
            var target = context.Targets[0];

            int healAmount = Amount.Resolve(context);

            target.Heal(healAmount);

            Console.WriteLine($"[Effet] {target.Name} récupère {healAmount} PV");

            return new HealEffectResult
            {
                Target = target,
                AmountHealed = healAmount
            };
        }
    }
    #endregion
}
