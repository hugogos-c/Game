# Brainstorming technique — Architecture du projet

> **Statut : réflexion / brainstorming**
>
> Ce document rassemble les réflexions, décisions provisoires et pistes d'architecture discutées autour du projet. Rien n'est considéré comme définitif tant que les systèmes n'ont pas été suffisamment prototypés et testés.

---

# 1. Vision générale du projet

Le projet est un jeu vidéo personnel développé avant tout par passion.

L'objectif n'est pas nécessairement de commercialiser le jeu. La priorité est de construire un projet sérieux, propre et suffisamment ambitieux pour pouvoir être enrichi pendant plusieurs années.

Le projet doit pouvoir évoluer progressivement :

- système de combat ;
- exploration ;
- progression ;
- personnages ;
- ennemis ;
- équipements ;
- quêtes ;
- narration ;
- monde ;
- lore ;
- direction artistique ;
- audio ;
- etc.

Il ne s'agit donc pas de concevoir un petit jeu destiné à être terminé rapidement, mais un projet susceptible de devenir important en volume et en complexité.

Le contenu pourra évoluer considérablement au cours du développement.

---

# 2. Expérience recherchée

La vision actuelle peut être résumée ainsi :

> « Je veux créer un RPG où le joueur découvre un monde immense et mystérieux, comprend progressivement ses règles et construit une équipe dont les synergies rendent les combats extrêmement tactiques. »

Cette phrase constitue une intention de design et pourra évoluer.

Les principaux objectifs d'expérience sont :

- donner envie d'explorer ;
- donner envie de comprendre le monde ;
- laisser une part de mystère ;
- permettre au joueur de construire une équipe ;
- rendre les combats tactiques ;
- permettre de nombreuses synergies entre personnages ;
- donner suffisamment de profondeur aux systèmes pour que le joueur puisse expérimenter.

---

# 3. Genre et représentation

Le jeu est actuellement envisagé comme un **RPG**, potentiellement proche de l'Action-RPG sur certains aspects de présentation, mais avec un système de combat actuellement pensé comme du **tour par tour**.

La représentation envisagée est principalement en 2D.

Deux possibilités sont actuellement envisagées :

- vue de dessus, dans l'esprit de Pokémon ;
- vue de côté, dans l'esprit de Hollow Knight.

La décision n'est pas encore prise.

---

# 4. Direction artistique et univers

La direction artistique n'est volontairement pas encore définie.

Il n'est pas encore décidé si l'univers sera :

- médiéval ;
- fantasy ;
- sombre ;
- futuriste ;
- science-fiction ;
- ou un mélange de plusieurs influences.

Cette absence de décision est volontaire.

L'idée est d'éviter de concevoir trop tôt le système de jeu autour d'un univers particulier.

Exemple :

> « Ah merde, mon système de combat nécessite finalement un jeu futuriste alors que mon monde est médiéval. »

Le système de jeu doit donc, autant que possible, rester suffisamment abstrait pour ne pas dépendre inutilement de la direction artistique.

Cela ne signifie pas que le système devra être totalement indépendant du lore ou de l'univers définitif.

L'objectif est plutôt de ne pas prendre de décisions irréversibles alors que l'univers n'existe pas encore.

---

# 5. Inspirations

## RPG

Principales inspirations :

- Pokémon ;
- Final Fantasy.

## Combat

Le système actuellement envisagé est un système de combat en équipe et au tour par tour.

Une évolution éventuelle vers un système ATB est également envisagée, notamment sous l'influence de Final Fantasy VII Remake et Final Fantasy VII Rebirth.

## Narration et découverte du lore

Principales inspirations :

- Hollow Knight ;
- Dark Souls.

---

# 6. Narration

La narration doit privilégier une approche indirecte et cryptique.

Les informations relatives à l'histoire et au monde ne doivent pas nécessairement être expliquées explicitement au joueur.

Le joueur doit pouvoir :

- observer ;
- expérimenter ;
- trouver des indices ;
- écouter certains personnages ;
- découvrir des éléments du monde ;
- faire des rapprochements ;
- interpréter certains événements ;
- reconstruire progressivement le lore.

Le jeu doit éviter autant que possible les longues séquences d'exposition dont l'objectif principal est d'expliquer directement au joueur l'histoire et le fonctionnement du monde.

---

# 7. Système de combat

Le combat est actuellement envisagé comme un système :

- en équipe ;
- au tour par tour ;
- contrôlé directement par le joueur.

Le joueur contrôle plusieurs personnages.

La distinction importante est la suivante :

> Le joueur choisit les actions des personnages.

et non :

> Les personnages choisissent automatiquement leurs propres actions.

Exemple :

```text
Hugo
→ Coup d'épée

Alice
→ Boule de feu

Bob
→ Défense
```

Le joueur contrôle donc les décisions de chaque personnage.

---

# 8. Déroulement d'un tour

Vision actuelle :

