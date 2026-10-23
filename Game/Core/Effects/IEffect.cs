namespace Game.Core.Effects
{
    public interface IEffect
    {
        IEffectResult Apply(EffectContext context);
    }
}
