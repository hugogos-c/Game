# Réflexion sur `IValue` et le calcul des montants

> Document de réflexion / architecture du prototype  
> Statut : **décision de conception retenue pour le prototype, à éprouver par les cas d'usage.**

---

## 1. Contexte

Le système de combat cherche progressivement à devenir **déclaratif et data-driven**.

L'objectif à terme est de pouvoir définir beaucoup de capacités sans devoir créer une nouvelle implémentation C# pour chaque capacité.

Le modèle actuel repose notamment sur des briques comme :

```text
IAbility
    ↓
Sequences
    ↓
IEffect
    ↓
ITargetSelector
    ↓
Conditions / Values / ...
```

Cette direction doit permettre de composer des capacités à partir d'un vocabulaire générique.

Le problème rencontré avec `IValue` vient précisément de là :

> Si une capacité peut utiliser n'importe quelle valeur calculée à partir du contexte, qu'est-ce qu'une « valeur » dans le système ?

---

# 2. Un point important : une capacité peut avoir plusieurs casters

Dans le système actuel, `IAbility` n'est pas conçue autour d'un seul personnage lanceur.

La signature est :

```csharp
bool CanUse(IReadOnlyList<Character> casters, Battle battle);
void PayCost(IReadOnlyList<Character> casters, Battle battle);
void Execute(AbilityContext context);
```

Et le contexte contient :

```csharp
public required IReadOnlyList<Character> Casters { get; init; }
```

Une capacité possède donc potentiellement **plusieurs casters**.

Les synergies rendent ce besoin évident :

```text
Caster 1
Caster 2
Caster 3
...
        ↓
    IAbility
```

Une `Synergy` peut par exemple nécessiter plusieurs personnages possédant cette même capacité.

Cependant, ce point ne concerne pas uniquement `Synergy`.

Le contrat de `IAbility` utilise déjà :

```csharp
IReadOnlyList<Character> casters
```

Cela signifie que le système doit considérer les casters comme une collection au niveau de l'architecture générale.

`Synergy` exploite simplement particulièrement fortement cette possibilité.

---

# 3. Le problème initial : `Power` dans `DamageEffect`

Une première version pouvait ressembler à :

```csharp
new DamageEffect
{
    Power = 100
}
```

Le nom `Power` laisse cependant entendre qu'il s'agit d'une **puissance** qui doit ensuite être transformée en dégâts par une formule interne.

Cela devient problématique dès que les capacités peuvent avoir des formules différentes.

Par exemple :

```text
Dégâts = Attaque du lanceur × 150 %
Dégâts = Défense du lanceur × 100 %
Dégâts = PV max du lanceur × 20 %
Dégâts = PM max de la cible × 30 %
Dégâts = Attaque ennemie × 200 %
Dégâts = 50
Dégâts = 50 + Attaque du lanceur × 100 %
...
```

Et avec plusieurs casters :

```text
Dégâts = Somme des attaques des casters × 150 %
Dégâts = Moyenne des attaques des casters
Dégâts = Attaque du caster ayant la plus grande attaque
Dégâts = ...
```

Si `DamageEffect` doit connaître toutes ces possibilités, il devient progressivement responsable du calcul des dégâts.

Cela pousse vers des solutions du genre :

```text
DamageEffect
DamageEffectCalculated
DamageEffectWithFormula
IDamageCalculator
IDamageFormula
...
```

ou vers un `DamageEffect` rempli de logique conditionnelle.

Ce n'est pas souhaitable.

---

# 4. Le besoin réel : un effet de dégâts ne calcule pas nécessairement les dégâts

La responsabilité fondamentale de `DamageEffect` est beaucoup plus simple :

> **Appliquer un montant de dégâts à ses cibles.**

Le modèle devient donc :

```csharp
new DamageEffect
{
    Amount = ...
}
```

avec :

```text
Amount
    ↓
Resolve(context)
    ↓
int
    ↓
Application des dégâts
```

Autrement dit :

```text
DamageEffect
    ├── Amount : IValue
    │      └── Resolve(context) → int
    │
    └── applique cet int à la cible
```

C'est une distinction importante :

- `DamageEffect` définit **ce qui arrive** : des dégâts sont appliqués.
- `IValue` définit **combien** : il produit le montant final.

---

# 5. Définition retenue de `IValue`

La définition qui semble la plus cohérente avec les besoins actuels est :

> **Une `IValue` est une expression capable de résoudre un contexte en une valeur brute.**

Dans le prototype, cette valeur brute est actuellement un `int`.

