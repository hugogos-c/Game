namespace Game.Core.Conditions
{
    public interface ICondition
    {
        bool Evaluate(EffectContext context);
    }
}
