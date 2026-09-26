namespace Game.Core
{
    public interface ISequence
    {
        ITargetsSelector TargetSelector { get; }
        IReadOnlyList<IEffect> Effects { get; }
        void Execute(AbilityContext context);
    }

    public sealed class Sequence : ISequence
    {
        public required ITargetsSelector TargetSelector { get; init; }
        public required IReadOnlyList<IEffect> Effects { get; init; }

        public void Execute(AbilityContext context)
        {
            var targets = TargetSelector.Select(context);

            foreach (var effect in Effects)
            {
                if (context.IsAborted) break;

                foreach (var target in targets)
                {
                    var result = effect.Apply(new EffectContext
                    {
                        AbilityContext = context,
                        Target = target,
                    });

                    context.Results.Add(result);
                }
            }
        }
    }
}
