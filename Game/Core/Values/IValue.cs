namespace Game.Core.Values
{
    public interface IValue
    {
        int Resolve(EffectContext context);
    }
}
