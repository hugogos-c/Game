# Projet jeu vidéo

## Présentation
Projet personnel de très long terme consacré au développement d'un RPG tour par tour.

Ce dépôt comprend notamment la logique des différents systèmes (combat, progression, quêtes, inventaire...), les règles et les éléments nécessaires à leur présentation en jeu.

Les données et ressources propres au jeu (personnages, capacités, objets, dialogues, histoires et autres assets) sont conservées dans un dépôt privé distinct.

Le développement des systèmes de combat vise à favoriser la stratégie, la préparation et les interactions entre capacités, plutôt que la seule recherche de dégâts maximaux.

Les actions doivent pouvoir modifier l'état du combat et créer des opportunités tactiques, notamment grâce aux synergies entre personnages, aux faiblesses ennemies, aux statuts et à la gestion des ressources.

L'univers, les systèmes de jeu, l'architecture et le contenu sont amenés à évoluer au fil du développement.

## État actuel
Le jeu est actuellement en développement. Une première version expérimentale fonctionne en console Windows.

## Technologies
- **Langage** : C#
- **Framework** : .NET
- **Moteur de jeu envisagé** : Unity
- **IDE** : Visual Studio et Visual Studio Code
- **Documentation** : Markdown

## Architecture
L'objectif est de concevoir une architecture aussi modulaire et extensible que possible.

Les principaux axes de réflexion sont :
- Séparer la logique du jeu, le contenu et la présentation.
- Privilégier une conception pilotée par les données (data-driven).
- Étudier la nécessité d'une couche d'orchestration entre les différents composants.

L'architecture est encore en réflexion et pourra évoluer au fil du développement.

## Lancer le projet

### Version Console

#### Prérequis
- .NET 10 SDK
- Git
- Visual Studio (si besoin)

#### Récupérer le projet
```bash
git clone https://github.com/hugogos-c/Game.git
```

#### Exécution
```bash
dotnet run --project Game/Game.csproj
```

### Version Unity
La version Unity n'est pas encore disponible.

## Contribution
Les modalités de contribution seront définies ultérieurement.

## Licence
À définir