1. Le tour commence.
2. Le joueur choisit une action pour chaque personnage.
3. Une fois les actions sélectionnées, elles sont validées.
4. Les actions sont exécutées.
5. L'ordre d'exécution dépend de la vitesse des personnages.
6. Les actions sont exécutées du personnage le plus rapide au personnage le plus lent.

Exemple :

```text
Hugo : 80 Vitesse → attaque
Alice : 60 Vitesse → soin
Bob : 30 Vitesse → défense

Ordre d'exécution :

1. Hugo
2. Alice
3. Bob
```

Ce système est provisoire.

---

# 9. Tour par tour ou ATB

Le tour par tour est actuellement le choix privilégié.

Cependant, il existe une incertitude concernant le système de combat définitif.

Il pourrait être intéressant, à terme, d'avoir :

- un système tour par tour classique ;
- un système ATB ;
- voire éventuellement plusieurs variantes.

L'inspiration vient notamment de l'évolution du système de combat de Final Fantasy VII Remake et Rebirth.

Il n'est cependant pas nécessaire de résoudre cette question immédiatement.

Une contrainte architecturale intéressante est que le système de capacités ne devrait pas dépendre inutilement du fait que le temps soit géré :

- par tours ;
- par une jauge ATB ;
- ou par un autre système.

---

# 10. Capacités

Une capacité représente une action utilisable pendant un combat.

Une capacité peut être très simple :

```text
Coup d'épée
→ inflige des dégâts.
```

Ou beaucoup plus complexe :

```text
Exécution
→ vérifie les PV de la cible
→ si elle possède moins de 10 % de ses PV :
    tue la cible
→ sinon :
    inflige des dégâts
```

Autre exemple :

```text
Drain
→ inflige des dégâts
→ soigne le lanceur en fonction des dégâts infligés
```

L'objectif est que les capacités simples et complexes soient traitées par le même système.

Il ne devrait pas être nécessaire de créer une classe spécifique pour chaque capacité :

```text
SwordStrike
Drain
Execution
Fireball
...
```

---

# 11. Pourquoi une architecture basée sur des Effects ?

Une première approche serait :

```csharp
class SwordStrike
{
    ...
}

class Drain
{
    ...
}

class Execution
{
    ...
}
```

Cette solution est très simple au départ, mais devient peu pratique avec plusieurs centaines de capacités.

Le projet pourrait potentiellement contenir :

- plusieurs centaines de capacités ;
- plus d'une centaine de personnages et ennemis ;
- plus d'une cinquantaine de statuts ;
- énormément de combinaisons et de variations.

Une capacité peut également être modifiée très fréquemment pendant le développement.

L'objectif est donc de ne pas coder chaque capacité individuellement.

---

# 12. Principe de composition

Une capacité devrait être composée de briques génériques.

Exemple :

```text
Coup d'épée
└── DealDamage

Drain
├── DealDamage
└── Heal

Exécution
└── ConditionalEffect
    ├── Condition : cible sous 10 % de PV
    ├── IfTrue  : KillTarget
    └── IfFalse : DealDamage

Boule de feu
├── DealDamage
└── ConditionalEffect
    ├── Condition : RandomChance(20 %)
    └── IfTrue    : ApplyStatus(Burning)
```

La capacité est donc principalement de la donnée décrivant une composition d'effets.

---

# 13. `Effect`

Un `Effect` représente une opération pouvant modifier l'état du jeu.

Exemple d'interface :

```csharp
public interface IEffect
{
    IEffectResult Execute(AbilityContext context);
}
```

Quelques effets possibles :

```text
DealDamage
Heal
ApplyStatus
RemoveStatus
KillTarget
ConditionalEffect
...
```

Un `Effect` n'est donc pas nécessairement une capacité.

C'est une brique de comportement utilisée par une capacité.

---

# 14. `EffectResult`

Un effet peut produire un résultat.

Exemple :

```csharp
public interface IEffectResult
{
}
```

Un résultat concret pourrait être :

```csharp
public sealed class DamageResult : IEffectResult
{
    public required Character Target { get; init; }

    public required int DamageDealt { get; init; }
}
```

Le contexte conserve les résultats produits par les effets.

Cela permet aux effets suivants de connaître ce qui s'est produit auparavant.

---

# 15. Pourquoi les résultats sont importants ?

Prenons `Drain`.

La capacité doit :

1. infliger des dégâts ;
2. récupérer le montant réellement infligé ;
3. soigner le lanceur de cette quantité.

On peut donc avoir :

```text
DealDamage
    ↓
DamageResult
    ↓
Heal
    ↓
lit DamageResult
```

Cela évite de créer un effet spécial tel que :

```text
HealCasterFromPreviousDamage
```

qui ne servirait pratiquement qu'à Drain.

---

# 16. `AbilityContext`

Le contexte contient les informations nécessaires à l'exécution d'une capacité.

Une première forme possible :

