namespace Game.Core
{
    public interface ICondition
    {
        bool Evaluate(EffectContext context);
    }

    public class TargetHpBelowCondition : ICondition
    {
        public required int Percentage { get; init; }

        public bool Evaluate(EffectContext context)
        {
            var currentPercentage =
                context.Target.CurrentHp * 100.0 /
                context.Target.BaseStats.MaxHp;

            return currentPercentage < Percentage;
        }
    }

    public class AndCondition : ICondition
    {
        public required IReadOnlyList<ICondition> Conditions { get; init; }

        public bool Evaluate(EffectContext context)
        {
            return Conditions.All(condition => condition.Evaluate(context));
        }
    }

    public class OrCondition : ICondition
    {
        public required IReadOnlyList<ICondition> Conditions { get; init; }

        public bool Evaluate(EffectContext context)
        {
            return Conditions.Any(condition => condition.Evaluate(context));
        }
    }

    public class NotCondition : ICondition
    {
        public required ICondition Condition { get; init; }

        public bool Evaluate(EffectContext context)
        {
            return !Condition.Evaluate(context);
        }
    }
}
