# Étape de refactoring et de déplacement des réponsabilités complet

## Architecture

Faire un fichier par classe, interface, record, struct...

## Données de références

Les implémentations de ces interfaces deviendront des records :
- IAbility
- ISequence
- IEffect
- ICondition
- IValue
- IValues
- IAggregation

Les futurs statuts seront égalements des records.

Voir [Sérialisation et optimisation JSON](./Sérialisation%20et%20optimisation%20JSON.md)

## Répertoires

Chaque ficheir deviendra un répertoire (à quelques exceptions près)

## Séparation

Séparer la définition de l'implémentation, notamment pour IAbility ses méthodes de validations et d'exécution (qui devront aller dans QueuedAction ou un AbilityExecutor peut-être).

## Coûts

Peut-être ajouter une dimension (interface + implémentations) de coût (ICost ou IAbilityCost).

## ITargetsFilter et ITargetsSelector

Voir pour regrouper ITargetsFIlter avec ITargetsSelector, ou peut-être tout mettre en ICondition (à creuser)

Peut-être un `public ICondition TargetSelection { get; init; }` pour les IAbility ?

## AbilityContext et EffectContext

Peut-être revoir la structure du AbilityContext et du EffectContext

## Battle

### Avant
- Battle
- AbilityContext
- QueuedActon

### Pendant
- Séparer Battle en BattleState et BattleEngine
- (peut-être) Transformer AbilityContext -> BattleContext
- Transformer QueuedAction en BattleAction
- Créer BattleResult

### Après
- BattleState
- BattleEngine
- BattleAction
- (peut-être) BattleContext
- BattleResult

## BattleAction

Porter une logique de validation de l'action et une logique d'exécution de l'action

## Autres

Réflechir à des abstractions du genre :
```csharp
public interface IValue<in TContext>
{
    int Resolve(TContext context);
}

public interface ICondition<in TContext>
{
    bool Evaluate(TContext context);
}

public interface IEvaluationContext
{
}

public sealed class BattleEvaluationContext : IEvaluationContext
{
    public required Battle Battle { get; init; }
    public required Character Initiator { get; init; }
    public required IReadOnlyList<Character> Participants { get; init; }
    public required IReadOnlyList<Character> Targets { get; init; }
    public required Character CurrentTarget { get; init; }
}

public sealed class CharacterDataValue : IValue<BattleEvaluationContext>
{
    ...
}
```

---

### Expressions pures

```text
ConstantValue
OperationValue
PercentageValue
AverageAggregation
ComparisonCondition
AndCondition
OrCondition
```

Elles calculent quelque chose.

### Expressions dépendantes du domaine

```text
CharacterDataValues
AbilityTargetsSelector
AbilityUsersSelector
```

Elles vont chercher quelque chose dans le monde du jeu.

### Commandes / mutations

```text
DamageEffect
HealEffect
ConsumeMp
ConsumeBars
```

Elles modifient l'état.