```csharp
public sealed class AbilityContext
{
    public required IReadOnlyList<Character> Casters { get; init; }

    public required IReadOnlyList<Character> Targets { get; init; }

    public required Battle Battle { get; init; }

    public List<IEffectResult> Results { get; } = [];
}
```

Le choix de `IReadOnlyList<Character> Casters` est important.

Il permet à une capacité d'avoir :

- un seul lanceur ;
- plusieurs lanceurs.

---

# 17. Plusieurs Casters

Une capacité normale peut avoir :

```csharp
Casters = [hugo]
```

Une capacité de synergie peut avoir :

```csharp
Casters = [hugo, alice]
```

Le système de capacité n'a donc pas besoin d'avoir deux modèles complètement différents pour :

```text
Ability normale
```

et :

```text
Synergy
```

Une synergie peut simplement être une capacité ayant plusieurs `Casters`.

---

# 18. Synergies entre personnages

Une mécanique importante envisagée est celle des **attaques coordonnées entre personnages d'une même équipe**.

Exemple :

Hugo utilise :

```text
Feu
```

qui inflige simplement des dégâts de feu.

Alice utilise :

```text
Tornade
```

qui inflige simplement des dégâts de vent.

Mais certaines combinaisons permettent une nouvelle capacité :

```text
Tornade de feu
```

Cette capacité pourrait :

- nécessiter Hugo ;
- nécessiter Alice ;
- remplacer leurs deux actions individuelles ;
- infliger de lourds dégâts de feu à toute l'équipe adverse.

---

# 19. Menu « Synergie »

La piste privilégiée est un menu distinct :

```text
Choisir un personnage
        ↓
Choisir une action
        ↓
Synergie
        ↓
Choisir une synergie
        ↓
Les personnages concernés ont leur action validée
```

Exemple :

```text
Hugo
├── Attaques
├── Techniques
├── Objets
└── Synergie
    └── Tornade de feu
        ├── Hugo
        └── Alice
```

Une fois `Tornade de feu` sélectionnée :

```text
Hugo  → action validée
Alice → action validée
```

Le système de combat peut ensuite considérer la synergie comme une seule capacité dont :

```csharp
Casters = [hugo, alice]
```

---

# 20. Les synergies doivent utiliser le même système

L'objectif est que :

```text
Coup d'épée
```

et :

```text
Tornade de feu
```

soient toutes deux des `Ability`.

La différence est simplement que :

```text
Coup d'épée
→ 1 Caster

Tornade de feu
→ 2 Casters
```

Le moteur d'exécution n'a donc pas besoin d'un système complètement séparé pour les synergies.

---

# 21. `Battle` et exécution des capacités

Une question soulevée est de savoir si un `AbilityExecutor` séparé est nécessaire.

La piste actuelle est de laisser le système de combat (`Battle`) orchestrer l'exécution.

Une version simplifiée pourrait ressembler à :

```csharp
public void ExecuteAbility(
    Ability ability,
    AbilityContext context)
{
    foreach (IEffect effect in ability.Effects)
    {
        IEffectResult result = effect.Execute(context);

        context.Results.Add(result);
    }
}
```

Cela signifie que :

```text
Battle
→ exécute une Ability
→ qui exécute ses Effects
→ qui produisent des EffectResults
```

Un `AbilityExecutor` pourrait éventuellement être introduit plus tard si le besoin apparaît, mais il n'est pas nécessaire de créer cette abstraction immédiatement.

---

# 22. Pourquoi `TakeDamage()` ?

Une méthode comme :

```csharp
target.TakeDamage(amount);
```

peut sembler naturelle.

Cependant, il faut faire attention à ne pas mettre toute la logique de dégâts dans `Character`.

Les dégâts peuvent dépendre de :

- la puissance de l'attaque ;
- la statistique d'attaque ;
- la défense ;
- l'élément ;
- les résistances ;
- les faiblesses ;
- les critiques ;
- les buffs ;
- les debuffs ;
- les boucliers ;
- les passifs ;
- les statuts ;
- etc.

Il peut donc être préférable que le système de combat calcule le résultat réel des dégâts.

Conceptuellement :

```csharp
public DamageResult DealDamage(
    Character source,
    Character target,
    int power,
    Element element)
{
    // Calcul des dégâts
    // Application des résistances
    // Critique
    // Boucliers
    // etc.

    ...
}
```

`Character` représente alors principalement l'état du personnage, tandis que `Battle` ou un système de combat gère les règles du combat.

---

# 23. `Condition`

Une `Condition` répond à une question.

Exemple :

```csharp
public interface ICondition
{
    bool Evaluate(AbilityContext context);
}
```

Exemples :

```text
TargetHealthBelow
TargetHasStatus
CasterIsAlive
TargetIsBoss
RandomChance
...
```

Une condition ne modifie pas directement le jeu.

Elle répond simplement :

```text
vrai
```

ou :

```text
faux
```

---

# 24. `ConditionalEffect`

Un `ConditionalEffect` permet d'exécuter un effet selon une condition.

Exemple :

