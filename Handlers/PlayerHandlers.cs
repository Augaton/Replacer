using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using AugatonLib.Arbitration;
using AugatonLib.Bus;
using AugatonLib.Text;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using Replacer.API;
using CustomPlayerEffects;

namespace Replacer.Handlers
{
    public sealed class PlayerHandlers
    {
        private readonly Plugin plugin;
        private readonly SpectatorQueue queue;
        private readonly List<CoroutineHandle> pendingSnapshots = new List<CoroutineHandle>(4);

        public PlayerHandlers(Plugin plugin, SpectatorQueue queue)
        {
            this.plugin = plugin;
            this.queue = queue;
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            try
            {
                if (ev?.Player is null)
                    return;

                if (ev.NewRole == RoleTypeId.Spectator)
                    queue.MarkSpectating(ev.Player);
                else
                    queue.MarkAlive(ev.Player);
            }
            catch (Exception e)
            {
                Log.Error($"OnChangingRole: {e}");
            }
        }

        public void OnLeft(LeftEventArgs ev)
        {
            try
            {
                if (ev?.Player is null)
                    return;

                Player leaver = ev.Player;
                queue.Remove(leaver);
                API.HintBridge.Remove(leaver);

                if (!IsReplaceable(leaver))
                    return;

                if (!DepartureArbiter.IsOwner(leaver, Plugin.DepartureOwner))
                    return;

                PlayerSnapshot snapshot = CaptureSnapshot(leaver);
                Player replacement = queue.Pick(leaver, plugin.Config);

                if (replacement is null)
                {
                    if (plugin.Config.Debug)
                        Log.Debug($"Aucun spectateur eligible pour remplacer {snapshot.Nickname}.");

                    return;
                }

                queue.MarkReplaced(replacement);
                ApplySnapshot(replacement, snapshot);

                if (plugin.Config.TransferInventory && !leaver.IsScp && replacement.Role.Type == snapshot.Role)
                    leaver.ClearInventory();
            }
            catch (Exception e)
            {
                Log.Error($"OnLeft: {e}");
            }
        }

        private bool IsReplaceable(Player player)
        {
            if (!Exiled.API.Features.Round.IsStarted || Exiled.API.Features.Round.IsEnded)
                return false;

            if (plugin.Config.MaxRoundSeconds > 0f
                && Exiled.API.Features.Round.ElapsedTime.TotalSeconds > plugin.Config.MaxRoundSeconds)
            {
                return false;
            }

            int remainingPlayers = Player.List.Count - 1;

            if (remainingPlayers < plugin.Config.MinimumPlayers)
                return false;

            RoleTypeId role = player.Role.Type;

            if (role == RoleTypeId.None || role == RoleTypeId.Spectator || player.Role.Team == Team.Dead)
                return false;

            if (plugin.Config.IgnoredRoles.Contains(role))
                return false;

            if (plugin.Config.ReplaceableRoles.Count > 0 && !plugin.Config.ReplaceableRoles.Contains(role))
                return false;

            return true;
        }

        private PlayerSnapshot CaptureSnapshot(Player player)
        {
            PlayerSnapshot snapshot = new PlayerSnapshot
            {
                Nickname = player.Nickname,
                Role = player.Role.Type,
                Position = player.Position,
                Rotation = player.Rotation,
                Health = player.Health,
                MaxHealth = player.MaxHealth,
                HumeShield = player.HumeShield,
            };

            if (plugin.Config.TransferInventory)
            {
                foreach (Exiled.API.Features.Items.Item item in player.Items)
                {
                    if (item is null)
                        continue;

                    snapshot.Items.Add(item.Type);
                }

                foreach (System.Collections.Generic.KeyValuePair<ItemType, ushort> ammo in player.Ammo)
                    snapshot.Ammo[ammo.Key] = ammo.Value;
            }

            if (plugin.Config.TransferEffects)
            {
                foreach (StatusEffectBase effect in player.ActiveEffects)
                {
                    if (effect is null || !effect.IsEnabled)
                        continue;

                    EffectType type = effect.GetEffectType();

                    if (type == EffectType.None)
                        continue;

                    snapshot.Effects.Add(new EffectSnapshot(type, effect.Intensity, effect.Duration == 0f ? 0f : effect.TimeLeft));
                }
            }

            if (player.Role is Scp079Role scp079)
            {
                snapshot.IsScp079 = true;
                snapshot.Scp079Level = scp079.Level;
                snapshot.Scp079Experience = scp079.Experience;
                snapshot.Scp079Energy = scp079.Energy;
            }

            return snapshot;
        }

