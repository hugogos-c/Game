# Réflexion sur le système de ciblage des capacités et des effets

> Document de réflexion / architecture du prototype  
> Statut : expérimental — aucune décision définitive n'est prise.

---

## 1. Contexte

Le prototype possède actuellement des capacités (`IAbility`) et des effets (`IEffect`).

Une capacité contient notamment :

- un identifiant ;
- un nom ;
- un type (`Skill`, `Spell`, `Synergy`, etc.) ;
- un coût éventuel ;
- un mode de ciblage (`TargetMode`) ;
- une liste d'effets.

Les effets, eux, agissent actuellement directement sur `AbilityContext.Targets`.

Exemple actuel :

```csharp
public interface IAbility
{
    string Id { get; }
    string Name { get; }
    TargetMode TargetMode { get; }
    IReadOnlyList<IEffect> Effects { get; }

    bool CanUse(IReadOnlyList<Character> casters, Battle battle);
    void PayCost(IReadOnlyList<Character> casters, Battle battle);
    void Execute(AbilityContext context);
}
```

Et :

```csharp
public interface IEffect
{
    IEffectResult Execute(AbilityContext context);
}
```

---

# 2. Première observation : le ciblage d'une Ability et celui d'un Effect ne sont pas nécessairement la même chose

Une capacité peut demander au joueur :

> « Choisis une cible ennemie. »

Mais ses effets peuvent avoir des cibles complètement différentes.

Exemple :

```text
Amour Impossible

Cible choisie par le joueur :
    → un ennemi

Effets :
    → soigne les lanceurs
    → inflige des dégâts à la cible choisie
```

Le système doit donc distinguer :

### Ciblage de la capacité

Il répond à la question :

> Quelles cibles cette capacité demande-t-elle au joueur ou au système de sélectionner ?

### Ciblage d'un effet

Il répond à la question :

> Sur quels personnages cet effet doit-il réellement agir ?

Ces deux choses peuvent coïncider, mais elles ne sont pas obligées de le faire.

---

# 3. Pourquoi `TargetMode` commence à poser problème

Le prototype utilise actuellement :

```csharp
public enum TargetMode
{
    Self,
    SingleEnemy,
    AllEnemies,
    SingleAlly,
    AllAllies
}
```

Cela fonctionne très bien tant que les besoins restent simples.

Mais les possibilités peuvent rapidement devenir beaucoup plus nombreuses :

```text
Self
SingleAlly
AllAllies
EntireAllyTeam
SingleEnemy
EntireEnemyTeam
AllEnemies
Everyone
RandomEnemy
RandomAlly
DeadAlly
LowestHpAlly
HighestAttackCaster
...
```

Le problème est que ces notions ne représentent pas toutes la même dimension.

On mélange potentiellement :

### Relation

```text
Self
Ally
Enemy
Everyone
```

### Étendue

```text
One
All
Team
```

### État

```text
Alive
Dead
Any
```

### Méthode de sélection

```text
PlayerSelected
Random
LowestHp
HighestAttack
...
```

Un seul enum finit donc par essayer de représenter une véritable **grammaire de ciblage**.

Cela risque de produire un enum gigantesque et difficile à faire évoluer.

---

# 4. À quoi servent les enums dans ce contexte ?

Les enums ne sont pas mauvais.

Ils sont très utiles pour représenter des concepts **fermés**.

Par exemple :

```csharp
public enum DamageType
{
    Physical,
    Magical,
    True
}
```

Ici, un enum peut être parfaitement adapté si le jeu possède effectivement un ensemble fermé de types de dégâts.

Même chose pour des concepts comme :

```csharp
public enum TeamRelation
{
    Ally,
    Enemy
}
```

Le problème apparaît lorsque l'enum doit représenter un système ouvert et composable.

Un `TargetMode` contenant toutes les combinaisons possibles de :

- relation ;
- état ;
- quantité ;
- équipe ;
- sélection ;
- conditions particulières ;

risque de devenir une mauvaise abstraction.

---

# 5. Première piste : `ITargetSelector`

Une abstraction intéressante apparue pendant le brainstorming est :

```csharp
public interface ITargetSelector
{
    IReadOnlyList<Character> Select(AbilityContext context);
}
```

Un effet peut alors demander son propre sélecteur.

