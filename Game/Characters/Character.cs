namespace Game.Characters
{
    public class Character
    {
        public string Name { get; }
        public Stats BaseStats { get; }
        public int CurrentHp { get; private set; }
        public List<Ability> Abilities { get; }
        public bool IsAlive => CurrentHp > 0;

        public Character(string name, Stats stats, List<Ability> abilities)
        {
            Name = name;
            BaseStats = stats;
            CurrentHp = BaseStats.MaxHp;
            Abilities = abilities;
        }

        public void TakeDamage(int amount)
        {
            CurrentHp = Math.Max(0, CurrentHp - amount);
        }

        public void Heal(int amount)
        {
            CurrentHp = Math.Min(BaseStats.MaxHp, CurrentHp + amount);
        }
    }
}
