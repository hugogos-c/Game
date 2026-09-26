namespace Game.Core
{
    public interface ITargetSelector
    {
        IReadOnlyList<Character> Select(AbilityContext context);
    }

    public class AbilityTargetsSelector : ITargetSelector
    {
        public IReadOnlyList<Character> Select(AbilityContext context)
        {
            return context.Targets;
        }
    }

    public class CastersSelector : ITargetSelector
    {
        public IReadOnlyList<Character> Select(AbilityContext context)
        {
            return context.Casters;
        }
    }
}
