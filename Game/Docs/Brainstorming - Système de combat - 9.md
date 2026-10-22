# Documentation d'Architecture : Système d'Expressions Numériques Data-Driven (Prototype_7)

## 📌 Contexte et Objectif
Dans le cadre d'une architecture **data-driven** (pilotée par les données), ce système permet de concevoir des compétences, des effets (soins, dégâts, jauges) et des modificateurs directement depuis des fichiers de configuration (JSON/YAML) sans avoir à recompiler le code C#. 

L'objectif de cette documentation est de figer la chaîne de réflexion qui a mené au choix du design final articulé autour de **`IValue`** et **`IMultiValue`**, afin d'éviter toute régression ou remise en question inutile lors des futures phases de refactoring.

---

## 🧭 L'Évolution du Design (La Chaîne de Réflexion)

Pour arriver à la structure actuelle, l'architecture est passée par 4 étapes clés. Voici pourquoi les pistes alternatives ont été légitimement explorées, puis écartées :

### Étape 1 : Le modèle `IFunction` et `FunctionValue` (L'approche fonctionnelle)
*   **Concept :** Une interface `IFunction` (boîte noire avec une liste d'arguments `int`) et une enveloppe `FunctionValue` (implémentant `IValue`).
*   **Pourquoi ça a été écarté :** Manque d'élégance à l'usage. La classe `FunctionValue` résolvait à l'aveugle tous les arguments dans une liste anonyme (`Arguments[0]`, `Arguments[1]`) avant même de savoir ce que la fonction allait en faire. Cela empêchait d'avoir des propriétés nommées claires (comme `BaseValue` et `Ratio` pour un pourcentage) et bloquait l'évaluation paresseuse (lazy evaluation).

### Étape 2 : L'unification totale ("Tout est Expression / Tout est Tableau")
*   **Concept :** Renommer `IValue` en `IExpression` pour refléter la nature active des calculs, et forcer la méthode `Evaluate()` à toujours retourner une liste d'entiers (`IReadOnlyList<int>`). Une constante `50` renvoyait `[50]`.
*   **Pourquoi ça a été écarté :** "Pollution d'API". Forcer des concepts structurellement simples (comme une constante ou une statistique de personnage unique) à renvoyer un tableau juste pour accommoder un cas particulier (le multi-cible) compliquait inutilement le débogage et le code des opérations binaires strictes (divisions, soustractions).

### Étape 3 : Le débat sémantique sur le nommage Métier
Plusieurs termes spécifiques à l'univers des RPG ont été mis sur la table pour remplacer le terme générique `IValue` :
*   `IFormula` : Écarté car l'architecture veut **interdire** l'écriture de formules de calcul globales dans les fichiers de données (ce qui risquerait d'omettre les dimensions fondamentales du jeu codées en dur, comme les résistances, les faiblesses élémentaires, les buffs, etc.).
*   `IModifier` : Écarté car cela implique sémantiquement la modification d'une statistique existante (un buff), ce qui n'est pas le rôle de cette brique.
*   `IMagnitude` / `IPotency` / `IAmount` : Écartés car trop connotés "puissance brute d'un effet". Or, ce système doit être réutilisable partout (calcul de dégâts, mais aussi gain de jauge de synergie, coût en mana, calcul d'une durée de statut, etc.).

---

## 🛠️ Le Design Final : `IValue` et `IMultiValue`

Le choix s'est arrêté sur une distinction nette entre le monde **scalaire** (un seul nombre) et le monde **vectoriel** (une liste de nombres), portée par des noms neutres qui définissent la nature de la donnée produite, et non ce que le jeu va en faire.

```csharp
namespace Prototype_7.Core
{
    // Produit STRICTEMENT UN unique nombre entier
    public interface IValue
    {
        int Resolve(EffectContext context);
    }

    // Produit une LISTE de nombres entiers (zéro, un ou plusieurs)
    public interface IMultiValue
    {
        IReadOnlyList<int> ResolveMulti(EffectContext context);
    }
}
```

### 🧠 Justification de cette dualité
1.  **Pureté du contrat :** `IValue` garantit au consommateur (l'effet de jeu) qu'il obtiendra un unique `int`. Pas besoin de faire des vérifications de taille de tableau.
2.  **L'Agrégation comme "Le Pont" :** Les classes d'agrégation (comme `AverageValue` ou `MaxValue`) implémentent `IValue` mais consomment une `IMultiValue`. Elles servent de réducteurs de flux de données. Elles encapsulent proprement la transition du multi-cible vers le mono-cible.
3.  **Réutilisabilité et Sémantique :** Les sources de données multi-cibles (`IMultiValue`) deviennent complètement interchangeables. On peut calculer la moyenne des PV des cibles, de l'équipe alliée, ou d'une liste fixe d'expressions via un adaptateur, sans jamais modifier la logique de `AverageValue`.

---

## 📋 Exemples de Référence pour l'Implémentation

### 1. Les Sources de Données (Mono et Multi)
```csharp
public class ConstantValue : IValue
{
    public required int Value { get; init; }
    public int Resolve(EffectContext context) => Value;
}

public class TargetsStatMultiValue : IValues
{
    public required Stat Stat { get; init; }

    public IReadOnlyList<int> ResolveMulti(EffectContext context)
    {
        // Extrait la statistique demandée de TOUTES les cibles actuellement dans le contexte
        return context.Targets.Select(character => character.GetStat(Stat)).ToList();
    }
}
```

### 2. Le Réducteur d'Agrégation
```csharp
public interface IAggregation
{
    int Aggregate(IReadOnlyList<int> values);
}

public class AverageAggregation : IAggregation
{
    public int Aggregate(IReadOnlyList<int> values)
    {
        return (int)values.Average();
    }
}

public class AggregateValue : IValue
{
    public required IValues Values { get; init; }
    public required IAggregation Aggregation { get; init; }

    public int Resolve(EffectContext context)
    {
        return Aggregation.Aggregate(Values.Resolve(context));
    }
}
```

### 3. L'utilisation Métier Sans Friction
Dans une capacité d'attaque de zone (AoE), le `DamageEffect` utilise un `AddValue` (qui est une `IValue`). Le code métier de l'effet reste maître de l'application des résistances, mais sa valeur brute est construite dynamiquement :

```csharp
var degatsAoE = new DamageEffect
{
    // Dégâts bruts attendent une IValue unique
    RawAmount = new AddValue
    {
        Terms = [
            new ConstantValue { Value = 100 }, // Base fixe
            new AverageValue                   // Bonus basé sur la moyenne de la vitesse ennemie
            {
                Source = new TargetsStatMultiValue { Stat = Stat.Speed }
            }
        ]
    }
};
```

---
*Fin du document d'architecture. Conserver ce raisonnement pour toute modification future du sous-système de calcul.*
