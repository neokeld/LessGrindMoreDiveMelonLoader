# Less Grind More Dive Melon Loader

Mod MelonLoader pour **Dave the Diver**. C'est le premier mod MelonLoader pour Dave the Diver à ma connaissance !

Il permet de :
- supprimer l'augmentation du coût des améliorations de recettes
- ajouter un bonus de 5 pièges à crabes et 5 drones de récupération

Dans le style de ce mod : https://www.nexusmods.com/davethediver/mods/19

## Fonctionnement

Le jeu calcule normalement le nombre d'ingrédients nécessaires pour améliorer une recette en fonction de son niveau.

Ce mod intercepte ce calcul et force un coût fixe basé sur la quantité d'ingrédients d'origine de la recette.

Dans l'idée :

| Niveau | Jeu vanilla | Avec le mod |
|----------|----------|----------|
| 1 → 2 | 1 | 1 |
| 2 → 3 | 2 | 1 |
| 3 → 4 | 4 | 1 |
| 4 → 5 | 8 | 1 |
| 5 → 6 | 16 | 1 |

# Pour les joueurs

## Prérequis

- Dave the Diver

J'ai testé uniquement avec Steam sous Linux.

## Backup des saves

Sous Linux avec Steam, il faut backup <SteamLibrary-folder>/steamapps/compatdata/1868140/pfx/

Pour plus de détails, voir https://www.pcgamingwiki.com/wiki/Dave_the_Diver

## Installation

### 1. Installer MelonLoader

Il existe un guide très complet ici : https://github.com/TLD-Mods/Linux_Proton_Guide

En résumé :

Téléchargez la version Linux de MelonLoader :

https://melonwiki.xyz/#/?id=automated-installation

Donnez les droits d'exécution au MelonLoader.Installer.Linux et lancez le.

Il détecte automatiquement les jeux Unity même s'ils tournent dans Proton donc il devrait lister Dave The Diver.

Il faut aussi ajouter les paramètres de lancement Steam qui sont fournis par MelonLoader : WINEDLLOVERRIDES="version=n,b" %command%

### 2. Premier lancement

Lancez le jeu une première fois.

MelonLoader créera automatiquement plusieurs dossiers :

```text
Dave The Diver/
├── MelonLoader/
├── Mods/
└── UserData/
```

### 3. Installer le mod

Copiez :

```text
LessGrindMoreDiveMelonLoader.dll
```

dans :

```text
Dave The Diver/Mods/
```

### 4. Vérification

Au démarrage, ouvrez :

```text
MelonLoader/Latest.log
```

Vous devriez voir :

```text
[Fixed Recipe Cost] Fixed Recipe Cost loaded
```

Vous trouverez aussi un fichier de configuration dans UserData/MelonPreferences.cfg

Vous pouvez changer le nombre de drones de récupération et de pièges en crabes en bonus et activer ou désactiver la fixation des prix des recettes.

### Désinstallation

Supprimez simplement :

```text
Dave The Diver/Mods/LessGrindMoreDiveMelonLoader.dll
```

# Pour les développeurs

Le but de cette section est de servir de tuto pour créer des mods MelonLoader. N'hésitez pas à fork et créer des PR !

## Prérequis

Installez .NET depuis https://dotnet.microsoft.com/fr-fr/

Tout ce qui suit a été fait sous Windows.

## Création du projet

```bash
$env:DOTNET_CLI_TELEMETRY_OPTOUT=1
dotnet new classlib -n LessGrindMoreDiveMelonLoader
cd LessGrindMoreDiveMelonLoader
```

### Fichier `.csproj`

```xml
    <TargetFramework>net472</TargetFramework>
```

## Dépendances

```bash
dotnet add package HarmonyX
```

Récupérez MelonLoader.dll dans MelonLoader.x64.zip dans la release de MelonLoader.

Voir https://github.com/LavaGang/MelonLoader/releases

Ajouter

```text
MelonLoader.dll
```

Dans :

```text
Libs/
```

```xml
    <Reference Include="MelonLoader">
      <HintPath>.\Libs\MelonLoader.dll</HintPath>
    </Reference>
```

## Compilation

```bash
dotnet build -c Release
```

Des DLL seront générés dans :

```text
bin/Release/net472/
```

Il n'y a que LessGrindMoreDiveMelonLoader.dll qui nous intéresse.

## Architecture du mod

Le mod cible actuellement :

```csharp
GameFormulaManager.RequiredIngredientsCount(
    int level,
    int originCount)
```

La méthode qui est responsable du calcul du nombre d'ingrédients requis pour les améliorations de recettes.

Le patch Harmony remplace le calcul du coût par :

```csharp
originCount
```

afin de conserver un coût fixe pour tous les niveaux d'amélioration.

## Débogage

Les logs MelonLoader sont disponibles dans :

```text
MelonLoader/Latest.log
```

Pour vérifier les patches Harmony :

```text
MelonLoader/Logs/
```

## Reverse Engineering

Les travaux actuels reposent sur les classes identifiées dans le dump IL2CPP :

```text
GameFormulaManager
CookStudyFormula
CookStudyFormulaData
FormulaContext.CookStudy
```

Pour obtenir un dump, il faut récupérer GameAssembly.dll et global-metadata.dat dans les fichiers du jeu.

On peut utiliser Il2CppDumper qui est opensource : https://github.com/Perfare/Il2CppDumper

Il existe aussi une version web Il2CppDumper en ligne ici https://il2cppdumper.com/

Le fichier produit le plus intéressant est dump.cs

Il contient les définitions des classes et méthodes à patcher.

Pour l'ajout des pièges à crabes et des drones, je me suis basé sur :

```text
PlayerCharacter.AvailableCrabTrapCount
PlayerCharacter.AvailableLiftDroneCount
```

Et je patche leur valeur juste après l'Init de PlayerCharacter.
