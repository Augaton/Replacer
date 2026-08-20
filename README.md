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
