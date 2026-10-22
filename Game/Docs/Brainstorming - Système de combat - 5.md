# Brainstorming - Système de combat - 4

# Prototype 4 — Séquences et ciblage

## Contexte

Dans les prototypes précédents, une capacité contenait directement une liste de `IEffect`.

Chaque effet possédait son propre `ITargetSelector` et récupérait lui-même les personnages sur lesquels il devait s'exécuter.

Conceptuellement :

```text
Ability
│
└── Effects
    │
    ├── DamageEffect
    │   └── TargetSelector
    │
    ├── HealEffect
    │   └── TargetSelector
    │
    └── ApplyStatusEffect
        └── TargetSelector
```

Cela permettait à chaque effet de sélectionner librement ses propres targets.

Par exemple :

```text
DamageEffect
    → EnemyTargets

HealEffect
    → Casters
```

Cette approche fonctionnait, mais plusieurs questions sont progressivement apparues.

---

# Problème 1 — La responsabilité du ciblage dans les effets

Un `IEffect` avait alors deux responsabilités :

1. déterminer sur qui il devait s'exécuter ;
2. appliquer son comportement.

Par exemple :

```text
DamageEffect
    ↓
sélectionner les targets
    ↓
boucler sur les targets
    ↓
appliquer les dégâts
```

Cela rendait les effets moins atomiques.

L'objectif recherché est plutôt :

```text
DamageEffect
    ↓
infliger des dégâts à une target
```

Le ciblage et l'itération doivent donc être déplacés hors des effets.

---

# Problème 2 — L'ordre Effects → Targets ou Targets → Effects

Deux ordres d'exécution sont possibles.

## Effects → Targets

```text
Effect 1
    Target A
    Target B

Effect 2
    Target A
    Target B
```

Exemple :

```text
Damage
    Gobelin A
    Gobelin B

Heal
    Aldrick
    Veya
```

## Targets → Effects

```text
Target A
    Effect 1
    Effect 2

Target B
    Effect 1
    Effect 2
```

Exemple :

```text
Gobelin A
    Damage
    Bleeding

Gobelin B
    Damage
    Bleeding
```

Ces deux ordres ne sont pas équivalents dans tous les cas.

Certaines capacités nécessitent une exécution globale :

```text
Infliger des dégâts à tous les ennemis
↓
Soigner les casters
```

D'autres nécessitent une exécution locale par target :

```text
Pour chaque ennemi
    infliger des dégâts
    appliquer Bleeding
```

Il fallait donc pouvoir représenter une unité d'exécution regroupant :

- une sélection de targets ;
- une liste ordonnée d'effets.

---

# Première idée — ISequence

L'idée d'introduire `ISequence` est apparue.

Conceptuellement :

```text
ISequence
│
├── TargetSelector
│
└── Effects
```

Une séquence représente :

> Une liste ordonnée d'effets exécutée sur un ensemble de targets sélectionnées.

Une capacité devient alors :

```text
Ability
│
├── Sequence
│   │
│   ├── EnemyTargets
│   │
│   └── Effects
│       ├── Damage
│       └── Bleeding
│
└── Sequence
    │
    ├── Casters
    │
    └── Effects
        └── Heal
```

---

# Doute sur ISequence

Une question est ensuite apparue :

> Est-il réellement nécessaire d'avoir `ISequence` ?

Une première hypothèse consistait à utiliser uniquement des `CompositeEffect`.

---

# Alternative étudiée — CompositeEffect

Un `CompositeEffect` est un `IEffect` contenant d'autres effets.

```text
CompositeEffect
│
└── Effects
    ├── Damage
    ├── Bleeding
    └── Stun
```

Comme le composite est lui-même un `IEffect`, il peut être exécuté sur une target unique.

Conceptuellement :

```text
Target A
    CompositeEffect
        Damage
        Bleeding
        Stun
```

Cela permet de représenter :