Conceptuellement :

```csharp
public interface IValue
{
    int Resolve(EffectContext context);
}
```

Le choix du nom `Resolve` est volontaire.

Une `IValue` peut effectivement effectuer des calculs en interne, mais sa responsabilité externe reste :

```text
Contexte
   ↓
IValue.Resolve(...)
   ↓
int
```

Le mot « valeur » ne signifie donc pas nécessairement « constante ».

Une valeur peut être :

```text
50
```

mais aussi être obtenue à partir :

```text
d'une statistique
du contexte
du nombre de casters
des PV actuels
des PV maximum
des PM
du nombre de stacks d'un statut
d'une opération
d'un pourcentage
d'une formule
...
```

La propriété essentielle est qu'à la fin :

```text
IValue → int
```

---

# 6. Une constante est une `IValue`

Le cas le plus simple est une constante.

```csharp
new ConstantValue(50)
```

Elle signifie :

```text
quel que soit le contexte
        ↓
      50
```

Donc :

```text
ConstantValue(50)
    ↓ Resolve(context)
      50
```

Cela permet de conserver un système uniforme.

Un `DamageEffect` peut recevoir :

```csharp
Amount = new ConstantValue(50)
```

sans avoir un cas particulier pour les constantes.

---

# 7. Une statistique peut également être une `IValue`

Par exemple :

```text
Attack(casters)
```

peut être une `IValue`.

Mais comme une capacité peut avoir plusieurs casters, il faut distinguer deux questions :

### 1. Quelles entités consulter ?

Ici :

```text
Casters
```

### 2. Comment transformer leurs valeurs en une seule valeur ?

Par exemple :

```text
Somme
Moyenne
Minimum
Maximum
Caster n°
Caster possédant la statistique maximale
...
```

On pourrait donc conceptuellement avoir :

```text
Attack(casters)
    ↓
récupération de l'attaque de chaque caster
    ↓
agrégation
    ↓
int
```

Par exemple :

```text
Caster A → Attack = 50
Caster B → Attack = 80

Somme
    ↓
130
```

ou :

```text
Caster A → Attack = 50
Caster B → Attack = 80

Maximum
    ↓
80
```

L'architecture devra déterminer comment représenter cette sélection et cette agrégation.

**Mais cette décision ne doit pas être prise uniquement sur la base d'un besoin hypothétique.**

Il faudra d'abord rencontrer des capacités réelles qui nécessitent ces comportements.

---

# 8. Les opérations utilisent elles-mêmes des `IValue`

C'est là que `IValue` devient réellement intéressant.

Une opération peut prendre d'autres `IValue` en entrée.

Par exemple :

```text
AddValue
    Left  : IValue
    Right : IValue
```

et :

```text
AddValue.Resolve(context)
    ↓
Left.Resolve(context) + Right.Resolve(context)
    ↓
int
```

On peut alors construire :

```text
50 + Attack(casters)
```

comme :

```text
AddValue
├── Left
│   └── ConstantValue(50)
│
└── Right
    └── AttackValue(casters)
```

Ou :

```text
Attack(casters) × 150 %
```

comme une composition de valeurs.

Cela signifie qu'une `IValue` peut contenir d'autres `IValue`.

On commence donc à avoir une **expression** plutôt qu'une simple donnée.

---

# 9. `IValue` est donc une petite grammaire d'expressions

C'est probablement la compréhension la plus importante à conserver.

Au départ :

```text
IValue = une valeur
```

Mais en pratique :

```text
IValue = une expression qui produit une valeur
```

Par exemple :

```text
             Multiply
             /      \
         Attack      150%
```

ou :

```text
                Add
               /   \
         MaxHp      50
```

ou encore :

```text
                Subtract
                /       \
              Add      Defense
             /   \
         Attack   20
```

Cette structure pourra éventuellement être sérialisée dans un format de données pour permettre à un éditeur de construire les expressions sans écrire de C#.

---

# 10. Pourquoi les formules ne doivent pas être dans `DamageEffect`

Supposons une capacité :

```text
Coup puissant
→ dégâts = Attaque des casters × 150 %
```

On pourrait être tenté d'écrire :

```csharp
new DamageEffect
{
    Power = 150,
    Formula = ...
}
```

Mais cela donne à `DamageEffect` une responsabilité supplémentaire :

```text
appliquer les dégâts
+
comprendre les formules de dégâts
```

Alors qu'il peut simplement recevoir :

```csharp
new DamageEffect
{
    Amount = new PhysicalDamageFormula(...)
}
```

