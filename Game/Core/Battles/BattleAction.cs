using Game.Core.Abilities;
using Game.Core.Entities;

namespace Game.Core.Battles
{
    public class BattleAction
    {
        public required Combatant Initiator { get; init; }
        public required IReadOnlyList<Combatant> Participants { get; init; }
        public required IReadOnlyList<Combatant> Targets { get; init; }
        public required IAbility Ability { get; init; }

        public int Speed => Initiator.BaseStats.Speed;
    }
}
