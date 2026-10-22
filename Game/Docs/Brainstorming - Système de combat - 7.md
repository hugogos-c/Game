# Architecture des actions — Initiator et Participants

> Document de conception — réflexion en cours

## 1. Contexte

Le système de combat permet au joueur de choisir une action à partir d'un personnage.

La logique de sélection visée est :

```text
Choisir un personnage
        ↓
Choisir son action
        ↓
Skill / Spell / Synergy / Ultimate / Inventaire / ...
```

Le joueur ne choisit donc pas d'abord une Ability abstraite puis un ou plusieurs personnages capables de l'exécuter.

Le personnage sélectionné est naturellement celui qui **initie l'action**.

Cette philosophie doit être prise en compte dans l'architecture du moteur de combat.

---

## 2. Problème initial : `Casters`

L'architecture initiale utilisait :

```csharp
IReadOnlyList<Character> Casters
```

à la fois dans `QueuedAction` et dans `AbilityContext`.

Cette approche avait plusieurs avantages :

- elle permettait naturellement plusieurs personnages impliqués dans une action ;
- elle convenait particulièrement bien aux Synergies ;
- elle évitait de créer artificiellement une distinction entre les différents types d'Ability ;
- elle correspondait au fait que certaines capacités peuvent être exécutées par plusieurs personnages.

Exemple :

```text
Aldrick + Veya
→ Amour impossible
```

Cependant, une liste de `Casters` ne permet pas d'exprimer clairement la différence entre :

- le personnage qui a choisi/lancé l'action ;
- les personnages supplémentaires impliqués dans son exécution.

Le code avait donc tendance à utiliser implicitement :

```csharp
casters.First()
```

comme référence principale.

Cela revient à considérer implicitement le premier caster comme un personnage particulier.

---

## 3. Nouvelle distinction : Initiator / Participants

Le modèle envisagé est désormais :

```text
Initiator
    +
Participants
```

### Initiator

L'**Initiator** est le personnage qui :

- est sélectionné par le joueur ;
- choisit l'Ability ;
- déclenche l'action ;
- constitue la référence principale de l'action.

### Participants

Les **Participants** sont les autres personnages impliqués dans l'exécution de l'Ability.

Ils ne sont pas nécessairement équivalents à l'Initiator.

Une Ability peut donc être représentée par :

```text
Initiator    = Aldrick
Participants = [Veya]
```

plutôt que :

```text
Participants = [Aldrick, Veya]
```

L'Initiator n'est donc pas inclus dans la liste des Participants.

---

## 4. Pourquoi ne pas mettre l'Initiator dans `Participants` ?

Les deux modèles sont techniquement possibles :

### Modèle A

```text
Participants = [Aldrick, Veya]
```

avec une convention implicite :

```text
Participants[0] = Initiator
```

### Modèle B

```text
Initiator    = Aldrick
Participants = [Veya]
```

Le modèle B semble mieux correspondre au game design.

Il permet de représenter explicitement :

```text
Aldrick
→ initie l'action

Veya
→ participe à l'action
```

Cela évite de donner une signification cachée à la position d'un personnage dans une liste.

Avec le modèle A, le moteur doit constamment supposer que le premier élément possède un statut particulier :

```csharp
participants.First()
```

Avec le modèle B, cette information est directement exprimée par le modèle :

```csharp
context.Initiator
```

---

## 5. Relation avec le choix du joueur

Cette distinction correspond directement à l'interface de combat.

Le joueur effectue :

```text
Aldrick
    ↓
Amour impossible
```

Le moteur crée alors :

```text
Initiator    = Aldrick
Participants = [Veya]
```

Si le joueur choisit Veya :

```text
Veya
    ↓
Amour impossible
```

le moteur crée :

```text
Initiator    = Veya
Participants = [Aldrick]
```

Ainsi, la même Ability peut être disponible dans le set de capacités de plusieurs personnages tout en ayant un Initiator différent selon le choix du joueur.

---

## 6. Exemple : `Amour impossible`

Aldrick et Veya possèdent tous les deux :

```text
Amour impossible
```

Le joueur peut donc choisir :

```text
Aldrick
→ Amour impossible
```

ou :

```text
Veya
→ Amour impossible
```

Dans le premier cas :

```text
Initiator    = Aldrick
Participants = [Veya]
```

Dans le second :

```text
Initiator    = Veya
Participants = [Aldrick]
```

Il s'agit de la même définition d'Ability, mais de deux exécutions différentes.

Cela permet notamment à l'Ability de différencier :

```text
context.Initiator
```

et :

```text
context.Participants
```

---

## 7. Conséquence importante : le multi-caster n'est plus nécessairement une catégorie

La question initiale était :

