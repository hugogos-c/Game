# Décisions d'Architecture (ADR) — Moteur de Capacités RPG (C# / .NET)

Ce document conserve l'historique des réflexions, les choix d'architecture retenus pour le moteur de jeu, ainsi que les alternatives rejetées et les raisons de leur exclusion.

---

## 1. Découpage de la Solution (Clean Architecture)

### Décision
Séparer la solution C# en 3 projets distincts à responsabilités uniques :
* `Game.Core` (DLL) : Logique pure du jeu, interfaces, structures de données (AST), règles de combat. Aucune dépendance externe.
* `Game.Content` (DLL) : Infrastructure de persistance, chargement JSON, repositories, configuration de la sérialisation, fabrique de combat (*BattleFactory*).
* `Game.Prototypes` (Console/UI) : Point d'entrée minimaliste (*Program.cs* ou interface utilisateur) qui orchestre le chargement et lance le moteur.

### Alternatives Rejetées
* **Projet monolithique unique (`Program.cs` contenant tout) :**
  * *Raison du rejet :* Rend le code intestable unitairement, empêche la réutilisation de la logique C# dans d'autres interfaces (Blazor, MonoGame, Godot, Unity) et mélange l'infrastructure avec la logique métier.

---

## 2. Persistance : JSON vs Base de Données (EF Core)

### Décision
Utiliser des fichiers **JSON** pour la configuration du contenu (*Abilities*, *Conditions*, *Status*) et pour les sauvegardes de parties.

### Alternatives Rejetées
* **Base de données relationnelle via Entity Framework Core :**
  * *Raison du rejet :* Les capacités sont représentées sous forme d'arbres de syntaxe abstraite récursifs (AST). Mapper un arbre polymorphique avec des nœuds imbriqués de profondeurs variables (`ConditionalEffect` contenant d'autres `IEffect`) dans un modèle relationnel SQL nécessite des jointures complexes et une table par type de nœud. La complexité de maintenance surpasse largement les gains d'une BDD.

---

## 3. Stratégie de Lecture : Catalogue en RAM vs Lecture Fichier à la Volée

### Décision
Charger tous les fichiers JSON au démarrage de l'application, les désérialiser en mémoire et les stocker dans un dictionnaire centralisé (`AbilityRepository`).

### Alternatives Rejetées
* **Lecture/Parsing du fichier JSON sur le disque à chaque exécution de capacité :**
  * *Raison du rejet :* Entraîne des accès I/O disque lents, génère une forte pression sur le *Garbage Collector* (allocations mémoire répétées) et risque de faire planter le jeu en plein combat si le fichier contient une erreur, au lieu d'échouer immédiatement au lancement (*Fail-Fast*).

---

## 4. Identifiants : Strongly Typed IDs (Slugs vs GUIDs)

### Décision
Adopter le pattern **Strongly Typed IDs** (`readonly record struct AbilityId(string Value)`).
* **Données de référence (Capacités, Statuts) :** Utiliser des **Slugs textuels fixes** (ex: `warrior_slash`).
* **Entités dynamiques (Personnages instanciés) :** Utiliser des **GUIDs** (ex: `CharacterId(Guid)`).

### Alternatives Rejetées
* **Chaînes de caractères brutes (`string`) partout :**
  * *Raison du rejet :* Source de bugs silencieux (ex: inverser deux paramètres `string` dans une méthode `ApplyStatus(string charId, string statusId)` compile sans erreur).
* **GUIDs purs pour les données de référence :**
  * *Raison du rejet :* Rend la lecture manuelle des JSON et le débogage dans Git illisibles (`"equippedAbilities": ["3f2504e0-4f89..."]`). Le slug textuel permet de conserver une clé immuable et lisible, tout en autorisant la modification du nom d'affichage (`"Name": "Coup d'épée"`) sans casser les références.
* **Opérateurs de conversion `implicit` débridés :**
  * *Raison du rejet :* Annule le bénéfice du typage fort en autorisant le compilateur à convertir silencieusement un `AbilityId` en `CharacterId` via leur type sous-jacent.

---

## 5. Framework de Sérialisation : System.Text.Json vs Newtonsoft.Json

### Décision
Utiliser **`System.Text.Json`** (.NET natif) configuré dans le projet `Game.Content`.

### Alternatives Rejetées
* **`Newtonsoft.Json` (Json.NET) :**
  * *Raison du rejet :* Plus lent, gourmand en allocations mémoire (conversion UTF-16), et lie le polymorphisme aux noms complets de classes C# (`AssemblyQualifiedName`), ce qui casse la compatibilité des JSON dès qu'une classe C# est déplacée de dossier/namespace.
  * *Bénéfices de System.Text.Json :* Lecture directe en UTF-8 via `Span<T>`, support AOT natif, et polymorphisme propre basé sur des discriminants explicites (`$type: "damage"`).

---

## 6. Enregistrement du Polymorphisme : Réflexion vs Configuration Manuelle

### Décision
Enregistrer le polymorphisme JSON de manière **automatique par Réflexion (Assembly Scanning)** dans `Game.Content` lors du démarrage. Le configurateur scanne l'assembly `Core` et enregistre toutes les classes concrètes qui implémentent `IEffect`, `ICondition`, `IValue`, etc.

### Alternatives Rejetées
* **Attributs `[JsonPolymorphic]` directement sur les interfaces du `Core` :**
  * *Raison du rejet :* Pollue le domaine métier (`Core`) avec des détails d'infrastructure de sérialisation (violation du Clean Architecture).
* **Déclaration manuelle de chaque classe dérivée dans la configuration :**
  * *Raison du rejet :* Coût de maintenance double et risque d'oubli à chaque ajout d'un nouvel effet ou d'une nouvelle condition dans le moteur.

---

## 7. Encodage des Caractères JSON

### Décision
Configurer `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` dans les options de sérialisation.

### Raison
Par défaut, `System.Text.Json` échappe les caractères non-ASCII et HTML (`'` devient `\u0027`, `é` devient `\u00E9`), rendant les fichiers JSON illisibles pour les concepteurs de jeu.

### Clarification Sécurité
Le terme `Unsafe` concerne uniquement l'injection XSS dans un contexte d'affichage web HTML brut. Dans une application C# .NET, la chaîne reste du texte brut en mémoire et ne présente aucun risque d'injection ni d'exécution de code à distance.

---

## 8. Outillage & Éditeur : Génération de Schéma et Éditeur Générique

### Décision
1. **Validation & Autocomplétion :** Générer un fichier `abilities.schema.json` via `System.Text.Json.Schema.JsonSchemaExporter` pour offrir l'autocomplétion et la validation à la saisie dans VS Code.
2. **Éditeur Blazor :** Reporter le développement de l'interface graphique après la stabilisation de la documentation du DSL. L'éditeur sera **générique** et utilisera la Réflexion pour générer dynamiquement les formulaires en fonction des propriétés C# des classes.

### Alternatives Rejetées
* **Développer un composant d'édition sur-mesure par type d'effet :**
  * *Raison du rejet :* Nécessite de réécrire du code UI à chaque ajout de fonctionnalité dans le moteur. L'approche par Réflexion garantit que toute nouvelle classe ajoutée au `Core` est automatiquement éditable sans modifier le projet Blazor.