et ne connaître que le contrat :

```text
Amount.Resolve(context)
```

Le détail du calcul reste dans la `IValue`.

---

# 11. Une formule peut elle-même être une `IValue`

Une formule n'a donc pas besoin d'une hiérarchie parallèle obligatoire.

Conceptuellement :

```text
PhysicalDamageFormula : IValue
```

pourrait prendre plusieurs `IValue` en entrée :

```text
PhysicalDamageFormula
├── Power     : IValue
└── Defense   : IValue
```

Elle peut alors résoudre :

```text
Power.Resolve(context)
Defense.Resolve(context)
        ↓
calcul de la formule
        ↓
int
```

Par exemple :

```text
Power
    = Attack(casters) × 150 %

Defense
    = Defense(target)
```

La formule produit ensuite le montant final.

L'intérêt est que les sources elles-mêmes restent composables.

---

# 12. Le problème fondamental : plusieurs casters

Le fait que `AbilityContext` contienne :

```csharp
IReadOnlyList<Character> Casters
```

est particulièrement important pour `IValue`.

Une valeur peut dépendre d'un seul personnage, mais elle peut également dépendre de **l'ensemble des casters**.

Par exemple :

```text
Somme des attaques des casters
```

donne :

```text
Caster A : 50
Caster B : 70
Caster C : 80

50 + 70 + 80
    ↓
200
```

Une autre capacité pourrait vouloir :

```text
Attaque moyenne des casters
```

ou :

```text
Attaque maximale des casters
```

ou :

```text
Défense du caster principal
```

ou :

```text
Nombre de casters
```

Cela montre pourquoi une `IValue` ne doit pas être pensée comme un simple :

```csharp
GetValue(Character character)
```

mais comme une résolution à partir d'un **contexte d'exécution**.

Le contexte possède déjà cette information :

```csharp
public required IReadOnlyList<Character> Casters { get; init; }
```

`IValue` pourra donc exploiter cette collection lorsque cela sera nécessaire.

---

# 13. Attention : ne pas inventer immédiatement toute la mécanique d'agrégation

Le besoin des plusieurs casters est réel.

En revanche, cela ne signifie pas qu'il faut immédiatement créer :

```text
ICharacterSelector
IAggregator
ISource
IStatProvider
IValueResolver
...
```

avec vingt implémentations.

Le bon ordre de réflexion reste :

```text
Capacité réelle
    ↓
besoin réel
    ↓
abstraction nécessaire
```

et non :

```text
abstraction potentielle
    ↓
chercher un problème à lui donner
```

Le prototype doit permettre de découvrir progressivement les besoins.

---

# 14. Le cas des soins

Le raisonnement ne concerne évidemment pas uniquement les dégâts.

Un soin peut avoir :

```text
Soin = 50
```

ou :

```text
Soin = Attaque des casters × 100 %
```

ou :

```text
Soin = PV max d'un caster × 20 %
```

ou encore :

```text
Soin = moyenne des PV max des casters × 20 %
```

Le système peut donc avoir :

```csharp
new HealEffect
{
    Amount = ...
}
```

avec exactement la même philosophie :

```text
HealEffect
    ↓
Amount.Resolve(context)
    ↓
int
    ↓
applique le soin
```

L'effet ne sait pas comment le montant a été obtenu.

---

# 15. Le même principe peut servir à d'autres effets

La logique peut s'étendre à de nombreux effets :

```text
DamageEffect
    Amount : IValue

HealEffect
    Amount : IValue

RestoreMpEffect
    Amount : IValue

ShieldEffect
    Amount : IValue

ApplyStatusEffect
    Stacks : IValue

RemoveStatusEffect
    Count : IValue
```

Le vocabulaire exact reste à définir avec les besoins du jeu.

L'idée fondamentale reste :

> Quand un effet a besoin d'un nombre qui peut varier selon le contexte, il peut recevoir une `IValue` plutôt qu'un entier figé.

---

# 16. Les conditions restent une autre responsabilité

Il est important de ne pas mélanger `IValue` avec les conditions.

Une capacité comme `Exécution` peut avoir :

```text
ConditionalEffect
├── Condition
│   └── TargetHpBelowCondition(10%)
│
├── Then
│   └── KillEffect
│
└── Else
    └── DamageEffect
        └── Amount = ConstantValue(50)
```

La condition décide :

```text
quelle branche exécuter
```

L'`IValue` décide :

```text
quelle valeur produire pour un effet qui en a besoin
```

Ce sont deux problèmes différents.