        private void ApplySnapshot(Player replacement, PlayerSnapshot snapshot)
        {
            string userId = replacement.UserId;

            RoleSpawnFlags flags = RoleSpawnFlags.None;

            if (!plugin.Config.TransferPosition)
                flags |= RoleSpawnFlags.UseSpawnpoint;

            if (!plugin.Config.TransferInventory)
                flags |= RoleSpawnFlags.AssignInventory;

            replacement.Role.Set(snapshot.Role, SpawnReason.Respawn, flags);

            pendingSnapshots.Add(Timing.CallDelayed(plugin.Config.ReplacementDelay, () =>
            {
                try
                {
                    Player target = Player.Get(userId);

                    if (target is null || !target.IsConnected || target.Role.Type != snapshot.Role)
                        return;

                    if (plugin.Config.TransferPosition)
                    {
                        target.Position = snapshot.Position;
                        target.Rotation = snapshot.Rotation;
                    }

                    if (plugin.Config.TransferInventory && !target.IsScp)
                    {
                        target.ClearInventory();

                        foreach (ItemType type in snapshot.Items)
                        {
                            if (type == ItemType.None)
                                continue;

                            target.AddItem(type);
                        }

                        foreach (System.Collections.Generic.KeyValuePair<ItemType, ushort> ammo in snapshot.Ammo)
                            target.SetAmmo(ammo.Key.GetAmmoType(), ammo.Value);
                    }

                    if (plugin.Config.TransferHealth)
                    {
                        target.MaxHealth = snapshot.MaxHealth;
                        target.Health = Math.Min(snapshot.Health, snapshot.MaxHealth);
                        target.HumeShield = snapshot.HumeShield;
                    }

                    if (plugin.Config.TransferEffects)
                    {
                        foreach (EffectSnapshot effect in snapshot.Effects)
                            target.EnableEffect(effect.Type, effect.Intensity, effect.Duration);
                    }

                    if (snapshot.IsScp079 && target.Role is Scp079Role scp079)
                    {
                        scp079.Level = snapshot.Scp079Level;
                        scp079.Experience = snapshot.Scp079Experience;
                        scp079.Energy = snapshot.Scp079Energy;
                    }

                    Announce(target, snapshot);

                    PluginBus.Publish(BusTopics.PlayerReplaced, Plugin.DepartureOwner, target);

                    Log.Info($"{target.Nickname} ({target.UserId}) remplace {snapshot.Nickname} en tant que {snapshot.Role}.");
                }
                catch (Exception e)
                {
                    Log.Error($"ApplySnapshot: {e}");
                }
            }));
        }

        public void Reset()
        {
            foreach (CoroutineHandle handle in pendingSnapshots)
                Timing.KillCoroutines(handle);

            pendingSnapshots.Clear();
        }

        private void Announce(Player target, PlayerSnapshot snapshot)
        {
            string nickname = SafeText.Sanitize(snapshot.Nickname, SafeText.DefaultMaxLength, plugin.Translation.UnknownNickname);

            string hint = plugin.Translation.ReplacedHint
                .Replace("{nickname}", nickname)
                .Replace("{role}", snapshot.Role.GetFullName());

            API.HintBridge.Show(target, hint, plugin.Config.HintDuration);

            if (!plugin.Config.AnnounceToServer)
                return;

            Map.Broadcast(plugin.Config.BroadcastDuration, plugin.Translation.ReplacedBroadcast);
        }
    }
}
