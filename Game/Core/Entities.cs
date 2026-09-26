namespace Game.Core
{
    public record struct Stats(int MaxHp, int MaxMp, int Attack, int Defense, int Speed);

    public class Character
    {
        public string Name { get; }
        public Stats BaseStats { get; }
        public int CurrentHp { get; private set; }
        public int CurrentMp { get; private set; }
        public List<IAbility> Abilities { get; }

        public bool IsAlive => CurrentHp > 0;

        public Character(string name, Stats stats, List<IAbility> abilities)
        {
            Name = name;
            BaseStats = stats;
            CurrentHp = stats.MaxHp;
            CurrentMp = stats.MaxMp;
            Abilities = abilities;
        }

        public void TakeDamage(int amount) => CurrentHp = Math.Max(0, CurrentHp - amount);
        public void Heal(int amount) => CurrentHp = Math.Min(BaseStats.MaxHp, CurrentHp + amount);
        public void ConsumeMp(int amount) => CurrentMp = Math.Max(0, CurrentMp - amount);
        public void RestoreMp(int amount) => CurrentMp = Math.Min(BaseStats.MaxMp, CurrentMp + amount);
    }

    public class Team
    {
        public string Name { get; }
        public List<Character> Members { get; }
        public SynergyGauge SynergyGauge { get; } = new();

        public bool IsDefeated => Members.All(m => !m.IsAlive);

        public Team(string name, IEnumerable<Character> members)
        {
            Name = name;
            Members = members.ToList();
        }
    }

    public class SynergyGauge
    {
        public int MaxBars { get; init; } = 100;
        public int CurrentBars { get; private set; } = 100;

        public bool HasEnoughBars(int amount) => CurrentBars >= amount;

        public bool ConsumeBars(int amount)
        {
            if (!HasEnoughBars(amount)) return false;
            CurrentBars -= amount;
            return true;
        }

        public void AddBars(int amount)
        {
            CurrentBars = Math.Min(MaxBars, CurrentBars + amount);
        }
    }
}
