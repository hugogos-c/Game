namespace Game.Core
{
    public class QueuedAction
    {
        public required Character Initiator { get; init; }
        public required IReadOnlyList<Character> Participants { get; init; }
        public required IReadOnlyList<Character> Targets { get; init; }
        public required IAbility Ability { get; init; }

        public int Speed => Initiator.BaseStats.Speed;
    }

    public class Battle
    {
        public List<Team> Teams { get; }

        public bool IsOver => Teams.Count(t => !t.IsDefeated) <= 1;
        public Team? Winner => IsOver ? Teams.FirstOrDefault(t => !t.IsDefeated) : null;

        public Battle(IEnumerable<Team> teams)
        {
            Teams = teams.ToList();
        }

        public void ExecuteRound(List<QueuedAction> actions)
        {
            var sortedActions = actions.OrderByDescending(a => a.Speed).ToList();

            foreach (var action in sortedActions)
            {
                var initialContext = new AbilityContext
                {
                    Initiator = action.Initiator,
                    Participants = action.Participants,
                    Targets = action.Targets,
                    Battle = this
                };

                var availableTargets = action.Ability.GetAvailableTargets(initialContext);

                var validTargets = initialContext.Targets.Where(availableTargets.Contains).ToList();

                if (initialContext.Targets.Any() && (!availableTargets.Any() || validTargets.Count == 0))
                {
                    Console.WriteLine($"\n> Action annulée : aucune cible valide");
                    continue;
                }

                var context = new AbilityContext
                {
                    Initiator = action.Initiator,
                    Participants = action.Participants,
                    Targets = validTargets,
                    Battle = this
                };

                var validation = action.Ability.Validate(context);

                if (!validation.IsValid)
                {
                    Console.WriteLine($"\n> Action annulée : {validation.Error}");
                    continue;
                }

                #region À mettre dans QueuedAction.Execute()
                action.Ability.PayCost(context);

                string castersName = string.Join(" & ", [action.Initiator.Name, ..action.Participants.Select(c => c.Name)]);
                Console.WriteLine($"\n> Exec : '{action.Ability.Name}' par {castersName}");

                action.Ability.Use(context);
                #endregion
            }
        }
    }
}