```text
Pour chaque target
    Damage
    Bleeding
    Stun
```

sans avoir besoin que chaque effet enfant boucle sur les targets.

---

# Pourquoi CompositeEffect ne remplace pas ISequence

Le `CompositeEffect` répond à une question différente.

## CompositeEffect

Répond à :

> Que doit-on exécuter sur la target courante ?

```text
Target
    ↓
Damage
Bleeding
Stun
```

## ISequence

Répond à :

> Sur quelles targets doit-on exécuter une liste d'effets ?

```text
TargetSelector
    ↓
Targets
    ↓
Effects
```

Les deux concepts sont donc complémentaires.

`CompositeEffect` permet la composition récursive des comportements.

`ISequence` définit le scope d'exécution et le ciblage.

---

# Alternative étudiée — TargetedEffect

Une autre solution envisagée consistait à introduire :

```text
TargetedEffect
│
├── TargetSelector
│
└── Effect
```

Exemple :

```text
TargetedEffect
│
├── EnemyTargets
│
└── CompositeEffect
    ├── Damage
    └── Bleeding
```

Cette solution permettait de déplacer le ciblage hors des effets.

Cependant, un problème apparaît lorsque les `TargetedEffect` deviennent imbriqués.

Exemple :

```text
TargetedEffect
│
├── EnemyTargets
│
└── CompositeEffect
    │
    ├── Damage
    │
    └── TargetedEffect
        │
        ├── Casters
        │
        └── Heal
```

Une question devient alors ambiguë :

> Quelle est la target courante ?

La première sélection établit :

```text
Target = Enemy A
```

La sélection imbriquée établit ensuite :

```text
Target = Caster A
Target = Caster B
```

Il faudrait alors gérer des scopes imbriqués ou une pile de contextes.

Cette complexité n'apporte pas de bénéfice évident.

---

# Décision

La décision actuelle est de conserver `ISequence`.

Une capacité ne contient plus directement :

```csharp
IReadOnlyList<IEffect> Effects
```

mais :

```csharp
IReadOnlyList<ISequence> Sequences
```

Une séquence contient :

```csharp
ITargetSelector TargetSelector

IReadOnlyList<IEffect> Effects
```

---

# Architecture résultante

```text
Ability
│
└── Sequences
    │
    ├── Sequence
    │   │
    │   ├── TargetSelector
    │   │
    │   └── Effects
    │       │
    │       ├── DamageEffect
    │       ├── ConditionalEffect
    │       └── ApplyStatusEffect
    │
    └── Sequence
        │
        ├── TargetSelector
        │
        └── Effects
            │
            └── HealEffect
```

---

# Responsabilités

## IAbility

Responsable de :

> Représenter une capacité et exécuter ses séquences dans leur ordre.

---

## ISequence

Responsable de :

> Sélectionner un ensemble de targets et exécuter une liste ordonnée d'effets sur chaque target.

Une séquence définit donc un scope d'exécution.

Conceptuellement :

```text
Selector
    ↓
Targets
    ↓
pour chaque target
    ↓
Effects
```

---

## ITargetSelector

Responsable de :

> Déterminer quelles targets sont sélectionnées.

Exemples possibles :

```text
AbilityTargetsSelector
CastersSelector
AlliesSelector
EnemiesSelector
DeadTargetsSelector
```

Le selector ne modifie pas les targets.

Il les sélectionne uniquement.

---

## IEffect

Responsable de :

> Appliquer son comportement à une target unique.

Exemples :

```text
DamageEffect
HealEffect
ApplyStatusEffect
ConditionalEffect
ChanceEffect
CompositeEffect
```

Un `IEffect` ne doit pas avoir besoin de boucler sur une liste de targets.

Il reçoit une target dans son `EffectContext`.

---

## EffectContext

Responsable de transmettre le contexte nécessaire à l'exécution d'un effet.

Il représente notamment la target courante.

Conceptuellement :

