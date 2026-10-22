# Grand plan de refactoring — Architecture du moteur de jeu

> Document de conception et de refactoring à long terme.
>
> Objectif : faire évoluer le prototype actuel vers une architecture suffisamment propre, explicite, testable et durable pour supporter un projet de très grande taille et potentiellement plusieurs décennies de développement.
>
> Ce document ne cherche pas à appliquer prématurément toutes les abstractions possibles. Il sert à conserver le **raisonnement architectural**, les décisions prises, les problèmes identifiés et l'ordre dans lequel les résoudre.

---

# 0. Philosophie générale

Le projet actuel est encore jeune. Le code n'est donc pas considéré comme « mauvais » : il remplit actuellement son rôle de prototype et permet d'explorer le game design.

Au contraire, plusieurs choix sont déjà particulièrement intéressants :

- les capacités sont décrites comme des compositions de petits objets ;
- les effets sont composables ;
- les conditions sont composables ;
- les valeurs sont composables ;
- les séquences permettent de construire des capacités en plusieurs étapes ;
- les filtres et selectors séparent la sélection des cibles de l'exécution ;
- `Spell` et `Synergy` expriment des règles métier propres au jeu ;
- le système commence déjà à ressembler à un petit DSL permettant de décrire des capacités sous forme d'arbre d'expressions.

Le principal danger n'est donc pas l'architecture actuelle en elle-même.

Le danger serait de **laisser l'architecture de prototype devenir accidentellement l'architecture définitive**.

L'objectif du refactoring est donc :

1. clarifier les responsabilités ;
2. séparer les concepts génériques des concepts propres au combat ;
3. stabiliser les frontières entre systèmes ;
4. réduire les dépendances inutiles ;
5. rendre les expressions réutilisables ;
6. rendre les définitions de données sérialisables ;
7. rendre les capacités facilement testables ;
8. permettre l'ajout futur de systèmes sans contaminer les abstractions existantes.

---

# 1. Principe fondamental : ne pas confondre le DSL et le moteur de combat

Le projet contient actuellement plusieurs familles de concepts qui se ressemblent parce qu'elles sont toutes utilisées par les capacités.

Mais elles n'ont pas toutes le même rôle.

Une distinction importante est à conserver :

## 1.1. Le DSL

Le DSL est principalement constitué de :

- `ICondition`
- `IValue`
- `IValues`
- `IAggregation`

Ces abstractions décrivent des **expressions**.

Elles répondent essentiellement à des questions du type :

> « Quelle est la valeur de cette expression ? »

ou :

> « Est-ce que cette condition est vraie ? »

Elles devraient idéalement pouvoir être utilisées dans plusieurs systèmes.

Exemples futurs :

- combat ;
- progression ;
- quêtes ;
- déblocage de capacités ;
- équipement ;
- prérequis ;
- événements ;
- dialogues conditionnels ;
- IA ;
- éventuellement scripts de gameplay.

---

## 1.2. Le moteur de combat

D'autres abstractions sont beaucoup plus clairement liées au combat :

- `IAbility`
- `ISequence`
- `IEffect`
- `ITargetsSelector`
- `ITargetsFilter`
- `AbilityContext`
- `EffectContext`
- `Battle`
- `BattleEngine`
- `BattleAction`
- `Character`
- `Team`
- `SynergyGauge`

Elles décrivent le fonctionnement du combat.

Il n'est donc pas nécessaire de forcer ces abstractions à devenir universelles.

En particulier :

> `IEffect` est volontairement une abstraction de mutation du domaine du combat.

Un `DamageEffect` ou un `HealEffect` n'a aucune raison d'être une abstraction générique du jeu entier.

À terme, on pourra avoir par exemple :

```text
Combat/
    Effects/
        DamageEffect
        HealEffect
        ApplyStatusEffect
        ...

Quest/
    Effects/
        CompleteQuestEffect
        GiveQuestRewardEffect
        ...

Inventory/
    Effects/
        AddItemEffect
        RemoveItemEffect
        ...
```

Les deux familles peuvent utiliser le même DSL d'expressions sans partager leurs commandes/mutations.

---

# 2. Point architectural majeur : séparer expression, lecture et mutation

Une distinction particulièrement importante est apparue pendant le refactoring.

Il existe trois catégories fondamentales.

## 2.1. Expressions

Elles calculent quelque chose sans modifier l'état.

Exemples :

```text
ConstantValue
OperationValue
PercentageValue
RandomValue
AverageAggregation
ComparisonCondition
AndCondition
OrCondition
NotCondition
ConditionalValue
```

Elles peuvent être vues comme des nœuds d'un AST.

Exemple :

```text
CurrentHP
    ↓
Average
    ↓
Percentage(10%)
    ↓
Compare <=
    ↓
Condition
```

---

## 2.2. Expressions dépendantes du domaine

Elles ne sont plus complètement pures parce qu'elles doivent lire l'état du jeu.

Exemples :

```text
CharacterDataValues
```

qui doit aller chercher :

```text
CurrentHp
MaxHp
Attack
Defense
Speed
...
```

dans des personnages.

Le principe souhaité est :

```text
Expression générique
        ↓
contexte d'évaluation
        ↓
données du domaine
```

et non :

```text
Expression générique
        ↓
référence directe au moteur de combat
```

---

## 2.3. Commandes / mutations

Elles modifient l'état.

Exemples :

```text
DamageEffect
HealEffect
ApplyStatusEffect
ConsumeMp
ConsumeBars
...
```

Elles appartiennent au moteur de combat.

Cette distinction doit devenir une règle architecturale.

---

# 3. Les expressions ne devraient pas dépendre inutilement de EffectContext

Constat important après inspection du code actuel :

```csharp
ICondition
IValue
IValues
IAggregation
```

acceptent actuellement :

```csharp
EffectContext
```

mais quasiment aucune de ces abstractions n'a réellement besoin de l'intégralité de ce contexte.

Le cas réellement intéressant est :

```csharp
CharacterDataValues
```

qui utilise indirectement :

```csharp
AbilityContext
```

pour accéder aux personnages.

Cela révèle une dépendance probablement trop forte.

---

# 4. Première règle de refactoring : réduire les contextes

Actuellement :

```text
AbilityContext
    ├── Initiator
    ├── Participants
    ├── Targets
    ├── Battle
    ├── IsAborted
    ├── Results
    └── CustomData
```

et :

```text
EffectContext
    ├── AbilityContext
    └── CurrentTarget
```

Puis :

```text
IValue.Resolve(EffectContext)
ICondition.Evaluate(EffectContext)
IAggregation.Aggregate(...)
```

Cela fait remonter énormément de dépendances.

Il serait préférable de ne pas donner à chaque expression plus d'informations que nécessaire.

---

# 5. Direction envisagée : contexte d'évaluation

Une piste prometteuse est :

```csharp
public interface IEvaluationContext
{
}
```

puis :

```csharp
public interface IValue<in TContext>
    where TContext : IEvaluationContext
{
    int Resolve(TContext context);
}
```

et :

```csharp
public interface ICondition<in TContext>
    where TContext : IEvaluationContext
{
    bool Evaluate(TContext context);
}
```

Avec par exemple :

