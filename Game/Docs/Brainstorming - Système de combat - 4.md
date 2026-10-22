# Brainstorming — Architecture des capacités : Values, Conditions, Target Selectors et Sequences

## Contexte

Le système de combat possède déjà une abstraction basée sur des capacités (`IAbility`) composées d'effets (`IEffect`).

Initialement, certains effets géraient eux-mêmes plusieurs responsabilités :

- déterminer les personnages sur lesquels ils devaient agir ;
- parcourir une liste de personnages ;
- appliquer leur logique sur chaque personnage.

Par exemple :

```text
DealDamageEffect
    ↓
foreach target
    appliquer les dégâts
```

ou :

```text
HealEffect
    ↓
foreach target
    appliquer les soins
```

Cette approche fonctionne pour un prototype simple, mais plusieurs problèmes sont progressivement apparus.

Notamment :

- certains effets ont besoin de valeurs dynamiques ;
- certains effets doivent être conditionnels ;
- plusieurs effets doivent pouvoir s'appliquer à différents ensembles de personnages ;
- la boucle sur les cibles dans chaque `IEffect` mélange plusieurs responsabilités ;
- certaines capacités nécessitent plusieurs phases d'exécution.

L'objectif de cette évolution est donc de conserver des effets simples et spécialisés, tout en ajoutant progressivement des briques permettant de construire des comportements plus complexes.

---

# 1. `IValue`

## Problème

Certaines propriétés utilisent actuellement une valeur fixe.

Par exemple :

```text
HealEffect
    Amount = 100
```

Cette approche devient limitée dès qu'une valeur doit dépendre du contexte.

Exemples :

- une valeur constante ;
- un pourcentage des HP maximum ;
- un pourcentage des dégâts infligés juste avant ;
- potentiellement des statistiques ;
- potentiellement le niveau ;
- potentiellement d'autres informations du combat.

L'objectif est donc de représenter une valeur comme quelque chose qui peut être **résolu au moment de l'exécution**.

---

## Abstraction

```csharp
public interface IValue
{
    int Resolve(/* contexte */);
}
```

Le contexte exact sera déterminé en fonction de l'architecture finale du système d'exécution.

L'idée importante est que :

> `IValue` ne représente pas forcément une valeur fixe.

Il représente une instruction permettant d'obtenir une valeur.

---

## Exemple conceptuel

### Valeur constante

```text
ConstantValue
    ↓
Resolve()
    ↓
100
```

### Pourcentage des HP maximum

```text
MaxHpPercentageValue
    ↓
Resolve(context)
    ↓
50 % des HP maximum
```

---

## Responsabilité

`IValue` répond uniquement à la question :

> **Quelle valeur dois-je utiliser dans ce contexte ?**

Il ne doit pas :

- appliquer des dégâts ;
- appliquer des soins ;
- sélectionner des personnages ;
- modifier directement le combat.

---

# 2. `ICondition`

## Problème

Certaines actions doivent uniquement être exécutées si une condition est remplie.

Exemples :

- exécuter un effet uniquement si la cible possède moins de 25 % de ses HP ;
- appliquer un effet avec une certaine probabilité ;
- appliquer un effet uniquement si certaines conditions du combat sont remplies.

Créer une classe spécialisée pour chaque combinaison possible deviendrait rapidement problématique.

Par exemple :

```text
HealLowHpTargetEffect
DamageLowHpTargetEffect
ExecuteLowHpTargetEffect
```

Ce type d'approche ferait exploser le nombre de classes spécialisées.

---

## Abstraction

```csharp
public interface ICondition
{
    bool Evaluate(/* contexte */);
}
```

Le contexte exact sera déterminé avec le reste de l'architecture d'exécution.

---

## Exemple : HP de la cible

```text
TargetHpBelow
    ↓
Evaluate(context)
    ↓
true / false
```

La condition pourrait vérifier :

```text
Target.CurrentHp < Target.MaxHp × Threshold
```

---

## `ConditionalEffect`

Une condition peut être utilisée pour encapsuler un autre effet.

Conceptuellement :

```text
ConditionalEffect
│
├── Condition
│
└── Effect
```

Exécution :

```text
si Condition == true
    exécuter Effect
sinon
    ne rien faire
```

Exemple :

```text
ConditionalEffect
│
├── TargetHpBelow(25%)
│
└── ExecuteEffect
```

---

## `ChanceEffect`

Une probabilité peut également être représentée comme une forme de condition ou comme une brique spécialisée.

Conceptuellement :

```text
ChanceEffect
│
├── Chance = 50 %
│
└── Effect
```

Exécution :

```text
tirer un nombre aléatoire

si succès
    exécuter Effect
sinon
    ne rien faire
```

