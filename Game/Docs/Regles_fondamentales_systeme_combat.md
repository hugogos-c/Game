# Règles fondamentales du système de combat

> Document de conception — version de travail

## 1. Philosophie générale

Le système de combat doit favoriser la **stratégie, la tactique, la préparation et les interactions entre capacités**, plutôt que le simple spam de l'attaque affichant le plus gros nombre.

Une action doit idéalement pouvoir modifier la valeur des actions suivantes.

Le joueur doit régulièrement avoir à choisir entre :
- agir immédiatement ;
- préparer une situation plus favorable ;
- conserver une ressource ;
- exploiter une faiblesse ;
- répondre à une menace ;
- utiliser une ressource rare au bon moment.

Les capacités ne doivent donc pas uniquement représenter des niveaux croissants de puissance. Elles doivent avoir des **fonctions différentes dans la boucle de combat**.

---

## 2. Les quatre types d'Ability

### 2.1 Skill

Un **Skill** est une compétence de base, fiable et réutilisable, permettant au personnage d'agir sans consommer de ressource rare.

Exemples :
- `Coup d'épée`
- `Décharge électrique`
- `Exécution`
- `Champ de force`
- `Changement modulaire`

Un Skill peut être simple ou complexe. Il peut :
- infliger des dégâts ;
- soigner ;
- appliquer ou exploiter un statut ;
- modifier des statistiques ;
- préparer une autre action ;
- appliquer plusieurs effets ;
- remplir une jauge ;
- ou produire toute autre interaction pertinente.

Le fait qu'un Skill soit gratuit ne signifie donc pas qu'il doit être faible ou simpliste.

**Coût :**
- Aucun coût de ressource actuellement.
- Les PA pourront éventuellement être introduits plus tard comme une dimension distincte du système d'action.

**Casters :**
- Par défaut, un Skill est lancé par **un personnage**.
- Le moteur peut conserver une représentation générique permettant plusieurs casters si une capacité future le justifie, mais le multi-caster n'est pas une règle fondamentale du Skill à ce stade.

**Philosophie :**
> « Que puis-je faire de manière fiable et répétable ? »

### 2.2 Spell

Un **Spell** est une capacité permettant d'obtenir un effet particulièrement puissant ou pratique en échange de Mana.

Les Spells peuvent notamment permettre :
- de gros dégâts ;
- de gros soins ;
- des boucliers importants ;
- des statuts puissants ;
- des effets particuliers ou difficiles à obtenir autrement.

Les Spells sont inspirés dans leur philosophie des sorts de Final Fantasy : ils constituent une réserve de puissance dont l'utilisation doit être réfléchie.

Le Mana est une ressource **rare et difficile à restaurer**.

De plus, les PM peuvent être utilisés **hors combat**, par exemple pour soigner. Le joueur doit donc parfois arbitrer entre l'utilisation immédiate de ses PM et leur conservation pour la suite.

Un Spell peut être :
- un dernier recours ;
- une réponse à une situation critique ;
- une capacité particulièrement rentable lorsqu'une condition est préparée ;
- ou une capacité nécessitant de préparer ses ressources avant le combat.

Un Spell ne doit pas simplement être « un Skill avec un plus gros chiffre ». Sa distinction principale vient notamment de son **économie de ressource** et de la décision liée à sa dépense.

**Coût :**
- Mana (PM).
- Le coût exact des différents Spells reste à équilibrer.

**Casters :**
- Par défaut, un Spell est lancé par **un personnage**.
- Le multi-caster n'est pas retenu comme règle fondamentale pour le moment.

**Philosophie :**
> « Est-ce que cette situation justifie de dépenser une ressource rare ? »

### 2.3 Synergy

Une **Synergy** est une capacité collective qui réunit plusieurs personnages afin de produire un effet puissant ou particulier.

Exemples :
- `Amour impossible`
- `Combo gobelin`
- `Garde royale`

Une Synergy peut notamment :
- infliger de gros dégâts ;
- exploiter plusieurs types de dégâts ;
- soigner les lanceurs ;
- protéger l'équipe ;
- appliquer un statut ;
- produire plusieurs effets à la suite ;
- ou créer une autre forme de payoff collectif.

