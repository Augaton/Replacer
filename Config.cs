using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;
using PlayerRoles;

namespace Replacer
{
    public sealed class Config : IConfig
    {
        [Description("Active ou desactive le plugin.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Active les logs de debug.")]
        public bool Debug { get; set; } = false;

        [Description("Delai en secondes avant d'appliquer le role au remplacant.")]
        public float ReplacementDelay { get; set; } = 0.4f;

        [Description("Temps maximum apres le debut du round, en secondes, pendant lequel un joueur peut etre remplace. 0 = sans limite.")]
        public float MaxRoundSeconds { get; set; } = 0f;

        [Description("Nombre minimum de joueurs connectes pour qu'un remplacement soit tente.")]
        public int MinimumPlayers { get; set; } = 4;

        [Description("Roles pouvant etre remplaces. Liste vide = tous les roles vivants.")]
        public List<RoleTypeId> ReplaceableRoles { get; set; } = new List<RoleTypeId>();

        [Description("Roles ne pouvant jamais etre remplaces.")]
        public List<RoleTypeId> IgnoredRoles { get; set; } = new List<RoleTypeId>
        {
            RoleTypeId.Tutorial,
            RoleTypeId.Overwatch,
            RoleTypeId.Filmmaker,
            RoleTypeId.Scp0492,
        };

        [Description("Transfere la position du joueur parti au remplacant.")]
        public bool TransferPosition { get; set; } = true;

        [Description("Transfere les points de vie du joueur parti au remplacant.")]
        public bool TransferHealth { get; set; } = true;

        [Description("Transfere l'inventaire et les munitions du joueur parti au remplacant.")]
        public bool TransferInventory { get; set; } = true;

        [Description("Transfere les effets actifs du joueur parti au remplacant.")]
        public bool TransferEffects { get; set; } = false;

        [Description("Selectionne le spectateur qui attend depuis le plus longtemps au lieu d'un spectateur aleatoire.")]
        public bool UseFairQueue { get; set; } = true;

        [Description("Duree en secondes pendant laquelle un joueur deja remplacant ne peut pas etre repioche. 0 = desactive.")]
        public float PerPlayerCooldownSeconds { get; set; } = 120f;

        [Description("Annonce le remplacement a tout le serveur en plus du remplacant.")]
        public bool AnnounceToServer { get; set; } = false;

        [Description("Duree du hint affiche au remplacant, en secondes.")]
        public float HintDuration { get; set; } = 6f;


        [Description("Position verticale du hint de ce plugin dans HintServiceMeow. Doit differer des autres plugins.")]
        public float HintYCoordinate { get; set; } = 600f;

        [Description("Taille de police du hint de ce plugin.")]
        public int HintFontSize { get; set; } = 20;

        [Description("Duree du broadcast serveur, en secondes.")]
        public ushort BroadcastDuration { get; set; } = 5;
    }
}
