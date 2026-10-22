# Spécification Architecturale : Modularité des Coûts et Refactoring des Aptitudes
**Projet :** Prototype_7 (RPG au tour par tour découplé)  
**Date :** Octobre 2026  
**Statut :** Phase de conception avant grand refactoring  
**Cible :** Document de transmission pour IA/Développeur Senior  

---

## 1. Contexte Général et Philosophie de Game Design

Le projet est un moteur de combat RPG au tour par tour découplé de son moteur graphique (Unity). Les règles de jeu sont programmées dans une bibliothèque C# pure (.NET 8/9/10), validées par des Tests Unitaires et exécutables en ligne de commande (Console).

Le Game Design repose sur quatre catégories d'aptitudes actives (`AbilityCategory`) aux dynamiques de ressources distinctes :
1. **Skill :** Action fiable, répétable et gratuite (aucun coût de ressource).
2. **Spell :** Action puissante consommant du **Mana** (ressource rare, persistante et difficile à restaurer, utilisable hors combat).
3. **Synergy :** Action collective majeure consommant une **Jauge de Synergie d'équipe** (commence à 0 au début du combat, se charge via les actions du combat) et requérant **plusieurs participants**.
4. **Ultimate :** Action exceptionnelle liée à un personnage consommant une **Jauge d'Ultimate individuelle** (se charge lentement en combat).

