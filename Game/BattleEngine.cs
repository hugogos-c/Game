namespace Game
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

        public void DisplayStatus()
        {
            Console.WriteLine("\n=================== ÉTAT DU COMBAT ===================");
            foreach (var team in Teams)
            {
                string statusTag = team.IsDefeated ? " [ÉQUIPE ÉLIMINÉE]" : "";
                Console.WriteLine($"ÉQUIPE : {team.Name.ToUpper()} (Synergie : {team.SynergyGauge.CurrentBars}/{team.SynergyGauge.MaxBars}){statusTag}");

                foreach (var member in team.Members)
                {
                    string hpStatus = member.IsAlive ? $"{member.CurrentHp}/{member.BaseStats.MaxHp} PV" : "[K.O.]";
                    string mpStatus = $"{member.CurrentMp}/{member.BaseStats.MaxMp} PM";
                    Console.WriteLine($"  - {member.Name,-12} | {hpStatus,-14} | {mpStatus,-10} | Vit: {member.BaseStats.Speed,2}");
                }
                Console.WriteLine("------------------------------------------------------");
            }
        }

        public void ExecuteRound(List<QueuedAction> actions)
        {
            Console.WriteLine("--- DÉBUT DU TOUR ---");
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

            Console.WriteLine("\n--- FIN DU TOUR ---");
        }
    }
}
