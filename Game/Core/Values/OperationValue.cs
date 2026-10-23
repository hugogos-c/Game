namespace Game.Core.Values
{
    public enum Operator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
    }

    public sealed class OperationValue : IValue
    {
        public required IValue Left { get; init; }
        public required Operator Operator { get; init; }
        public required IValue Right { get; init; }

        public int Resolve(EffectContext context)
        {
            var left = Left.Resolve(context);
            var right = Right.Resolve(context);

            return Operator switch
            {
                Operator.Add => left + right,
                Operator.Subtract => left - right,
                Operator.Multiply => left * right,
                Operator.Divide => right == 0 ? left : left / right,
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
