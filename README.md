# ⏳ HurryUpDavid

Application mobile développée avec **.NET MAUI** permettant de limiter le temps de réflexion des joueurs lors de parties de jeux de société.

🎲 Fini les tours interminables.
⏱️ Chaque joueur dispose d'un temps limité pour jouer.
🔥 Des effets visuels et sonores signalent l'approche de la fin du tour.

---

# 🚀 Fonctionnalités

### 👥 Gestion des joueurs

* De 2 à 8 joueurs
* Couleur personnalisée pour chaque joueur
* Rotation automatique des tours

### ⏱️ Gestion du temps

* Temps par tour configurable
* Pause / reprise de partie
* Passage automatique au joueur suivant

### 🎨 Effets visuels

* Cercle dynamique représentant le joueur actif
* Glow progressif à l'approche de la fin du tour
* Pulsation du cercle dans les dernières secondes
* Changement de couleur selon le joueur actif

### 🔊 Ambiances sonores

* 🔔 Clochettes
* ❤️ Battements de cœur / respiration
* 🔇 Mode silencieux

---

# 🛠️ Technologies utilisées

| Technologie           | Utilisation                    |
| --------------------- | ------------------------------ |
| C#                    | Logique métier                 |
| .NET MAUI             | Application multiplateforme    |
| XAML                  | Interface utilisateur          |
| GraphicsView          | Dessin du cercle et animations |
| Plugin.Maui.Audio     | Gestion des sons               |
| CommunityToolkit.Maui | Composants MAUI                |

---

# 🏗️ Architecture

```text
HurryUpDavid
│
├── Models
│   └── GameSettings
│
├── Pages
│   ├── SetupPage
│   ├── ColorSelectionPage
│   └── GamePage
│
├── Resources
│   ├── Audios
│   └── Images
│
└── App
```

### 📦 Responsabilités principales

| Classe             | Rôle                          |
| ------------------ | ----------------------------- |
| GameSettings       | Paramètres de la partie       |
| SetupPage          | Configuration initiale        |
| ColorSelectionPage | Choix des couleurs            |
| GamePage           | Gestion du timer et des tours |
| CircleGameDrawable | Dessin et animations          |

---

# ⚙️ Défis techniques rencontrés

### ⏱️ Gestion du timer

Utilisation de :

* Task
* Stopwatch
* CancellationTokenSource

afin de gérer :

* les pauses
* les reprises
* les changements de joueur
* la synchronisation des animations

---

### 🎨 Dessin personnalisé

L'interface principale repose sur **GraphicsView**.

Le cercle est entièrement dessiné par code afin de gérer :

* les couleurs dynamiques
* les effets de glow
* les pulsations
* les animations de fin de tour

---

### 🔊 Gestion audio

Les alertes sont synchronisées avec les dernières secondes du timer.

Plusieurs ambiances sont disponibles :

* 🔔 Clochettes
* ❤️ Respiration / battements de cœur
* 🔇 Aucun son

---

# 📱 Installation

## Prérequis

* Visual Studio 2022
* Workload .NET MAUI
* .NET 10 SDK

## Cloner le projet

```bash
git clone https://github.com/fcaneh-jeux/HurryUpDavid.git
```

```bash
dotnet restore
```

Puis lancer le projet depuis Visual Studio.

---

# 📸 Captures d'écran

🚧 À venir

* Configuration de partie
* Sélection des couleurs
* Écran principal
* Animation de fin de tour

---

# 🎯 Compétences travaillées

Ce projet m'a permis de pratiquer :

* 📱 Développement mobile avec .NET MAUI
* 🎨 Interfaces XAML
* ⏱️ Gestion de tâches asynchrones
* 🔊 Intégration audio
* 🎮 Gestion d'état applicatif
* 🧩 Résolution de problèmes et débogage
* 🎨 Dessin personnalisé avec GraphicsView

---

# 🤖 Utilisation de l'IA

Ce projet a été réalisé avec l'assistance de ChatGPT et Mistral AI.

Les outils d'IA ont été utilisés comme support technique pour :

* comprendre certains concepts MAUI ;
* résoudre des bugs ;
* explorer différentes approches ;
* améliorer certaines portions du code.

L'architecture, les choix fonctionnels, les tests et les validations ont été réalisés par l'auteur.

Cette démarche reflète une pratique moderne du développement logiciel : savoir rechercher l'information, exploiter les outils disponibles et rester autonome dans la réalisation d'un projet.

---

# 📦 Publication Android

L'application est compilée et distribuée sous forme d'APK Android signé.

Le projet a nécessité :

- la configuration de l'environnement Android (.NET MAUI) ;
- la gestion des signatures APK ;
- les tests sur appareil physique ;
- la validation du comportement sur différentes résolutions d'écran.

L'application est actuellement testée sur Windows et Android.

---

# 🗺️ Roadmap

## Version actuelle

✅ Gestion des joueurs  
✅ Timer configurable  
✅ Ambiances sonores  
✅ Animations de transition  
✅ Publication Android

## Améliorations prévues

⬜ Icônes et identité visuelle personnalisées  
⬜ Vibrations en fin de tour  
⬜ Sauvegarde des préférences utilisateur  
⬜ Historique des parties  
⬜ Publication sur Play Store

---

# 👨‍💻 Auteur

**Fabien Canehan**

Projet personnel réalisé dans le cadre de ma montée en compétences sur l'écosystème .NET et le développement mobile multiplateforme.
