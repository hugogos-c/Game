using Game.Core.Values;

namespace Game.Core.Effects
{
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
}
