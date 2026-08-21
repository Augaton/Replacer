using CommandSystem;
using AugatonLib.Commands;

namespace Replacer.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class ReplacerCommand : StaffParentCommand
    {
        public ReplacerCommand() => LoadGeneratedCommands();

        public override string Command => "replacer";

        public override string[] Aliases => new[] { "rep" };

        public override string Description => "Etat du systeme de remplacement des joueurs partis.";

        public override string Permission => "replacer.manage";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new CollectionCommand(Permission));

            RegisterCommand(new StatusCommand(
                "Replacer",
                typeof(Plugin), Permission, builder =>
            {
                Config config = Plugin.Instance.Config;
                builder.AppendLine($"  file equitable : {(config.UseFairQueue ? "activee" : "desactivee")}");
                builder.AppendLine($"  cooldown par joueur : {config.PerPlayerCooldownSeconds:0}s");
                builder.AppendLine($"  joueurs minimum : {config.MinimumPlayers}");
                builder.AppendLine($"  transferts : position {Yes(config.TransferPosition)}, vie {Yes(config.TransferHealth)}, inventaire {Yes(config.TransferInventory)}, effets {Yes(config.TransferEffects)}");
                return true;
            }));
        }

        private static string Yes(bool value) => value ? "oui" : "non";
    }
}