> Les Skills et Spells doivent-ils être multi-caster ?

Avec le modèle Initiator / Participants, cette question peut être reformulée.

Il n'est plus nécessaire de considérer :

```text
Skill multi-caster
Spell multi-caster
Ultimate multi-caster
```

comme des catégories particulières.

Le modèle général devient :

> Une Ability possède toujours un Initiator et peut éventuellement avoir des Participants.

La majorité des Ability peuvent simplement ne posséder aucun Participant.

Exemples :

```text
Skill
Initiator    = Aldrick
Participants = []
```

```text
Spell
Initiator    = Aldrick
Participants = []
```

```text
Ultimate
Initiator    = Veya
Participants = []
```

Une Ability exceptionnelle peut néanmoins avoir :

```text
Skill
Initiator    = Aldrick
Participants = [Veya]
```

ou :

```text
Spell
Initiator    = Aldrick
Participants = [Veya]
```

sans nécessiter une nouvelle catégorie d'Ability.

---

## 8. Rôle particulier des Synergies

Les Synergies restent naturellement les Ability les plus susceptibles d'avoir plusieurs Participants.

Exemple :

```text
Initiator    = Aldrick
Participants = [Veya]
```

ou :

```text
Initiator    = Veya
Participants = [Aldrick]
```

Le fait qu'une Synergy soit collective reste donc une propriété fondamentale de son design.

Cependant, le moteur ne doit pas nécessairement imposer que seules les Synergies puissent avoir des Participants.

Cela permet de conserver une certaine liberté de design pour quelques Ability exceptionnelles.

---

## 9. L'Initiator et la contribution des Participants

La séparation permet également de différencier la contribution des personnages.

Une Ability peut par exemple utiliser :

```text
100 % de la statistique de l'Initiator
+
20 % de la statistique d'un Participant
```

Exemple :

```text
Aldrick → Initiator
Veya    → Participant
```

Pourrait produire :

```text
Dégâts =
100 % ATK Aldrick
+
20 % ATK Veya
```

Une autre Ability pourrait utiliser davantage les Participants.

Par exemple :

```text
Dégâts =
100 % ATK Initiator
+
30 % ATK de chaque Participant
```

Ou encore :

```text
Initiator
→ fournit la puissance principale

Participants
→ ajoutent des propriétés, éléments, effets ou bonus
```

La contribution exacte dépendrait de l'Ability.

---

## 10. L'Initiator ne signifie pas nécessairement "personnage le plus important"

La distinction ne doit pas être interprétée comme :

```text
Initiator = important
Participant = secondaire
```

Elle représente avant tout une différence de rôle.

L'Initiator est le personnage qui déclenche l'action.

Un Participant peut néanmoins avoir une influence extrêmement importante sur le résultat.

Exemple :

```text
Initiator = Aldrick
Participant = Veya
```

Veya pourrait :

- modifier l'élément de l'Ability ;
- ajouter un effet ;
- fournir une statistique ;
- activer une condition ;
- ajouter un statut ;
- modifier un multiplicateur ;
- transformer une partie de l'Ability.

---

## 11. Vitesse des actions

Pour une Ability mono-personnage :

```text
Initiator    = Aldrick
Participants = []
```

la vitesse est naturellement :

```text
Speed = Speed d'Aldrick
```

Pour une Ability collective, notamment une Synergy, une piste envisagée est :

```text
Speed =
moyenne de la Speed de l'Initiator et des Participants
```

Exemple :

```text
Aldrick = 100 Speed
Veya    = 60 Speed

Synergy Speed =
(100 + 60) / 2
= 80
```

Pour trois personnages :

```text
Aldrick = 120
Veya    = 100
Kael    = 40

Synergy Speed =
(120 + 100 + 40) / 3
= 86,67
```

Cela crée potentiellement une décision intéressante :

> Ajouter un personnage à une action collective peut augmenter sa puissance ou ses possibilités, mais peut également ralentir l'action si ce personnage possède une Speed faible.

Cette règle reste à tester et à équilibrer.

Elle ne doit pas être considérée comme définitivement verrouillée tant que son intérêt gameplay n'a pas été validé.

---

## 12. Pourquoi la moyenne peut être intéressante pour une Synergy

Utiliser le `Max` des participants signifierait :

```text
Aldrick = 120
Veya    = 60

Synergy = 120 Speed
```

Le personnage le plus rapide déterminerait entièrement la vitesse de l'action.

Avec une moyenne :

```text
Synergy = 90 Speed
```

La vitesse représente davantage la capacité du groupe à agir ensemble.

Cela renforce également la notion de coordination :

> Une action collective doit potentiellement tenir compte de la capacité de l'ensemble du groupe à l'exécuter.

