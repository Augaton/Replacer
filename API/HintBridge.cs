using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Exiled.API.Features;
using MEC;

namespace Replacer.API
{
    public static class HintBridge
    {
        private static readonly Dictionary<string, object> ActiveHints = new Dictionary<string, object>();
        private static readonly Dictionary<string, CoroutineHandle> Timers = new Dictionary<string, CoroutineHandle>();

        private static bool serviceAvailable = true;
        private static bool warned;

        public static float YCoordinate { get; set; } = 600f;

        public static int FontSize { get; set; } = 20;

        public static void Show(Player player, string text, float duration)
        {
            if (player is null || !player.IsConnected || string.IsNullOrEmpty(text))
                return;

            if (duration <= 0f)
                duration = 3f;

            if (serviceAvailable)
            {
                try
                {
                    ShowThroughService(player, text, duration);
                    return;
                }
                catch (Exception e)
                {
                    serviceAvailable = false;
                    ActiveHints.Clear();

                    if (!warned)
                    {
                        warned = true;
                        Log.Warn($"HintServiceMeow indisponible, retour aux hints natifs : {e.Message}");
                    }
                }
            }

            player.ShowHint(text, duration);
        }

        public static void Remove(Player player)
        {
            if (player is null || string.IsNullOrEmpty(player.UserId))
                return;

            Drop(player.UserId);
        }

        public static void Clear()
        {
            List<string> userIds = new List<string>(ActiveHints.Keys);

            foreach (string userId in userIds)
                Drop(userId);

            ActiveHints.Clear();
            Timers.Clear();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ShowThroughService(Player player, string text, float duration)
        {
            string userId = player.UserId;

            if (!ActiveHints.TryGetValue(userId, out object stored) || !(stored is HintServiceMeow.Core.Models.Hints.Hint hint))
            {
                hint = new HintServiceMeow.Core.Models.Hints.Hint
                {
                    Id = "replacer",
                    YCoordinate = YCoordinate,
                    FontSize = FontSize,
                    Alignment = HintServiceMeow.Core.Enum.HintAlignment.Center,
                    YCoordinateAlign = HintServiceMeow.Core.Enum.HintVerticalAlign.Bottom,
                };

                ActiveHints[userId] = hint;
                HintServiceMeow.Core.Utilities.PlayerDisplay.Get(player).AddHint(hint);
            }

            hint.Text = text;

            KillTimer(userId);
            Timers[userId] = Timing.CallDelayed(duration, () => Drop(userId));
        }

        private static void Drop(string userId)
        {
            KillTimer(userId);

            if (!ActiveHints.TryGetValue(userId, out object stored))
                return;

            ActiveHints.Remove(userId);

            if (!serviceAvailable)
                return;

            try
            {
                DropThroughService(userId, stored);
            }
            catch (Exception e)
            {
                Log.Debug($"HintBridge.Drop: {e.Message}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void DropThroughService(string userId, object stored)
        {
            if (!(stored is HintServiceMeow.Core.Models.Hints.Hint hint))
                return;

            Player player = Player.Get(userId);

            if (player is null || !player.IsConnected)
                return;

            HintServiceMeow.Core.Utilities.PlayerDisplay.Get(player).RemoveHint(hint);
        }

        private static void KillTimer(string userId)
        {
            if (!Timers.TryGetValue(userId, out CoroutineHandle handle))
                return;

            Timing.KillCoroutines(handle);
            Timers.Remove(userId);
        }
    }
}
