# ⏳ HurryUpDavid – Minuteur pour jeux de société

## 📌 Présentation

**HurryUpDavid** est un **minuteur personnalisable** conçu pour **limiter le temps de réflexion des joueurs** lors des parties de jeux de société.
Inspiré des minuteurs de compétition, il permet d'éviter que les joueurs ne prennent trop de temps pour leur tour, **accélérant ainsi le rythme des parties** !

✅ **Idéal pour** :
- Les jeux de stratégie (ex: *Puerto Rico*, *7 Wonders*)
- Les jeux de cartes (ex: *Uno*, *Dixit*)
- Les jeux de rôle (pour limiter les temps de réflexion)
- Toute situation où un **temps de jeu équitable** est nécessaire

---

---

## 🚀 Fonctionnalités

### ⏱️ Gestion du temps
- **Durée configurable** : Choisis le temps par tour (en secondes).
- **Timer visuel** : Affichage clair du temps restant.
- **Signal en fin de tour** :
  - **Animation visuelle** (pulsations du cercle + effets de glow).
  - **Signal sonore** (ambiances personnalisables).

### 👥 Multijoueur
- **2 à 8 joueurs** supportés.
- **Tour par tour** : Le minuteur passe automatiquement au joueur suivant.
- **Affichage du joueur actuel** (nom + couleur).

### 🔊 Ambiances sonores
- **Clochettes douces** : Son discret pour un rappel élégant.
- **Respiration + battements de cœur** : Ambiance immersive pour un effet "stressant".
- **Aucun son** : Mode silencieux (uniquement les animations visuelles).

### 🎨 Interface intuitive
- **Bouton Pause/Reprise** : Met en pause le timer à tout moment.
- **Bouton Quitter** : Retour au menu principal.
- **Design épuré** : Adapté aux écrans tactiles (mobile) et desktop.

---

---