```csharp
public sealed class BattleEvaluationContext : IEvaluationContext
{
    public required BattleState Battle { get; init; }

    public required Character Initiator { get; init; }

    public required IReadOnlyList<Character> Participants { get; init; }

    public required IReadOnlyList<Character> Targets { get; init; }

    public required Character CurrentTarget { get; init; }
}
```

Mais cette direction doit être introduite avec prudence.

Le but n'est pas de créer immédiatement une hiérarchie de contextes extrêmement abstraite.

Le but est :

> donner aux expressions uniquement le contexte dont elles ont réellement besoin.

---

# 6. Attention : ne pas généraliser trop tôt

Une tentation serait de créer immédiatement :

```csharp
IValue<TContext>
ICondition<TContext>
IValues<TContext>
IAggregation<TContext>
IEvaluationContext
```

partout.

Cela peut être pertinent.

Mais avant de le faire, vérifier combien de types de contextes existent réellement.

Une architecture préférable serait :

```text
DSL
│
├── Expressions pures
│
└── Expressions dépendantes d'un contexte
       │
       └── Battle
```

plutôt que :

```text
DSL
│
├── 18 interfaces génériques
├── 9 types de contexte
├── 12 adaptateurs
└── 4 niveaux d'abstraction
```

Le critère doit être :

> est-ce que cette abstraction élimine réellement une dépendance ou permet une réutilisation concrète ?

et non :

> est-ce que cette abstraction est théoriquement généralisable ?

---

# 7. Architecture cible conceptuelle

Une architecture possible à terme :

```text
Game/
│
├── Core/
│   │
│   ├── Expressions/
│   │   ├── Values/
│   │   ├── Conditions/
│   │   ├── Aggregations/
│   │   └── ...
│   │
│   └── ...
│
├── Combat/
│   │
│   ├── Abilities/
│   ├── Battle/
│   ├── Effects/
│   ├── Targeting/
│   ├── Values/
│   ├── Conditions/
│   └── ...
│
├── Characters/
│
├── Items/
│
├── Quests/
│
├── Progression/
│
└── World/
```

Cette structure n'est pas obligatoire.

Le principe important est la séparation :

```text
Core / DSL
        ↑
Combat
        ↑
Characters / Battle state
```

et non :

```text
Tout dépend de Game.Core
qui dépend de tout
```

---

# 8. IAbility : conserver le concept métier

Une discussion importante concerne :

```csharp
IAbility
```

avec :

```csharp
Skill
Spell
Synergy
```

Le système actuel est :

```text
BaseAbility
    ├── Skill
    ├── Spell
    └── Synergy
```

Ce n'est pas intrinsèquement un mauvais design.

Au contraire, dans le game design actuel :

```text
Skill
    = capacité sans coût particulier

Spell
    = capacité utilisant du mana

Synergy
    = capacité utilisant la jauge de synergie de l'équipe
      et potentiellement plusieurs participants
```

La distinction est donc sémantique et métier.

Il ne faut pas supprimer cette distinction uniquement parce qu'on pourrait techniquement représenter tout cela avec :

```text
Ability
+
List<Cost>
```

---

# 9. IAbilityCost : abstraction à introduire seulement si nécessaire

Une abstraction :

```csharp
IAbilityCost
```

peut devenir pertinente si les coûts se multiplient ou se combinent.

Exemples futurs :

```text
ManaCost
HealthCost
EnergyCost
SynergyCost
ItemCost
CooldownCost
ChargeCost
...
```

Et potentiellement :

```text
Spell
    = Ability + ManaCost

Synergy
    = Ability + SynergyCost + ParticipantRequirement
```

Mais actuellement :

```text
Spell
    → Mana

Synergy
    → SynergyGauge
```

sont des concepts de gameplay suffisamment spécifiques pour que leur séparation reste parfaitement défendable.

Ne pas introduire `IAbilityCost` uniquement parce qu'il est possible de le faire.

---

# 10. Séparer définition et exécution de Ability

Actuellement `BaseAbility` possède :

```csharp
GetAvailableTargets(...)
Validate(...)
PayCost(...)
Use(...)
```

Cela mélange deux choses :

## Définition

```text
Id
Name
TargetsFilters
RequiredTargetsCount
RequiredParticipantsCount
Sequences
ManaCost
RequiredSynergyBars
```

## Exécution

```text
Validate
PayCost
Use
```

Pour une architecture durable, il est préférable que `IAbility` devienne progressivement davantage une **définition de données**.

Par exemple :

```text
Ability
    ├── metadata
    ├── target constraints
    ├── costs
    └── sequences
```

et qu'un service/moteur s'occupe de :

```text
validation
→ coût
→ exécution
→ résultats
```

---

# 11. Future responsabilité de BattleAction

`QueuedAction` est actuellement une structure contenant :

```text
Initiator
Participants
Targets
Ability
```

avec :

```csharp
Speed => Initiator.BaseStats.Speed;
```

À terme :

```text
BattleAction
```

pourrait représenter une action décidée par un participant.

Puis :

```text
BattleEngine
```

pourrait gérer :

```text
1. validation
2. résolution des cibles
3. validation finale
4. paiement du coût
5. exécution
6. résultats
```

Ce qui donne :

```text
BattleEngine
    ↓
BattleAction
    ↓
Ability
    ↓
Sequence
    ↓
Effect
```

---

# 12. Battle doit probablement être séparé en BattleState + BattleEngine

Actuellement :

```csharp
Battle
```

contient à la fois :

- l'état du combat ;
- la logique d'exécution du combat.

À terme :

```text
BattleState
    ├── Teams
    ├── ...
    └── état mutable du combat

BattleEngine
    ├── ExecuteRound
    ├── ExecuteAction
    ├── validation
    └── résolution
```

Cela permet une séparation beaucoup plus nette :

```text
BattleState
    = données

BattleEngine
    = comportement
```

---

# 13. BattleResult

Prévoir éventuellement :

```csharp
BattleResult
```

pour éviter que le moteur dépende directement de :

```csharp
Console.WriteLine(...)
```

Les résultats du moteur pourraient devenir des données :

```text
BattleResult
    ├── ActionResult
    ├── EffectResult
    ├── DamageResult
    ├── HealResult
    └── ...
```

Puis une couche supérieure déciderait comment afficher ces résultats.

Cela sera particulièrement important si le projet possède un jour :

- UI ;
- replay ;
- logs ;
- animations ;
- réseau ;
- sauvegarde ;
- tests automatisés ;
- outils de debug.

---

# 14. IEffectResult mérite probablement une vraie réflexion

Actuellement :

```csharp
public interface IEffectResult { }

public class GenericResult : IEffectResult { }
```

et les effets renvoient presque toujours :

```csharp
new GenericResult()
```

C'est suffisant pour un prototype.

Mais à terme, le résultat pourrait devenir extrêmement utile.

Exemple :

```text
DamageEffect
    ↓
DamageResult
    ├── Source
    ├── Target
    ├── RawDamage
    ├── FinalDamage
    ├── Critical
    └── TargetKilled
```

Puis :

```text
HealEffect
    ↓
HealResult
```

etc.

Cela pourrait alimenter :

