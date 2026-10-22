# Documentation d'Architecture : Sérialisation et Optimisation JSON
**Projet :** Prototype_7  
**Date :** Octobre 2026  
**Objectif :** Assurer la sérialisation polymorphique automatique de structures de règles de jeu complexes (effets, compétences, filtres) tout en garantissant un fichier JSON ultra-léger, lisible et performant.

---

## 1. L'Architecture Actuelle et Son Fonctionnement

Le système repose sur l'utilisation avancée de `System.Text.Json` combiné avec un résolveur de type dynamique (`DefaultJsonTypeInfoResolver`) et des modificateurs de métadonnées de sérialisation.

### Le mécanisme de l'Instance Fantôme ("Ghost Instance")
Pour alléger le JSON, le code crée, au démarrage du sérialiseur, une **instance fantôme vide** de chaque classe concrète en mémoire via réflexion (`Activator.CreateInstance`). 
Lorsqu'un objet est sérialisé, le script intercepte chaque propriété et compare sa valeur actuelle avec la valeur d'initialisation C# de l'instance fantôme.
* **Si les valeurs sont identiques :** La propriété est masquée du JSON final (`ShouldSerialize = false`).
* **Si la valeur a été modifiée :** Elle est inscrite dans le JSON.
* **Exception de sécurité :** Les propriétés marquées du mot-clé C# `required` contournent cette optimisation et sont **toujours** sérialisées pour garantir la validité du typage au chargement.

---

## 2. Historique des Décisions Technologiques & Alternatives Rejetées

Pour pérenniser cette architecture sur le long terme (projet s'étalant sur plusieurs décennies), plusieurs choix stratégiques ont été faits. Voici les solutions alternatives étudiées et les raisons objectives de leur rejet.

### ⚠️ Alternative 1 : Utiliser le comportement natif `.DefaultIgnoreCondition = WhenWritingDefault`
* **Ce que c'est :** Une option native de `System.Text.Json` pour masquer les valeurs par défaut.
* **Pourquoi elle a été REJETÉE :** Cette option est trop simpliste. Elle se base uniquement sur les valeurs par défaut *intrinsèques au langage C#* (soit `0` pour un `int`, `false` pour un `bool`, et `null` pour un objet). Si une propriété possède une valeur par défaut de Game Design différente (ex: `public int RequiredTargetsCount { get; init; } = 1;`), l'option native écrira quand même la valeur `1` dans le JSON car `1 != 0`. L'approche par "Instance Fantôme" est la seule capable de respecter les valeurs par défaut définies par le développeur.

### ⚠️ Alternative 2 : Passer les listes et collections en type Nullable (`?`)
* **Ce que c'est :** Autoriser une propriété comme `TargetsFilters` à valoir `null` pour signifier l'absence de filtres, au lieu de l'initialiser à liste vide `= [];`.
* **Pourquoi elle a été REJETÉE :** 
  1. **Fragilité du code (Gameplay) :** Oblige à polluer tout le moteur de combat de vérifications de nullité (`if (Filters != null)`) ou d'opérateurs de propagation (`Filters?.Count`), augmentant drastiquement le risque de crashs de type `NullReferenceException`.
  2. **Ambiguïté sémantique :** Une liste vide `[]` signifie explicitement "aucun filtre appliqué". Une liste `null` introduit un doute (donnée manquante ? erreur de chargement ?).
  3. **Absence de gain sur le JSON :** Grâce au modificateur personnalisé de collection, la liste vide `= [];` est déjà parfaitement masquée du JSON sans avoir besoin de manipuler des `null`.

### ⚠️ Alternative 3 : Utiliser des classes classiques pour les Effets et Compétences
* **Ce que c'est :** Conserver le mot-clé traditionnel `class` pour les objets de données.
* **Pourquoi elle a été REJETÉE :** Par défaut, `.Equals()` sur une classe compare uniquement les **références mémoires** (l'adresse physique de l'objet) et non son contenu. Si une compétence imbrique un effet initialisé par défaut via `= new AbilityTarget();`, l'instance fantôme et l'instance de jeu posséderont deux adresses différentes. Le sérialiseur croira à tort que l'objet a été modifié et polluera le JSON.
* **Solution retenue :** L'utilisation des **`record class`** (ou `record`). Les records génèrent automatiquement une égalité basée sur les **valeurs internes**. `value.Equals(defaultValue)` renvoie `true` si les données intérieures sont identiques, permettant un masquage JSON parfait sans surcharger le moteur avec de la réflexion lourde au runtime.

---

## 3. Blindage des Collections : La Sécurité Anti-Référence

Pour pallier le fait que deux listes distinctes ne partagent pas la même référence mémoire (ce qui brise l'égalité classique), un bloc de sécurité dédié aux types implémentant `IEnumerable` a été intégré au modificateur :

```csharp
// SÉCURITÉ POUR LES COLLECTIONS : Si les deux sont des listes/tableaux et sont vides
if (value is System.Collections.IEnumerable enumerableValue && 
    defaultValue is System.Collections.IEnumerable enumerableDefault)
{
    bool isValueEmpty = !enumerableValue.Cast<object>().Any();
    bool isDefaultEmpty = !enumerableDefault.Cast<object>().Any();
    
    if (isValueEmpty && isDefaultEmpty) return false; // Les deux sont vides -> on masque du JSON
}
```
**Raison d'être :** Ce code intercepte le contenu des collections. Si la liste en jeu et la liste de l'instance fantôme sont toutes les deux vides (zéro élément via `.Any()`), le système garantit leur masquage dans le fichier JSON, faisant fi de leurs adresses mémoires respectives.

---

## 4. Synthèse des Règles pour le Futur du Projet

1. **Règles de Données (Effets, Aptitudes, Filtres, Conditions) :** Toujours utiliser des `sealed record` immuables avec des propriétés `{ get; init; }`. Utiliser `= [];` pour les listes et collections.
2. **Contextes de Transit (AbilityContext, EffectContext) :** Rester sur des `class` traditionnelles. Ce sont des objets vivants avec un état mutable au runtime (`IsAborted`) qui n'ont pas vocation à être sérialisés ou comparés par valeur.
3. **Propriétés Obligatoires :** Utiliser le mot-clé `required` sans initialisation par défaut pour forcer la présence de la donnée dans le JSON et à la compilation.