La Synergy doit idéalement justifier la présence de plusieurs personnages par son résultat. Elle ne doit pas être uniquement une façon de remplacer deux actions individuelles par un nombre plus élevé.

La jauge de Synergy :
- appartient à l'équipe ;
- commence à **0 au début du combat** ;
- se remplit au cours du combat ;
- peut être remplie par les Skills et les Spells ;
- se remplit davantage lorsqu'une faiblesse ennemie est exploitée ;
- est consommée lors de l'utilisation d'une Synergy.

Ainsi, la Synergy représente une possibilité collective que l'équipe **construit pendant le combat**.

**Coût :**
- Jauge de Synergy.

**Casters :**
- Une Synergy nécessite **plusieurs personnages**.
- Le nombre exact de casters requis dépend de la Synergy.

**Philosophie :**
> « Comment mes personnages peuvent-ils coordonner leurs actions pour obtenir un avantage collectif ? »

### 2.4 Ultimate

Une **Ultimate** est une capacité exceptionnelle liée à un personnage.

Elle peut notamment :
- infliger de très gros dégâts ;
- appliquer un statut puissant ;
- créer un gros bouclier ;
- produire un effet particulièrement important ou spectaculaire.

Chaque personnage possède sa propre jauge d'Ultimate.

La jauge :
- appartient au personnage ;
- commence à **0 au début du combat** ;
- se remplit progressivement ;
- est difficile à remplir ;
- peut être alimentée par les Skills, les Spells et les Synergies ;
- est également alimentée par l'exploitation des faiblesses ennemies ;
- est consommée lors de l'utilisation de l'Ultimate.

L'Ultimate ne doit pas nécessairement être uniquement « l'attaque qui fait le plus de dégâts ». Elle doit surtout représenter une **opportunité exceptionnelle** que le joueur doit choisir de déclencher au moment pertinent.

**Coût :**
- Jauge d'Ultimate propre au personnage.

**Casters :**
- Par défaut, une Ultimate est lancée par **un personnage**.

**Philosophie :**
> « Est-ce que c'est le bon moment pour libérer le potentiel exceptionnel de ce personnage ? »

---

## 3. Les ressources

Le système distingue plusieurs ressources ayant des fonctions différentes.

### Mana
- Utilisé par les Spells.
- Difficile à restaurer.
- Peut également être utilisé hors combat.
- Son utilisation doit représenter un choix ayant potentiellement des conséquences à plus long terme.

### Jauge de Synergy
- Partagée par l'équipe.
- Commence à 0 au début du combat.
- Se construit principalement grâce aux actions réalisées pendant le combat.
- Est dépensée pour les Synergies.

### Jauge d'Ultimate
- Une jauge par personnage.
- Commence à 0 au début du combat.
- Se construit progressivement et difficilement.
- Est dépensée pour l'Ultimate du personnage.

Ces ressources ne doivent pas être de simples « barres de super attaque » interchangeables : chacune doit créer une décision différente.

---

## 4. Faiblesses et fenêtres d'exploitation

L'exploitation d'une faiblesse ennemie doit avoir une valeur supérieure à un simple bonus de dégâts.

Dans les règles actuelles, exploiter une faiblesse peut notamment :
- augmenter la valeur immédiate de l'action ;
- remplir davantage la jauge de Synergy ;
- remplir la jauge d'Ultimate.

Cela crée une boucle :

```text
Préparation
    ↓
Exploitation d'une faiblesse
    ↓
Gain de ressources
    ↓
Accès à des capacités puissantes
    ↓
Nouveau choix tactique
```

Les faiblesses peuvent donc encourager le joueur à préparer une fenêtre plutôt qu'à utiliser immédiatement sa meilleure capacité.

Les seuils, conditions et fenêtres exacts des boss restent à définir au cas par cas.

---

## 5. Statuts et interactions

Les statuts peuvent modifier la valeur des actions futures et doivent pouvoir créer des boucles d'exploitation.

### Exemple : Saignement

