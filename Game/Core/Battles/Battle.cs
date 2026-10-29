using Game.Core.Entities;

namespace Game.Core.Battles
{
    public class Battle
    {
        public List<Team> Teams { get; }
        public int RoundNumber { get; private set; } = 1;
        public int MaxRound { get; } = -1;

        public bool IsOver => Teams.Count(t => !t.IsDefeated) <= 1 || (MaxRound > 0 && RoundNumber >= MaxRound);
        public Team? Winner => IsOver ? Teams.FirstOrDefault(t => !t.IsDefeated) : null;

        public Battle(IEnumerable<Team> teams, int maxRound)
        {
            Teams = teams.ToList();
            MaxRound = maxRound;
        }

        public void AdvanceRound()
        {
            RoundNumber++;
        }
    }
}