```text
EffectContext
│
├── AbilityContext
│
└── Target
```

Toutes les informations globales nécessaires peuvent être accessibles via l'`AbilityContext`.

---

## IValue

Responsable de :

> Résoudre une valeur dans un contexte donné.

Exemples :

```text
ConstantValue
TargetMaxHpPercentageValue
PreviousDamagePercentageValue
```

---

## ICondition

Responsable de :

> Déterminer si une condition est vraie ou fausse dans un contexte donné.

Exemples :

```text
TargetHpBelowCondition
TargetHasStatusCondition
```

---

# Effets composites

Certains effets peuvent contenir d'autres effets.

Exemple :

```text
ConditionalEffect
│
├── Condition
│
├── Then
│   ├── Damage
│   └── ApplyBleeding
│
└── Else
    └── Heal
```

Cela correspond au pattern Composite.

Un `IEffect` peut donc être :

- atomique ;
- composite.

Cette composition est volontairement autorisée.

---

# Pourquoi Then et Else peuvent contenir plusieurs effets

Même si les premières capacités n'ont pas nécessairement besoin de plusieurs effets dans une branche conditionnelle, ce besoin peut apparaître rapidement.

Exemple :

```text
Si HP < 30 %
    Damage 100
    Apply Bleeding
    Apply Stun

Sinon
    Damage 50
```

Il est donc préférable que `ConditionalEffect` puisse directement contenir :

```csharp
IReadOnlyList<IEffect> Then

IReadOnlyList<IEffect>? Else
```

Cela évite d'introduire une modification structurelle plus tard pour un besoin déjà prévisible.

---

# Ordre d'exécution

Les séquences d'une capacité sont exécutées dans leur ordre.

Exemple :

```text
Sequence 1
    EnemyTargets
        Damage
        Bleeding

Sequence 2
    Casters
        Heal
```

L'exécution devient :

```text
Pour chaque ennemi
    Damage
    Bleeding

Puis

Pour chaque caster
    Heal
```

Chaque séquence définit donc sa propre granularité d'exécution.

---

# Pourquoi ISequence est préférable à TargetedEffect

`TargetedEffect` peut devenir ambigu lorsqu'il est imbriqué.

`ISequence`, au contraire, définit une frontière claire :

```text
ISequence
    ↓
sélection des targets
    ↓
création d'un EffectContext par target
    ↓
exécution des effets
```

La target courante est donc toujours celle actuellement traitée par la séquence.

Les effets n'ont pas à gérer de scope de ciblage.

---

# Évolutions volontairement reportées

Les sujets suivants ne sont pas nécessaires pour Prototype 4 :

```text
Nested sequences
ILoop
RepeatEffect
WhileEffect
SwitchEffect
ForEachCasterSequence
ForEachTargetSequence
```

Ils seront étudiés uniquement lorsqu'un besoin concret apparaîtra.

Principe retenu :

> Ne pas ajouter une abstraction uniquement parce qu'elle pourrait être utile un jour.

Une exception peut être faite lorsqu'une évolution est très proche, très probable et que son anticipation ne complexifie pratiquement pas le modèle existant.

---

# État actuel du système

Le modèle de création de capacités devient progressivement une architecture déclarative composée de briques.

```text
Ability
│
├── Sequence
│   │
│   ├── TargetSelector
│   │
│   └── Effects
│       │
│       ├── Values
│       │
│       ├── Conditions
│       │
│       └── Effects composites
│
└── Sequence
```

Le système ne constitue pas un langage de programmation généraliste.

Il constitue cependant progressivement un modèle déclaratif permettant de décrire des capacités complexes à partir de briques composables.

Ce modèle pourrait éventuellement, dans le futur, servir de base à un outil de création de capacités.

Nom temporairement non officiel :

```text
After Effects
```

Parce que manifestement créer un système de combat raisonnable n'était pas suffisamment dangereux.