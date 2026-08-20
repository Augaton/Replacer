using System.Collections.Generic;
using Exiled.API.Features;
using PlayerRoles;

namespace Replacer.API
{
    public sealed class SpectatorQueue
    {
        private readonly Dictionary<string, float> spectatingSince = new Dictionary<string, float>();
        private readonly Dictionary<string, float> lastReplacement = new Dictionary<string, float>();
        private readonly List<Player> candidates = new List<Player>(32);

        public void MarkSpectating(Player player)
        {
            if (player is null || string.IsNullOrEmpty(player.UserId))
                return;

            if (!spectatingSince.ContainsKey(player.UserId))
                spectatingSince[player.UserId] = UnityEngine.Time.realtimeSinceStartup;
        }

        public void MarkAlive(Player player)
        {
            if (player is null || string.IsNullOrEmpty(player.UserId))
                return;

            spectatingSince.Remove(player.UserId);
        }

        public void MarkReplaced(Player player)
        {
            if (player is null || string.IsNullOrEmpty(player.UserId))
                return;

            lastReplacement[player.UserId] = UnityEngine.Time.realtimeSinceStartup;
            spectatingSince.Remove(player.UserId);
        }

        public void Remove(Player player)
        {
            if (player is null || string.IsNullOrEmpty(player.UserId))
                return;

            spectatingSince.Remove(player.UserId);
            lastReplacement.Remove(player.UserId);
        }

        public void Clear()
        {
            spectatingSince.Clear();
            lastReplacement.Clear();
            candidates.Clear();
        }

        public Player Pick(Player leaver, Config config)
        {
            candidates.Clear();

            float now = UnityEngine.Time.realtimeSinceStartup;

            foreach (Player player in Player.List)
            {
                if (player is null || player == leaver)
                    continue;

                if (!player.IsVerified || string.IsNullOrEmpty(player.UserId))
                    continue;

                if (player.Role.Type != RoleTypeId.Spectator)
                    continue;

                if (player.IsOverwatchEnabled)
                    continue;

                if (config.PerPlayerCooldownSeconds > 0f
                    && lastReplacement.TryGetValue(player.UserId, out float last)
                    && now - last < config.PerPlayerCooldownSeconds)
                {
                    continue;
                }

                candidates.Add(player);
            }

            if (candidates.Count == 0)
                return null;

            if (!config.UseFairQueue)
                return candidates[UnityEngine.Random.Range(0, candidates.Count)];

            Player best = candidates[0];
            float bestSince = GetSpectatingSince(best, now);

            for (int i = 1; i < candidates.Count; i++)
            {
                float since = GetSpectatingSince(candidates[i], now);

                if (since >= bestSince)
                    continue;

                best = candidates[i];
                bestSince = since;
            }

            return best;
        }

        private float GetSpectatingSince(Player player, float fallback)
        {
            return spectatingSince.TryGetValue(player.UserId, out float since) ? since : fallback;
        }
    }
}
