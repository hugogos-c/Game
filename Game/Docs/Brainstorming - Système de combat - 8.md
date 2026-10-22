# Brainstorming --- Système de combat --- Logique de ciblage (Prototype 6)

> Document de réflexion --- conserver les décisions et les points encore
> ouverts pour éviter de reprendre le raisonnement depuis le début.

## 1. Point de départ

Dans le système initial, chaque capacité possède un `TargetMode`, avec
des valeurs comme :

-   `SingleEnemy`
-   `AllEnemies`
-   `SingleAlly`
-   `AllAllies`
-   `Self`

Ce modèle fonctionne pour des cas simples, mais il lie deux questions
différentes :

1.  **Qui peut être ciblé ?** (ennemis, alliés, personnages vivants,
    etc.)
2.  **Combien de cibles la capacité prend-elle ?** (une, plusieurs,
    toutes...)

Il pousse aussi à ajouter une valeur d'enum dès qu'un nouveau
comportement de ciblage apparaît.

## 2. Idée retenue : dissocier éligibilité et quantité

La réflexion a conduit à remplacer les modes prédéfinis par deux
éléments composables :

-   `TargetsFilters` : détermine quels personnages sont éligibles.
-   `RequiredTargetsCount` : détermine combien de cibles la capacité
    demande.

L'objectif est que la règle de ciblage soit décrite par la combinaison
de ces éléments, plutôt que par une valeur unique de `TargetMode`.

## 3. Pourquoi des filtres composables ?

Les candidats initiaux sont tous les personnages des équipes du combat :

``` csharp
context.Battle.Teams.SelectMany(t => t.Members)
```

Les filtres réduisent ensuite cette liste.

Dans la liste principale, les filtres sont appliqués successivement :
ils se combinent donc en **ET**.

Exemple :

``` csharp
TargetsFilters =
[
    new EnemiesFilter(),
    new IsAliveFilter(),
]
```

Lecture : conserver les personnages qui sont à la fois ennemis de
l'initiateur et vivants.

Les filtres discutés :

-   `EnemiesFilter` : exclut l'équipe de l'initiateur.
-   `AlliesFilter` : conserve l'équipe de l'initiateur.
-   `IsAliveFilter` : conserve les personnages vivants.
-   `OrFilter` : permet de combiner plusieurs critères avec **OU**.

`OrFilter` évalue ses sous-filtres sur la même liste de candidats, puis
réunit leurs résultats. Les filtres extérieurs à cet `OrFilter`
continuent à s'appliquer ensuite.

## 4. La quantité est une autre décision

Les filtres ne doivent pas décider si la capacité est « mono-cible » ou
« multicible ». Ils répondent uniquement à la question de l'éligibilité.

`RequiredTargetsCount` exprime la quantité demandée :

-   `1` : demander une cible.
-   `N` : demander N cibles.
-   `-1` : prendre automatiquement toutes les cibles disponibles, sans
    demander de sélection au joueur.

Règle importante discutée : si la capacité demande davantage de cibles
qu'il n'en reste d'éligibles, on prend celles qui existent au lieu de
rendre l'action impossible. Exemple : demande de 3, seulement 2 cibles
valides → les 2 sont utilisées.

Lorsque le nombre demandé est inférieur au nombre de candidats
disponibles, la sélection manuelle doit permettre de choisir le nombre
demandé.

## 5. Comment exprimer les anciens modes ?

Les modes ne sont plus des valeurs d'enum : ils deviennent des
combinaisons.

### Une cible ennemie vivante

``` csharp
TargetsFilters =
[
    new EnemiesFilter(),
    new IsAliveFilter(),
],
RequiredTargetsCount = 1,
```

### Tous les ennemis vivants

``` csharp
TargetsFilters =
[
    new EnemiesFilter(),
    new IsAliveFilter(),
],
RequiredTargetsCount = -1,
```

### Une cible alliée vivante

``` csharp
TargetsFilters =
[
    new AlliesFilter(),
    new IsAliveFilter(),
],
RequiredTargetsCount = 1,
```

### Tous les alliés vivants

``` csharp
TargetsFilters =
[
    new AlliesFilter(),
    new IsAliveFilter(),
],
RequiredTargetsCount = -1,
```

Ces exemples montrent le principe : le filtre choisit la catégorie, la
quantité choisit le nombre.

## 6. Pourquoi retirer `TargetMode` ?

Conserver `TargetMode` en parallèle des filtres et de la quantité
créerait deux sources de vérité : une partie du ciblage serait décrite
par l'enum, une autre par les filtres.

La direction retenue est donc de retirer l'enum et la propriété
`TargetMode`, puis de faire dépendre le choix des cibles de :

1.  la liste des personnages du combat ;
2.  l'application des filtres ;
3.  `RequiredTargetsCount` ;
4.  la règle de sélection manuelle ou automatique.

## 7. Liste de filtres vide : possibilité conservée, ergonomie à surveiller

Une liste `TargetsFilters = []` peut signifier **aucune restriction** :
tous les personnages du combat restent éligibles.