- UI ;
- animations ;
- journal de combat ;
- achievements ;
- triggers ;
- statistiques ;
- replay ;
- événements ;
- tests.

Ne pas nécessairement implémenter tout cela maintenant.

Mais conserver une architecture qui ne bloque pas cette évolution.

---

# 15. Contextes : AbilityContext vs EffectContext

Le découpage actuel :

```text
AbilityContext
    +
EffectContext
```

est logique dans son principe.

Une capacité connaît :

```text
Initiator
Participants
Targets
Battle
```

un effet connaît en plus :

```text
CurrentTarget
```

Cependant, le problème actuel est que `EffectContext` devient implicitement le contexte universel de tout.

Cela provoque :

```text
ICondition
    ↓
EffectContext
    ↓
AbilityContext
    ↓
Battle
```

même lorsque la condition n'a besoin de rien.

---

# 16. Objectif pour les contextes

Les contextes doivent représenter des informations réellement nécessaires.

Par exemple :

```text
AbilityExecutionContext
    ├── Initiator
    ├── Participants
    ├── Targets
    ├── BattleState
    ├── IsAborted
    └── Results
```

et :

```text
EffectExecutionContext
    ├── AbilityExecutionContext
    └── CurrentTarget
```

tandis que les expressions pourraient avoir :

```text
BattleEvaluationContext
```

ou un autre contexte spécialisé.

La séparation conceptuelle devient alors :

```text
Evaluation
    = lire/calculer

Execution
    = agir/modifier
```

---

# 17. ICondition / IValue / IValues / IAggregation comme mini-langage

Le point architectural le plus important du projet est probablement ici.

Ces abstractions permettent de construire un AST.

Par exemple :

```text
ComparisonCondition
│
├── Left
│   └── AggregateValue
│       ├── AverageAggregation
│       └── CharacterDataValues
│           └── CurrentHp
│
├── LessThanOrEqual
│
└── Right
    └── PercentageValue
        ├── 10
        └── AggregateValue
            ├── AverageAggregation
            └── CharacterDataValues
                └── MaxHp
```

Cela correspond conceptuellement à :

```text
Average(CurrentHp)
    <=
10% × Average(MaxHp)
```

C'est une architecture très intéressante pour un système de capacités data-driven.

---

# 18. Les capacités actuelles démontrent que le DSL fonctionne

Exemple :

```csharp
new ConditionalEffect
{
    Condition = new ComparisonCondition
    {
        Left = new AggregateValue
        {
            Aggregation = new AverageAggregation(),
            Values = new CharacterDataValues
            {
                CharacterData = CharacterData.CurrentHp,
                TargetsSelector = new AbilityTargetsSelector()
            },
        },

        Operator = ComparisonOperator.LessThanOrEqual,

        Right = new PercentageValue
        {
            Percentage = new ConstantValue { Value = 10 },

            Source = new AggregateValue
            {
                Aggregation = new AverageAggregation(),
                Values = new CharacterDataValues
                {
                    CharacterData = CharacterData.MaxHp,
                    TargetsSelector = new AbilityTargetsSelector()
                },
            },
        },
    },

    Then = ...,
    Else = ...
}
```

Ce code est certes verbeux.

Mais il démontre une propriété importante :

> les règles de gameplay peuvent être construites par composition plutôt que codées individuellement dans chaque capacité.

C'est exactement le type de propriété qui peut devenir extrêmement puissant dans un gros RPG.

---

# 19. Exemple conceptuel : Exécution

La capacité :

```text
Exécution
```

exprime :

```text
SI
    moyenne(CurrentHp des cibles)
    <=
    10% de moyenne(MaxHp des cibles)
ALORS
    Damage(999)
SINON
    Damage(50)
```

Ce n'est pas une classe :

```text
ExecutionAbility
```

avec une méthode codée en dur.

C'est un arbre de données.

C'est un excellent signal pour l'objectif data-driven.

---

# 20. Exemple conceptuel : Coup chanceux

Actuellement :

```text
ChanceEffect
    ├── Chance = 30
    ├── Success = Damage(50)
    └── Failure = null
```

et `ChanceEffect` est lui-même du sucre syntaxique pour :

```text
ConditionalEffect
    ├── Random(1..100) <= Chance
    ├── Success
    └── Failure
```

C'est une très bonne propriété du DSL.

Il faut conserver cette philosophie :

> Les constructions pratiques peuvent être du sucre syntaxique au-dessus de primitives plus fondamentales.

---

# 21. Sucre syntaxique

Exemples :

```text
PercentageValue
ChanceValue
ChanceEffect
```

peuvent être considérés comme des raccourcis.

Par exemple :

```text
PercentageValue(Source, 10)
```

équivaut à :

```text
(Source * 10) / 100
```

Cela donne une hiérarchie conceptuelle :

```text
Primitives
    ↓
Compositions
    ↓
Sucre syntaxique
```

Il est important de ne pas multiplier inutilement les primitives lorsque quelque chose peut être exprimé avec celles existantes.

---

# 22. ComparisonCondition et ComparisonOperator

Question initiale :

> Est-il pertinent d'avoir `ComparisonCondition.cs` et `ComparisonOperator.cs` séparés alors que l'opérateur n'est utilisé que par `ComparisonCondition` ?

Réponse :

Oui, cela peut être parfaitement raisonnable.

`ComparisonOperator` n'est pas nécessairement une classe métier autonome.

C'est un type de données décrivant un paramètre d'une comparaison.

Conceptuellement :

```text
ComparisonCondition
    ├── Left
    ├── Operator
    └── Right
```

est très cohérent.

Il n'est donc pas nécessaire de créer un fichier séparé uniquement parce que chaque type doit obligatoirement être dans son propre fichier.

La règle :

> « un fichier par type »

est une convention, pas une loi architecturale.

---

# 23. Faut-il absolument un fichier par classe/interface/record/struct ?

Pour le projet à long terme, oui, cela peut être une excellente convention.

Mais il faut comprendre pourquoi.

Avantages :

- fichiers courts ;
- navigation IDE plus simple ;
- historique Git plus précis ;
- conflits Git réduits ;
- recherche plus facile ;
- responsabilités visuellement isolées ;
- déplacement des types simplifié ;
- fichiers plus faciles à supprimer ;
- refactoring plus localisé.

Exemple :

```text
ComparisonCondition.cs
ComparisonOperator.cs
ICondition.cs
AndCondition.cs
OrCondition.cs
NotCondition.cs
```

est tout à fait acceptable.

---

# 24. Mais « un fichier par type » n'est pas une règle absolue

Il ne faut pas tomber dans :

```text
ICondition.cs
AndCondition.cs
OrCondition.cs
NotCondition.cs
ComparisonCondition.cs
ComparisonOperator.cs
...
```

si le résultat devient une forêt de centaines de micro-fichiers sans organisation conceptuelle.

Le critère supérieur est :

```text
cohésion
+
navigation
+
responsabilité
+
frontières de module
```

Dans un très gros projet, cependant, un fichier par type est une convention très défendable.

---

# 25. Répertoires par famille

La direction envisagée :

```text
Conditions/
    ICondition.cs
    AndCondition.cs
    OrCondition.cs
    NotCondition.cs
    ComparisonCondition.cs
    ComparisonOperator.cs
```

