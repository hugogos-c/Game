namespace Game.Core.Battles
{
    public static class BattleEngine
    {
        public static void ExecuteRound(Battle battle, List<BattleAction> actions)
        {
            var sortedActions = actions.OrderByDescending(a => a.Speed).ToList();

            foreach (var action in sortedActions)
            {
                var initialContext = new AbilityContext
                {
                    Initiator = action.Initiator,
                    Participants = action.Participants,
                    Targets = action.Targets,
                    Battle = battle,
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
                    Battle = battle,
                };

                var validation = action.Ability.Validate(context);

                if (!validation.IsValid)
                {
                    Console.WriteLine($"\n> Action annulée : {validation.Error}");
                    continue;
                }

                #region À mettre dans QueuedAction.Execute()
                action.Ability.PayCost(context);

                string castersName = string.Join(" & ", [action.Initiator.Name, .. action.Participants.Select(c => c.Name)]);
                Console.WriteLine($"\n> Exec : '{action.Ability.Name}' par {castersName}");

                action.Ability.Use(context);
                #endregion
            }

            battle.AdvanceRound();
        }
    }
}
