namespace Game.Core.Entities
{
    public sealed class SynergyGauge
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
