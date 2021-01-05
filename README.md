# AtlasUserAPI

<p>
  <img alt="Version" src="https://img.shields.io/badge/version-DEPRECATED-critical.svg?style=for-the-badge" />
  <img alt="Maintenance" src="https://img.shields.io/badge/maintained-no-critical.svg?style=for-the-badge" />
</p>

## Avertissement
Je ne vous recommande pas d'utiliser cette API avec les versions actuelles du jeu.  
L'API  REST  commence à se faire vieille, et les  plugins  ne sont absolument plus compatibles.  
À utiliser en connaissance de cause.

##

AtlasUserAPI est une API pour les serveurs de jeux SCP:SL en JS.
Voici les principales fonctionnalités:

  - Stocker les informations sur vos joueurs (date de première connexion, dernière connexion, adresse IP, rôle) sur base de donnée
  - Stocker la liste de vos bannissements sur base de donnée
  - Utilisation de MongoDB pour de meilleures performances
  - Synchronisez autant de serveurs que vous voulez (bans et joueurs)
  - Implémentez facilement un bot Discord pour bannir et consulter les sanctions d'un joueur via l'API
  - Implémentez facilement une interface WEB pour contrôler les sanctions et les joueurs via l'API

### Informations importantes

AtlasUserAPI n'est pas maintenu. Si vous désirez soumettre un pull request, libre à vous de le faire.
**AtlasUserAPI n'est pas compatible avec les versions d'EXILED supérieurs à 1.10**.

### Installation

AtlasUserAPI a besoin de [Node.js](https://nodejs.org/) pour fonctionner.
Installez les dépendances afin d'initialiser le serveur.

```sh
$ cd AUAPI Serveur
$ npm install
$ node app
```

N'oubliez pas de configurer le serveur en copiant le fichier default.yaml.

Licence
----
Apache License 2.0

Créé par [Antt0n](https://t.me/Antt0n).
