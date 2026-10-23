namespace Game.Core.Entities
{
    public sealed class Team
    {
        public string Name { get; }
        public List<Character> Members { get; } = [];
        public SynergyGauge SynergyGauge { get; } = new();

        public bool IsDefeated => Members.All(m => !m.IsAlive);

        public Team(string name)
        {
            Name = name;
        }

        public void AddMember(Character member)
        {
            if (!Members.Contains(member)) Members.Add(member);
        }
    }
}
