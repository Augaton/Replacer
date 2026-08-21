using System;
using Exiled.API.Features;
using Replacer.API;
using AugatonLib.Arbitration;
using AugatonLib.Runtime;
using Replacer.Handlers;
using PlayerEvents = Exiled.Events.Handlers.Player;
using ServerEvents = Exiled.Events.Handlers.Server;

namespace Replacer
{
    public sealed class Plugin : Plugin<Config, Translation>
    {
        public override string Name => "Replacer";

        public override string Author => "Zone-Shilari (base: DGvagabond)";

        public override string Prefix => "replacer";

        public override Version Version => new Version(2, 0, 0);

        public override Version RequiredExiledVersion => new Version(9, 14, 2);

        public const string DepartureOwner = "Replacer";

        public static Plugin Instance { get; private set; }

        private SpectatorQueue queue;
        private PlayerHandlers playerHandlers;

        public override void OnEnabled()
        {
            Instance = this;

            ValidateConfig();

            API.HintBridge.YCoordinate = Config.HintYCoordinate;
            API.HintBridge.FontSize = Config.HintFontSize;

            queue = new SpectatorQueue();
            playerHandlers = new PlayerHandlers(this, queue);

            PlayerEvents.Left += playerHandlers.OnLeft;
            PlayerEvents.ChangingRole += playerHandlers.OnChangingRole;

            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundEnded += OnRoundEnded;
            ServerEvents.RestartingRound += OnRestartingRound;

            DepartureArbiter.Declare(DepartureOwner, 50, player => player is not null);

            PluginDirectory.Register(
                this,
                Capability.Hints,
                Capability.Departure,
                Capability.Bus);

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            PlayerEvents.Left -= playerHandlers.OnLeft;
            PlayerEvents.ChangingRole -= playerHandlers.OnChangingRole;

            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundEnded -= OnRoundEnded;
            ServerEvents.RestartingRound -= OnRestartingRound;

            queue?.Clear();
            API.HintBridge.Clear();

            DepartureArbiter.Withdraw(DepartureOwner);
            PluginDirectory.Unregister(this);

            playerHandlers = null;
            queue = null;
            Instance = null;

            base.OnDisabled();
        }

        private void ValidateConfig()
        {
            if (Config.ReplacementDelay < 0f)
            {
                Log.Warn($"ReplacementDelay ({Config.ReplacementDelay}) est negatif, remis a 0.4.");
                Config.ReplacementDelay = 0.4f;
            }

            if (Config.MinimumPlayers < 1)
            {
                Log.Warn($"MinimumPlayers ({Config.MinimumPlayers}) est invalide, remis a 1.");
                Config.MinimumPlayers = 1;
            }

            if (Config.PerPlayerCooldownSeconds < 0f)
            {
                Log.Warn($"PerPlayerCooldownSeconds ({Config.PerPlayerCooldownSeconds}) est negatif, remis a 0.");
                Config.PerPlayerCooldownSeconds = 0f;
            }
        }

        private void OnRoundStarted()
        {
            queue.Clear();
            API.HintBridge.Clear();
        }

        private void OnRoundEnded(Exiled.Events.EventArgs.Server.RoundEndedEventArgs ev) => queue.Clear();

        private void OnRestartingRound() => queue.Clear();
    }
}
