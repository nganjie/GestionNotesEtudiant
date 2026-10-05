# Barème — Gestion des notes étudiants

Application mobile .NET MAUI de saisie des notes par évaluation et de calcul automatique des moyennes par unité d'enseignement (UE).

## Fonctionnalités

- Connexion, inscription (enseignant ou étudiant) avec validation des champs, mot de passe oublié
- Liste des unités d'enseignement avec leur statut (brouillon, en cours, publié) et la reprise de la saisie en cours
- Détail d'une UE : évaluations, coefficients, moyenne provisoire
- Création d'une évaluation (type, coefficient, barème, date)
- Saisie des notes par évaluation avec contrôle des valeurs (0 à 20, absence « ABS »)
- Moyennes pondérées par UE, étudiants validés / non validés
- Publication des notes aux étudiants
- Export PDF, CSV ou Excel (avec formules de moyenne), avec option d'anonymisation
- Liste des étudiants par promotion avec recherche et fiche étudiant
- États de chargement, d'erreur (hors connexion) et états vides

## Compte de démonstration

Au premier lancement, des données de démonstration sont créées (promotions GL4 et GL3, trois UE).

- E-mail : `m.etoundi@univ-exemple.cm`
- Mot de passe : `Bareme2026`

## Calcul des moyennes

Moyenne d'un étudiant = Σ (note ramenée sur 20 × coefficient) / Σ coefficients des évaluations notées.
Un étudiant absent à une évaluation n'a pas de moyenne. Une UE est validée à partir de 10/20.

## Architecture

```
GestionNotesEtudiant/
├── Models/        Utilisateur, Etudiant, UniteEnseignement, Evaluation
├── Services/      Stockage local JSON, authentification, calculs, exports PDF/CSV/XLSX
├── ViewModels/    Logique des écrans (MVVM, CommunityToolkit.Mvvm)
├── Views/         Écrans XAML
├── Controls/      Composants de la charte : champ de saisie, sélecteur segmenté, étiquette, en-tête
└── Resources/     Couleurs, styles, police Hanken Grotesk, icônes SVG
```

## Choix d'écoconception

- Données stockées localement dans un seul fichier JSON : aucun serveur ni appel réseau pour consulter ou saisir
- Listes paginées (20 éléments) et virtualisées
- Icônes vectorielles, aucune image matricielle ni animation (squelette de chargement statique)
- Exports générés sans bibliothèque externe, taille du fichier affichée avant l'export
- Notification des étudiants sans e-mail

## Lancer le projet

Prérequis : SDK .NET 10 et charge de travail MAUI (`dotnet workload install maui`).

```
dotnet build -t:Run -f net10.0-android
```
