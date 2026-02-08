using System;
using System.Collections.Generic;
using EFT;
using MasterTool.Config;
using MasterTool.Plugin;
using MasterTool.Utils;
using UnityEngine;

namespace MasterTool.ESP
{
    /// <summary>
    /// Applies and removes "chams" (colored wallhack shaders) on player models.
    /// Replaces mesh renderer shaders with a flat-colored always-visible shader when enabled,
    /// and restores original shaders when disabled or the player is out of range.
    /// </summary>
    public class ChamsManager
    {
        private readonly Dictionary<Renderer, Shader> _originalShaders = new Dictionary<Renderer, Shader>();
        private static Shader _chamsShader;
        private float _nextCleanup;
        private const float CleanupIntervalSeconds = 30f;
        private bool _errorLogged;

        /// <summary>
        /// Loads the flat-colored shader used for chams rendering. Call once during plugin startup.
        /// </summary>
        public void Initialize()
        {
            _chamsShader = Shader.Find("Hidden/Internal-Colored");
        }

        /// <summary>
        /// Iterates all registered players and applies or removes chams based on
        /// distance, alive status, and the config toggle. Call once per frame.
        /// </summary>
        /// <param name="gameWorld">The current game world instance.</param>
        /// <param name="mainCamera">The active camera used for distance calculations.</param>
        public void Update(GameWorld gameWorld, Camera mainCamera)
        {
            if (gameWorld == null || mainCamera == null)
                return;

            var players = gameWorld.RegisteredPlayers;
            if (players == null)
                return;

            try
            {
                foreach (var player in players)
                {
                    if (player is Player playerClass)
                    {
                        float dist = Vector3.Distance(mainCamera.transform.position, playerClass.Transform.position);

                        bool shouldChams =
                            PluginConfig.ChamsEnabled.Value
                            && !playerClass.IsYourPlayer
                            && playerClass.HealthController.IsAlive
                            && dist <= PluginConfig.EspMaxDistance.Value;

                        if (shouldChams)
                        {
                            Color color = PlayerUtils.GetPlayerColor(playerClass);
                            ApplyChams(playerClass, color);
                        }
                        else
                        {
                            ResetChams(playerClass);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_errorLogged)
                {
                    MasterToolPlugin.Log?.LogWarning($"[Chams] {ex.Message}");
                    _errorLogged = true;
                }
            }

            if (Time.time > _nextCleanup)
            {
                _nextCleanup = Time.time + CleanupIntervalSeconds;
                PurgeDestroyedEntries();
            }
        }

        private void PurgeDestroyedEntries()
        {
            var dead = new List<Renderer>();
            foreach (var kv in _originalShaders)
            {
                if (kv.Key == null)
                {
                    dead.Add(kv.Key);
                }
            }

            foreach (var r in dead)
            {
                _originalShaders.Remove(r);
            }
        }

        private void ApplyChams(Player player, Color color)
        {
            if (player == null)
                return;
            foreach (var renderer in player.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (renderer == null || renderer.material == null)
                    continue;

                // Force renderer visible — prevents Unity's occlusion culling from hiding chams
                renderer.forceRenderingOff = false;
                renderer.allowOcclusionWhenDynamic = false;

                if (renderer.material.shader != _chamsShader)
                {
                    if (!_originalShaders.ContainsKey(renderer))
                        _originalShaders[renderer] = renderer.material.shader;

                    renderer.material.shader = _chamsShader;
                    renderer.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                    renderer.material.SetInt("_ZWrite", 0);
                    renderer.material.renderQueue = 4000;
                }

                float intensity = Mathf.Clamp(PluginConfig.ChamsIntensity.Value, 0.1f, 1f);
                Color adjusted = new Color(color.r * intensity, color.g * intensity, color.b * intensity, color.a);
                renderer.material.SetColor("_Color", adjusted);
            }
        }

        private void ResetChams(Player player)
        {
            if (player == null)
                return;
            foreach (var renderer in player.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (renderer != null && renderer.material != null && _originalShaders.ContainsKey(renderer))
                {
                    renderer.material.shader = _originalShaders[renderer];
                    renderer.allowOcclusionWhenDynamic = true;
                    _originalShaders.Remove(renderer);
                }
            }
        }
    }
}