Il ne faut donc pas transformer :

```text
ConditionalEffect
```

en une sorte de « valeur conditionnelle » simplement parce que `IValue` existe.

Le cas d'`Exécution` illustre justement pourquoi les deux abstractions doivent rester séparées.

---

# 17. Les effets restent atomiques

Cette architecture pousse vers une distinction saine :

```text
Condition
    → décide si / quelle branche

Selector
    → décide sur quelles entités agir

Value
    → produit un nombre

Effect
    → applique une transformation

Ability
    → compose ces éléments
```

Par exemple :

```text
Ability
│
├── Targeting
│
└── Sequence
    │
    └── ConditionalEffect
        │
        ├── Condition
        ├── Then
        │   └── KillEffect
        │
        └── Else
            └── DamageEffect
                └── Amount
                    └── IValue
```

Chaque élément possède une responsabilité identifiable.

---

# 18. Pourquoi cette architecture répond au besoin data-driven

Le besoin de fond n'est pas simplement :

> « Je veux écrire moins de code. »

Le besoin est plutôt :

> **Je veux pouvoir créer de nombreuses capacités en assemblant des briques déjà connues du moteur, sans devoir ajouter une nouvelle implémentation C# à chaque capacité.**

Avec cette philosophie :

```text
Code
    ↓
définit le vocabulaire du système
```

et :

```text
Données
    ↓
composent ce vocabulaire pour créer des capacités
```

Une capacité pourrait donc être constituée de :

```text
Targeting
+
Sequences
+
Effects
+
Conditions
+
Values
+
Selectors
```

L'éditeur pourrait ensuite manipuler ces structures.

---

# 19. Exemple concret : une capacité simple

Une capacité :

```text
Coup d'épée
→ inflige 50 dégâts
```

peut conceptuellement devenir :

```text
DamageEffect
└── Amount
    └── ConstantValue(50)
```

Aucune formule spécialisée n'est nécessaire.

---

# 20. Exemple : dégâts basés sur une statistique

Une capacité :

```text
Frappe lourde
→ dégâts = Attaque des casters × 150 %
```

peut devenir :

```text
DamageEffect
└── Amount
    └── Multiply
        ├── Attack(casters)
        └── 150 %
```

Le `DamageEffect` ne sait toujours rien de la formule.

---

# 21. Exemple : formule plus complexe

Une capacité pourrait avoir :

```text
Dégâts =
    Attaque des casters
    + 20
    - Défense de la cible
```

Conceptuellement :

```text
DamageEffect
└── Amount
    └── Subtract
        ├── Add
        │   ├── Attack(casters)
        │   └── Constant(20)
        │
        └── Defense(target)
```

On obtient ainsi un véritable arbre d'expression.

C'est précisément ce qui rend le système potentiellement sérialisable.

---

# 22. Le cas des synergies

Les synergies sont un cas particulièrement révélateur.

Une `Synergy` peut exiger :

```text
plusieurs casters
+
une jauge de synergie suffisante
```

Par exemple :

```text
Amour Impossible

Casters :
    Aldrick
    Veya
```

Une fois les casters réunis, les effets peuvent utiliser ces mêmes casters de différentes manières.

Par exemple :

```text
HealEffect
    TargetSelector = CastersSelector
    Amount = 80
```

et :

```text
DamageEffect
    TargetSelector = ContextTargetsSelector
    Amount = ...
```

Le fait que les deux effets utilisent le même `AbilityContext` permet à chacun de consulter :

```text
Casters
Targets
Battle
Results
CustomData
```

sans que l'effet ait besoin de connaître le type concret de l'ability qui l'a déclenché.

---

# 23. `IValue` et les casters ne doivent pas être confondus avec le ciblage

Il faut également conserver une distinction entre :

```text
Casters
```

et :

```text
Targets
```

Les casters sont les personnages qui participent à l'exécution de la capacité.

Les targets sont les personnages sur lesquels un effet peut agir.

Ils peuvent être complètement différents.

Exemple :

```text
Synergy
│
├── Casters
│   ├── Aldrick
│   └── Veya
│
└── Target
    └── Ennemi
```

Un effet peut donc demander :

```text
Amount
    → dépend des casters
```

tout en agissant :

```text
TargetSelector
    → sur l'ennemi
```

C'est justement l'une des raisons pour lesquelles le contexte est central.

---

# 24. Pourquoi `Resolve` et pas `Calculate` ?

Le terme peut sembler étrange au début parce qu'une `IValue` peut effectuer de vrais calculs.

