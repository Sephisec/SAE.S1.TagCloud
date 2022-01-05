
# SAE S1: Tag Cloud

## Introduction

Analyse des textes littéraires à la recherche de mots-clefs.

Exporte les résultats sous la forme d'une page Web contenant des nuages de mots.
## Génération de la page avec les résultats

1- Placer les textes à analyser dans le dossier ./txt

2- Le nom du fichier doit être le titre du livre: filename.txt

3- Lancer l'analyse et la génération pour ces textes:

```bash
  mono tagCloud.exe
```

Entrer le nom de l'auteur dans la console lorsque le programme le demande, selon la manière suivante:

```bash
 Prénom Nom
```

Les résultats se trouvent sur la page results.html du site.
## Historique des versions

Les fichiers **versionX.exe** affichent les résultats dans la console.

Les codes sources se trouvent dans **versionX.cs**.
# Arborescence

*./asset*   : contient les fichiers de terminaisons à vérifier (3 fichiers) et des mots à ignorer dans l'affichage.

*./txt*     : répertoire dans lequel placer les textes à analyser

*./organigramme*    : contient les organigrammes de chaque fonction pour chaque version

*./css* et *./img* :    contiennent les fichiers CSS et images nécessaires au site
