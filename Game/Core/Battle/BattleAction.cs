using Game.Core.Abilities;
using Game.Core.Entities;

namespace Game.Core.Battle
{
    public class BattleAction
    {
        public required Character Initiator { get; init; }
        public required IReadOnlyList<Character> Participants { get; init; }
        public required IReadOnlyList<Character> Targets { get; init; }
        public required IAbility Ability { get; init; }

        public int Speed => Initiator.BaseStats.Speed;
    }
}
