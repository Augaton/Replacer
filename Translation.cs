using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Replacer
{
    public sealed class Translation : ITranslation
    {
        [Description("Hint affiche au remplacant. {nickname} est remplace par le pseudo du joueur parti, {role} par son role.")]
        public string ReplacedHint { get; set; } = "<color=yellow>Vous remplacez <b>{nickname}</b> en tant que {role}.</color>";

        [Description("Broadcast diffuse a tous lors d'un remplacement, si announce_to_server est actif.")]
        public string ReplacedBroadcast { get; set; } = "<color=yellow>Un joueur a quitte la partie et a ete remplace.</color>";

        [Description("Pseudo utilise quand celui du joueur parti est vide ou refuse par le filtre de texte.")]
        public string UnknownNickname { get; set; } = "un joueur deconnecte";
    }
}