Effets envisagés :
- attaque réduite de `10 % × nombre de cumuls` ;
- à la fin de chaque tour, la cible subit des dégâts proportionnels au nombre de cumuls ;
- lorsqu'une attaque de type `Slash` touche une cible portant Saignement, le statut gagne un cumul.

Exemple de boucle :

```text
Déchaînement sanglant
    ↓
Saignement
    ↓
Coup d'épée (Slash)
    ↓
+1 cumul
    ↓
Coup d'épée (Slash)
    ↓
+1 cumul
    ↓
diminution de l'attaque + dégâts de fin de tour
```

L'intérêt recherché est qu'une attaque puisse devenir particulièrement intéressante **en raison de l'état actuel de la cible**, même si elle n'est pas la meilleure attaque en valeur brute.

---

## 6. Exemple de déroulé : Aldrick et Veya contre un boss

### Aldrick
- `Coup d'épée` — Skill
- `Amour impossible` — Synergy
- `Exécution` — Skill
- `Feu` — Spell
- `Déchaînement sanglant` — Ultimate
- `Soin` — Spell, soigne 33 % des PV max

### Veya
- `Décharge électrique` — Ultimate
- `Amour impossible` — Synergy
- `Foudre` — Spell, dégâts électriques moyens
- `Champ de force` — Skill
- `Changement modulaire` — Skill
- `Frappe` — Skill

### Amour impossible
- soigne les lanceurs de 25 % de leurs PV max ;
- inflige un gros coup d'épée de type Slash et Électrique.

### Boss
Scientifique robot, lié indirectement à l'histoire de Veya.

- À 70 % de PV, il entre en rage et gagne 50 % d'attaque.
- Sous 35 % de PV, il devient faible à l'Électrique.

### Déroulé

**Étape 1 — Construction**

Aldrick et Veya utilisent des Skills basiques afin d'infliger des dégâts et de charger les jauges de Synergy et d'Ultimate.

**Étape 2 — Préparation défensive**

Veya utilise `Changement modulaire` pour passer en mode défense, puis `Champ de force`.

L'équipe gagne +15 % de défense afin de se préparer à la prochaine menace.

**Étape 3 — Réponse à la rage**

Le boss atteint 70 % de PV et entre en rage.

Il lance une grosse attaque.

Aldrick et Veya survivent mais sont fortement blessés.

Aldrick utilise son Spell `Soin`.

Les personnages reprennent ensuite leurs Skills pour continuer à construire leurs ressources.

**Étape 4 — Conservation**

Les jauges de Synergy et d'Ultimate sont disponibles alors que le boss est autour de 40 %.

Les personnages n'utilisent pas immédiatement ces ressources.

Ils continuent leurs Skills afin de faire descendre le boss sous 35 %.

**Étape 5 — Fenêtre électrique**

Le boss passe sous 35 % et devient faible à l'Électrique.

Aldrick et Veya sont respectivement à environ 28 % et 34 % de PV.

Ils utilisent `Amour impossible`.

La Synergy soigne les deux personnages et inflige de gros dégâts en profitant de la faiblesse Électrique.

**Étape 6 — Finisher**

Le boss descend autour de 18 %.

Veya utilise son Ultimate `Décharge électrique`.

Le boss tombe autour de 3 %.

Aldrick utilise `Exécution` pour terminer le combat.

---

## 7. Exemple d'une autre boucle possible

Un autre combat peut fonctionner différemment :

```text
Skill de préparation
    ↓
débuff / création d'une fenêtre
    ↓
Spell exploitant cette fenêtre
    ↓
gain de Synergy et d'Ultimate
    ↓
menace du boss
    ↓
Synergy défensive
    ↓
survie
    ↓
conservation des ressources
    ↓
Ultimate au moment opportun
    ↓
finisher
```

L'objectif n'est pas que tous les combats suivent cette séquence.

Les règles doivent permettre à différents combats de produire des boucles différentes selon :
- les capacités des personnages ;
- les statuts ;
- les faiblesses ;
- les mécaniques du boss ;
- l'état de l'équipe ;
- les ressources disponibles ;
- les fenêtres d'opportunité.

