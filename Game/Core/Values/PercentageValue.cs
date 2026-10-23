namespace Game.Core.Values
{
    public sealed class PercentageValue : IValue
    {
        public required IValue Source { get; init; }
        public required IValue Percentage { get; init; }

        public int Resolve(EffectContext context)
        {
            return new OperationValue
            {
                Left = new OperationValue
                {
                    Left = Source,
                    Operator = Operator.Multiply,
                    Right = Percentage,
                },
                Operator = Operator.Divide,
                Right = new ConstantValue { Value = 100 },
            }.Resolve(context);
        }
    }
}