```csharp
public sealed class ConditionalEffect : IEffect
{
    public required ICondition Condition { get; init; }

    public required IEffect IfTrue { get; init; }

    public IEffect? IfFalse { get; init; }

    public IEffectResult Execute(AbilityContext context)
    {
        if (Condition.Evaluate(context))
        {
            return IfTrue.Execute(context);
        }

        return IfFalse?.Execute(context)
            ?? EmptyEffectResult.Instance;
    }
}
```

Cela permet par exemple de représenter `Exécution`.

---

# 25. `Condition`, `Effect` et `Chance`

Ces trois concepts ne jouent pas le même rôle.

## Effect

Un `Effect` fait quelque chose.

Exemples :

```text
DealDamage
Heal
ApplyStatus
KillTarget
```

## Condition

Une `Condition` vérifie quelque chose.

Exemples :

```text
TargetHealthBelow(10 %)
TargetHasStatus(Burning)
CasterIsAlive
```

## Chance

Une chance détermine aléatoirement si une condition est vraie.

Elle peut donc être modélisée comme une `Condition` particulière :

```csharp
public sealed class RandomChance : ICondition
{
    public required double Probability { get; init; }

    public bool Evaluate(AbilityContext context)
    {
        return Random.Shared.NextDouble() < Probability;
    }
}
```

Ainsi :

```text
20 % de chance d'appliquer Brûlure
```

peut devenir :

```text
ConditionalEffect
├── Condition : RandomChance(20 %)
└── IfTrue    : ApplyStatus(Burning)
```

Cela évite nécessairement d'avoir un système séparé `Chance`.

---

# 26. `Value`

Une `Value` représente une valeur qui peut être déterminée au moment de l'exécution.

Une valeur peut être constante :

```csharp
new ConstantValue
{
    Amount = 100
}
```

Ou dynamique :

```text
PV actuels du lanceur
PV maximum de la cible
statistique du lanceur
dégâts précédemment infligés
etc.
```

Une forme possible :

```csharp
public abstract class Value
{
    public abstract int Resolve(AbilityContext context);
}
```

Par exemple :

```csharp
public sealed class ConstantValue : Value
{
    public required int Amount { get; init; }

    public override int Resolve(AbilityContext context)
    {
        return Amount;
    }
}
```

---

# 27. `PreviousDamageValue`

Pour `Drain`, une valeur dynamique pourrait récupérer les dégâts précédemment infligés.

Exemple :

```csharp
public sealed class PreviousDamageValue : Value
{
    public override int Resolve(AbilityContext context)
    {
        return context.Results
            .OfType<DamageResult>()
            .Last()
            .DamageDealt;
    }
}
```

Cela permettrait :

```text
Heal
└── Amount = PreviousDamageValue
```

La même logique `Heal` peut donc être utilisée pour :

```text
Soin
Drain
Régénération
Vol de vie
etc.
```

Il n'est pas nécessaire d'avoir un `HealCaster` spécifiquement créé pour Drain.

---

# 28. Exemple complet : Coup d'épée

```csharp
new Ability
{
    Id = "sword_strike",
    Name = "Coup d'épée",

    Effects =
    [
        new DealDamage
        {
            Power = 40,
            Element = Element.Physical
        }
    ]
};
```

---

# 29. Exemple complet : Drain

```csharp
new Ability
{
    Id = "drain",
    Name = "Drain",

    Effects =
    [
        new DealDamage
        {
            Power = 40,
            Element = Element.Dark
        },

        new Heal
        {
            Amount = new PreviousDamageValue()
        }
    ]
};
```

Ordre :

```text
1. DealDamage
       ↓
   DamageResult
       ↓
2. Heal
       ↓
   lit DamageResult
```

---

# 30. Exemple complet : Exécution

```csharp
new Ability
{
    Id = "execution",
    Name = "Exécution",

    Effects =
    [
        new ConditionalEffect
        {
            Condition = new TargetHealthBelow
            {
                Percentage = 0.10
            },

            IfTrue = new KillTarget(),

            IfFalse = new DealDamage
            {
                Power = 80,
                Element = Element.Physical
            }
        }
    ]
};
```

Structure :

```text
Exécution
└── ConditionalEffect
    ├── Condition
    │   └── TargetHealthBelow(10 %)
    │
    ├── IfTrue
    │   └── KillTarget
    │
    └── IfFalse
        └── DealDamage
```

---

# 31. Exemple complet : Boule de feu

```csharp
new Ability
{
    Id = "fireball",
    Name = "Boule de feu",

    Effects =
    [
        new DealDamage
        {
            Power = 60,
            Element = Element.Fire
        },

        new ConditionalEffect
        {
            Condition = new RandomChance
            {
                Probability = 0.20
            },

            IfTrue = new ApplyStatus
            {
                Status = StatusId.Burning,
                Duration = 3
            }
        }
    ]
};
```

Structure :