est cohérente.

Puis :

```text
Values/
    IValue.cs
    ConstantValue.cs
    RandomValue.cs
    OperationValue.cs
    ...
```

et :

```text
Effects/
    IEffect.cs
    DamageEffect.cs
    HealEffect.cs
    CompositeEffect.cs
    ConditionalEffect.cs
    ...
```

Cela donne une organisation beaucoup plus lisible.

---

# 26. Répertoire uniquement pour les interfaces ?

Il n'est généralement pas nécessaire d'avoir :

```text
Interfaces/
    IAbility.cs
    ICondition.cs
    IValue.cs
    IEffect.cs
    ...
```

au niveau global.

Une interface devrait plutôt vivre avec la famille conceptuelle qu'elle définit.

Préférer :

```text
Conditions/
    ICondition.cs
    AndCondition.cs
    OrCondition.cs
```

à :

```text
Interfaces/
    ICondition.cs

Conditions/
    AndCondition.cs
    OrCondition.cs
```

Cela garde l'abstraction et ses implémentations ensemble.

---

# 27. Les namespaces doivent suivre les frontières conceptuelles

À terme, éviter un énorme :

```csharp
namespace Game.Core;
```

contenant presque tout.

Préférer progressivement quelque chose comme :

```text
Game.Core.Expressions
Game.Core.Expressions.Values
Game.Core.Expressions.Conditions
Game.Core.Expressions.Aggregations

Game.Combat
Game.Combat.Abilities
Game.Combat.Battle
Game.Combat.Effects
Game.Combat.Targeting
Game.Combat.Values
```

ou une variante équivalente.

Le but est que le namespace reflète la responsabilité.

---

# 28. ITargetsFilter et ITargetsSelector

Il faut conserver une distinction conceptuelle entre :

```text
Filter
```

et :

```text
Selector
```

Même si les deux travaillent avec des personnages.

Un filtre répond :

> Parmi ces candidats, lesquels sont autorisés ?

Exemple :

```text
EnemiesFilter
IsAliveFilter
AlliesFilter
IsDeadFilter
```

Un selector répond :

> Quels personnages dois-je utiliser maintenant ?

Exemple :

```text
AbilityTargetsSelector
AbilityUsersSelector
```

C'est donc :

```text
Filter
    = restriction d'un ensemble

Selector
    = production/sélection d'un ensemble
```

Cette distinction est utile.

---

# 29. Ne pas transformer immédiatement les TargetFilters en ICondition

Il existe une ressemblance :

```text
IsAliveFilter
```

pourrait conceptuellement être vu comme :

```text
IsAliveCondition
```

Mais les responsabilités sont différentes.

Un `ICondition` actuel répond :

```text
true / false
```

alors qu'un `ITargetsFilter` répond :

```text
liste de personnages
```

Il ne faut donc pas fusionner les deux abstractions simplement parce qu'elles utilisent des prédicats.

Une architecture future pourrait partager des primitives internes, mais ce n'est pas obligatoire.

---

# 30. Possibilité future : TargetSelection comme expression

Une idée à garder dans les pistes de recherche :

```csharp
public ICondition TargetSelection { get; init; }
```

ou une abstraction similaire.

Mais attention :

une condition booléenne n'est pas naturellement une sélection de plusieurs entités.

Il serait probablement plus propre, si ce besoin apparaît réellement, d'avoir une abstraction spécialisée du type :

```text
IEntityPredicate
ITargetPredicate
ISelectionExpression
```

plutôt que de forcer `ICondition` à tout faire.

Cette piste doit donc rester ouverte, mais ne doit pas être implémentée prématurément.

---

# 31. IValue et IValues

La distinction :

```text
IValue
IValues
```

est intéressante.

`IValue` :

```text
une valeur
```

`IValues` :

```text
une collection de valeurs
```

Exemple :

```text
CharacterDataValues
```

peut retourner :

```text
[100, 80, 40]
```

puis :

```text
AverageAggregation
```

transforme :

```text
[100, 80, 40]
```

en :

```text
73
```

Cela permet :

```text
Values
    ↓
Aggregation
    ↓
Value
```

Cette composition est particulièrement pertinente pour un DSL.

---

# 32. IAggregation

`IAggregation` est probablement l'une des abstractions les plus réutilisables.

Exemples futurs :

```text
Average
Sum
Min
Max
Count
Median
...
```

Une agrégation n'a pas besoin de connaître le combat.

Elle reçoit :

```csharp
IReadOnlyList<int>
```

et renvoie :

```csharp
int
```

Elle constitue donc une excellente primitive générique.

---

# 33. Attention aux divisions par zéro

Actuellement :

```csharp
Operator.Divide => right == 0 ? left : left / right,
```

Cette décision doit être documentée.

Elle signifie :

```text
x / 0
→ x
```

Ce n'est pas une propriété mathématique standard.

Il faut donc décider explicitement quelle sémantique le DSL veut avoir.

Possibilités :

```text
exception
```

ou :

```text
0
```

ou :

```text
x
```

ou une valeur spéciale.

Ce genre de règle doit être stable car elle fera partie du comportement du DSL.

---

# 34. Randomité : point architectural important

Actuellement :

```csharp
Random.Shared
```

est utilisé directement par :

```text
RandomValue
```

Cela rend les tests déterministes plus difficiles.

À terme, envisager une abstraction :

```csharp
IRandomSource
```

ou équivalent.

Ainsi :

```text
production
    → random réel

tests
    → random déterministe
```

Cela deviendra particulièrement important pour :

- tests de capacités ;
- replay ;
- sauvegardes ;
- synchronisation réseau ;
- debugging.

Ce point est à traiter relativement tôt.

---

# 35. Résultats et CustomData

Actuellement :

```csharp
List<IEffectResult> Results
Dictionary<string, object> CustomData
```

Le `CustomData` est pratique au début.

Mais :

```csharp
Dictionary<string, object>
```

est une zone à surveiller.

À grande échelle, cela peut devenir :

```text
"foo"
"bar"
"whatever"
```

avec des clés implicites et des casts.

Cela détruit progressivement le typage.

Le conserver temporairement est acceptable.

Mais dès que son usage devient significatif, préférer des structures typées.

---

# 36. Les données de référence doivent devenir immuables

Une direction envisagée :

```text
IAbility
ISequence
IEffect
ICondition
IValue
IValues
IAggregation
```

avec des implémentations devenant des `record`.

L'idée est pertinente si ces objets deviennent réellement des **définitions de données**.

Par exemple :

```csharp
public sealed record ConstantValue(int Value) : IValue;
```

ou une forme équivalente.

Cela facilite :

- sérialisation ;
- comparaison ;
- immutabilité ;
- cache ;
- partage ;
- génération ;
- édition ;
- éventuellement hash/identité structurelle.

Mais il ne faut pas transformer aveuglément tous les objets en records.

Une classe représente généralement mieux un objet avec identité et comportement mutable.

---

# 37. Ce qui devrait potentiellement devenir des records

Très bons candidats :

```text
AbilityDefinition
Sequence
Condition expressions
Value expressions
Aggregation expressions
Effect definitions
Status definitions
```

