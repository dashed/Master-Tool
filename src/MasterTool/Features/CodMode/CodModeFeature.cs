using System;
using EFT;
using EFT.HealthSystem;
using MasterTool.Config;
using MasterTool.Core;
using MasterTool.Plugin;
using UnityEngine;

namespace MasterTool.Features.CodMode
{
    public static class CodModeFeature
    {
        private const int HealCycleFrames = 60;

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

                if (_frameCount < HealCycleFrames)
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
                catch (Exception ex)
                {
                    MasterToolPlugin.Log?.LogDebug($"[CodMode] Unsubscribe error: {ex.Message}");
                }

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
            return HealingLogic.ShouldHeal(timeSinceHit, healDelay);
        }

        internal static float CalculateHealAmount(float current, float maximum, float healRate)
        {
            return HealingLogic.CalculateHealAmount(current, maximum, healRate);
        }

        internal static bool ShouldHealBodyPart(float current, float maximum)
        {
            return HealingLogic.ShouldHealBodyPart(current, maximum);
        }
    }
}