```text
Boule de feu
├── DealDamage
│
└── ConditionalEffect
    ├── RandomChance(20 %)
    └── ApplyStatus(Burning)
```

---

# 32. Ordre d'exécution

L'ordre des effets est important.

Par exemple :

```text
Drain
├── DealDamage
└── Heal
```

ne peut pas être équivalent à :

```text
Drain
├── Heal
└── DealDamage
```

Dans le premier cas :

```text
dégâts
→ résultat
→ soin basé sur les dégâts
```

Dans le deuxième :

```text
soin
→ aucun dégât précédent disponible
→ dégâts
```

La liste des `Effects` représente donc également l'ordre dans lequel ils sont exécutés.

---

# 33. Statuts

Les statuts pourraient utiliser une architecture similaire.

Exemples :

```text
Brûlure
→ inflige régulièrement des dégâts.

Poison
→ inflige des dégâts proportionnels aux PV maximum.

Régénération
→ soigne régulièrement.

Silence
→ empêche certaines capacités.
```

Un statut peut lui-même avoir des effets.

Exemple conceptuel :

```text
Brûlure
└── DealDamage
```

ou :

```text
Poison
└── DealDamage
    └── Power = une Value dépendant des PV max
```

La structure exacte des statuts reste à concevoir.

---

# 34. Une capacité n'est pas forcément une classe

Le but est de pouvoir écrire :

```csharp
new Ability
{
    Id = "drain",
    Name = "Drain",
    Effects =
    [
        ...
    ]
};
```

plutôt que :

```csharp
new Drain();
```

Cela permet de séparer :

```text
Ability
→ données décrivant la capacité

Effect
→ comportement générique

Condition
→ logique de vérification

Value
→ calcul d'une valeur dynamique

EffectResult
→ résultat produit par un effet
```

---

# 35. Mais certaines capacités seront forcément particulières

Il existe une limite importante.

Certaines futures capacités pourraient être tellement particulières qu'elles ne seront pas exprimables avec les primitives initiales.

Exemples imaginés :

- Résonance ;
- Dernier souffle ;
- Miroir ;
- Destin ;
- etc.

Il est possible qu'une capacité ne puisse pas être correctement représentée uniquement avec :

```text
DealDamage
Heal
ApplyStatus
Condition
Value
...
```

Dans ce cas, le système devra évoluer.

---

# 36. Ajouter de nouveaux Effects

Le principe est :

```text
Nouvelle capacité
       ↓
Peut-elle être représentée avec les primitives existantes ?
       │
    ┌──┴──┐
   Oui    Non
    │      │
    │      ↓
    │   Nouveau Effect
    │      │
    └──┬───┘
       ↓
   Nouvelle capacité
```

Exemple :

Une capacité nécessite une mécanique complètement nouvelle.

On peut créer :

```csharp
public sealed class ReflectDamage : IEffect
{
    ...
}
```

Puis l'utiliser dans une nouvelle capacité.

Les capacités existantes ne sont pas modifiées.

---

# 37. Pourquoi accepter des Effects très particuliers ?

Il existe une volonté de conserver une grande liberté créative.

Il est acceptable qu'un `Effect` soit utilisé par :

- 100 capacités ;
- 10 capacités ;
- 2 capacités ;
- voire une seule capacité.

Le but n'est pas forcément d'avoir une abstraction parfaite pour chaque mécanique.

Le but est de pouvoir dire :

> « J'ai une idée complètement farfelue. Je veux pouvoir l'implémenter sans devoir casser les systèmes précédents. »

---

# 38. Le risque du « langage de programmation »

En poussant très loin l'approche composée de données, on pourrait finir par créer quelque chose ressemblant à un langage de programmation interne.

Par exemple :

```text
Effect
Condition
Value
EffectResult
Sequence
Trigger
Expression
Event
...
```

Cela pourrait devenir suffisamment complexe pour ressembler à un véritable langage.

C'est une possibilité, mais ce n'est pas l'objectif.

Le terme de « langage » est surtout une image permettant d'illustrer le niveau de complexité potentiel du système.

---

# 39. Risque d'overengineering

Le danger serait de concevoir dès maintenant toutes les abstractions nécessaires pour les capacités hypothétiques des dix prochaines années.

Cela pourrait produire une architecture énorme avant même d'avoir un jeu fonctionnel.

Principe retenu :

> **Prototyper d'abord. Généraliser ensuite.**

Une abstraction doit idéalement apparaître lorsqu'un besoin concret la justifie.

---

# 40. Architecture générale du projet

L'organisation envisagée est :

```text
Game.sln
│
├── Game.Core
│      ↓
│   Toute la logique du jeu
│
├── Game.Prototypes
│      ↓
│   Prototypes / tests interactifs
│
├── Game.Tests
│      ↓
│   Tests automatisés
│
└── Game.Presentation
       ↓
    Présentation du jeu
```

Les noms ont volontairement été choisis pour ne pas dépendre d'un moteur spécifique.

---

# 41. `Game.Core`