Cette décision pourra évoluer selon les besoins futurs.

Une question importante sera notamment :

> Une probabilité est-elle simplement une `ICondition` ?

Il est possible qu'un futur `RandomChanceCondition` permette de représenter ce comportement.

Il n'est cependant pas nécessaire de décider immédiatement si `ChanceEffect` doit disparaître ou être conservé comme abstraction spécialisée.

---

# 3. `EffectContext`

## Problème

Les différentes briques du système n'ont pas toutes besoin des mêmes informations.

Une méthode de ce type pourrait progressivement devenir problématique :

```csharp
Resolve(
    AbilityContext context,
    Character caster,
    Character target,
    Battle battle,
    ...
)
```

Certaines implémentations n'utiliseraient qu'une partie des paramètres.

À mesure que le système évolue, cette liste pourrait devenir de plus en plus importante.

---

## Solution envisagée

Introduire un contexte spécifique à l'exécution d'un effet.

Conceptuellement :

```text
AbilityContext
    ↓
contexte global de la capacité

EffectContext
    ↓
contexte d'une application précise d'un effet
```

Exemple conceptuel :

```csharp
public sealed class EffectContext
{
    public required AbilityContext AbilityContext { get; init; }

    public required Character Target { get; init; }
}
```

La structure exacte pourra évoluer.

---

## Responsabilité

`EffectContext` représente :

> **L'exécution d'un effet dans un contexte précis, sur une cible précise.**

Cela permet notamment à :

- `IEffect` ;
- `IValue` ;
- `ICondition` ;

de recevoir un contexte unique et cohérent.

---

# 4. Le problème du ciblage

## Ancienne approche

Les effets géraient directement plusieurs cibles.

Par exemple :

```text
DealDamageEffect
    ↓
foreach target
    appliquer dégâts
```

Cette approche fonctionne, mais pose un problème de responsabilité.

`DealDamageEffect` devrait conceptuellement répondre à :

> Comment infliger des dégâts à une cible ?

Il ne devrait pas nécessairement répondre à :

> Quelles sont les cibles ?

ni :

> Comment parcourir une liste de cibles ?

---

# 5. `ITargetSelector`

## Objectif

Séparer la sélection des personnages de la logique des effets.

Abstraction conceptuelle :

```csharp
public interface ITargetSelector
{
    IReadOnlyList<Character> Select(AbilityContext context);
}
```

Exemples futurs :

```text
AbilityTargetsSelector
CastersSelector
AlliesSelector
EnemiesSelector
```

Le système pourra évoluer progressivement.

---

## Responsabilité

`ITargetSelector` répond uniquement à la question :

> **Quels personnages doivent être sélectionnés dans ce contexte ?**

Il ne doit pas :

- appliquer des dégâts ;
- appliquer des soins ;
- évaluer une condition ;
- résoudre une valeur.

---

# 6. Pourquoi ne pas mettre `ITargetSelector` directement dans `IEffect` ?

Une première possibilité serait :

```text
IEffect
│
├── Selector
│
└── Execute
```

Par exemple :

```text
HealEffect
    ↓
Selector : CastersSelector
```

Cela fonctionne.

Mais cela signifie que l'effet possède deux responsabilités :

```text
HealEffect
│
├── Je soigne
│
└── Je sais qui je dois soigner
```

Ces deux responsabilités sont conceptuellement différentes.

La question devient alors :

> Où doit vivre la logique qui associe une sélection de personnages à une série d'effets ?

La réponse envisagée est :

```text
ISequence
```

---

# 7. `ISequence`

## Objectif

Une séquence représente une phase d'exécution.

Elle permet de définir :

- quels personnages sont sélectionnés ;
- quels effets sont exécutés ;
- dans quel ordre.

Structure conceptuelle :

```text
ISequence
│
├── ITargetSelector
│
└── IReadOnlyList<IEffect>
```

Une capacité devient alors conceptuellement :

```text
Ability
│
└── Sequences
    │
    ├── Sequence 1
    │   ├── Selector
    │   └── Effects
    │
    └── Sequence 2
        ├── Selector
        └── Effects
```

---

# 8. Exécution d'une séquence

Une séquence sélectionne des personnages.

```text
Sequence
    ↓
ITargetSelector.Select()
    ↓
liste de personnages
```

Puis chaque personnage sélectionné reçoit les effets de la séquence.

Conceptuellement :

```text
pour chaque target sélectionnée
    créer un EffectContext

    pour chaque effect
        exécuter l'effet
```

Pseudo-code :