---

## 8. Multi-caster : principe actuel

Le moteur peut conserver une représentation générique avec plusieurs casters.

Cependant, le game design actuel ne nécessite pas de rendre toutes les catégories multi-caster.

Principe retenu pour le moment :

```text
Skill    → 1 caster par défaut
Spell    → 1 caster par défaut
Synergy  → plusieurs casters
Ultimate → 1 caster par défaut
```

Le multi-caster pour un Skill ou un Spell pourra être ajouté plus tard **uniquement si une capacité concrète démontre qu'il crée une décision ou une possibilité de gameplay réellement intéressante**.

L'existence d'une possibilité technique ne constitue pas à elle seule une raison de l'ajouter au game design.

---

## 9. Principes de design à préserver

### 9.1 Une action doit pouvoir préparer la suivante

Le système doit favoriser les interactions :

```text
Action A
    ↓
modifie l'état du combat
    ↓
Action B devient plus intéressante
```

### 9.2 Une ressource doit créer une décision

Une jauge ou une réserve n'est intéressante que si sa dépense implique un arbitrage.

Exemples :
- Mana : « Est-ce que ça vaut le coût ? »
- Synergy : « Est-ce que je dépense mon potentiel collectif maintenant ? »
- Ultimate : « Est-ce le bon moment pour déclencher mon potentiel exceptionnel ? »

### 9.3 La puissance ne doit pas être uniquement numérique

Une capacité peut être puissante parce qu'elle :
- soigne ;
- protège ;
- prépare ;
- applique un statut ;
- exploite une faiblesse ;
- modifie une statistique ;
- crée une fenêtre ;
- permet une combinaison.

### 9.4 Les combinaisons doivent émerger du système

Il n'est pas nécessaire de créer une capacité nommée pour chaque combinaison possible.

Les interactions entre effets, statuts, valeurs, conditions, types de dégâts, faiblesses et ressources doivent permettre au joueur de découvrir des synergies de manière naturelle.

### 9.5 Le joueur doit pouvoir conserver une ressource disponible

Une jauge pleine ne doit pas obligatoirement signifier « il faut l'utiliser maintenant ».

Le joueur doit pouvoir choisir de conserver une Synergy ou une Ultimate afin d'attendre une meilleure fenêtre.

### 9.6 Les capacités doivent conserver une identité

Un Skill, un Spell, une Synergy et une Ultimate peuvent tous produire des dégâts ou des soins.

Ce qui les distingue principalement est :
- leur rôle dans la boucle ;
- leur économie de ressource ;
- leur portée collective ou individuelle ;
- leur fréquence d'utilisation ;
- la décision qu'ils demandent au joueur.

---

## 10. Points volontairement non définis

Cette version ne cherche pas encore à verrouiller :
- les PA et l'économie exacte des actions ;
- les valeurs finales de Mana ;
- la régénération ou récupération du Mana ;
- les coûts précis des Spells ;
- la vitesse de remplissage des jauges ;
- les valeurs de gain de jauge lors de l'exploitation des faiblesses ;
- les règles exactes de génération de Synergy et d'Ultimate ;
- les règles de calcul des dégâts ;
- l'ordre d'application des multiplicateurs ;
- la durée exacte des buffs, debuffs et statuts ;
- les résistances et immunités ;
- les règles détaillées des boss ;
- l'équilibrage des capacités ;
- les éventuelles exceptions permettant plusieurs casters pour un Skill ou un Spell.

---

## 11. Résumé

```text
SKILL
→ agir régulièrement
→ construire et exploiter

SPELL
→ dépenser une ressource rare
→ obtenir une puissance ou une utilité particulière

SYNERGY
→ coordonner plusieurs personnages
→ convertir une ressource collective en avantage

ULTIMATE
→ construire une opportunité individuelle
→ déclencher un moment exceptionnel
```

La philosophie générale recherchée est :

> **Le bon choix n'est pas toujours l'action la plus puissante maintenant, mais l'action qui crée la meilleure situation pour la suite.**
