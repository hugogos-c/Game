using Game;
using Game.Battle;
using Game.Characters;

class Program
{
    static void Main(string[] args)
    {
        // 1. Définition des compétences
        var coupEpee = new Ability
        {
            Id = "coup_epee",
            Name = "Coup d'épée",
            Effects = new List<IEffect> { new DamageEffect { Power = 30, } }
        };

        var dechargeElectrique = new Ability
        {
            Id = "decharge_electrique",
            Name = "Décharge électrique",
            Effects = new List<IEffect> { new DamageEffect { Power = 40, } }
        };

        var soin = new Ability
        {
            Id = "soin",
            Name = "Soin",
            Effects = new List<IEffect> { new HealEffect { Amount = new ConstantValue { Amount = 60 } } }
        };

        // 2. Création des personnages
        var aldrick = new Character("Aldrick", new Stats(MaxHp: 400, Attack: 35, Defense: 20, Speed: 40), [coupEpee, soin]);
        var veya = new Character("Veya", new Stats(MaxHp: 300, Attack: 50, Defense: 10, Speed: 80), [coupEpee, dechargeElectrique]);

        var gobelin1 = new Character("Gobelin 1", new Stats(MaxHp: 150, Attack: 25, Defense: 5, Speed: 60), [coupEpee]);
        var gobelin2 = new Character("Gobelin 2", new Stats(MaxHp: 150, Attack: 25, Defense: 5, Speed: 50), [coupEpee]);

        var mercenaire = new Character("Mercenaire", new Stats(MaxHp: 200, Attack: 45, Defense: 10, Speed: 70), [coupEpee, soin]);

        // 3. Constitution des équipes et du combat
        var heroesTeam = new Team("Héros", [aldrick, veya]);
        var monstersTeam = new Team("Gobelins", [gobelin1, gobelin2]);
        var renegadesTeam = new Team("Mercenaires", [mercenaire]);

        var battle = new Battle([heroesTeam, monstersTeam, renegadesTeam]);

        int roundNumber = 1;

        // =================================================================
        // BOUCLE INTERACTIVE DE COMBAT
        // =================================================================
        while (!battle.IsOver)
        {
            Console.Clear();
            Console.WriteLine($"=================== TOUR {roundNumber} ===================");
            battle.DisplayStatus();

            var roundActions = new List<QueuedAction>();

            // Récupération de tous les combattants encore en vie
            var activeCharacters = battle.Teams
                .SelectMany(t => t.Members)
                .Where(c => c.IsAlive)
                .ToList();

            // PHASE 1 : Sélection des actions pour chaque personnage
            foreach (var caster in activeCharacters)
            {
                Console.WriteLine($"\n>>> Action pour : {caster.Name} ({caster.CurrentHp}/{caster.BaseStats.MaxHp} PV)");

                //var availableAbilities = characterAbilities[caster];

                // Choisir la capacité
                Console.WriteLine("Choisir une compétence :");
                for (int i = 0; i < caster.Abilities.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {caster.Abilities[i].Name}");
                }
                int abilityIndex = ReadChoice(caster.Abilities.Count);
                var chosenAbility = caster.Abilities[abilityIndex];

                // Récupération de toutes les cibles vivantes sur le terrain
                var livingTargets = battle.Teams
                    .SelectMany(t => t.Members)
                    .Where(m => m.IsAlive)
                    .ToList();

                // Choisir la cible
                Console.WriteLine($"Choisir la cible pour '{chosenAbility.Name}' :");
                for (int i = 0; i < livingTargets.Count; i++)
                {
                    var target = livingTargets[i];
                    Console.WriteLine($"  {i + 1}. {target.Name} ({target.CurrentHp}/{target.BaseStats.MaxHp} PV)");
                }
                int targetIndex = ReadChoice(livingTargets.Count);
                var chosenTarget = livingTargets[targetIndex];

                roundActions.Add(new QueuedAction
                {
                    Caster = caster,
                    Target = chosenTarget,
                    Ability = chosenAbility
                });
            }

            // PHASE 2 : Exécution du tour
            Console.WriteLine("\n[Toutes les actions sont validées. Appuyez sur Entrée pour lancer le tour...]");
            Console.ReadLine();

            Console.Clear();
            battle.ExecuteRound(roundActions);

            roundNumber++;

            if (!battle.IsOver)
            {
                Console.WriteLine("\n[Appuyez sur Entrée pour passer au tour suivant...]");
                Console.ReadLine();
            }
        }

        // =================================================================
        // FIN DU COMBAT
        // =================================================================
        Console.WriteLine("\n=================== COMBAT TERMINÉ ===================");
        battle.DisplayStatus();

        if (battle.Winner != null)
        {
            Console.WriteLine($"Victoire de l'équipe : {battle.Winner.Name} !");
        }
        else
        {
            Console.WriteLine("Égalité ! Aucun survivant sur le terrain.");
        }

        Console.ReadLine();
    }

    /// <summary>
    /// Utilitaire pour lire une saisie utilisateur sécurisée dans la console.
    /// </summary>
    private static int ReadChoice(int maxOptions)
    {
        while (true)
        {
            Console.Write("> Choix : ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= maxOptions)
            {
                return choice - 1; // Convertit l'index affiché (1..N) en index tableau (0..N-1)
            }

            Console.WriteLine($"Saisie invalide. Veuillez entrer un nombre entre 1 et {maxOptions}.");
        }
    }
}