`Game.Core` contient la logique du jeu.

Exemples :

```text
Battle
Ability
Character
Enemy
Status
Equipment
Quest
Progression
...
```

Il ne doit pas dépendre de Unity.

Il doit idéalement pouvoir fonctionner dans une application C# classique.

---

# 42. `Game.Prototypes`

`Game.Prototypes` est le laboratoire du projet.

Il sert à tester rapidement les systèmes sans avoir besoin de créer :

- sprites ;
- animations ;
- scènes ;
- UI ;
- assets ;
- effets visuels ;
- etc.

Exemple :

```text
> Combat

Hugo : 500/500 HP
Alice : 350/350 HP
Gobelin : 300/300 HP

Choisir une action :

1. Coup d'épée
2. Drain
3. Exécution
4. Synergie

> 2

Drain !

Gobelin subit 40 dégâts.
Hugo récupère 40 PV.
```

---

# 43. `Game.Tests`

`Game.Tests` contient les tests automatisés.

Exemples :

```text
Drain inflige des dégâts puis soigne correctement.

Exécution tue une cible sous 10 % de PV.

Exécution inflige des dégâts à une cible au-dessus de 10 %.

Boule de feu peut appliquer Brûlure.

Une synergie peut avoir plusieurs Casters.

Les Effects sont exécutés dans le bon ordre.

Les dégâts prennent correctement en compte les résistances.
```

Les tests deviennent particulièrement importants si le système finit par contenir plusieurs centaines de capacités.

---

# 44. `Game.Presentation`

`Game.Presentation` représente la couche de présentation du jeu.

À terme, elle pourra utiliser Unity.

Elle sera responsable notamment de :

- graphismes ;
- animations ;
- sons ;
- interface ;
- contrôles ;
- scènes ;
- caméra ;
- affichage des personnages ;
- représentation des combats ;
- etc.

Elle utilisera `Game.Core` pour appliquer les règles du jeu.

---

# 45. Séparation du moteur et de la logique

L'objectif architectural est :

```text
Game.Core
     ↑
     │
Game.Presentation
     ↑
     │
   Unity
```

et non :

```text
Unity
 ├── Battle
 ├── Ability
 ├── Character
 ├── Damage
 └── toute la logique du jeu
```

La logique du jeu doit donc pouvoir exister indépendamment de son rendu.

---

# 46. Pourquoi cette séparation ?

Elle permet notamment :

- de tester le système de combat sans Unity ;
- de créer rapidement des prototypes ;
- de faire des tests automatisés ;
- de modifier les règles sans toucher aux assets ;
- de changer potentiellement de moteur ;
- de développer la logique pendant que la présentation n'existe pas encore ;
- de réduire le couplage avec le moteur.

---

# 47. Unity n'est pas le jeu

L'idée générale est que :

```text
Game.Core
```

représente les règles et l'état du jeu.

Alors que :

```text
Game.Presentation
```

représente la manière dont le joueur voit et manipule ces règles.

Par exemple :

```text
Game.Core
→ Hugo possède 350 PV.
```

Unity :

```text
→ affiche Hugo avec sa barre de PV à 350/500.
```

Le changement de PV est décidé par `Game.Core`.

Unity ne fait qu'en présenter le résultat.

---

# 48. Prototypage C# avant Unity

Le projet étant actuellement au stade de réflexion et de prototypes, il n'est pas nécessaire de commencer immédiatement par Unity.

Le système de combat peut être développé en C# pur :

```text
Game.Core
+
Game.Prototypes
+
Game.Tests
```

Puis Unity pourra être intégré lorsque les systèmes auront suffisamment mûri.

---

# 49. Capacités définies en C# pendant le prototypage

Pendant la phase de conception, les capacités peuvent être créées directement dans le code C#.

Exemple :

```csharp
new Ability
{
    Id = "drain",
    Name = "Drain",

    Effects =
    [
        new DealDamage
        {
            Power = 40,
            Element = Element.Dark
        },

        new Heal
        {
            Amount = new PreviousDamageValue()
        }
    ]
};
```

Cela permet de modifier et tester très rapidement le système.

---

# 50. Passage éventuel au JSON

Une fois le modèle suffisamment stable, les capacités pourraient être représentées sous forme de données externes.

Exemple :

```json
{
  "id": "drain",
  "name": "Drain",
  "effects": [
    {
      "type": "deal_damage",
      "power": 40,
      "element": "dark"
    },
    {
      "type": "heal",
      "amount": {
        "type": "previous_damage"
      }
    }
  ]
}
```

Le principe serait :

```text
C# pendant le prototypage
        ↓
modèle stabilisé
        ↓
définition des données
        ↓
JSON
        ↓
chargement
        ↓
Game.Core
```

---

# 51. C# et JSON

Une distinction importante :

```text
C#
→ définit le fonctionnement du système.

JSON
→ décrit le contenu du jeu.
```

Le JSON ne contient pas directement du code C#.

Par exemple :