Exemple :

```csharp
public class Heal : IEffect
{
    public required int Amount { get; init; }
    public required ITargetSelector TargetSelector { get; init; }

    public IEffectResult Execute(AbilityContext context)
    {
        var targets = TargetSelector.Select(context);

        foreach (var target in targets)
            target.Heal(Amount);

        return new GenericResult();
    }
}
```

Cela permet de supprimer la nécessité d'avoir des effets spécialisés comme :

```text
Heal
HealCasters
HealAllies
HealRandomAlly
HealLowestHpAlly
...
```

On peut avoir un seul :

```text
Heal
```

et faire varier sa cible.

---

# 6. Exemple de composition

On pourrait écrire :

```csharp
new Heal
{
    Amount = 100,
    TargetSelector =
        new LowestHpSelector(
            new AlliesSelector()
        )
}
```

Ce qui signifie :

> Soigner l'allié ayant le moins de PV.

Ou :

```csharp
new Heal
{
    Amount = 80,
    TargetSelector = new CastersSelector()
}
```

Ce qui signifie :

> Soigner les lanceurs de la capacité.

Ou :

```csharp
new DealDamage
{
    Power = 100,
    TargetSelector = new ContextTargetsSelector()
}
```

Ce qui signifie :

> Infliger les dégâts aux cibles choisies pour la capacité.

Cette composition est particulièrement intéressante car les effets restent génériques.

---

# 7. Distinction entre `TargetRule` et `TargetSelector`

Une distinction supplémentaire est apparue.

Un sélecteur répond à :

> « Parmi les cibles possibles, lesquelles dois-je sélectionner ? »

Mais il faut également savoir :

> « Qui a le droit d'être ciblé ? »

Cela mène potentiellement à deux abstractions différentes.

```csharp
public interface ITargetRule
{
    IReadOnlyList<Character> GetCandidates(AbilityContext context);

    bool IsValidTarget(Character target, AbilityContext context);
}
```

Et :

```csharp
public interface ITargetSelector
{
    IReadOnlyList<Character> Select(
        AbilityContext context,
        IReadOnlyList<Character> candidates);
}
```

---

# 8. Responsabilité de `ITargetRule`

`ITargetRule` définit l'ensemble des personnages qui peuvent être ciblés.

Exemples conceptuels :

```text
AlliesRule
EnemiesRule
LivingRule
DeadRule
CasterRule
CasterTeamRule
EnemyTeamRule
...
```

Ces règles peuvent être composées.

Par exemple :

```csharp
new LivingRule(
    new AlliesRule()
)
```

signifie :

> Les alliés vivants.

Et :

```csharp
new DeadRule(
    new AlliesRule()
)
```

signifie :

> Les alliés morts.

---

# 9. Responsabilité de `ITargetSelector`

Le sélecteur choisit réellement les cibles parmi les candidats.

Exemples :

```text
PlayerSelectedTargetSelector
AllTargetsSelector
RandomTargetSelector
LowestHpSelector
HighestAttackSelector
...
```

Ainsi :

```csharp
new LowestHpSelector(
    new LivingRule(
        new AlliesRule()
    )
)
```

peut conceptuellement signifier :

> Parmi les alliés vivants, sélectionner celui qui possède le moins de PV.

---

# 10. Le ciblage peut alors devenir composable

Au lieu de créer :

```text
LowestHpAlly
LowestHpLivingAlly
LowestHpDeadAlly
RandomLivingEnemy
RandomEnemyTeamMember
...
```

on peut combiner des briques :

```text
Allies
    ↓
Living
    ↓
LowestHp
```

ou :

```text
Enemies
    ↓
Living
    ↓
Random
```

ou :

```text
Allies
    ↓
Dead
    ↓
PlayerSelected
```

Cela évite de créer une classe pour chaque combinaison possible.

---

# 11. Le menu du prototype peut utiliser les mêmes règles

C'est une conséquence particulièrement intéressante de cette architecture.

Le `Program.cs` n'a pas besoin de connaître les détails de chaque capacité.

Lorsqu'une capacité est sélectionnée, il peut demander :

```csharp
var candidates = ability.Targeting.GetCandidates(context);
```

Le menu affiche alors les candidats.

Exemple :