On a envisagé que cette propriété `required` oblige à répéter
`TargetsFilters = []` dans chaque capacité sans filtre. Cela risque de
devenir répétitif et encombrant.

Décision provisoire : **on conserve la possibilité d'une liste vide**,
même si on pourra revoir plus tard si la propriété doit rester
`required` ou recevoir une valeur par défaut vide. Une liste vide ne
signifie pas « aucune cible » ; elle signifie « aucun filtre ».

## 8. Plusieurs équipes : comportement actuel et question ouverte

Le combat de démonstration contient plusieurs équipes.

La définition actuelle de `EnemiesFilter` considère comme ennemis tous
les personnages qui ne font pas partie de l'équipe de l'initiateur.
`AlliesFilter` correspond aux membres de cette même équipe.

Cela permet de fonctionner avec plus de deux équipes sans coder « équipe
joueur contre équipe ennemie » en dur.

En revanche, le concept d'alliances entre équipes n'est pas défini à ce
stade. Si le jeu introduit des équipes neutres ou alliées entre elles,
il faudra préciser la règle métier ; ne pas supposer cette mécanique
avant qu'elle soit décidée.

## 9. Réutiliser les filtres dans les séquences

L'idée est de remplacer les `Selector` par des `Filter`.

Les filtres ne servent donc pas uniquement à déterminer les cibles initiales de la capacité : ils pourront aussi être réutilisés dans les séquences.

Par exemple, `IsAliveFilter` pourra servir à ne conserver que les personnages vivants, que ce soit lors du ciblage de la capacité ou lors du traitement d'une séquence.

Cela permet de réutiliser les mêmes règles de filtrage à différents endroits du système, plutôt que de maintenir des sélecteurs distincts pour chaque usage.

Dans `amourImpossible`, la séquence de soin pourra ainsi appliquer ses filtres aux personnages concernés par le soin, tandis que la séquence de dégâts appliquera les siens aux personnages concernés par les dégâts.

## 10. Application au `Program.cs` actuel

Les capacités du code fourni ont déjà leurs filtres :

  Capacité               Filtres dans le code fourni
  ---------------------- -----------------------------
  `coupEpee`             Ennemis + vivants
  `dechargeElectrique`   Ennemis + vivants
  `execution`            Ennemis + vivants
  `soin`                 Alliés + vivants
  `feu`                  Ennemis + vivants
  `foudre`               Ennemis + vivants
  `amourImpossible`      Ennemis + vivants
  `comboGobelin`         Ennemis + vivants

Le `TargetMode` qui subsiste dans la boucle interactive sert
actuellement à décider si le joueur choisit une cible ou si les cibles
sont résolues automatiquement. C'est cette branche qu'il faut remplacer
par une logique basée sur les cibles éligibles et le nombre requis.

Les séquences et les effets n'ont pas besoin d'être réécrits simplement
pour retirer `TargetMode`.

## 11. Validation : ce qui existe et ce qui reste à raccorder

La validation actuelle vérifie notamment :

-   que l'initiateur est vivant ;
-   que les participants sont vivants ;
-   qu'il existe au moins une cible éligible après les filtres ;
-   les conditions spécifiques, par exemple les PM ou les barres de
    synergie.

Elle ne garantit pas encore, dans le code montré, que les cibles
choisies sont toutes issues de la liste éligible ni que leur quantité
correspond à `RequiredTargetsCount`.

Ces contrôles devront être raccordés à la validation quand la sélection
des cibles sera implémentée. Il faut éviter de confondre « il existe des
cibles disponibles » avec « la sélection fournie est valide ».

## 12. Décisions et questions à garder en mémoire

### Décisions prises

-   Le ciblage est travaillé dans **Prototype 6**.
-   `TargetMode` doit être supprimé.
-   `TargetsFilters` décrit l'éligibilité des personnages.
-   `RequiredTargetsCount` décrit la quantité demandée.
-   `-1` signifie prendre toutes les cibles éligibles automatiquement.
-   Si le nombre demandé dépasse les candidats éligibles, on prend les
    candidats disponibles.
-   Une liste de filtres vide reste autorisée comme absence de
    restriction.
-   Les équipes autres que celle de l'initiateur sont actuellement
    considérées comme ennemies.

### Points à ne pas considérer comme déjà décidés

-   La gestion éventuelle des alliances ou des équipes neutres.
-   Le choix définitif entre `TargetsFilters required` et une valeur par
    défaut vide.
-   Les validations finales de quantité et d'appartenance des cibles.
-   Les règles précises d'interface de sélection, au-delà du principe de
    sélection manuelle lorsque le nombre requis est inférieur aux
    candidats disponibles.

## Résumé du raisonnement

On remplace un enum qui encode des scénarios de ciblage complets par une
règle composée :

**Tous les personnages du combat → filtres d'éligibilité → nombre de
cibles → sélection manuelle ou automatique → `context.Targets` →
séquences.**

Le gain recherché est de pouvoir composer de nouveaux ciblages sans
enrichir `TargetMode` à chaque nouveau cas, tout en gardant séparées les
responsabilités de filtrage, de quantité et d'exécution.
