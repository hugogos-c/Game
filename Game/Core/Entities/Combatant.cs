using Game.Core.Abilities;

namespace Game.Core.Entities
{
    public sealed class Combatant
    {
        public string Name { get; }
        public Team Team { get; }
        public Stats BaseStats { get; }
        public int CurrentHp { get; private set; }
        public int CurrentMp { get; private set; }
        public IReadOnlyList<IAbility> Abilities { get; }

        public bool IsAlive => CurrentHp > 0;

        public Combatant(string name, Stats stats, List<IAbility> abilities, Team team)
        {
            Name = name;
            BaseStats = stats;
            CurrentHp = stats.MaxHp;
            CurrentMp = stats.MaxMp;
            Abilities = abilities;
            Team = team;

            Team.AddMember(this);
        }

        public void TakeDamage(int amount) => CurrentHp = Math.Max(0, CurrentHp - amount);
        public void Heal(int amount) => CurrentHp = Math.Min(BaseStats.MaxHp, CurrentHp + amount);
        public void ConsumeMp(int amount) => CurrentMp = Math.Max(0, CurrentMp - amount);
        public void RestoreMp(int amount) => CurrentMp = Math.Min(BaseStats.MaxMp, CurrentMp + amount);
    }
}
