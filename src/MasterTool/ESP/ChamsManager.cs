using System.Collections.Generic;
using EFT;
using MasterTool.Config;
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
            if (gameWorld == null || mainCamera == null) return;

            var players = gameWorld.RegisteredPlayers;
            if (players == null) return;

            foreach (var player in players)
            {
                if (player is Player playerClass)
                {
                    float dist = Vector3.Distance(mainCamera.transform.position, playerClass.Transform.position);

                    bool shouldChams = PluginConfig.ChamsEnabled.Value &&
                                       !playerClass.IsYourPlayer &&
                                       playerClass.HealthController.IsAlive &&
                                       dist <= PluginConfig.EspMaxDistance.Value;

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

        private void ApplyChams(Player player, Color color)
        {
            if (player == null) return;
            foreach (var renderer in player.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (renderer == null || renderer.material == null) continue;

                if (renderer.material.shader != _chamsShader)
                {
                    if (!_originalShaders.ContainsKey(renderer))
                        _originalShaders[renderer] = renderer.material.shader;

                    renderer.material.shader = _chamsShader;
                    renderer.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                    renderer.material.SetInt("_ZWrite", 0);
                }

                renderer.material.SetColor("_Color", color);
            }
        }

        private void ResetChams(Player player)
        {
            if (player == null) return;
            foreach (var renderer in player.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (renderer != null && renderer.material != null && _originalShaders.ContainsKey(renderer))
                {
                    renderer.material.shader = _originalShaders[renderer];
                    _originalShaders.Remove(renderer);
                }
            }
        }
    }
}
