namespace Game.Core
{
    public interface ITargetsFilter
    {
        IReadOnlyList<Character> Filter(IReadOnlyList<Character> candidates, AbilityContext context);
    }

    public sealed class OrFilter : ITargetsFilter
    {
        public required IReadOnlyList<ITargetsFilter> Filters { get; init; }

        public IReadOnlyList<Character> Filter(IReadOnlyList<Character> candidates, AbilityContext context)
        {
            var matchingCharacters = new HashSet<Character>();

            foreach (var filter in Filters)
            {
                var filteredCharacters = filter.Filter(candidates, context);

                foreach (var filteredCharacter in filteredCharacters)
                {
                    matchingCharacters.Add(filteredCharacter);
                }
            }

            return candidates.Where(matchingCharacters.Contains).ToList();
        }
    }

    public sealed class EnemiesFilter : ITargetsFilter
    {
        public IReadOnlyList<Character> Filter(IReadOnlyList<Character> candidates, AbilityContext context)
        {
            return candidates.Where(target => !context.Initiator.Team.Members.Contains(target)).ToList();
        }
    }

    public sealed class AlliesFilter : ITargetsFilter
    {
        public IReadOnlyList<Character> Filter(IReadOnlyList<Character> candidates, AbilityContext context)
        {
            return candidates.Where(context.Initiator.Team.Members.Contains).ToList();
        }
    }

    public sealed class IsAliveFilter : ITargetsFilter
    {
        public IReadOnlyList<Character> Filter(IReadOnlyList<Character> candidates, AbilityContext context)
        {
            return candidates.Where(target => target.IsAlive).ToList();
        }
    }

    public sealed class IsDeadFilter : ITargetsFilter
    {
        public IReadOnlyList<Character> Filter(IReadOnlyList<Character> candidates, AbilityContext context)
        {
            return candidates.Where(target => !target.IsAlive).ToList();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ITargetsSelector
    {
        IReadOnlyList<Character> Select(AbilityContext context);
    }

    public sealed class AbilityTargetsSelector : ITargetsSelector
    {
        public IReadOnlyList<Character> Select(AbilityContext context)
        {
            return context.Targets;
        }
    }

    public sealed class AbilityUsersSelector : ITargetsSelector
    {
        public IReadOnlyList<Character> Select(AbilityContext context)
        {
            return [context.Initiator, ..context.Participants];
        }
    }
}
