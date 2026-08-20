# Replacer 2.0


> Portage EXILED 9.14.2 d'un plugin de **DGvagabond**. Depot non affilie a
> l'auteur d'origine. Voir [NOTICE.md](NOTICE.md) pour l'attribution.

Remplace un joueur qui quitte la partie par un spectateur.

**EXILED 9.14.2** — `dotnet build -c Release Replacer.csproj`

## Fonctionnement

Quand un joueur vivant se deconnecte, le plugin capture un instantane de son etat
(role, position, orientation, vie, inventaire, munitions, donnees SCP-079) puis
attribue ce role a un spectateur eligible.

## Selection du remplacant

Un spectateur est eligible s'il est verifie, hors Overwatch, et hors cooldown.
Avec `use_fair_queue: true` (defaut), c'est le spectateur qui attend depuis le
plus longtemps qui est choisi, au lieu d'un tirage aleatoire. Le cooldown
`per_player_cooldown_seconds` evite qu'un meme joueur enchaine les remplacements.

## Configuration

| Cle | Defaut | Role |
|---|---|---|
| `replacement_delay` | `0.4` | Delai avant application de l'etat, laisse le spawn se terminer |
| `max_round_seconds` | `0` | Fenetre de remplacement apres le debut du round. `0` = illimite |
| `minimum_players` | `4` | Nombre de joueurs connectes requis |
| `replaceable_roles` | `[]` | Roles remplacables. Vide = tous les roles vivants |
| `ignored_roles` | Tutorial, Overwatch, Filmmaker, SCP-049-2 | Roles jamais remplaces |
| `transfer_position` / `transfer_health` / `transfer_inventory` | `true` | Elements transferes |
| `transfer_effects` | `false` | Transfert des effets actifs |
| `use_fair_queue` | `true` | File d'attente equitable |
| `per_player_cooldown_seconds` | `120` | Anti-repetition. `0` = desactive |
| `announce_to_server` | `false` | Broadcast serveur en plus du hint |

Les textes joueur sont dans le fichier de traduction.

## Dependances

Ce plugin depend de **AugatonLib**, la bibliotheque partagee de la
collection.

| Fichier | Destination |
|---|---|
| `Replacer.dll` | `Plugins/7777/` |
| `AugatonLib.dll` | `Plugins/dependencies/` |
| HintServiceMeow | `Plugins/7777/` |

`AugatonLib.dll` ne va **jamais** dans `Plugins/7777/` : EXILED
tenterait de le charger comme plugin. Il doit etre deploye avant ce plugin et
mis a jour en meme temps.

Pour compiler ce depot isolement, cloner
[AugatonLib](https://github.com/Augaton/AugatonLib) a cote,
ou passer `-p:CommonProject=chemin/vers/AugatonLib.csproj`.

## Commandes staff

| Commande | Permission | Effet |
|---|---|---|
| `replacer status` | `replacer.manage` | File d'attente, cooldown et transferts actifs |

Alias `rep`.

Toutes les commandes de la collection partagent le meme socle : verification de
permission en premiere ligne, arguments bornes en longueur, exceptions
capturees, actions a impact tracees avec l'auteur. Une commande parente sans
argument liste ses sous-commandes.

## Installation depuis une release

Chaque tag `v*` declenche une release qui publie une archive **contenant deja
AugatonLib**. Extraire `Replacer.zip` dans `.config/EXILED/` :

```
Plugins/7777/Replacer.dll
Plugins/dependencies/AugatonLib.dll
```

Les DLL sont aussi publiees separement pour une mise a jour ciblee.

Si plusieurs plugins de la collection sont installes, garder la version
d'AugatonLib la plus recente : elle est partagee par tous.

## Integration continue

| Workflow | Declencheur | Role |
|---|---|---|
| `build` | push sur `main`, pull request | Compile le plugin contre AugatonLib et verifie la sortie |
| `release` | tag `v*` | Compile, empaquette avec AugatonLib et publie la release |

La CI recupere AugatonLib par `actions/checkout` sur le depot
[Augaton/AugatonLib](https://github.com/Augaton/AugatonLib), branche `main` par
defaut. Le declenchement manuel de `release` permet de fixer une autre version
via l'entree `augatonlib_ref`.

Gitleaks scanne l'historique complet a chaque push et bloque en cas de secret
detecte. Il est invoque en binaire plutot que via son action GitHub : l'action
calcule une plage de commits `<precedent>^..<actuel>` qui echoue sur le commit
initial d'un depot.
