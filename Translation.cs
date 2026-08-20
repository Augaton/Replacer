using Exiled.API.Interfaces;

namespace Replacer
{
    public sealed class Translation : ITranslation
    {
        public string ReplacedHint { get; set; } = "<color=yellow>Vous remplacez <b>{nickname}</b> en tant que {role}.</color>";

        public string ReplacedBroadcast { get; set; } = "<color=yellow>Un joueur a quitte la partie et a ete remplace.</color>";

        public string UnknownNickname { get; set; } = "un joueur deconnecte";
    }
}