à condition qu'ils représentent bien des données immuables.

Moins évident :

```text
BattleState
Character
Team
BattleEngine
```

qui possèdent une identité et/ou un état mutable.

---

# 38. Sérialisation

L'objectif final semble être :

```text
Code
    ↓
définition de données
    ↓
JSON
    ↓
chargement
    ↓
runtime
```

Exemple conceptuel :

```json
{
  "type": "ComparisonCondition",
  "left": {
    "type": "AggregateValue",
    "aggregation": {
      "type": "Average"
    },
    "values": {
      "type": "CharacterDataValues",
      "data": "CurrentHp"
    }
  },
  "operator": "LessThanOrEqual",
  "right": {
    "type": "PercentageValue",
    "percentage": 10,
    "source": {
      "type": "AggregateValue",
      "aggregation": {
        "type": "Average"
      },
      "values": {
        "type": "CharacterDataValues",
        "data": "MaxHp"
      }
    }
  }
}
```

Cela permettra potentiellement de créer les capacités hors du code C#.

Mais il faut d'abord stabiliser le modèle conceptuel.

Ne pas optimiser la sérialisation d'une architecture encore mouvante.

---

# 39. Ordre de priorité du refactoring

L'erreur serait de commencer par :

```text
renommer 300 fichiers
```

ou :

```text
transformer tout en records
```

sans stabiliser les responsabilités.

L'ordre recommandé est :

```text
1. Clarifier les frontières conceptuelles
2. Clarifier les contextes
3. Séparer BattleState / BattleEngine
4. Séparer définition / exécution des abilities
5. Stabiliser le DSL
6. Séparer DSL générique / combat
7. Organiser fichiers/namespaces
8. Transformer les définitions en records
9. Ajouter sérialisation
10. Optimiser
```

---

# 40. Plan d'action détaillé

## Phase 0 — Geler temporairement le comportement

Avant le gros refactoring :

- conserver les capacités actuelles ;
- écrire quelques tests de comportement ;
- vérifier que le comportement actuel est connu.

Créer des tests pour :

```text
Coup d'épée
Triple flèches
Exécution
Coup chanceux
Soin
Résurrection
Soin multi
Feu
Foudre
Amour impossible
Combo gobelin
```

Le but n'est pas de tester chaque ligne.

Le but est d'avoir une protection contre les régressions.

---

# 41. Phase 1 — Tests du DSL

Tester indépendamment :

## Values

```text
ConstantValue
RandomValue
OperationValue
PercentageValue
ConditionalValue
AggregateValue
```

## Values collections

```text
ConstantValues
CharacterDataValues
ConditionalValues
```

## Aggregations

```text
AverageAggregation
```

## Conditions

```text
ComparisonCondition
AndCondition
OrCondition
NotCondition
```

Cela permet de refactorer l'architecture sans avoir peur de casser la sémantique.

---

# 42. Phase 2 — Retirer EffectContext des abstractions génériques

Commencer par examiner :

```text
IValue
IValues
ICondition
IAggregation
```

et noter exactement ce dont chaque implémentation a besoin.

Objectif :

```text
ConstantValue
    → aucun contexte

OperationValue
    → contexte nécessaire uniquement pour ses enfants

AverageAggregation
    → aucun contexte

ComparisonCondition
    → contexte nécessaire uniquement pour ses enfants

CharacterDataValues
    → contexte de combat
```

Cela révèle la vraie dépendance.

---

# 43. Phase 3 — Introduire un contexte d'évaluation

Une fois les dépendances connues, introduire éventuellement :

```csharp
IEvaluationContext
```

puis :

```text
BattleEvaluationContext
```

ou une structure équivalente.

L'objectif :

```text
CharacterDataValues
    ↓
BattleEvaluationContext
```

et non :

```text
CharacterDataValues
    ↓
EffectContext
    ↓
AbilityContext
```

---

# 44. Phase 4 — Séparer EvaluationContext et ExecutionContext

Conceptuellement :

```text
Evaluation
    ├── lire
    └── calculer

Execution
    ├── modifier
    ├── interrompre
    └── produire des résultats
```

Cela donnera probablement :

```text
BattleEvaluationContext
```

pour le DSL,

et :

```text
AbilityExecutionContext
EffectExecutionContext
```

pour l'exécution.

---

# 45. Phase 5 — BattleState

Extraire de `Battle` :

```text
Teams
IsOver
Winner
```

et toutes les données d'état.

Créer :

```text
BattleState
```

Puis laisser :

```text
BattleEngine
```

gérer les règles d'exécution.

Objectif :

```text
BattleState
    = ce qui existe

BattleEngine
    = ce qui se passe
```

---

# 46. Phase 6 — BattleAction

Renommer progressivement :

```text
QueuedAction
→
BattleAction
```

Puis déplacer progressivement la logique :

```text
validation
résolution des cibles
paiement
exécution
```

vers :

```text
BattleEngine
```

ou des services dédiés.

Le `BattleAction` doit principalement représenter :

```text
qui
avec qui
quelle capacité
quelles cibles
```

---

# 47. Phase 7 — Ability devient principalement une définition

Objectif :

```text
Ability
```

contient :

```text
Id
Name
Target rules
Participant rules
Sequences
Costs
```

et non toute la mécanique d'exécution.

Créer éventuellement :

```text
AbilityValidator
AbilityExecutor
```

ou un :

```text
AbilityEngine
```

si cela correspond mieux à l'architecture finale.

Ne pas créer trois classes si une seule suffit.

---

# 48. Phase 8 — Clarifier Spell et Synergy

Conserver :

```text
Skill
Spell
Synergy
```

tant qu'ils correspondent à de vraies notions du game design.

Pour `Spell` :

```text
ManaCost
```

Pour `Synergy` :

```text
RequiredParticipants
RequiredSynergyBars
```

Puis surveiller l'évolution du système.

Si les coûts se multiplient :

```text
IAbilityCost
```

devient alors une abstraction justifiée.

---

# 49. Phase 9 — Targeting

Conserver :

```text
ITargetsFilter
ITargetsSelector
```

tant que leurs responsabilités restent différentes.

Réorganiser :

```text
Combat/
    Targeting/
        Filters/
        Selectors/
```

ou une structure équivalente.

Éviter de les fusionner prématurément.

---

# 50. Phase 10 — Effects

Organiser :

```text
Combat/
    Effects/
        IEffect.cs
        DamageEffect.cs
        HealEffect.cs
        CompositeEffect.cs
        ConditionalEffect.cs
        ChanceEffect.cs
```

Puis progressivement :

```text
DamageResult
HealResult
...
```

si les résultats deviennent utiles.

---

# 51. Phase 11 — DSL

Créer une frontière explicite :

```text
Core/
    Expressions/
```

avec :

```text
Values/
Conditions/
Aggregations/
```

et les interfaces :

```text
IValue
IValues
ICondition
IAggregation
```

Les implémentations purement génériques y vivent.

---

# 52. Phase 12 — Expressions dépendantes du combat

Ne pas mettre automatiquement toutes les implémentations de `IValue` dans `Core`.

Par exemple :

```text
CharacterDataValues
```

est dépendant de :

```text
Character
Battle
Targets
```

