# Spécifications Techniques — Prototype du Système de Combat

Ce document récapitule l'architecture logicielle, les structures de données et les règles de gestion établies pour le prototype du système de combat en tour par tour.

---

## 1. Vue d'ensemble

Le système gère un combat au tour par tour multi-équipes. Chaque tour se déroule en deux phases :
1. **Phase de décision** : Chaque combattant vivant planifie une action.
2. **Phase d'exécution** : Les actions sont ordonnées selon la vitesse des lanceurs puis exécutées séquentiellement.

Le modèle repose sur le principe de **découplage total** entre les entités (personnages/équipes), les capacités (`Abilities`) et la logique d'impact (`Effects`).

---

## 2. Modèle de Données & Entités (`Entities.cs`)

### `Stats`
Structure de valeur (`record struct`) regroupant les attributs numériques :
* **`MaxHp` / `MaxMp`** : Santé maximale et réserves de mana.
* **`Attack` / `Defense`** : Puissance offensive et réduction des dégâts reçus.
* **`Speed`** : Valeur d'initiative déterminant l'ordre de passage.

### `Character`
Représente une unité combattante.
* **Données** : Statistiques de base, points actuels (`CurrentHp`, `CurrentMp`) et catalogue de capacités (`List<IAbility>`).
* **État** : Propriété calculée `IsAlive` (`CurrentHp > 0`).
* **Mutateurs** : Méthodes de modification d'état avec contrôle des bornes (`TakeDamage`, `Heal`, `ConsumeMp`, `RestoreMp`).

### `Team`
Regroupe un ensemble de personnages sous un nom d'équipe.
* Contient la propriété `IsDefeated` (vraie si tous les membres sont K.O.).

---

## 3. Architecture des Capacités (`Abilities.cs`)

L'architecture s'appuie sur le contrat `IAbility` et la classe de base `BaseAbility` pour éviter la duplication de code (*DRY*).

```
IAbility (Interface)
 └── BaseAbility (Abstraite : gère Id, Name, TargetMode, Effects, Execute loop)
      ├── Skill    (Compétence physique : sans coût)
      ├── Spell    (Sortilège : requiert et consomme des PM)
      └── Synergy  (Action combinée : requiert la jauge d'équipe et des co-lanceurs)
```

| Type | Ressource requise | Condition de disponibilité (`CanCast`) |
| :--- | :--- | :--- |
| **Skill** | Aucune | Disponible par défaut. |
| **Spell** | Mana (`ManaCost`) | PM du lanceur principal $$\ge$$ `ManaCost`. |
| **Synergy** | Barres de synergie (`RequiredSynergyBars`) | Jauge d'équipe suffisante **ET** nombre suffisant d'alliés vivants, non occupés, possédant la même capacité (`Id`). |

---

## 4. Système d'Effets et Contexte (`Effects.cs` & `AbilityContext.cs`)

### `AbilityContext`
Objet conteneur éphémère transmis aux effets lors de l'exécution d'une capacité :
* **`Casters`** : Liste des personnages participant à l'action (1 à $$N$$).
* **`Targets`** : Liste des cibles vivantes recevant l'impact.
* **`Battle`** : Référence au moteur de combat.
* **`Results`** : Registre d'exécution (`List<IEffectResult>`).

### Effets Génériques (`IEffect`)
Les effets sont atomiques, réutilisables et sans logique métier liée à un personnage spécifique :

* **`DealDamage`** : Calcule les dégâts selon la formule $$(Power + \sum Attack_{casters}) - Defense_{target}$$ (minimum 1) et les applique aux cibles.
* **`Heal`** : Restaure une quantité fixe de PV aux cibles (`Targets`).
* **`HealCasters`** : Restaure une quantité fixe de PV à l'ensemble des lanceurs (`Casters`).

---

## 5. Moteur de Combat (`BattleEngine.cs`)

### Résolution de Clic et Ciblage (`TargetResolver`)
Le ciblage est automatisé ou sélectionné selon le `TargetMode` :

| `TargetMode` | Comportement |
| :--- | :--- |
| **`Self`** | Le lanceur principal uniquement. |
| **`SingleEnemy`** | Un ennemi choisi dans une équipe adverse. |
| **`AllEnemies`** | L'intégralité des ennemis vivants. |
| **`SingleAlly`** | Un allié choisi dans l'équipe du lanceur. |
| **`AllAllies`** | L'intégralité des alliés vivants. |

### Ordonnancement et Exécution (`QueuedAction` & `ExecuteRound`)
1. **Pendant le tour** : Les choix sont enregistrés dans des structures `QueuedAction`.
2. **Priorité** : La vitesse d'une action est égale à la vitesse maximale parmi ses lanceurs :
   $$\text{VitesseAction} = \max_{c \in \text{Casters}} (c.\text{Speed})$$
3. **Résolution** :
   * Tri des actions par vitesse décroissante.
   * Annulation de l'action si le lanceur principal tombe K.O. avant son passage.
   * Vérification des ressources et des cibles restantes avant paiement du coût et application des effets.

---

## 6. Spécificités du Système de Synergie

La mécanique de synergie répond aux règles suivantes :

1. **Identification par ID** : Aucune référence en dur aux noms de personnages. La présence de la capacité dans la liste `Abilities` fait foi.
2. **Filtrage des compétences** : Un personnage ne voit une synergie dans son menu que si assez de co-lanceurs valides (vivants, possédant la capacité, et n'ayant pas encore agi ce tour) sont disponibles.
3. **Sélection dynamique des partenaires** : Si plusieurs alliés sont éligibles, le joueur choisit explicitement quel(s) partenaire(s) mobiliser.
4. **Verrouillage du tour** : Tous les participants d'une synergie sont intégrés au registre des unités occupées (`busyCharacters`) et consomment leur action pour le tour en cours.