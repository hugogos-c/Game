namespace Game.Core
{
    public interface IAbility
    {
        string Id { get; }
        string Name { get; }
        TargetMode TargetMode { get; }
        IReadOnlyList<ISequence> Sequences { get; }

        bool CanUse(IReadOnlyList<Character> casters, Battle battle);
        void PayCost(IReadOnlyList<Character> casters, Battle battle);
        void Execute(AbilityContext context);
    }

    public abstract class BaseAbility : IAbility
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required TargetMode TargetMode { get; init; }
        public required List<ISequence> Sequences { get; init; }

        IReadOnlyList<ISequence> IAbility.Sequences => Sequences;

        public virtual bool CanUse(IReadOnlyList<Character> casters, Battle battle) => true;
        public virtual void PayCost(IReadOnlyList<Character> casters, Battle battle) { }

        public virtual void Execute(AbilityContext context)
        {
            foreach (var sequence in Sequences)
            {
                if (context.IsAborted) break;

                sequence.Execute(context);
            }
        }
    }

    public class Skill : BaseAbility
    {
        // Competence physique / basique (sans cout en PM)
    }

    public class Spell : BaseAbility
    {
        public required int ManaCost { get; init; }

        public override bool CanUse(IReadOnlyList<Character> casters, Battle battle)
        {
            return casters.First().CurrentMp >= ManaCost;
        }

        public override void PayCost(IReadOnlyList<Character> casters, Battle battle)
        {
            casters.First().ConsumeMp(ManaCost);
        }
    }

    public class Synergy : BaseAbility
    {
        public int RequiredCastersCount { get; init; } = 2;
        public required int RequiredSynergyBars { get; init; }

        public override bool CanUse(IReadOnlyList<Character> casters, Battle battle)
        {
            var initiator = casters.First();
            var team = battle.Teams.First(t => t.Members.Contains(initiator));

            // Compte combien de membres vivants de l'équipe possèdent aussi cette synergie
            int availableCasters = team.Members
                .Count(m => m.IsAlive && m.Abilities.Any(a => a.Id == Id));

            if (availableCasters < RequiredCastersCount) return false;

            return team.SynergyGauge.HasEnoughBars(RequiredSynergyBars);
        }

        public override void PayCost(IReadOnlyList<Character> casters, Battle battle)
        {
            var team = battle.Teams.First(t => t.Members.Contains(casters.First()));
            team.SynergyGauge.ConsumeBars(RequiredSynergyBars);
        }
    }
}
