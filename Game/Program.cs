using Game.Core;

namespace Game
{
    class Program
    {
        static void Main(string[] args)
        {
            // --------------------------------------------------------------------
            // 1. DÉFINITION DES CAPACITÉS
            // --------------------------------------------------------------------
            var coupEpee = new Skill
            {
                Id = "coup_epee",
                Name = "Coup d'épée",
                TargetMode = TargetMode.SingleEnemy,
                Effects = new List<IEffect>
                {
                    new DamageEffect
                    {
                        Selector = new AbilityTargetsSelector(),
                        Power = 30
                    }
                }
            };

            var dechargeElectrique = new Skill
            {
                Id = "decharge_electrique",
                Name = "Décharge électrique",
                TargetMode = TargetMode.SingleEnemy,
                Effects = new List<IEffect>
                {
                    new DamageEffect
                    {
                        Selector = new AbilityTargetsSelector(),
                        Power = 25
                    }
                }
            };

            var soin = new Skill
            {
                Id = "soin",
                Name = "Soin",
                TargetMode = TargetMode.SingleAlly,
                Effects = new List<IEffect>
                {
                    new HealEffect
                    {
                        Selector = new AbilityTargetsSelector(),
                        Amount = 60
                    }
                }
            };

            var feu = new Spell
            {
                Id = "feu",
                Name = "Feu",
                TargetMode = TargetMode.SingleEnemy,
                ManaCost = 10,
                Effects = new List<IEffect>
                {
                    new DamageEffect
                    {
                        Selector = new AbilityTargetsSelector(),
                        Power = 45
                    }
                }
            };

            var foudre = new Spell
            {
                Id = "foudre",
                Name = "Foudre",
                TargetMode = TargetMode.SingleEnemy,
                ManaCost = 15,
                Effects = new List<IEffect>
                {
                    new DamageEffect
                    {
                        Selector = new AbilityTargetsSelector(),
                        Power = 55
                    }
                }
            };

            var amourImpossible = new Synergy
            {
                Id = "amour_impossible",
                Name = "Amour Impossible",
                TargetMode = TargetMode.SingleEnemy,
                RequiredCastersCount = 2,
                RequiredSynergyBars = 2,
                Effects = new List<IEffect>
                {
                    new HealEffect
                    {
                        Selector = new CastersSelector(),
                        Amount = 80
                    },
                    new DamageEffect
                    {
                        Selector = new AbilityTargetsSelector(),
                        Power = 100
                    }
                }
            };

            var comboGobelin = new Synergy
            {
                Id = "combo_gobelin",
                Name = "Combo Gobelin",
                TargetMode = TargetMode.SingleEnemy,
                RequiredCastersCount = 2,
                RequiredSynergyBars = 1,
                Effects = new List<IEffect>
                {
                    new DamageEffect
                    {
                        Selector = new AbilityTargetsSelector(),
                        Power = 70
                    }
                }
            };

            // --------------------------------------------------------------------
            // 2. INSTANCIATION DES PERSONNAGES
            // --------------------------------------------------------------------
            var aldrick = new Character(
                "Aldrick",
                new Stats(MaxHp: 400, MaxMp: 30, Attack: 35, Defense: 15, Speed: 40),
                [coupEpee, feu, soin, amourImpossible]
            );

            var veya = new Character(
                "Veya",
                new Stats(MaxHp: 300, MaxMp: 60, Attack: 45, Defense: 10, Speed: 80),
                [dechargeElectrique, foudre, soin, amourImpossible]
            );

            var gobelin1 = new Character(
                "Gobelin 1",
                new Stats(MaxHp: 160, MaxMp: 0, Attack: 25, Defense: 5, Speed: 60),
                [coupEpee, comboGobelin]
            );

            var gobelin2 = new Character(
                "Gobelin 2",
                new Stats(MaxHp: 160, MaxMp: 0, Attack: 25, Defense: 5, Speed: 50),
                [coupEpee, comboGobelin]
            );

            var mercenaire = new Character(
                "Mercenaire",
                new Stats(MaxHp: 220, MaxMp: 20, Attack: 40, Defense: 10, Speed: 70),
                [coupEpee, soin]
            );

            // --------------------------------------------------------------------
            // 3. CONSTITUTION DES ÉQUIPES ET DU COMBAT
            // --------------------------------------------------------------------
            var playerTeam = new Team("Joueur", [aldrick, veya]);
            var goblinsTeam = new Team("Gobelins", [gobelin1, gobelin2]);
            var mercenaryTeam = new Team("Mercenaire", [mercenaire]);

            var battle = new Battle([playerTeam, goblinsTeam, mercenaryTeam]);

            int roundNumber = 1;

            // --------------------------------------------------------------------
            // 4. BOUCLE DE COMBAT INTERACTIVE
            // --------------------------------------------------------------------
            while (!battle.IsOver)
            {
                Console.Clear();
                Console.WriteLine($"=================== TOUR {roundNumber} ===================");
                DisplayStatus(battle);

                var roundActions = new List<QueuedAction>();
                var busyCharacters = new HashSet<Character>();

                var activeCharacters = battle.Teams
                    .SelectMany(t => t.Members)
                    .Where(c => c.IsAlive)
                    .ToList();

                foreach (var caster in activeCharacters)
                {
                    if (busyCharacters.Contains(caster)) continue;

                    var casterTeam = battle.Teams.First(t => t.Members.Contains(caster));

                    Console.WriteLine($"\n>>> Tour de : {caster.Name} (Équipe {casterTeam.Name})");

                    // FILTRE CORRIGÉ : On vérifie que la synergie a suffisamment de partenaires NON OCCUPÉS
                    var usableAbilities = caster.Abilities
                        .Where(a => a.CanUse([caster], battle))
                        .Where(a =>
                        {
                            if (a is Synergy syn)
                            {
                                int availablePartnersCount = casterTeam.Members
                                    .Count(m => m.IsAlive
                                             && m != caster
                                             && !busyCharacters.Contains(m)
                                             && m.Abilities.Any(ab => ab.Id == syn.Id));

                                return (availablePartnersCount + 1) >= syn.RequiredCastersCount;
                            }
                            return true;
                        })
                        .ToList();

                    if (usableAbilities.Count == 0)
                    {
                        Console.WriteLine("Aucune capacité disponible.");
                        continue;
                    }

                    Console.WriteLine("Choisir une capacité :");
                    for (int i = 0; i < usableAbilities.Count; i++)
                    {
                        var ab = usableAbilities[i];
                        string typeLabel = ab switch
                        {
                            Spell s => $"[Sort - {s.ManaCost} PM]",
                            Synergy syn => $"[Synergie - {syn.RequiredSynergyBars} Barres]",
                            _ => "[Skill]"
                        };
                        Console.WriteLine($"  {i + 1}. {ab.Name} {typeLabel}");
                    }

                    int choice = ReadChoice(usableAbilities.Count);
                    var chosenAbility = usableAbilities[choice];

                    var casters = new List<Character> { caster };

                    if (chosenAbility is Synergy synergy)
                    {
                        int neededPartnersCount = synergy.RequiredCastersCount - 1;

                        var availablePartners = casterTeam.Members
                            .Where(m => m.IsAlive
                                     && m != caster
                                     && !busyCharacters.Contains(m)
                                     && m.Abilities.Any(a => a.Id == chosenAbility.Id))
                            .ToList();

                        if (availablePartners.Count == neededPartnersCount)
                        {
                            casters.AddRange(availablePartners);
                        }
                        else if (availablePartners.Count > neededPartnersCount)
                        {
                            for (int p = 0; p < neededPartnersCount; p++)
                            {
                                Console.WriteLine($"\nChoisir le partenaire {p + 1}/{neededPartnersCount} pour la synergie :");
                                for (int i = 0; i < availablePartners.Count; i++)
                                {
                                    Console.WriteLine($"  {i + 1}. {availablePartners[i].Name} ({availablePartners[i].CurrentHp}/{availablePartners[i].BaseStats.MaxHp} PV)");
                                }

                                int partnerChoice = ReadChoice(availablePartners.Count);
                                var selectedPartner = availablePartners[partnerChoice];

                                casters.Add(selectedPartner);
                                availablePartners.RemoveAt(partnerChoice);
                            }
                        }

                        string partners = string.Join(", ", casters.Select(c => c.Name));
                        Console.WriteLine($"   🤝 Synergie préparée avec : {partners}");
                    }

                    List<Character> targets;
                    if (chosenAbility.TargetMode == TargetMode.SingleEnemy || chosenAbility.TargetMode == TargetMode.SingleAlly)
                    {
                        var options = chosenAbility.TargetMode == TargetMode.SingleEnemy
                            ? battle.Teams.Where(t => t != casterTeam).SelectMany(t => t.Members).Where(m => m.IsAlive).ToList()
                            : casterTeam.Members.Where(m => m.IsAlive).ToList();

                        Console.WriteLine("Choisir une cible :");
                        for (int i = 0; i < options.Count; i++)
                        {
                            Console.WriteLine($"  {i + 1}. {options[i].Name} ({options[i].CurrentHp}/{options[i].BaseStats.MaxHp} PV)");
                        }
                        int targetChoice = ReadChoice(options.Count);
                        targets = new List<Character> { options[targetChoice] };
                    }
                    else
                    {
                        targets = TargetResolver.ResolveAuto(chosenAbility.TargetMode, caster, casterTeam, battle.Teams);
                    }

                    roundActions.Add(new QueuedAction
                    {
                        Casters = casters,
                        Targets = targets,
                        Ability = chosenAbility
                    });

                    foreach (var c in casters) busyCharacters.Add(c);
                }

                Console.WriteLine("\n[Appuyez sur Entrée pour exécuter le tour...]");
                Console.ReadLine();

                Console.Clear();

                Console.WriteLine("--- DÉBUT DU TOUR ---");
                battle.ExecuteRound(roundActions);
                Console.WriteLine("\n--- FIN DU TOUR ---");

                roundNumber++;

                if (!battle.IsOver)
                {
                    Console.WriteLine("\n[Appuyez sur Entrée pour passer au tour suivant...]");
                    Console.ReadLine();
                }
            }

            Console.WriteLine("\n=================== COMBAT TERMINÉ ===================");
            DisplayStatus(battle);
            Console.WriteLine($"Gagnant : {battle.Winner?.Name ?? "Égalité"}");
        }

        private static int ReadChoice(int max)
        {
            while (true)
            {
                Console.Write("> ");
                if (int.TryParse(Console.ReadLine(), out int c) && c >= 1 && c <= max)
                    return c - 1;
                Console.WriteLine($"Saisie invalide (1 à {max}).");
            }
        }

        private static void DisplayStatus(Battle battle)
        {
            Console.WriteLine("\n=================== ÉTAT DU COMBAT ===================");
            foreach (var team in battle.Teams)
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
    }
}