Il peut donc rester dans :

```text
Combat/Values/
```

tout en implémentant :

```text
IValues
```

Cela donne :

```text
Core DSL
    ↑
Combat-specific expressions
```

---

# 53. Phase 13 — Organisation des fichiers

Une fois les responsabilités stabilisées :

```text
un type principal
=
un fichier
```

devient la convention générale.

Exemple :

```text
ComparisonCondition.cs
ComparisonOperator.cs
ICondition.cs
AndCondition.cs
OrCondition.cs
NotCondition.cs
```

Puis supprimer les gros fichiers :

```text
Conditions.cs
Values.cs
Effects.cs
Entities.cs
```

progressivement.

---

# 54. Phase 14 — Namespaces

Faire correspondre namespaces et répertoires.

Exemple :

```csharp
Game.Core.Expressions.Conditions
Game.Core.Expressions.Values
Game.Core.Expressions.Aggregations

Game.Combat.Abilities
Game.Combat.Battle
Game.Combat.Effects
Game.Combat.Targeting
```

L'objectif est que le namespace permette de comprendre immédiatement le rôle d'un type.

---

# 55. Phase 15 — Records

Une fois le modèle stabilisé :

Transformer progressivement les définitions immuables en records.

Exemples :

```text
ConstantValue
OperationValue
ComparisonCondition
Sequence
AbilityDefinition
DamageEffect
HealEffect
...
```

Tester la sérialisation après chaque famille.

Ne pas tout transformer en une seule opération.

---

# 56. Phase 16 — Sérialisation

Construire progressivement :

```text
JSON
↓
Definition
↓
Runtime
```

Commencer par quelques objets simples :

```text
ConstantValue
OperationValue
ComparisonCondition
DamageEffect
Sequence
```

Puis progresser vers :

```text
Ability
```

---

# 57. Phase 17 — Factory / polymorphic serialization

Lorsque le DSL devient suffisamment stable, définir comment les types polymorphiques sont identifiés.

Exemple conceptuel :

```json
{
    "type": "DamageEffect"
}
```

ou :

```json
{
    "$type": "damage"
}
```

Le format exact n'est pas important à ce stade.

Ce qui compte est que :

```text
JSON
    ↓
type connu
    ↓
objet correspondant
```

soit déterministe et versionnable.

---

# 58. Phase 18 — Versionnement des données

Pour un projet qui vise plusieurs décennies, les données de jeu finiront probablement par vivre plus longtemps que certaines classes C#.

Il faut donc prévoir :

```text
schema version
```

ou une stratégie équivalente.

Exemple :

```json
{
    "version": 3,
    ...
}
```

L'objectif est de pouvoir faire évoluer :

```text
Ability v1
→
Ability v2
→
Ability v3
```

sans rendre toutes les anciennes données inutilisables.

---

# 59. Phase 19 — Tests du moteur

Après séparation :

```text
BattleState
BattleEngine
BattleAction
Ability
```

tester :

```text
action valide
action invalide
initiateur mort
participant mort
cible invalide
mana insuffisant
synergie insuffisante
ordre de vitesse
mort d'une cible avant action suivante
plusieurs participants
plusieurs cibles
```

Puis tester les interactions.

---

# 60. Phase 20 — Tests du DSL

Construire des tests qui ne connaissent absolument rien au moteur de combat lorsque cela est possible.

Exemple :

```text
Constant(10)
+
Constant(20)
=
30
```

ou :

```text
Percentage(100, 10)
=
10
```

ou :

```text
Average([10, 20, 30])
=
20
```

ou :

```text
10 < 20
=
true
```

Puis seulement les expressions dépendantes du combat :

```text
Average(CurrentHp(targets))
```

---

# 61. Point très important : les capacités dans Program.cs

Le code actuel :

```csharp
var coupEpee = new Skill
{
    ...
};
```

est excellent comme prototype.

Il montre très clairement le DSL.

Mais à terme, `Program.cs` ne doit évidemment pas devenir :

```text
10000 capacités
```

Le but final est probablement :

```text
JSON / fichiers de données
        ↓
loader
        ↓
Ability definitions
        ↓
Battle
```

Le `Program.cs` actuel est donc un excellent banc d'essai.

Il ne doit simplement pas devenir le système de production.

---

# 62. Ce qu'il faut absolument préserver

Plusieurs idées actuelles sont suffisamment bonnes pour constituer le noyau conceptuel futur.

## 62.1. Composition

```text
ConditionalEffect
    ↓
Effect

Sequence
    ↓
Effects

Ability
    ↓
Sequences
```

Très bonne direction.

---

## 62.2. Expressions imbriquées

```text
PercentageValue
    ↓
AggregateValue
        ↓
CharacterDataValues
```

Très bonne direction.

---

## 62.3. Séparation lecture / mutation

```text
IValue
ICondition
```

pour lire/calculer.

```text
IEffect
```

pour modifier.

Cette séparation doit devenir encore plus nette.

---

## 62.4. Séparation sélection / effet

```text
TargetSelector
    ↓
targets
    ↓
Effect
```

Très bonne direction.

---

## 62.5. Séquences

```text
Sequence 1
    Heal users

Sequence 2
    Damage targets
```

permettent déjà de décrire des capacités complexes sans créer une classe spécifique pour chaque capacité.

---

# 63. Ce qui est actuellement fragile

## 63.1. Contextes trop larges

```text
EffectContext
```

est actuellement utilisé comme contexte universel.

À réduire.

---

## 63.2. Battle mélange état et orchestration

À séparer.

---

## 63.3. Ability mélange définition et exécution

À séparer progressivement.

---

## 63.4. `Dictionary<string, object>`

À surveiller.

---

## 63.5. Random.Shared

À abstraire lorsque les tests/replays deviendront importants.

---

## 63.6. Console.WriteLine dans les Effects

À sortir progressivement du moteur.

---

## 63.7. GenericResult

Acceptable maintenant.

Mais à remplacer progressivement par des résultats métier lorsque cela devient utile.

---

# 64. Ce qu'il ne faut PAS faire

Ne pas faire :

```text
tout générique
```

Ne pas créer :

```text
IUniversalGameContext
```

qui contient :

```text
Battle
Character
Quest
Inventory
World
Player
...
```

Ce serait exactement le genre d'abstraction qui détruit la séparation recherchée.

---

Ne pas créer non plus :

```text
IEntity
IGameObject
IThing
IAction
IManager
IService
```

uniquement pour rendre l'architecture « générique ».

Chaque abstraction doit résoudre un vrai problème.

---

# 65. Règle d'or pour les futures abstractions

Avant de créer une interface, demander :

```text
1. Quelles implémentations existent ?
2. Qu'est-ce qu'elles ont réellement en commun ?
3. Qui consomme cette abstraction ?
4. Est-ce que plusieurs systèmes en ont besoin ?
5. Est-ce que cette abstraction réduit réellement le couplage ?
6. Est-ce qu'elle représente un concept métier réel ?
```

Si la réponse est :

> « Je pense que ça pourrait être utile un jour »

alors attendre.

---

# 66. Règle d'or pour les dépendances

Une dépendance doit idéalement aller :

```text
générique
    ↑
spécifique
```

et non :

