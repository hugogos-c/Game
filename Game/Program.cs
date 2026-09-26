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
                TargetsFilters =
                [
                    new EnemiesFilter(),
                    new IsAliveFilter(),
                ],
                Sequences =
                [
                    new Sequence
                    {
                        TargetSelector = new AbilityTargetsSelector(),
                        Effects =
                        [
                            new DamageEffect
                            {
                                Power = 30,
                            },
                        ],
                    },
                ],
            };

            var tripleFleches = new Skill
            {
                Id = "triple_fleches",
                Name = "Triple flèches",
                RequiredTargetsCount = 3,
                TargetsFilters =
                [
                    new EnemiesFilter(),
                    new IsAliveFilter(),
                ],
                Sequences =
                [
                    new Sequence
                    {
                        TargetSelector = new AbilityTargetsSelector(),
                        Effects =
                        [
                            new DamageEffect
                            {
                                Power = 30,
                            },
                        ],
                    },
                ],
            };

            var dechargeElectrique = new Skill
            {
                Id = "decharge_electrique",
                Name = "Décharge électrique",
                TargetsFilters =
                [
                    new EnemiesFilter(),
                    new IsAliveFilter(),
                ],
                Sequences =
                [
                    new Sequence
                    {
                        TargetSelector = new AbilityTargetsSelector(),
                        Effects =
                        [
                            new DamageEffect
                            {
                                Power = 25,
                            },
                        ],
                    },
                ],
            };

            var execution = new Skill
            {
                Id = "execution",
                Name = "Exécution",
                TargetsFilters =
                [
                    new EnemiesFilter(),
                    new IsAliveFilter(),
                ],
                Sequences =
                [
                    new Sequence
                    {
                        TargetSelector = new AbilityTargetsSelector(),

                        Effects =
                        [
                            new ConditionalEffect
                            {
                                Condition = new TargetHpBelowCondition
                                {
                                    Percentage = 10,
                                },

                                Then = new DamageEffect
                                {
                                    Power = 999,
                                },

                                Else = new DamageEffect
                                {
                                    Power = 50,
                                },
                            },
                        ],
                    },
                ],
            };

            var soin = new Skill
            {
                Id = "soin",
                Name = "Soin",
                TargetsFilters =
                [
                    new AlliesFilter(),
                    new IsAliveFilter(),
                ],
                Sequences =
                [
                    new Sequence
                    {
                        TargetSelector = new AbilityTargetsSelector(),
                        Effects =
                        [
                            new HealEffect
                            {
                                Amount = 60,
                            },
                        ],
                    },
                ],
            };

            var resurrection = new Skill
            {
                Id = "resurrection",
                Name = "Résurrection",
                TargetsFilters =
                [
                    new AlliesFilter(),
                    new IsDeadFilter(),
                ],
                Sequences =
                [
                    new Sequence
                    {
                        TargetSelector = new AbilityTargetsSelector(),
                        Effects =
                        [
                            new HealEffect
                            {
                                Amount = 60,
                            },
                        ],
                    },
                ],
            };

            var soinMulti = new Skill
            {
                Id = "soin_multi",
                Name = "Soin multi",
                RequiredTargetsCount = 2,
                TargetsFilters =
                [
                    new AlliesFilter(),
                    new IsAliveFilter(),
                ],
                Sequences =
                [
                    new Sequence
                    {
                        TargetSelector = new AbilityTargetsSelector(),
                        Effects =
                        [
                            new HealEffect
                            {
                                Amount = 60,
                            },
                        ],
                    },
                ],
            };

            var feu = new Spell
            {
                Id = "feu",
                Name = "Feu",
                TargetsFilters =
                [
                    new EnemiesFilter(),
                    new IsAliveFilter(),
                ],
                ManaCost = 10,
                Sequences =
                [
                    new Sequence
                    {
                        TargetSelector = new AbilityTargetsSelector(),
                        Effects =
                        [
                            new DamageEffect
                            {
                                Power = 45,
                            },
                        ],
                    },
                ],
            };

            var foudre = new Spell
            {
                Id = "foudre",
                Name = "Foudre",
                TargetsFilters =
                [
                    new EnemiesFilter(),
                    new IsAliveFilter(),
                ],
                ManaCost = 15,
                Sequences =
                [
                    new Sequence
                    {
                        TargetSelector = new AbilityTargetsSelector(),
                        Effects =
                        [
                            new DamageEffect
                            {
                                Power = 55,
                            },
                        ],
                    },
                ],
            };

            var amourImpossible = new Synergy
            {
                Id = "amour_impossible",
                Name = "Amour impossible",
                TargetsFilters =
                [
                    new EnemiesFilter(),
                    new IsAliveFilter(),
                ],
                RequiredParticipantsCount = 1,
                RequiredSynergyBars = 2,
                Sequences =
                [
                    new Sequence
                    {
                        TargetSelector = new AbilityUsersSelector(),
                        Effects =
                        [
                            new HealEffect
                            {
                                Amount = 80,
                            },
                        ],
                    },
                    new Sequence
                    {
                        TargetSelector = new AbilityTargetsSelector(),
                        Effects =
                        [
                            new DamageEffect
                            {
                                Power = 100,
                            },
                        ],
                    },
                ],
            };

            var comboGobelin = new Synergy
            {
                Id = "combo_gobelin",
                Name = "Combo gobelin",
                TargetsFilters =
                [
                    new EnemiesFilter(),
                    new IsAliveFilter(),
                ],
                RequiredParticipantsCount = 1,
                RequiredSynergyBars = 1,
                Sequences =
                [
                    new Sequence
                    {
                        TargetSelector = new AbilityTargetsSelector(),
                        Effects =
                        [
                            new CompositeEffect
                            {
                                Effects =
                                [
                                    new DamageEffect
                                    {
                                        Power = 70,
                                    },
                                    new DamageEffect
                                    {
                                        Power = 70,
                                    },
                                ],
                            },
                        ],
                    },
                ],
            };

            // --------------------------------------------------------------------
            // 2. INSTANCIATION DES PERSONNAGES
            // --------------------------------------------------------------------
            var aldrick = new Character(
                "Aldrick",
                new Stats(MaxHp: 400, MaxMp: 30, Attack: 35, Defense: 15, Speed: 40),
                [coupEpee, execution, feu, soin, resurrection, amourImpossible]
            );

            var veya = new Character(
                "Veya",
                new Stats(MaxHp: 300, MaxMp: 60, Attack: 45, Defense: 10, Speed: 80),
                [dechargeElectrique, tripleFleches, foudre, soin, amourImpossible]
            );

            var gobelin1 = new Character(
                "Gobelin 1",
                new Stats(MaxHp: 160, MaxMp: 0, Attack: 25, Defense: 5, Speed: 50),
                [coupEpee, soin, soinMulti, comboGobelin]
            );

            var gobelin2 = new Character(
                "Gobelin 2",
                new Stats(MaxHp: 160, MaxMp: 0, Attack: 25, Defense: 5, Speed: 50),
                [coupEpee, soin, soinMulti, comboGobelin]
            );

            var gobelin3 = new Character(
                "Gobelin 3",
                new Stats(MaxHp: 160, MaxMp: 0, Attack: 25, Defense: 5, Speed: 50),
                [coupEpee, soin, soinMulti, comboGobelin]
            );

            var mercenaire = new Character(
                "Mercenaire",
                new Stats(MaxHp: 220, MaxMp: 20, Attack: 40, Defense: 10, Speed: 70),
                [coupEpee, tripleFleches, soin]
            );

            // --------------------------------------------------------------------
            // 3. CONSTITUTION DES ÉQUIPES ET DU COMBAT
            // --------------------------------------------------------------------
            var playerTeam = new Team("Joueur", [aldrick, veya]);
            var goblinsTeam = new Team("Gobelins", [gobelin1, gobelin2, gobelin3]);
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

                    var usableAbilities = caster.Abilities
                        .Where(a =>
                        {
                            var initialContext = new AbilityContext
                            {
                                Initiator = caster,
                                Participants = [],
                                Targets = [],
                                Battle = battle
                            };

                            if (a.RequiredTargetsCount != 0)
                            {
                                var availableTargets = a.GetAvailableTargets(initialContext);

                                if (!availableTargets.Any()) return false;
                            }

                            return a.Validate(initialContext).IsValid;
                        })
                        .Where(a =>
                        {
                            if (a.RequiredParticipantsCount == 0)
                                return true;

                            int availableParticipantsCount = casterTeam.Members
                                .Count(m => m.IsAlive
                                         && m != caster
                                         && !busyCharacters.Contains(m)
                                         && m.Abilities.Any(ab => ab == a));

                            return availableParticipantsCount >= a.RequiredParticipantsCount;
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

                    var participants = new List<Character>();

                    if (chosenAbility.RequiredParticipantsCount > 0)
                    {
                        var availableParticipants = casterTeam.Members
                            .Where(m => m.IsAlive
                                     && m != caster
                                     && !busyCharacters.Contains(m)
                                     && m.Abilities.Any(a => a == chosenAbility))
                            .ToList();

                        if (availableParticipants.Count == chosenAbility.RequiredParticipantsCount)
                        {
                            participants.AddRange(availableParticipants);
                        }
                        else
                        {
                            for (int p = 0; p < chosenAbility.RequiredParticipantsCount; p++)
                            {
                                Console.WriteLine($"\nChoisir le participant {p + 1}/{chosenAbility.RequiredParticipantsCount} :");

                                for (int i = 0; i < availableParticipants.Count; i++)
                                {
                                    Console.WriteLine($"  {i + 1}. {availableParticipants[i].Name} ({availableParticipants[i].CurrentHp}/{availableParticipants[i].BaseStats.MaxHp} PV)");
                                }

                                int participantChoice = ReadChoice(availableParticipants.Count);

                                var selectedParticipant = availableParticipants[participantChoice];

                                participants.Add(selectedParticipant);
                                availableParticipants.RemoveAt(participantChoice);
                            }
                        }

                        string participantNames = string.Join(", ", participants.Select(p => p.Name));

                        Console.WriteLine($"   🤝 Participants sélectionnés : {participantNames}");
                    }

                    var availableTargets = chosenAbility.GetAvailableTargets(new AbilityContext
                    {
                        Initiator = caster,
                        Participants = participants,
                        Targets = [],
                        Battle = battle
                    });

                    List<Character> targets;

                    if (availableTargets.Count == 0)
                    {
                        Console.WriteLine("Aucune cible disponible.");
                        continue;
                    }
                    else if (chosenAbility.RequiredTargetsCount == -1 || chosenAbility.RequiredTargetsCount >= availableTargets.Count)
                    {
                        targets = availableTargets.ToList();
                    }
                    else
                    {
                        int targetsCount = Math.Min(chosenAbility.RequiredTargetsCount, availableTargets.Count);

                        targets = new List<Character>();

                        for (int i = 0; i < targetsCount; i++)
                        {
                            var options = availableTargets
                                .Where(target => !targets.Contains(target))
                                .ToList();

                            Console.WriteLine(targetsCount == 1 ? "Choisir une cible :" : $"Choisir la cible {i + 1}/{targetsCount} :");

                            for (int j = 0; j < options.Count; j++)
                            {
                                var target = options[j];

                                Console.WriteLine($"  {j + 1}. {target.Name} ({target.CurrentHp}/{target.BaseStats.MaxHp} PV)");
                            }

                            int targetChoice = ReadChoice(options.Count);

                            targets.Add(options[targetChoice]);
                        }
                    }

                    roundActions.Add(new QueuedAction
                    {
                        Initiator = caster,
                        Participants = participants,
                        Targets = targets,
                        Ability = chosenAbility
                    });

                    busyCharacters.Add(caster);

                    foreach (var participant in participants) busyCharacters.Add(participant);
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
                if (int.TryParse(Console.ReadLine(), out int c) && c >= 1 && c <= max) return c - 1;
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