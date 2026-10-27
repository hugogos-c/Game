namespace Game.Core.Entities
{
    public sealed class Team
    {
        public string Name { get; }
        public List<Combatant> Members { get; } = [];
        public SynergyGauge SynergyGauge { get; } = new();

        public bool IsDefeated => Members.All(m => !m.IsAlive);

        public Team(string name)
        {
            Name = name;
        }

        public void AddMember(Combatant member)
        {
            if (!Members.Contains(member)) Members.Add(member);
        }
    }
}