Mais :

```text
Calculate()
```

mettrait l'accent sur le mécanisme interne.

Alors que :

```text
Resolve()
```

met l'accent sur le contrat :

> « À partir de ce contexte, quelle est la valeur représentée par cette expression ? »

Une `ConstantValue` :

```text
Resolve() → 50
```

Une `StatValue` :

```text
Resolve() → attaque actuelle
```

Une `AddValue` :

```text
Resolve() → résolution de ses deux enfants + addition
```

Une formule :

```text
Resolve() → calcul de sa formule
```

Toutes respectent le même contrat.

---

# 25. Ce qui a réellement été décidé

À ce stade, la décision de conception à conserver est :

```text
DamageEffect
{
    Amount : IValue
}
```

et non :

```text
DamageEffect
{
    Power : int
}
```

avec une formule interne.

La responsabilité devient :

```text
IValue
    → produit le montant final

DamageEffect
    → applique ce montant
```

C'est la séparation qui répond le mieux au besoin actuel de composition.

---

# 26. Ce qui reste volontairement ouvert

Cette décision ne signifie pas que toute l'architecture de `IValue` est déjà définie.

Restent notamment à découvrir :

- la signature exacte de `Resolve` ;
- `EffectContext` ou `AbilityContext` comme contexte d'entrée exact ;
- les types de valeurs de base ;
- les statistiques accessibles ;
- la manière de représenter les pourcentages ;
- les opérations disponibles ;
- la gestion des arrondis ;
- la gestion des valeurs négatives ;
- la gestion des divisions par zéro ;
- les règles d'agrégation de plusieurs casters ;
- les valeurs aléatoires ;
- les valeurs dépendant des statuts ;
- les valeurs dépendant des résultats précédents ;
- la sérialisation ;
- la représentation dans l'éditeur ;
- les éventuelles formules spécialisées qui deviendront réellement nécessaires.

Ces questions doivent être traitées **à partir de capacités concrètes**, pas toutes à l'avance.

---

# 27. Philosophie générale

Le piège serait de construire maintenant un langage mathématique complet parce que le système pourrait un jour en avoir besoin.

La bonne démarche reste celle déjà retenue pour le ciblage :

> **Construire une abstraction suffisamment simple pour pouvoir expérimenter avec elle, puis la torturer jusqu'à découvrir ses limites.**

On peut donc créer quelques capacités volontairement variées :

```text
50 dégâts fixes
Attaque × 150 %
PV max × 20 %
Attaque + 50
Attaque - Défense
Soin basé sur l'attaque
Soin basé sur les PV max
Dégâts basés sur plusieurs casters
Valeur basée sur des stacks de statut
...
```

Puis observer :

```text
Le modèle reste naturel ?
        ↓
Oui → continuer
        ↓
Non → comprendre pourquoi
        ↓
Nouvelle abstraction
        ↓
Nouveaux cas de test
```

---

# 28. Résumé à retenir dans trois mois

Si dans trois mois je me demande :

> « Mais putain, pourquoi j'ai créé `IValue` ? »

La réponse courte est :

```text
Parce qu'un effet ne devrait pas avoir à connaître
la manière dont un nombre a été calculé.
```

Et surtout :

```text
DamageEffect
    Amount = IValue
```

signifie :

```text
L'effet applique un montant.
L'IValue détermine ce montant.
```

Une `IValue` peut représenter :

```text
une constante
une statistique
une donnée du contexte
une opération
une formule
une expression composée d'autres IValue
```

et elle peut exploiter le fait que le contexte contient plusieurs casters.

Son contrat reste toujours :

```text
contexte
   ↓
Resolve(...)
   ↓
int
```

---

# 29. Et pourquoi tout ça ?

Parce qu'à force de vouloir pouvoir écrire :

```text
50 capacités
500 capacités
5000 capacités
```

sans créer :

```text
DamageEffectMachin
DamageEffectBidule
DamageEffectEncoreUnTruc
...
```

on finit forcément par construire un petit langage déclaratif.

Et oui :

> Pour volontairement se créer ce genre de problèmes d'architecture, il faut probablement avoir une vie suffisamment tranquille pour ne pas avoir de copine. 🤣

Mais la raison technique, elle, est parfaitement légitime :

```text
Code = vocabulaire du moteur
Data = composition des capacités
Editor = outil de création de ces compositions
```

`IValue` est l'une des briques qui permet à ce vocabulaire de décrire **comment obtenir un nombre sans que l'effet qui l'utilise ait besoin de connaître cette logique**.