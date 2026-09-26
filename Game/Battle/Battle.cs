using Game.Characters;

namespace Game.Battle
{
    public class QueuedAction
    {
        public required Character Caster { get; init; }
        public required Character Target { get; init; }
        public required Ability Ability { get; init; }
    }

    public class Battle
    {
        public List<Team> Teams { get; }

        // Le combat est terminé s'il reste au maximum 1 équipe en vie
        public bool IsOver => Teams.Count(t => !t.IsDefeated) <= 1;

        // Renvoie l'unique équipe gagnante (ou null si tout le monde est K.O.)
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
                DisplayTeamStatus(team);
                Console.WriteLine("------------------------------------------------------");
            }
            Console.WriteLine("======================================================\n");
        }

        private static void DisplayTeamStatus(Team team)
        {
            string statusTag = team.IsDefeated ? " [ÉQUIPE ÉLIMINÉE]" : "";
            Console.WriteLine($"ÉQUIPE : {team.Name.ToUpper()}{statusTag}");

            foreach (var member in team.Members)
            {
                string hpStatus = member.IsAlive ? $"{member.CurrentHp}/{member.BaseStats.MaxHp} PV" : "[K.O.]";
                Console.WriteLine($"  - {member.Name,-14} | {hpStatus,-14} | Atk: {member.BaseStats.Attack,2} | Def: {member.BaseStats.Defense,2} | Vit: {member.BaseStats.Speed,2}");
            }
        }

        /// <summary>
        /// Résout un tour complet d'actions triées par vitesse.
        /// </summary>
        public void ExecuteRound(List<QueuedAction> actions)
        {
            Console.WriteLine("--- DÉBUT DU TOUR ---");

            var sortedActions = actions.OrderByDescending(a => a.Caster.BaseStats.Speed).ToList();

            foreach (var action in sortedActions)
            {
                if (!action.Caster.IsAlive)
                {
                    Console.WriteLine($"\n> {action.Caster.Name} est K.O. et ne peut pas agir.");
                    continue;
                }

                Console.WriteLine($"\n> [{action.Caster.BaseStats.Speed} Vit] {action.Caster.Name} lance '{action.Ability.Name}' sur {action.Target.Name}");

                var context = new AbilityContext
                {
                    Casters = [action.Caster],
                    Targets = [action.Target],
                    //Battle = this
                };

                foreach (var effect in action.Ability.Effects)
                {
                    IEffectResult result = effect.Execute(context);
                    context.Results.Add(result);
                }
            }

            Console.WriteLine("\n--- FIN DU TOUR ---");
        }

        /// <summary>
        /// Boucle de simulation automatique jusqu'à la fin du combat.
        /// </summary>
        public void RunLoop(Func<List<QueuedAction>> getActionsForRound)
        {
            int roundNumber = 1;

            while (!IsOver)
            {
                Console.WriteLine($"\n=================== TOUR {roundNumber} ===================");
                DisplayStatus();

                var actions = getActionsForRound();
                ExecuteRound(actions);

                roundNumber++;
            }

            DisplayStatus();
            Console.WriteLine("\n=================== FIN DU COMBAT ===================");

            if (Winner != null)
            {
                Console.WriteLine($"Victoire de l'équipe : {Winner.Name} !");
            }
            else
            {
                Console.WriteLine("Toutes les équipes ont été éliminées ! Égalité.");
            }
        }
    }
}
