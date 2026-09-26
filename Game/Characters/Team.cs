namespace Game.Characters
{
    public class Team
    {
        public string Name { get; set; }
        public IReadOnlyList<Character> Members { get; }

        public bool IsDefeated => Members.All(m => !m.IsAlive);

        public Team(string name, IReadOnlyList<Character> members)
        {
            Name = name;
            Members = members;
        }
    }
}