```csharp
var targets = Selector.Select(abilityContext);

foreach (var target in targets)
{
    var effectContext = new EffectContext
    {
        AbilityContext = abilityContext,
        Target = target
    };

    foreach (var effect in Effects)
    {
        effect.Execute(effectContext);
    }
}
```

---

# 9. Conséquence : les effets deviennent extrêmement simples

Avec cette architecture :

```text
ITargetSelector
    ↓
sélectionne les personnages

ISequence
    ↓
organise l'exécution

IEffect
    ↓
agit sur une seule target
```

Un effet ne possède plus de boucle sur une liste de personnages.

Par exemple :

```text
HealEffect
    ↓
"Soigne cette target."
```

```text
DealDamageEffect
    ↓
"Inflige des dégâts à cette target."
```

La responsabilité devient beaucoup plus précise.

---

# 10. Exemple : plusieurs cibles

Une séquence :

```text
Sequence
│
├── Selector : AbilityTargetsSelector
│
└── Effects
    ├── DealDamageEffect
    └── ApplyStatusEffect
```

Exécution :

```text
Target A
    DealDamage
    ApplyStatus

Target B
    DealDamage
    ApplyStatus
```

Cela permet de représenter explicitement l'ordre d'exécution.

---

# 11. Exemple : `Amour impossible`

Certaines capacités possèdent plusieurs phases utilisant des ensembles de personnages différents.

Exemple conceptuel :

```text
Amour impossible
│
├── Sequence 1
│   │
│   ├── Selector : CastersSelector
│   │
│   └── Effects
│       └── HealEffect
│
└── Sequence 2
    │
    ├── Selector : AbilityTargetsSelector
    │
    └── Effects
        └── DealDamageEffect
```

Exécution :

```text
Sequence 1

Caster A
    Heal

Caster B
    Heal


Sequence 2

Target A
    DealDamage

Target B
    DealDamage
```

Cette architecture permet notamment de remplacer certains effets spécialisés comme :

```text
HealCastersEffect
```

par :

```text
HealEffect
```

utilisé dans une séquence possédant :

```text
CastersSelector
```

---

# 12. Suppression de responsabilités inutiles

Avant :

```text
HealCastersEffect
    ↓
connaît les casters
    ↓
boucle sur les casters
    ↓
applique les soins
```

Après :

```text
CastersSelector
    ↓
sélectionne les casters

Sequence
    ↓
crée un contexte par caster

HealEffect
    ↓
soigne un caster
```

La logique est répartie selon les responsabilités.

---

# 13. Différence importante entre deux stratégies d'exécution

Deux ordres d'exécution sont possibles.

## Effet puis targets

```text
Effect 1
    Target A
    Target B

Effect 2
    Target A
    Target B
```

## Target puis effets

```text
Target A
    Effect 1
    Effect 2

Target B
    Effect 1
    Effect 2
```

Ces deux comportements ne sont pas équivalents.

Le système actuel de `ISequence` permet de représenter explicitement :

```text
Une sélection
    ↓
Chaque target
    ↓
Tous les effets de cette phase
```

Les différentes phases d'une capacité peuvent ensuite être représentées par plusieurs séquences.

---

# 14. `OnceSequence`, `ForEachTargetSequence` et `ForEachCastersSequence`

Une première idée consistait à créer des implémentations comme :

```text
OnceSequence
ForEachTargetSequence
ForEachCastersSequence
```

Cependant, l'introduction de `ITargetSelector` remet cette nécessité en question.

Si toutes les séquences fonctionnent ainsi :

```text
Selector
    ↓
Select()
    ↓
foreach personnage sélectionné
    ↓
Effects
```

alors :

```text
AbilityTargetsSelector
```

et :

```text
CastersSelector
```

sont peut-être suffisants pour déterminer les personnages concernés.

Il n'est donc pas encore nécessaire de créer plusieurs types de séquences.

Une abstraction plus simple pourrait suffire :

```text
Sequence
│
├── Selector
└── Effects
```

Les spécialisations de séquence ne devront être ajoutées que lorsqu'un véritable comportement différent apparaîtra.

---

# 15. État actuel des responsabilités

## `IAbility`

Représente une capacité.

Responsabilités possibles :

- identité ;
- nom ;
- coût ;
- validation ;
- séquences d'exécution.

---

## `ISequence`

Représente une phase d'exécution.

Responsabilités :

- sélectionner des personnages via `ITargetSelector` ;
- organiser l'exécution des effets ;
- définir l'ordre des effets.

---

## `ITargetSelector`

Répond à :

> Quels personnages sont concernés ?

---

## `IEffect`

Répond à :

> Que dois-je faire à cette target ?

---

## `EffectContext`

Représente :

> L'exécution d'un effet sur une target précise.

---

## `IValue`

Répond à :

