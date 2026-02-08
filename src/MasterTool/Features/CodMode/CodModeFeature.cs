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
        private static bool _isRegenerating;
        private static int _frameCount;
        private static bool _errorLogged;

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
                _timeSinceLastHit += Time.deltaTime;
                _frameCount++;

                if (_frameCount < 60)
                    return;
                _frameCount = 0;

                if (!ShouldHeal(_timeSinceLastHit, PluginConfig.CodModeHealDelay.Value))
                    return;

                _isRegenerating = true;
                var controller = localPlayer.ActiveHealthController;
                var healRate = PluginConfig.CodModeHealRate.Value;

                foreach (var part in AllBodyParts)
                {
                    var health = controller.GetBodyPartHealth(part, false);
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

        public static void NotifyDamage()
        {
            _timeSinceLastHit = 0f;
            _isRegenerating = false;
        }

        internal static bool ShouldHeal(float timeSinceHit, float healDelay)
        {
            return timeSinceHit >= healDelay;
        }

        internal static float CalculateHealAmount(float current, float maximum, float healRate)
        {
            if (current >= maximum)
                return 0f;
            return Math.Min(healRate, maximum - current);
        }
    }
}