```text
Soin

Cibles possibles :
1. Aldrick
2. Veya
```

Si Veya est morte et que la capacité ne peut cibler que les alliés vivants :

```text
Soin

Cibles possibles :
1. Aldrick
```

Le `Program.cs` n'a pas besoin d'écrire :

```csharp
if (target.IsAlive)
```

pour chaque capacité.

La règle de ciblage porte cette connaissance.

---

# 12. Les règles peuvent également servir à valider l'exécution

Le ciblage ne sert pas uniquement à construire les menus.

Il peut également permettre de vérifier qu'une cible est toujours valide au moment de l'exécution.

C'est important parce que l'état du combat peut changer entre :

```text
Sélection de la cible
        ↓
Exécution de l'action
```

Par exemple :

1. Le joueur choisit un allié vivant.
2. Une autre action agit avant cette capacité.
3. L'allié meurt.
4. La capacité arrive finalement à son tour.

La cible qui était valide au moment de la sélection peut ne plus l'être.

On peut donc réutiliser la règle :

```csharp
if (!targetRule.IsValidTarget(target, context))
{
    // Cible devenue invalide.
}
```

Cela évite de dupliquer les règles de validité dans plusieurs endroits du programme.

---

# 13. Exemple : `Revive`

Une capacité de réanimation illustre très bien le problème.

Elle devrait cibler :

```text
Alliés
+
Morts
```

et surtout pas :

```text
Alliés vivants
```

On pourrait donc avoir conceptuellement :

```csharp
new Revive
{
    TargetRule =
        new DeadRule(
            new AlliesRule()
        )
}
```

Le menu ne proposerait automatiquement que les alliés morts.

Et l'exécution pourrait vérifier à nouveau que la cible est toujours morte.

Ainsi, le système sait naturellement :

```text
Allié vivant → impossible
Allié mort   → possible
Ennemi mort  → impossible
Ennemi vivant → impossible
```

---

# 14. Exemple : `Amour Impossible`

L'ancienne version :

```csharp
new Synergy
{
    Id = "amour_impossible",
    Name = "Amour Impossible",
    TargetMode = TargetMode.SingleEnemy,

    RequiredCastersCount = 2,
    RequiredSynergyBars = 2,

    Effects = new List<IEffect>
    {
        new HealCasters
        {
            Amount = 80
        },

        new DealDamage
        {
            Power = 100
        }
    }
}
```

pourrait progressivement évoluer vers quelque chose comme :

```csharp
new Synergy
{
    Id = "amour_impossible",
    Name = "Amour Impossible",

    RequiredCastersCount = 2,
    RequiredSynergyBars = 2,

    Targeting = new AbilityTargeting
    {
        Rule = new LivingEnemiesRule(),
        Selector = new PlayerSelectedTargetSelector()
    },

    Effects = new List<IEffect>
    {
        new Heal
        {
            Amount = 80,
            TargetSelector = new CastersSelector()
        },

        new DealDamage
        {
            Power = 100,
            TargetSelector = new ContextTargetsSelector()
        }
    }
}
```

La capacité demande donc :

> Choisis un ennemi vivant.

Puis ses deux effets font des choses différentes :

```text
Heal
    → CastersSelector
    → soigne les lanceurs

DealDamage
    → ContextTargetsSelector
    → frappe la cible choisie
```

---

# 15. Cela permet également de fusionner `Heal` et `HealCasters`

L'ancien modèle avait :

```text
Heal
HealCasters
```

Alors qu'ils font fondamentalement la même chose :

> Récupérer des PV à des personnages.

La différence venait uniquement de la provenance des cibles.

Avec un sélecteur :

```csharp
new Heal
{
    Amount = 80,
    TargetSelector = new CastersSelector()
}
```

et :

```csharp
new Heal
{
    Amount = 60,
    TargetSelector = new ContextTargetsSelector()
}
```

un seul effet `Heal` suffit.

La même réflexion s'applique à `DealDamage`.

---

# 16. Attention : ne pas chercher à concevoir tout le système maintenant

Cette architecture est une hypothèse intéressante, mais elle ne doit pas devenir une spécification exhaustive du jeu.

Le prototype doit précisément servir à la maltraiter.

Quelques premières briques peuvent suffire :

```text
ITargetRule
ITargetSelector
```