## 📸 Aperçu
*(À ajouter avec des captures d'écran réelles)*
<!--
![SetupPage – Configuration](Screenshots/SetupPage.png)
*Choix du nombre de joueurs, durée et ambiance sonore.*

![GamePage – Timer en cours](Screenshots/GamePage.png)
*Affichage du temps restant et du joueur actuel.*

![GamePage – Fin de tour](Screenshots/GamePage_EndTurn.png)
*Animations et son à 10 secondes (puis fin de tour).*
-->

---

---

## 🛠️ Technologies utilisées
   **Technologie**       | **Rôle**                                  |
 |-----------------------|------------------------------------------|
 | **.NET MAUI**         | Framework multiplateforme (mobile + desktop) |
 | **C#**               | Langage de développement                 |
 | **Plugin.Maui.Audio** | Gestion des effets sonores               |
 | **MVVM Light**        | Architecture (séparation UI/Logique)     |

---

---

## 🧩 Architecture du projet

HurryUpDavid/
├── Models/
│   └── GameSettings.cs      # Paramètres (joueurs, durée, ambiance sonore)
│
├── Pages/
│   ├── SetupPage.xaml(.cs)  # Configuration initiale
│   ├── ColorSelectionPage.xaml(.cs) # Choix des couleurs des joueurs
│   └── GamePage.xaml(.cs)   # Logique du timer et des tours
│
├── Resources/
│   ├── Raw/Audios/          # Fichiers sonores (bells.wav, breathing.wav)
│   └── Images/              # Icônes et images
│
└── App.xaml(.cs)           # Point d'entrée

### Rôle des classes clés

| **Classe**               | **Responsabilité**                                  |
|--------------------------|----------------------------------------------------|
| `GameSettings`           | Stocke les paramètres (durée, joueurs, ambiance).  |
| `CircleGameDrawable`     | Gère le rendu du cercle (couleurs, glow, pulsations). |
| `GamePage`               | Logique du timer, tours, et déclenchement des sons. |
| `SetupPage`              | Interface de configuration avant la partie.       |

---

---

## 🎮 Comment utiliser HurryUpDavid

### 1️⃣ **Configurer la partie**
- **Nombre de joueurs** : 2 à 8.
- **Durée par tour** : En secondes (ex: 30, 60, 90...).
- **Ambiance sonore** : Clochettes, Respiration, ou Aucune.

### 2️⃣ **Choisir les couleurs**
- Chaque joueur sélectionne une couleur pour son tour.

### 3️⃣ **Démarrer le timer**
- Le timer démarre pour le **Joueur 1**.
- **Le temps défile** : Le cercle reste statique.

### 4️⃣ **Fin du tour**
- **À 10 secondes restantes** :
  - Le cercle **commence à pulser**.
  - Le **son sélectionné** se déclenche (une seule fois).
- **À 0 seconde** :
  - Le tour se termine automatiquement.
  - **Passe au joueur suivant** (clic sur le cercle ou attente automatique).

### 5️⃣ **Contrôles**
- **❚❚** : Met en pause le timer (le son aussi).
- **▶** : Reprend le timer (le son reprend si on est à ≤10s).
- **✕** : Quitte la partie et retourne au menu.

---
---

## 📥 Installation et exécution

### Prérequis
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- **Visual Studio 2022** (avec charge de travail *Développement mobile .NET MAUI*)
- Pour **Android** : Android SDK + émulateur (ou appareil physique)
- Pour **iOS** : Mac + Xcode

### Étapes
1. Cloner le dépôt :
   ```bash
   git clone https://github.com/fcaneh-jeux/HurryUpDavid.git 
2. Ouvrir dans Visual Studio.
3. Restaurer les packages NuGet :
   ```bash
    dotnet restore
4. Sélectionner la plateforme cible (Windows, Android, ou iOS).
Lancer le projet (F5).

📦 Export vers mobile
Android
1. Configurer un appareil :
Ouvrir Android Device Manager (dans Visual Studio).
Créer un appareil virtuel (ou connecter un téléphone en USB).

2. Builder en Release :

Sélectionner Release + Android.
Lancer le build (Ctrl+Shift+B).

3. Récupérer l'APK :

Chemin : HurryUpDavid\bin\Release\net10.0-android\publish\com.companyname.hurryupdavid-Signed.apk
Installer via adb ou en copiant le fichier sur l'appareil.

iOS

1. Ouvrir sur Mac avec Visual Studio for Mac ou Rider.
2. Sélectionner un appareil (simulateur ou iPhone/iPad).
3. Builder en Release (⌘+B).
4. Exporter via Xcode :
  Archiver le projet (Product > Archive).
  Distribuer via TestFlight ou App Store.

⚠️ Pour publier sur les stores :

Google Play : Compte développeur (25$ une fois) + signature de l'APK.
App Store : Compte Apple Developer (99$/an) + provisioning profiles.

⚠️ Note sur la réalisation
Ce projet a été développé avec l’accompagnement de Mistral AI (Vibe) et ChatGPT
Collaboration humain + IA :

🧠 Conception :
Idée originale, besoins et évolutions définis par moi.
Architecture et logique métier pilotées par moi.

💬 Support technique :
ChaptGpt/Mistral AI ont aidé à :

Résoudre des bugs (gestion du timer, synchronisation son/animations).
Optimiser le code (ex: CancellationToken, Plugin.Maui.Audio).
Comprendre des concepts (.NET MAUI, animations, gestion audio).

🧱 Code :
Mon travail : Implémentation des fonctionnalités, tests, intégration.
Contribution de l'IA : Corrections, suggestions d’amélioration, explications.

👉 En résumé : Un projet 100% fonctionnel où j’ai dirigé la conception, avec un support technique pour accélérer le développement.

📈 Objectifs atteints
✅ Maîtriser .NET MAUI (développement multiplateforme).
✅ Gérer un timer précis avec CancellationToken.
✅ Intégrer des animations fluides (pulsations, glow).
✅ Ajouter des effets sonores avec Plugin.Maui.Audio.
✅ Structurer un projet modulaire (séparation UI/Logique).
✅ Créer un outil utile pour les jeux de société.

🔄 Améliorations possibles
🔹 Mode "Tour libre" : Pas de limite de temps, mais un bouton pour passer manuellement.
🔹 Historique des tours : Afficher le temps restant pour chaque joueur.
🔹 Personnalisation des sons : Ajouter ses propres fichiers audio.
🔹 Thèmes visuels : Changer les couleurs et animations.
🔹 Export Web : Version jouable dans un navigateur (Blazor).
🔹 Mode "Défis" : Durée aléatoire à chaque tour.

📬 Auteur
Fabien Canehan