```json
{
  "type": "deal_damage",
  "power": 40,
  "element": "dark"
}
```

peut être interprété par le système comme :

> utiliser l'Effect `DealDamage` avec une puissance de 40 et l'élément Dark.

---

# 52. Base de données éventuelle

À terme, les données pourraient être stockées dans :

- fichiers JSON ;
- base de données ;
- outil d'édition dédié ;
- autre format de données.

Cependant, une base de données n'est pas nécessaire au début.

Des fichiers JSON versionnés dans le projet seront probablement plus simples pendant la phase de développement.

Le choix définitif pourra être effectué beaucoup plus tard.

---

# 53. Pourquoi ne pas figer le JSON immédiatement ?

Le modèle des capacités va probablement beaucoup évoluer.

Il serait donc inutile de définir dès maintenant :

```text
le JSON définitif
```

alors que les concepts :

- `Effect` ;
- `Condition` ;
- `Value` ;
- `EffectResult` ;
- statuts ;
- synergies ;
- ciblage ;
- etc.

ne sont pas encore stabilisés.

Il est préférable de commencer avec C#, puis de transformer progressivement les modèles stables en données externes.

---

# 54. Exemple d'architecture conceptuelle

```text
Ability
│
├── Id
├── Name
├── Effects
│
└── ...
       │
       ▼
   IEffect
       │
       ├── DealDamage
       ├── Heal
       ├── ApplyStatus
       ├── KillTarget
       └── ConditionalEffect
                │
                ├── ICondition
                │     ├── TargetHealthBelow
                │     ├── TargetHasStatus
                │     └── RandomChance
                │
                ├── IfTrue
                └── IfFalse
```

Et pendant l'exécution :

```text
Ability
   ↓
AbilityContext
   ↓
Effect 1
   ↓
EffectResult
   ↓
Effect 2
   ↓
EffectResult
   ↓
...
   ↓
État final du combat
```

---

# 55. Vision des responsabilités

Une séparation possible est :

```text
Ability
→ décrit ce que la capacité veut faire.

Effect
→ réalise une opération.

Condition
→ détermine si quelque chose est vrai.

Value
→ détermine une valeur au moment de l'exécution.

EffectResult
→ décrit ce qu'un Effect a produit.

AbilityContext
→ fournit le contexte nécessaire à l'exécution.

Battle
→ orchestre le combat et applique les règles du système.
```

Cette séparation reste une proposition d'architecture et pourra évoluer avec les prototypes.

---

# 56. Exemple de synergie avec le modèle actuel

Une synergie pourrait être définie comme une capacité :

```csharp
new Ability
{
    Id = "fire_tornado",
    Name = "Tornade de feu",

    Effects =
    [
        new DealDamage
        {
            Power = 150,
            Element = Element.Fire,
            TargetMode = TargetMode.AllEnemies
        }
    ]
};
```

Lorsqu'elle est utilisée :

```csharp
var context = new AbilityContext
{
    Casters = [hugo, alice],
    Targets = enemies,
    Battle = battle
};
```

Le moteur ne se préoccupe pas fondamentalement du fait que deux personnages soient nécessaires.

Il exécute simplement la capacité avec deux `Casters`.

---

# 57. Exemple de sélection d'une synergie

Conceptuellement :

```text
Tour de combat

Hugo
↓
Choisir une action
↓
Synergie
↓
Tornade de feu
↓
Vérifier que Alice est disponible
↓
Valider l'action de Hugo
↓
Valider l'action d'Alice
↓
Tour résolu
```

Les règles permettant de déterminer si une synergie est disponible devront être définies plus tard.

---

# 58. Capacités et liberté créative

Le système ne doit pas chercher à empêcher la création de capacités atypiques.

Il doit plutôt fournir un ensemble de primitives réutilisables suffisamment puissant pour couvrir :

- les capacités simples ;
- les capacités conditionnelles ;
- les capacités probabilistes ;
- les capacités dépendant de résultats précédents ;
- les capacités utilisant plusieurs personnages ;
- les capacités liées aux statuts ;
- etc.

Et lorsque cela ne suffit plus :

> ajouter une nouvelle primitive.

---

# 59. Exemple de progression du système

Au début :

```text
DealDamage
Heal
ApplyStatus
```

Puis éventuellement :

```text
ConditionalEffect
RandomChance
KillTarget
```

Puis :

```text
Value
PreviousDamageValue
TargetHealthValue
```

Puis éventuellement d'autres primitives selon les besoins réels.

Le système doit donc évoluer **à partir des capacités rencontrées**, plutôt que de tenter de prévoir toutes les capacités imaginables.

---

# 60. Philosophie générale

Le projet est potentiellement destiné à être développé sur une très longue période.

L'objectif n'est donc pas :

> construire immédiatement une architecture capable de tout faire.

L'objectif est :

> construire une architecture suffisamment solide pour évoluer pendant longtemps sans devenir inutilement rigide.

Principes :