*Évolution future prévue :* Introduction potentielle de nouveaux types de coûts, notamment des **PA (Points d'Action)** ou de la **Stamina** pour rythmer le tour par tour.

---

## 2. La Problématique Architecturale : Le Piège de l'Héritage Strict

Dans la version initiale du prototype, les catégories étaient modélisées par héritage direct :
`BaseAbility` ➔ `Spell`, `Skill`, `Synergy`.

### Limites identifiées :
* **Rigidité structurelle (L'Essence vs La Folie) :** Si `Synergy` porte techniquement la notion de participants et `Spell` porte le Mana, le moteur s'interdit structurellement les cas hybrides (ex: un "Rituel Magique" qui consommerait du Mana *ET* demanderait 2 participants). 
* **Pollution des classes (L'Anti-Pattern de l'Accumulation) :** Si on centralise toutes les propriétés de ressources dans `BaseAbility` pour parer à toute éventualité (`ManaCost`, `SynergyCost`, `UltimateCost`, `ActionPointCost`), chaque simple coup d'épée gratuit instancié en mémoire traîne des variables inutiles à 0. La méthode de validation centrale devient un arbre de `if/else` géant et fragile.

---

## 3. L'Architecture Validée : Le Pattern de Composition (`ICost`)

Pour garantir une extensibilité totale sur plusieurs décennies sans polluer `BaseAbility`, la gestion des ressources est externalisée via le **Pattern Stratégie / Composition d'Objets**.

### Spécification du Contrat `ICost`
L'interface `ICost` possède une triple responsabilité : valider la ressource au runtime, la consommer, et **valider l'intégrité structurelle** de la capacité lors de son instanciation (Fail-Fast au démarrage).

```csharp
public interface ICost
{
    // 1. Validation au runtime (Ex: le personnage a-t-il assez de PM ?)
    AbilityValidationResult Validate(AbilityContext context);
    
    // 2. Paiement au runtime (Ex: déduire les PM)
    void Pay(AbilityContext context);
    
    // 3. Gardien de l'essence métier (Ex: crier si une synergie n'a pas de participant)
    void ValidateCapabilityStructure(BaseAbility ability);
}
```

### Implémentations des Composants (Exemples en `record`)

```csharp
public record ManaCost(int Amount) : ICost
{
    public AbilityValidationResult Validate(AbilityContext context) =>
        context.Initiator.CurrentMp >= Amount 
            ? AbilityValidationResult.Valid() 
            : AbilityValidationResult.Invalid(AbilityValidationError.NotEnoughMana);

    public void Pay(AbilityContext context) => context.Initiator.ConsumeMp(Amount);

    public void ValidateCapabilityStructure(BaseAbility ability) { /* Aucune contrainte spatiale */ }
}

public record SynergyGaugeCost(int Bars) : ICost
{
    public AbilityValidationResult Validate(AbilityContext context) =>
        context.Initiator.Team.SynergyGauge.HasEnoughBars(Bars)
            ? AbilityValidationResult.Valid()
            : AbilityValidationResult.Invalid(AbilityValidationError.NotEnoughSynergy);

    public void Pay(AbilityContext context) => context.Initiator.Team.SynergyGauge.ConsumeBars(Bars);

    public void ValidateCapabilityStructure(BaseAbility ability)
    {
        // PROTECTION DE L'ESSENCE MÉTIER : C'est le coût qui exige la règle, pas le moteur central.
        if (ability.RequiredParticipantsCount < 1)
        {
            throw new InvalidOperationException(
                $"Erreur d'architecture sur '{ability.Name}' : Un coût en Synergie requiert structurellement au moins 1 participant.");
        }
    }
}
```

### Structure de la Classe Unique `ActiveAbility`
Grâce à ce modèle, la classe centrale devient fermée aux modifications (Open/Closed Principle). Elle ne contient aucun `if`, aucun `is` de vérification de type, ni aucun couplage avec les sous-classes.

```csharp
public sealed class ActiveAbility : BaseAbility
{
    // Catégorisation purement informative (Tri UI Unity / Logs)
    public required AbilityCategory Category { get; init; }
    public int RequiredParticipantsCount { get; init; } = 0;
    
    // Liste modulaire de composants
    public IReadOnlyList<ICost> Costs { get; init; } = [];

    public ActiveAbility()
    {
        // Validation architecturale à la création de l'objet
        foreach (var cost in Costs)
        {
            cost.ValidateCapabilityStructure(this);
        }
    }

    protected override AbilityValidationResult ValidateSpecific(AbilityContext context)
    {
        foreach (var cost in Costs)
        {
            var result = cost.Validate(context);
            if (!result.IsValid) return result;
        }
        return AbilityValidationResult.Valid();
    }

    public override void PayCost(AbilityContext context)
    {
        foreach (var cost in Costs) cost.Pay(context);
    }
}
```

---

## 4. Pistes de Réflexion Ouvertes pour le Prochain Brainstorming

Pour le modèle d'IA ou le développeur reprenant ce fil, voici les chantiers techniques et conceptuels prioritaires à traiter :

### 📋 Axe 1 : Modélisation des Strongly Typed IDs (Identifiants Fortement Typés)
* **Objectif :** Remplacer le type primitif `string Id` des compétences et des personnages pour éviter les bugs d'assignation en mode texte.
* **Piste technique :** Implémenter des structures immuables de type valeur avec `readonly record struct AbilityId(string Value);`. Déterminer s'il faut y intégrer des fabriques (Factories) de validation ou des générateurs automatiques.

### 📋 Axe 2 : La Persistance & Sérialisation Polymorphique du Pattern `ICost`
* **Objectif :** Comment stocker et recharger proprement la liste `IReadOnlyList<ICost> Costs` depuis des fichiers JSON à l'aide de `System.Text.Json`.
* **Contrainte :** Le système devra utiliser des résolveurs de type polymorphique (`JsonDerivedType`) pour recréer dynamiquement les bons `record` de coût à partir d'un discriminant type (ex: `\$type: "ManaCost"`).

### 📋 Axe 3 : La Synchronisation des Données et les Mutabilités (`record class` vs `class`)
* **Objectif :** Arbitrer définitivement les types C# à utiliser lors de la refondation globale.
* **Rappel des décisions actuelles :** Les objets de configuration (Aptitudes, Effets, Coûts) doivent être des `sealed record` immuables (`{ get; init; }`) pour faciliter la comparaison par valeur. L'état dynamique du jeu (Personnages, Contextes de combat) doit rester sous forme de `class` traditionnelles mutables stockées dans la Heap.
