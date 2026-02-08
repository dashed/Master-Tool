using System;
using EFT;
using EFT.HealthSystem;
using MasterTool.Config;
using MasterTool.Plugin;
using UnityEngine;

namespace MasterTool.Features.CodMode
{
    public static class CodModeFeature
    {
        private static float _timeSinceLastHit;
        private static int _frameCount;
        private static bool _errorLogged;
        private static Player _subscribedPlayer;

        private static readonly EBodyPart[] AllBodyParts =
        {
            EBodyPart.Head,
            EBodyPart.Chest,
            EBodyPart.Stomach,
            EBodyPart.LeftArm,
            EBodyPart.RightArm,
            EBodyPart.LeftLeg,
            EBodyPart.RightLeg,
        };

        public static void Apply(Player localPlayer)
        {
            try
            {
                SubscribeToPlayer(localPlayer);

                _timeSinceLastHit += Time.unscaledDeltaTime;
                _frameCount++;

                if (_frameCount < 60)
                {
                    return;
                }

                _frameCount = 0;

                if (!ShouldHeal(_timeSinceLastHit, PluginConfig.CodModeHealDelay.Value))
                {
                    return;
                }

                var controller = localPlayer.ActiveHealthController;
                var healRate = PluginConfig.CodModeHealRate.Value;

                foreach (var part in AllBodyParts)
                {
                    var health = controller.GetBodyPartHealth(part, false);

                    // Skip destroyed (blacked) body parts
                    if (health.Current <= 0f)
                    {
                        continue;
                    }

                    var amount = CalculateHealAmount(health.Current, health.Maximum, healRate);
                    if (amount > 0f)
                    {
                        controller.ChangeHealth(part, amount, default(DamageInfoStruct));
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_errorLogged)
                {
                    MasterToolPlugin.Log?.LogWarning($"[CodMode] {ex.Message}");
                    _errorLogged = true;
                }
            }
        }

        /// <summary>
        /// Subscribe to BeingHitAction on the local player so only real hits
        /// (not bleed/fracture tick damage) reset the heal timer.
        /// </summary>
        private static void SubscribeToPlayer(Player localPlayer)
        {
            if (_subscribedPlayer == localPlayer)
            {
                return;
            }

            UnsubscribeFromPlayer();

            localPlayer.BeingHitAction += OnBeingHit;
            localPlayer.OnPlayerDeadOrUnspawn += OnPlayerDeadOrUnspawn;
            _subscribedPlayer = localPlayer;
        }

        private static void UnsubscribeFromPlayer()
        {
            if (_subscribedPlayer != null)
            {
                try
                {
                    _subscribedPlayer.BeingHitAction -= OnBeingHit;
                    _subscribedPlayer.OnPlayerDeadOrUnspawn -= OnPlayerDeadOrUnspawn;
                }
                catch { }

                _subscribedPlayer = null;
            }
        }

        private static void OnBeingHit(DamageInfoStruct damageInfo, EBodyPart bodyPart, float absorbed)
        {
            _timeSinceLastHit = 0f;
        }

        private static void OnPlayerDeadOrUnspawn(Player player)
        {
            UnsubscribeFromPlayer();
        }

        /// <summary>
        /// Legacy damage notification. Kept for backwards compatibility but
        /// no longer the primary trigger — BeingHitAction event is used instead.
        /// </summary>
        public static void NotifyDamage()
        {
            _timeSinceLastHit = 0f;
        }

        // TODO: Wire PluginConfig.CodModeRemoveEffects — requires subscribing to
        // healthController.EffectAddedEvent and calling RemoveEffectFromList for
        // negative effects (bleeds, fractures, pain). Blocked on identifying the
        // correct obfuscated GClass types for the current SPT version.

        internal static bool ShouldHeal(float timeSinceHit, float healDelay)
        {
            return timeSinceHit >= healDelay;
        }

        internal static float CalculateHealAmount(float current, float maximum, float healRate)
        {
            if (current >= maximum)
            {
                return 0f;
            }

            return Math.Min(healRate, maximum - current);
        }

        /// <summary>
        /// Pure logic: whether a body part should be healed.
        /// Returns false for destroyed/blacked parts (Current &lt;= 0).
        /// </summary>
        internal static bool ShouldHealBodyPart(float current, float maximum)
        {
            return current > 0f && current < maximum;
        }
    }
}