Puis quelques implémentations :

```text
AlliesRule
EnemiesRule
LivingRule
DeadRule

PlayerSelectedTargetSelector
AllTargetsSelector
RandomTargetSelector
LowestHpSelector
```

Ensuite, créer volontairement des capacités variées et difficiles.

Par exemple :

```text
- Soigner un allié choisi par le joueur.
- Soigner l'allié vivant ayant le moins de PV.
- Réanimer un allié mort.
- Infliger des dégâts à tous les ennemis vivants.
- Infliger des dégâts à un ennemi choisi.
- Soigner tous les alliés sauf le lanceur.
- Frapper trois ennemis aléatoires.
- Soigner le lanceur et l'allié ayant le moins de PV.
- Réanimer l'allié mort ayant le plus d'attaque.
```

Le but est de découvrir où le modèle devient inconfortable.

---

# 17. Signal d'alerte important

Une mauvaise évolution serait de devoir recommencer à écrire des choses comme :

```csharp
if (selector is LowestHpSelector)
{
    ...
}
```

ou :

```csharp
if (rule is DeadRule)
{
    ...
}
```

dans de nombreux endroits.

Cela signifierait que le système extérieur commence à connaître les implémentations concrètes.

À l'inverse, une évolution intéressante serait que le reste du jeu puisse simplement faire :

```csharp
var candidates = targeting.GetCandidates(context);
```

puis :

```csharp
var targets = targeting.Select(context, candidates);
```

sans avoir besoin de savoir si le ciblage repose sur :

```text
AlliesRule
EnemiesRule
LowestHpSelector
RandomSelector
PlayerSelectedTargetSelector
...
```

---

# 18. Philosophie générale

L'objectif n'est pas :

> « Concevoir aujourd'hui le système de ciblage parfait pour les vingt prochaines années. »

L'objectif est plutôt :

> « Construire une abstraction suffisamment simple pour pouvoir expérimenter avec elle, puis la torturer jusqu'à découvrir ses limites. »

Le projet étant amené à évoluer pendant potentiellement plusieurs décennies, il est impossible de connaître maintenant toutes les mécaniques qui existeront.

Le prototype est donc un laboratoire.

On ajoute volontairement des cas variés et parfois absurdes pour voir :

```text
Est-ce que le modèle reste naturel ?
        ↓
Oui → continuer
        ↓
Non → comprendre pourquoi
        ↓
Nouvelle abstraction
        ↓
Nouvelle série de tests
```

---

# 19. État actuel de la réflexion

À ce stade, les idées suivantes semblent particulièrement prometteuses :

### Capacités

Une `IAbility` définit ce qu'est une capacité :

```text
Identité
Nom
Coût
Conditions d'utilisation
Lanceurs
Ciblage de la capacité
Effets
```

### Effets

Un `IEffect` représente une transformation du contexte ou de l'état du jeu :

```text
DealDamage
Heal
Revive
ApplyStatus
RemoveStatus
...
```

Les effets ne devraient pas nécessairement posséder une connaissance spécifique du type de capacité qui les utilise.

### Ciblage

Le ciblage pourrait être séparé en plusieurs responsabilités :

```text
ITargetRule
    → qui peut être ciblé ?

ITargetSelector
    → quelles cibles sélectionner parmi celles qui sont disponibles ?
```

### Contexte

`AbilityContext` reste le point de rencontre entre :

```text
Casters
Targets
Battle
Results
CustomData
```

Cela permet notamment à un effet de récupérer les cibles choisies par la capacité via un sélecteur comme :

```csharp
new ContextTargetsSelector()
```

---

# 20. Question encore ouverte

Il reste à déterminer exactement quelle forme prendra l'API finale.

Par exemple, plusieurs architectures restent possibles :

```csharp
ITargetRule
ITargetSelector
```

ou une abstraction plus unifiée :

```csharp
ITargeting
```

ou encore une composition différente dans laquelle les règles sont elles-mêmes des sélecteurs composables.

Il ne faut pas trancher prématurément.

Le meilleur moyen de choisir est de continuer à construire des cas de plus en plus variés dans le prototype.

**Si une abstraction devient pénible à utiliser, c'est une information précieuse.**

Le but du prototype est justement de faire apparaître cette information.
