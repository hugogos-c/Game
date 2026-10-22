# Fiche de Décision Architecturale (ADR) : Choix de Modélisation du Moteur de Règles (Capacités & Statuts)

**Statut :** Validé  
**Contexte :** Prototype 7 / Passage sur le projet GitHub final (Projet de Vie)  
**Auteur :** Développeur Solo  

---

## 1. Problématique & Objectif
Le projet nécessite un système de capacités et de statuts (buffs/debuffs) hautement modulaire, piloté par les données (*Data-Driven*), capable d'évoluer sur plusieurs décennies sans nécessiter de recompilation du code source C#. 
Pour cela, un pattern **Composite** de bas niveau a été mis en place (`ICondition`, `IEffect`, `IValue`, `IValues`). 

La question centrale de cette réflexion est : **Comment modéliser et exposer le "sucre syntaxique" de haut niveau (ex: la notion de Chance/Jet de dés ou de Pourcentage) pour qu'il soit à la fois performant pour le moteur de jeu et exploitable par un futur éditeur de capacités ?**

---

## 2. Solutions Envisagées & Analyses

### 🛑 Option A : Les Opérateurs Bit-à-Bit (Bitwise Flags)
L'utilisation d'enums de type `[Flags]` et d'opérateurs binaires (`&`, `|`, `^`) pour catégoriser les filtres de cibles, les éléments ou les statuts.

*   **Pourquoi elle semblait séduisante :** Performances absolues au niveau du processeur (1 cycle CPU), compacité maximale en mémoire C#.
*   **Pourquoi elle a été REJETÉE :** 
    *   **Limite physique stricte** : Bloqué à 32 ou 64 flags maximum (taille d'un `int` ou d'un `ulong`). Impossible de créer des centaines de statuts sur le long terme.
    *   **Couplage fort au code source** : Ajouter un statut ou un tag impose de modifier un `enum` en C# et de recompiler, violant le principe *Open/Closed*. C'est l'antithèse d'une architecture *Data-Driven*.
*   **Alternative retenue pour ce besoin précis** : Utilisation de structures textuelles (`string`) avec recherche en temps constant $\mathcal{O}(1)$ via des `HashSet<string>`. Les opérateurs bitwise restent relégués aux couches techniques invisibles (rendu graphique, pathfinding).

### 🛑 Option B : L'Approche par Fonctions / Factory (`SharedLogicFactory`)
Supprimer complètement les classes de haut niveau (`ChanceEffect`, `PercentageValue`) du code source. À la place, une Factory statique en C# assemble et imbrique à la volée les briques de bas niveau existantes (ex: transformer une intention de pourcentage en une suite d'opérations mathématiques brutes `(Source * Pourcentage) / 100`).

*   **Pourquoi elle semblait séduisante :** Pureté maximale du code C#, aucune duplication de classes symétriques (`Effect` vs `Value`), centralisation absolue de la logique mathématique.
*   **Pourquoi elle a été REJETÉE :**
    *   **Le problème de la "Boîte Noire" (Reverse-Engineering impossible)** : Une fois l'arbre de bas niveau généré par la Factory, le concept sémantique de haut niveau disparaît. Si l'éditeur de capacités charge ce fichier, il est incapable de deviner que la suite d'opérations brutes était à l'origine un bête "Pourcentage". L'éditeur afficherait une tuyauterie mathématique incompréhensible pour l'utilisateur.
    *   **Double maintenance des données** : Pour que l'éditeur s'y retrouve, il aurait fallu stocker une représentation visuelle ET une représentation calculée pour le jeu dans les fichiers externes. Une source de bugs majeure sur le long terme.

---

## 3. Solution Retenue : L'Approche par Classes Spécialisées (Composants Éditables)
Création de types C# dédiés et distincts pour chaque concept de haut niveau (ex: `ChanceEffect : IEffect`, `ChanceValue : IValue`, `PercentageValue : IValue`). Ces classes encapsulent la plomberie logique et mathématique en interne lors de leur évaluation.

### 🟢 Les Avantages Cruciaux (Pourquoi ce choix pérennise le projet)

1.  **Parfaite adéquation avec le futur Éditeur de Capacités (Introspection / Réflexion)**
    L'éditeur graphique peut scanner le code C# de manière bijective. S'il détecte le type `ChanceEffect`, il sait exactement qu'il doit afficher une interface épurée avec deux cases : un champ numérique pour la probabilité et un nœud pour l'effet en cas de succès.
2.  **Lisibilité, Compacité et Clarté des Fichiers Externes (JSON/YAML)**
    Les fichiers de configuration décrivent des intentions de gameplay claires, lisibles par un humain, plutôt qu'une suite de plomberie mathématique illisible.
3.  **Encapsulation Métier & Évolutivité**
    Si la formule de calcul d'un jet de dés ou d'un pourcentage doit évoluer dans 5 ans (ex: ajout d'un lissage de chance anti-frustration ou modification des règles d'arrondis), le changement est isolé à l'intérieur de ces classes. Les milliers de fichiers JSON de capacités et l'éditeur visuel restent strictement inchangés.
4.  **Préparation de la "Galère des Statuts"**
    L'intégration future des statuts complexes (ex: le statut "Résonnance" qui écoute des événements pour accumuler de l'état avant de se dissiper) va ajouter une immense couche de complexité. Avoir une architecture claire où chaque nœud possède une identité propre (`Chance`, `Conditional`) en C# est indispensable pour éviter que le système d'événements ne devienne un nœud de serpent inextricable.

### ⚠️ Le Prix à Payer (Conscient et Accepté)
Cette approche crée une légère duplication structurelle à l'écriture en C# au tout début du projet (devoir écrire la déclinaison d'un concept en `Effect`, `Value`, et parfois `Values`). Pour limiter la duplication de la formule mathématique exacte à l'intérieur de ces classes parallèles, un helper interne ou une mini-factory de bas niveau (`SharedLogicFactory.CreateRollCondition`) sera utilisé pour unifier le comportement.

---

## 4. Règle d'Or pour le Futur
**Le haut niveau doit composer le bas niveau, et non le réinventer.**  
Chaque fois qu'une nouvelle mécanique de gameplay globale est introduite, elle doit prendre la forme d'une classe dédiée lisible par l'éditeur, mais son exécution doit réutiliser au maximum les briques fondamentales et atomiques déjà éprouvées du moteur.
