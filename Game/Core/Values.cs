namespace Game.Core
{
	public interface IValue
	{
		int Resolve(EffectContext context);
    }

    public interface IValues
    {
        IReadOnlyList<int> Resolve(EffectContext context);
    }

    #region Constant
    public sealed class ConstantValue : IValue
    {
        public required int Value { get; init; }

        public int Resolve(EffectContext context)
        {
            return Value;
        }
    }

    public sealed class ConstantValues : IValues
    {
        public required IReadOnlyList<int> Values { get; init; }

        public IReadOnlyList<int> Resolve(EffectContext context)
        {
            return Values;
        }
    }
    #endregion

    #region Random
    public sealed class RandomValue : IValue
    {
        public required IValue Min { get; init; }
        public required IValue Max { get; init; }

        public int Resolve(EffectContext context)
        {
            return Random.Shared.Next(Min.Resolve(context), Max.Resolve(context) + 1);
        }
    }
    #endregion

    #region CharacterData
    public enum CharacterData
    {
        MaxHp,
        MaxMp,
        CurrentHp,
        CurrentMp,
        Attack,
        Defense,
        Speed,
    }

    public sealed class CharacterDataValues : IValues
    {
        public required ITargetsSelector TargetsSelector { get; init; }
        public required CharacterData CharacterData { get; init; }

        public IReadOnlyList<int> Resolve(EffectContext context)
        {
            return TargetsSelector.Select(context.AbilityContext).Select(t => CharacterData switch
            {
                CharacterData.MaxHp => t.BaseStats.MaxHp,
                CharacterData.MaxMp => t.BaseStats.MaxMp,
                CharacterData.CurrentHp => t.CurrentHp,
                CharacterData.CurrentMp => t.CurrentMp,
                CharacterData.Attack => t.BaseStats.Attack,
                CharacterData.Defense => t.BaseStats.Defense,
                CharacterData.Speed => t.BaseStats.Speed,
                _ => throw new InvalidOperationException(),
            }).ToList();
        }
    }
    #endregion

    #region Operation
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
    #endregion

    #region Conditional
    public sealed class ConditionalValue : IValue
    {
        public required ICondition Condition { get; init; }
        public required IValue Then { get; init; }
        public required IValue Else { get; init; }

        public int Resolve(EffectContext context)
        {
            return Condition.Evaluate(context) ? Then.Resolve(context) : Else.Resolve(context);
        }
    }

    public sealed class ConditionalValues : IValues
    {
        public required ICondition Condition { get; init; }
        public required IValues Then { get; init; }
        public required IValues Else { get; init; }

        public IReadOnlyList<int> Resolve(EffectContext context)
        {
            return Condition.Evaluate(context) ? Then.Resolve(context) : Else.Resolve(context);
        }
    }
    #endregion

    #region Aggregation
    public interface IAggregation
    {
        int Aggregate(IReadOnlyList<int> values);
    }

    public sealed class AverageAggregation : IAggregation
    {
        public int Aggregate(IReadOnlyList<int> values)
        {
            return (int)values.Average();
        }
    }

    public sealed class AggregateValue : IValue
    {
        public required IValues Values { get; init; }
        public required IAggregation Aggregation { get; init; }

        public int Resolve(EffectContext context)
        {
            return Aggregation.Aggregate(Values.Resolve(context));
        }
    }
    #endregion

    #region Percentage - Sucre syntaxique
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
    #endregion

    #region Chance - Sucre syntaxique
    public sealed class ChanceValue : IValue
    {
        public required IValue Chance { get; init; }
        public required IValue Success { get; init; }
        public required IValue Failure { get; init; }

        public int Resolve(EffectContext context)
        {
            return new ConditionalValue
            {
                Condition = new ComparisonCondition
                {
                    Left = new RandomValue { Min = new ConstantValue { Value = 1 }, Max = new ConstantValue { Value = 100 }, },
                    Operator = ComparisonOperator.LessThanOrEqual,
                    Right = Chance,
                },
                Then = Success,
                Else = Failure,
            }.Resolve(context);
        }
    }

    public sealed class ChanceValues : IValues
    {
        public required IValue Chance { get; init; }
        public required IValues Success { get; init; }
        public required IValues Failure { get; init; }
        public IReadOnlyList<int> Resolve(EffectContext context)
        {
            return new ConditionalValues
            {
                Condition = new ComparisonCondition
                {
                    Left = new RandomValue { Min = new ConstantValue { Value = 1 }, Max = new ConstantValue { Value = 100 }, },
                    Operator = ComparisonOperator.LessThanOrEqual,
                    Right = Chance,
                },
                Then = Success,
                Else = Failure,
            }.Resolve(context);
        }
    }
    #endregion
}
