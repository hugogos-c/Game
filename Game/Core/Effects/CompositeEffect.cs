namespace Game.Core.Effects
{
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