```text
générique
    ↓
spécifique
```

Exemple souhaitable :

```text
IValue
    ↑
CharacterDataValues
```

et non :

```text
IValue
    ↓
Battle
```

---

# 67. Architecture conceptuelle finale visée

Une vision possible :

```text
                    ┌────────────────────────┐
                    │        Core DSL        │
                    │                        │
                    │ IValue                 │
                    │ IValues                │
                    │ ICondition             │
                    │ IAggregation           │
                    │                        │
                    │ Constant               │
                    │ Operation              │
                    │ Percentage             │
                    │ Comparison             │
                    │ And / Or / Not         │
                    │ Average / Sum / ...    │
                    └───────────▲────────────┘
                                │
                                │ utilise
                                │
              ┌─────────────────┴──────────────────┐
              │                                    │
              │                                    │
    ┌─────────┴─────────┐               ┌──────────┴─────────┐
    │      Combat       │               │       Quests       │
    │                   │               │                    │
    │ CharacterData     │               │ QuestDataValue     │
    │ DamageEffect      │               │ QuestCondition     │
    │ HealEffect        │               │ QuestEffect        │
    │ Sequence          │               │ ...                │
    │ Ability           │               │                    │
    │ BattleState       │               │                    │
    └─────────┬─────────┘               └────────────────────┘
              │
              │
       ┌──────▼──────┐
       │ BattleEngine│
       └─────────────┘
```

Le DSL ne sait pas que le combat existe.

Le combat sait utiliser le DSL.

---

# 68. Question fondamentale à conserver pendant toute la durée du projet

À chaque nouvelle fonctionnalité :

> Est-ce que je suis en train d'ajouter une nouvelle primitive au langage, une nouvelle donnée du domaine, ou une nouvelle opération du moteur ?

Cette question devrait guider l'emplacement du code.

Exemple :

### `CurrentHp`

```text
donnée du domaine
```

→ Combat.

### `Average`

```text
opération mathématique
```

→ DSL générique.

### `Damage`

```text
mutation du combat
```

→ Combat.

### `Compare <=`

```text
primitive logique
```

→ DSL.

### `ApplyPoison`

```text
mutation du combat
```

→ Combat.

### `HasCompletedQuest`

```text
lecture du domaine des quêtes
```

→ expression spécifique au système de quêtes.

---

# 69. Organisation finale possible

Une structure possible, à adapter au fur et à mesure :

```text
Game/
│
├── Core/
│   └── Expressions/
│       ├── Values/
│       │   ├── IValue.cs
│       │   ├── IValues.cs
│       │   ├── ConstantValue.cs
│       │   ├── ConstantValues.cs
│       │   ├── OperationValue.cs
│       │   ├── PercentageValue.cs
│       │   ├── ConditionalValue.cs
│       │   ├── ConditionalValues.cs
│       │   ├── AggregateValue.cs
│       │   └── ...
│       │
│       ├── Conditions/
│       │   ├── ICondition.cs
│       │   ├── ComparisonCondition.cs
│       │   ├── ComparisonOperator.cs
│       │   ├── AndCondition.cs
│       │   ├── OrCondition.cs
│       │   └── NotCondition.cs
│       │
│       └── Aggregations/
│           ├── IAggregation.cs
│           ├── AverageAggregation.cs
│           ├── SumAggregation.cs
│           └── ...
│
├── Combat/
│   │
│   ├── Abilities/
│   │   ├── IAbility.cs
│   │   ├── BaseAbility.cs
│   │   ├── Skill.cs
│   │   ├── Spell.cs
│   │   └── Synergy.cs
│   │
│   ├── Battle/
│   │   ├── BattleState.cs
│   │   ├── BattleEngine.cs
│   │   ├── BattleAction.cs
│   │   ├── BattleResult.cs
│   │   ├── AbilityExecutionContext.cs
│   │   └── EffectExecutionContext.cs
│   │
│   ├── Effects/
│   │   ├── IEffect.cs
│   │   ├── DamageEffect.cs
│   │   ├── HealEffect.cs
│   │   ├── CompositeEffect.cs
│   │   ├── ConditionalEffect.cs
│   │   └── ChanceEffect.cs
│   │
│   ├── Targeting/
│   │   ├── Filters/
│   │   │   ├── ITargetsFilter.cs
│   │   │   ├── EnemiesFilter.cs
│   │   │   ├── AlliesFilter.cs
│   │   │   ├── IsAliveFilter.cs
│   │   │   └── IsDeadFilter.cs
│   │   │
│   │   └── Selectors/
│   │       ├── ITargetsSelector.cs
│   │       ├── AbilityTargetsSelector.cs
│   │       └── AbilityUsersSelector.cs
│   │
│   ├── Values/
│   │   └── CharacterDataValues.cs
│   │
│   └── Entities/
│       ├── Character.cs
│       ├── Team.cs
│       ├── Stats.cs
│       └── SynergyGauge.cs
│
├── Characters/
│
├── Items/
│
├── Quests/
│
├── Progression/
│
└── World/
```

Ce n'est qu'une architecture cible possible.

Elle ne doit pas être appliquée intégralement immédiatement.

---

# 70. Ordre concret de migration du projet actuel

À partir du code actuel, l'ordre recommandé est :

```text
[1] Écrire les tests comportementaux
        ↓
[2] Extraire les types actuellement dans les gros fichiers
        ↓
[3] Séparer namespaces/répertoires
        ↓
[4] Étudier précisément les dépendances des expressions
        ↓
[5] Retirer EffectContext des abstractions génériques
        ↓
[6] Introduire le contexte d'évaluation si nécessaire
        ↓
[7] Séparer BattleState / BattleEngine
        ↓
[8] Renommer QueuedAction → BattleAction
        ↓
[9] Déplacer validation/exécution hors de Ability
        ↓
[10] Stabiliser IAbility
        ↓
[11] Stabiliser ISequence
        ↓
[12] Stabiliser Effects
        ↓
[13] Stabiliser Targeting
        ↓
[14] Stabiliser le DSL
        ↓
[15] Introduire les records
        ↓
[16] Construire la sérialisation
        ↓
[17] Déplacer les capacités hors de Program.cs
        ↓
[18] Ajouter versionnement des données
```

---

# 71. Critère de réussite

Le refactoring est réussi lorsque l'ajout d'une nouvelle capacité ressemble toujours à :

```text
combinaison de primitives existantes
```

plutôt qu'à :

```text
création d'une nouvelle classe spéciale
```

Par exemple :

```text
Nouvelle capacité :
    Condition
        +
    Value
        +
    Effect
        +
    Sequence
```

et non :

```text
NewMegaFireExecutionAbility.cs
```

avec 300 lignes de logique spéciale.

---

# 72. Critère de réussite n°2 : réutilisation du DSL

Le deuxième objectif est qu'une primitive comme :

```text
Average
```

ou :

```text
Comparison
```

puisse être utilisée ailleurs que dans le combat.

Par exemple :

```text
Combat
    Average(CurrentHp)
```

```text
Quest
    CurrentLevel >= 20
```

```text
Equipment
    Attack >= 100
```

```text
Ability Unlock
    Strength >= 50
```

Le DSL devient alors une infrastructure commune.

