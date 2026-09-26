namespace Game.Core
{
    public class QueuedAction
    {
        public required IReadOnlyList<Character> Casters { get; init; }
        public required IReadOnlyList<Character> Targets { get; init; }
        public required IAbility Ability { get; init; }

        public int Speed => Casters.Max(c => c.BaseStats.Speed);
    }

    public static class TargetResolver
    {
        public static List<Character> ResolveAuto(
            TargetMode mode,
            Character mainCaster,
            Team casterTeam,
            List<Team> allTeams)
        {
            var allies = casterTeam.Members.Where(m => m.IsAlive).ToList();
            var enemies = allTeams.Where(t => t != casterTeam).SelectMany(t => t.Members).Where(m => m.IsAlive).ToList();

            return mode switch
            {
                TargetMode.Self => new List<Character> { mainCaster },
                TargetMode.AllAllies => allies,
                TargetMode.AllEnemies => enemies,
                _ => new List<Character>()
            };
        }
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
                var primaryCaster = action.Casters.First();
                if (!primaryCaster.IsAlive)
                {
                    Console.WriteLine($"\n> Action annulée : {primaryCaster.Name} est K.O.");
                    continue;
                }

                if (!action.Ability.CanUse(action.Casters, this))
                {
                    Console.WriteLine($"\n> {primaryCaster.Name} n'a plus les ressources pour exécuter '{action.Ability.Name}'.");
                    continue;
                }

                var validTargets = action.Targets.Where(t => t.IsAlive).ToList();
                if (validTargets.Count == 0 && action.Ability.TargetMode != TargetMode.Self)
                {
                    Console.WriteLine($"\n> '{action.Ability.Name}' échoue : aucune cible valide.");
                    continue;
                }

                action.Ability.PayCost(action.Casters, this);

                var context = new AbilityContext
                {
                    Casters = action.Casters,
                    Targets = validTargets,
                    Battle = this
                };

                string castersName = string.Join(" & ", action.Casters.Select(c => c.Name));
                Console.WriteLine($"\n> Exec : '{action.Ability.Name}' par {castersName}");

                action.Ability.Execute(context);
            }
        }
    }
}
