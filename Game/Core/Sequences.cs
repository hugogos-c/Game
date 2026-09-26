namespace Game.Core
{
    public interface ISequence
    {
        ITargetSelector TargetSelector { get; }
        IReadOnlyList<IEffect> Effects { get; }
        void Execute(AbilityContext context);
    }

    public class Sequence : ISequence
    {
        public required ITargetSelector TargetSelector { get; init; }
        public required IReadOnlyList<IEffect> Effects { get; init; }

        public void Execute(AbilityContext context)
        {
            var targets = TargetSelector.Select(context);

            foreach (var effect in Effects)
            {
                if (context.IsAborted) break;

                foreach (var target in targets)
                {
                    var result = effect.Execute(new EffectContext
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