---

# 73. Critère de réussite n°3 : le combat ne pollue pas le DSL

Le test mental le plus important :

> Si je supprimais complètement le système de combat du projet, est-ce que les primitives mathématiques et logiques du DSL auraient encore du sens ?

Pour :

```text
Constant
Operation
Percentage
Average
Comparison
And
Or
Not
```

la réponse doit être oui.

Pour :

```text
DamageEffect
HealEffect
AbilityUsersSelector
CharacterDataValues
```

la réponse peut être non.

C'est exactement la frontière recherchée.

---

# 74. Critère de réussite n°4 : les systèmes futurs utilisent le DSL sans connaître le combat

Exemple futur :

```text
QuestCondition
```

devrait pouvoir utiliser :

```text
ICondition
IValue
```

sans dépendre de :

```text
Battle
AbilityContext
EffectContext
```

De même :

```text
EquipmentRequirement
```

pourrait utiliser le même mécanisme.

---

# 75. Critère de réussite n°5 : le moteur peut évoluer indépendamment du DSL

Le combat pourrait évoluer :

```text
BattleEngine v1
→
BattleEngine v2
```

sans casser :

```text
Average
Comparison
Percentage
Operation
```

Inversement, le DSL pourrait évoluer sans imposer une réécriture complète du moteur de combat.

---

# 76. Décisions actuelles

## Décision : conserver `Skill`, `Spell`, `Synergy`

Oui.

Ils correspondent à de vrais concepts du game design actuel.

---

## Décision : ne pas imposer immédiatement `IAbilityCost`

Pas encore.

Surveiller l'évolution.

---

## Décision : conserver `ICondition`, `IValue`, `IValues`, `IAggregation`

Oui.

Ce sont actuellement les abstractions les plus prometteuses pour le DSL.

---

## Décision : ne pas rendre `IEffect` universel

Oui.

`IEffect` est une abstraction de mutation du combat.

---

## Décision : conserver `ITargetsFilter` et `ITargetsSelector` séparés

Oui, pour l'instant.

---

## Décision : séparer le DSL du combat

Oui.

C'est une priorité architecturale majeure.

---

## Décision : fichier par type

Oui comme convention générale à terme.

Mais seulement après avoir stabilisé les familles et responsabilités.

---

## Décision : pas de dossier global `Interfaces`

Non.

Les interfaces doivent vivre dans leur famille conceptuelle.

---

## Décision : transformer les définitions en records

Oui, potentiellement.

Mais seulement après stabilisation du modèle.

---

# 77. Ce qu'il faut garder en tête sur le long terme

Le projet n'a actuellement qu'un système majeur :

```text
Combat
```

C'est pour cela que beaucoup de concepts semblent naturellement appartenir au même endroit.

Lorsque :

```text
Quêtes
Inventaire
Progression
Monde
Dialogue
Crafting
...
```

apparaîtront, les frontières deviendront beaucoup plus visibles.

Il ne faut donc pas essayer de prédire aujourd'hui toutes les abstractions nécessaires dans vingt ans.

Il faut plutôt construire des frontières qui permettent au système de découvrir progressivement ces abstractions.

---

# 78. Principe final

Le but n'est pas de construire :

> « une architecture parfaite pour les trente prochaines années ».

C'est impossible.

Le but est de construire une architecture qui permet :

> « de faire évoluer le projet pendant trente ans sans que chaque nouvelle fonctionnalité oblige à casser les anciennes frontières ».

C'est une différence fondamentale.

Une bonne architecture à long terme n'est pas celle qui prédit parfaitement le futur.

C'est celle qui **localise le changement**.

---

# 79. Conclusion

Le code actuel possède déjà une idée architecturale particulièrement intéressante :

```text
Ability
    ↓
Sequence
    ↓
Effect
```

combinée avec :

```text
Effect
    ↓
Condition
    ↓
Value
    ↓
Values
    ↓
Aggregation
```

Cela constitue déjà les bases d'un véritable langage déclaratif de gameplay.

Le refactoring ne doit donc pas chercher à jeter ce système.

Il doit au contraire :

```text
stabiliser
    ↓
clarifier
    ↓
découpler
    ↓
généraliser uniquement ce qui le mérite
    ↓
sérialiser
    ↓
industrialiser
```

La priorité architecturale absolue est de préserver cette distinction :

```text
                ┌─────────────────────┐
                │      EXPRESSIONS    │
                │                     │
                │ Value               │
                │ Values              │
                │ Condition           │
                │ Aggregation          │
                └──────────┬──────────┘
                           │
                           │ utilisées par
                           ▼
                ┌─────────────────────┐
                │      DOMAINES       │
                │                     │
                │ Combat              │
                │ Quêtes              │
                │ Inventaire          │
                │ Progression         │
                │ ...                 │
                └──────────┬──────────┘
                           │
                           ▼
                ┌─────────────────────┐
                │     EXÉCUTION       │
                │                     │
                │ Effects             │
                │ BattleEngine        │
                │ QuestEngine         │
                │ ...                 │
                └─────────────────────┘
```

Le DSL décrit.

Le domaine fournit les données.

Le moteur exécute les mutations.

C'est cette frontière qu'il faut protéger au fur et à mesure que le projet grossit.

---

# 80. Checklist de refactoring

## Architecture

- [ ] Séparer DSL et Combat
- [ ] Séparer état et moteur
- [ ] Séparer définition et exécution
- [ ] Réduire les contextes
- [ ] Identifier les dépendances réelles

## DSL

- [ ] Stabiliser `IValue`
- [ ] Stabiliser `IValues`
- [ ] Stabiliser `ICondition`
- [ ] Stabiliser `IAggregation`
- [ ] Définir les expressions pures
- [ ] Définir les expressions dépendantes du domaine
- [ ] Tester les expressions

## Combat

- [ ] Créer `BattleState`
- [ ] Créer `BattleEngine`
- [ ] Renommer `QueuedAction` en `BattleAction`
- [ ] Extraire la validation
- [ ] Extraire l'exécution
- [ ] Stabiliser `IAbility`
- [ ] Stabiliser `ISequence`
- [ ] Stabiliser `IEffect`
- [ ] Stabiliser Targeting

## Organisation

- [ ] Un type principal par fichier
- [ ] Répertoires par concept
- [ ] Pas de dossier global `Interfaces`
- [ ] Namespaces cohérents
- [ ] Supprimer progressivement les gros fichiers

## Data-driven

- [ ] Transformer les définitions appropriées en records
- [ ] Définir la sérialisation
- [ ] Gérer les types polymorphiques
- [ ] Définir une version de schéma
- [ ] Sortir les capacités de `Program.cs`

## Robustesse

- [ ] Tests déterministes
- [ ] Abstraction de random
- [ ] Résultats d'effets typés
- [ ] Réduire `Dictionary<string, object>`
- [ ] Sortir `Console.WriteLine` du moteur

## Long terme

- [ ] Vérifier que Core ne dépend pas du Combat
- [ ] Vérifier que les expressions sont réutilisables
- [ ] Vérifier que les domaines restent indépendants
- [ ] Éviter les abstractions prématurées
- [ ] Localiser le changement plutôt que prédire le futur