> Quelle valeur dois-je utiliser dans ce contexte ?

---

## `ICondition`

Répond à :

> Cette condition est-elle vraie dans ce contexte ?

---

# 16. Vue globale

```text
AbilityContext
    │
    ▼
IAbility
    │
    ▼
ISequence[]
    │
    ├── ITargetSelector
    │       │
    │       ▼
    │   Characters
    │
    ▼
foreach Character
    │
    ▼
EffectContext
    │
    ├── AbilityContext
    └── Target
            │
            ▼
        IEffect[]
            │
            ├── IValue
            │
            └── ICondition
```

---

# 17. Composition des comportements

Cette architecture permet de construire progressivement des capacités à partir de briques.

Exemple conceptuel :

```text
Sequence
│
├── Selector : AbilityTargetsSelector
│
└── Effects
    │
    ├── ConditionalEffect
    │   │
    │   ├── Condition
    │   │   └── TargetHpBelow
    │   │
    │   └── Effect
    │       └── DealDamageEffect
    │
    └── ApplyStatusEffect
```

Le système devient progressivement déclaratif et composable.

---

# 18. Risque d'évolution vers un système de création de capacités

La combinaison :

```text
IEffect
IValue
ICondition
ITargetSelector
ISequence
```

commence à former un système de composition.

Conceptuellement :

```text
Sequence
    ↓
sélectionne des personnages
    ↓
applique une liste d'opérations
    ↓
certaines opérations utilisent des valeurs calculées
    ↓
certaines opérations dépendent de conditions
```

Cette structure pourrait potentiellement être utilisée plus tard par :

- du code C# ;
- des fichiers de données ;
- un éditeur de capacités ;
- une application web.

Ce n'est pas un objectif immédiat.

L'objectif actuel reste :

> Construire et maltraiter les prototypes afin de vérifier que les abstractions répondent à de vrais problèmes.

---

# 19. Principe architectural important

Les abstractions ne doivent pas être ajoutées parce qu'elles semblent élégantes.

Elles doivent apparaître lorsqu'un problème réel révèle une responsabilité distincte.

La démarche actuelle est :

```text
1. Une idée apparaît.
        ↓
2. Elle semble prématurée.
        ↓
3. Elle est mise de côté.
        ↓
4. Un vrai problème apparaît.
        ↓
5. L'idée précédente est réexaminée.
        ↓
6. L'abstraction est ajoutée uniquement si elle résout réellement le problème.
```

---

# 20. État actuel du Prototype 3

La réflexion actuelle indique que l'ordre initialement prévu doit être réévalué.

L'ordre envisagé devient :

```text
Prototype 3
    │
    ├── ITargetSelector
    ├── ISequence
    ├── EffectContext
    │
    ├── simplification des IEffect
    │       ↓
    │   un effet agit sur une seule target
    │
    └── suppression progressive des effets spécialisés
        dont la responsabilité est uniquement
        de sélectionner des personnages
```

Puis :

```text
Prototype suivant
    │
    ├── IValue
    ├── ICondition
    ├── ConditionalEffect
    └── ChanceEffect
```

Puis, ultérieurement :

```text
Status
```

L'ordre exact pourra évoluer si les prototypes révèlent de nouveaux problèmes.

---

# 21. Règle principale pour la suite

Ne pas chercher à prévoir toutes les abstractions futures.

Notamment :

```text
Status
Tags
Elements
Weaknesses
Resistances
Immunities
Equipment
Passives
Progression
```

ne doivent pas encore influencer excessivement l'architecture actuelle.

Le système doit d'abord être confronté à des cas réels.

Le principe reste :

```text
Prototype
    ↓
rencontre un problème
    ↓
comprendre le problème
    ↓
identifier la responsabilité manquante
    ↓
refactoring
    ↓
nouveau prototype
    ↓
maltraitance
    ↓
"PUTAIN MAIS C'EST BEAU"
```

---

# Conclusion

Le système évolue progressivement d'une architecture où les effets possèdent plusieurs responsabilités :

```text
sélectionner
parcourir
appliquer
```

vers une architecture où chaque composant possède une responsabilité plus précise :

```text
ITargetSelector
    ↓
sélectionner

ISequence
    ↓
organiser

IEffect
    ↓
agir

IValue
    ↓
résoudre une valeur

ICondition
    ↓
évaluer une condition
```

Cette séparation permet de construire progressivement des capacités complexes à partir de briques simples.

L'objectif n'est pas de créer immédiatement un langage de programmation.

Même si, objectivement, on commence légèrement à en créer un.

Nom de travail :

```text
After Effects
```

Parce qu'apparemment, construire un jeu vidéo n'était pas suffisamment compliqué.