Cette mécanique devra toutefois être testée en jeu avant d'être considérée comme définitive.

---

## 13. Conséquence pour `QueuedAction`

Le modèle envisagé devient :

```csharp
public class QueuedAction
{
    public required Character Initiator { get; init; }
    public required IReadOnlyList<Character> Participants { get; init; }
    public required IReadOnlyList<Character> Targets { get; init; }
    public required IAbility Ability { get; init; }
}
```

La notion de `Casters` disparaît progressivement au profit d'une distinction plus explicite.

---

## 14. Conséquence pour `AbilityContext`

Le contexte d'exécution pourrait devenir :

```csharp
public class AbilityContext
{
    public required Character Initiator { get; init; }
    public required IReadOnlyList<Character> Participants { get; init; }
    public required IReadOnlyList<Character> Targets { get; init; }
    public required Battle Battle { get; init; }

    public bool IsAborted { get; set; }
    public List<IEffectResult> Results { get; } = new();
    public Dictionary<string, object> CustomData { get; } = new();
}
```

Les Sequences et Effects peuvent alors explicitement demander :

```csharp
context.Initiator
```

lorsqu'ils ont besoin du personnage principal de l'action.

Et :

```csharp
context.Participants
```

lorsqu'ils ont besoin des personnages supplémentaires impliqués.

---

## 15. Philosophie générale qui se dégage

Le modèle peut être résumé ainsi :

```text
Le joueur choisit un personnage
        ↓
Le personnage devient Initiator
        ↓
Le joueur choisit une Ability
        ↓
L'Ability détermine les Participants
        ↓
L'action est exécutée
        ↓
Le résultat dépend :
    - de l'Initiator
    - des Participants
    - des cibles
    - de l'état du combat
    - des ressources
    - des statuts
    - des faiblesses
```

Cela correspond bien à la philosophie générale du système :

> Le joueur choisit une action en fonction de la situation actuelle, et cette action peut avoir une valeur différente selon le personnage qui l'initie et les personnages qui y participent.

---

## 16. Règle de design proposée

Une formulation possible des règles fondamentales serait :

> **Toute Ability est initiée par un personnage.**
>
> **L'Initiator est le personnage sélectionné par le joueur pour effectuer l'action.**
>
> **Une Ability peut éventuellement impliquer un ou plusieurs Participants supplémentaires.**
>
> **Les Participants ne sont pas inclus dans la liste de l'Initiator.**
>
> **La majorité des Skills, Spells et Ultimates n'ont aucun Participant.**
>
> **Les Synergies utilisent généralement plusieurs Participants, mais le moteur n'impose pas que les Participants soient exclusifs aux Synergies.**

---

## 17. Point encore à définir : comment sont déterminés les Participants ?

C'est probablement l'une des prochaines questions importantes à résoudre.

Plusieurs modèles sont possibles.

### Participants imposés

Exemple :

```text
Amour impossible
→ Initiator + partenaire précis
```

### Participants choisis par le joueur

Exemple :

```text
Aldrick
→ Ability
→ choisir Veya et/ou Kael
```

### Participants déterminés automatiquement

Exemple :

```text
Ability
→ recherche automatiquement les alliés capables de participer
```

### Modèle hybride

Certaines Ability pourraient imposer les participants tandis que d'autres permettraient au joueur de les choisir.

Cette question doit être définie séparément du concept d'Initiator / Participants.

---

## 18. Conclusion actuelle

Le modèle `Initiator + Participants` semble mieux correspondre à la direction actuelle du jeu que le modèle générique :

```text
Casters[]
```

La raison principale n'est pas simplement architecturale.

Elle vient du fonctionnement voulu de l'interface de combat :

```text
Personnage
    ↓
Action
```

Le personnage sélectionné doit avoir un statut explicite dans l'exécution de l'action.

La distinction permet également :

- de conserver la possibilité d'actions collectives ;
- de laisser quelques Skills/Spells/Ultimates avoir des Participants si le game design le justifie ;
- de donner aux Synergies une identité collective sans créer une architecture spéciale uniquement pour elles ;
- de permettre à une même Ability d'être initiée par plusieurs personnages ;
- de faire varier le résultat d'une Ability selon son Initiator ;
- de faire contribuer les Participants de manière différente ;
- de remplacer la convention implicite `Casters.First()` par une sémantique explicite.

Le principe directeur devient donc :

```text
Initiator
→ celui qui entreprend l'action

Participants
→ ceux qui prennent part à l'action

Ability
→ définit comment leurs contributions sont utilisées
```

Ce modèle reste volontairement ouvert et devra être peaufiné avec les règles concrètes de sélection, de coût, de Speed, de contribution des Participants et de résolution des Synergies.