1. Prototyper rapidement.
2. Tester les concepts.
3. Identifier les problèmes réels.
4. Généraliser lorsqu'un motif se répète.
5. Ajouter une nouvelle primitive lorsqu'une mécanique le justifie.
6. Éviter de prévoir toutes les possibilités futures.
7. Garder la logique indépendante du moteur.
8. Garder les capacités composables.
9. Garder la possibilité d'avoir plusieurs Casters.
10. Garder la liberté de faire des capacités extrêmement particulières.

---

# 61. Questions encore ouvertes

Les éléments suivants ne sont volontairement pas définis :

## Jeu

- nom du jeu ;
- univers ;
- histoire ;
- personnages ;
- factions ;
- monde ;
- zones ;
- lore ;
- direction artistique ;
- époque ;
- technologie ;
- ton général.

## Gameplay

- exploration ;
- progression ;
- quêtes ;
- équipements ;
- système de statistiques ;
- système de ciblage ;
- gestion des ressources ;
- fonctionnement détaillé des synergies.

## Combat

- tour par tour définitif ou ATB ;
- ordre exact de résolution ;
- fonctionnement des tours ;
- interruptions ;
- réactions ;
- contre-attaques ;
- passifs ;
- statuts ;
- buffs/debuffs ;
- interactions élémentaires ;
- fonctionnement des critiques ;
- etc.

## Architecture

- forme définitive de `AbilityContext` ;
- forme définitive de `EffectResult` ;
- système définitif de `Value` ;
- système définitif de `Condition` ;
- fonctionnement exact des statuts ;
- système de déclencheurs/événements éventuel ;
- format JSON ;
- éventuelle base de données ;
- éventuel éditeur de contenu ;
- organisation définitive des assemblies.

---

# 62. Principes directeurs à conserver

### Ne pas figer trop tôt l'univers

La direction artistique, le monde, le lore et les personnages peuvent être développés progressivement.

### Découpler la logique du moteur

Le gameplay doit pouvoir être exécuté sans Unity.

### Traiter les capacités de manière uniforme

Une capacité simple et une capacité complexe doivent utiliser le même modèle autant que possible.

### Permettre plusieurs Casters

Cela permet notamment les synergies.

### Respecter l'ordre des Effects

L'ordre est significatif et permet à un effet de dépendre des résultats produits précédemment.

### Séparer Effects et Conditions

Un Effect fait quelque chose.

Une Condition vérifie quelque chose.

### Permettre des valeurs dynamiques

Une `Value` peut dépendre du contexte ou des résultats précédents.

### Accepter les cas particuliers

Une nouvelle mécanique peut justifier un nouvel `Effect`, une nouvelle `Condition`, une nouvelle `Value` ou un nouveau `EffectResult`.

### Prototyper avant de généraliser

> **Prototyper d'abord. Généraliser ensuite.**

---

# 63. Architecture cible provisoire

```text
                              Game.sln
                                  │
                ┌─────────────────┼─────────────────┐
                │                 │                 │
                ▼                 ▼                 ▼
           Game.Core       Game.Prototypes      Game.Tests
                │                 │
                │                 │
                │                 └── Prototypes
                │
                ├── Battle
                ├── Ability
                ├── Effect
                ├── Condition
                ├── Value
                ├── EffectResult
                ├── Character
                ├── Enemy
                ├── Status
                ├── Equipment
                ├── Quest
                ├── Progression
                └── ...
                │
                ▼
        Game.Presentation
                │
                ▼
              Unity
```

---

# 64. Résumé très court

Le projet vise à devenir un RPG ambitieux, développé avant tout par passion, avec un monde mystérieux, une forte composante d'exploration et un système de combat tactique en équipe.

L'architecture envisagée repose sur une séparation claire entre :

```text
Game.Core
→ logique du jeu

Game.Prototypes
→ prototypage

Game.Tests
→ tests automatisés

Game.Presentation
→ présentation Unity
```

Le système de capacités doit être fortement composable :

```text
Ability
→ Effects
→ Conditions
→ Values
→ EffectResults
```

Une capacité peut avoir plusieurs `Casters`, ce qui permet de représenter naturellement les synergies entre personnages.

Exemples :

```text
Coup d'épée
→ DealDamage

Drain
→ DealDamage
→ Heal(PreviousDamageValue)

Exécution
→ ConditionalEffect
   → TargetHealthBelow(10 %)
   → KillTarget
   → DealDamage

Boule de feu
→ DealDamage
→ ConditionalEffect
   → RandomChance(20 %)
   → ApplyStatus(Burning)

Tornade de feu
→ plusieurs Casters
→ DealDamage
```

Les capacités seront d'abord construites en C# dans `Game.Prototypes`, puis pourront éventuellement être transformées en données JSON lorsque le modèle sera suffisamment stable.

La philosophie générale est :

> **Ne pas essayer de prévoir tout le jeu aujourd'hui. Construire une base suffisamment solide pour que les futures idées puissent y trouver leur place.**