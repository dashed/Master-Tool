using System;
using System.Collections.Generic;
using EFT;
using MasterTool.Config;
using MasterTool.Core;
using MasterTool.Plugin;
using MasterTool.Utils;
using UnityEngine;
using Color = UnityEngine.Color;
using KeyCode = UnityEngine.KeyCode;

namespace MasterTool.ESP
{
    public class ChamsManager
    {
        private readonly Dictionary<Renderer, Shader> _originalShaders = new Dictionary<Renderer, Shader>();
        private readonly Dictionary<Renderer, GameObject> _outlineDuplicates = new Dictionary<Renderer, GameObject>();
        private static Shader _chamsShader;
        private float _nextCleanup;
        private const float CleanupIntervalSeconds = 30f;
        private bool _errorLogged;
        private bool _wasChamsEnabled;
        private bool _wasLootChamsEnabled;
        private ChamsMode _lastPlayerMode;
        private ChamsMode _lastLootMode;

        private const string OutlineObjectName = "_ChamsOutline";

        public void Initialize()
        {
            _chamsShader = Shader.Find("Hidden/Internal-Colored");
        }

        public void Update(GameWorld gameWorld, Camera mainCamera)
        {
            if (_wasChamsEnabled && !PluginConfig.ChamsEnabled.Value)
            {
                ResetAllPlayerChams();
            }

            ChamsMode currentPlayerMode = PluginConfig.ChamsRenderMode.Value;
            if (_wasChamsEnabled && PluginConfig.ChamsEnabled.Value && currentPlayerMode != _lastPlayerMode)
            {
                ResetAllPlayerChams();
            }
            _wasChamsEnabled = PluginConfig.ChamsEnabled.Value;
            _lastPlayerMode = currentPlayerMode;

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

        public void UpdateLootChams(GameWorld gameWorld, Camera mainCamera, Player localPlayer)
        {
            if (_wasLootChamsEnabled && !PluginConfig.LootChamsEnabled.Value)
            {
                ResetAllLootChams();
            }

            ChamsMode currentLootMode = PluginConfig.LootChamsRenderMode.Value;
            if (_wasLootChamsEnabled && PluginConfig.LootChamsEnabled.Value && currentLootMode != _lastLootMode)
            {
                ResetAllLootChams();
            }
            _wasLootChamsEnabled = PluginConfig.LootChamsEnabled.Value;
            _lastLootMode = currentLootMode;

            if (gameWorld == null || mainCamera == null || _chamsShader == null)
                return;

            var lootItems = gameWorld.LootItems;
            if (lootItems == null)
                return;

            try
            {
                float maxDistSq = PluginConfig.ItemEspMaxDistance.Value * PluginConfig.ItemEspMaxDistance.Value;
                Vector3 playerPos = localPlayer.Transform.position;

                for (int i = 0; i < lootItems.Count; i++)
                {
                    var loot = lootItems.GetByIndex(i);
                    if (loot == null)
                        continue;

                    float distSq = (loot.transform.position - playerPos).sqrMagnitude;
                    bool shouldChams = PluginConfig.LootChamsEnabled.Value && distSq <= maxDistSq;

                    if (shouldChams)
                    {
                        ApplyLootChams(loot.gameObject, PluginConfig.LootChamsColor.Value);
                    }
                    else
                    {
                        ResetLootChams(loot.gameObject);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_errorLogged)
                {
                    MasterToolPlugin.Log?.LogWarning($"[LootChams] {ex.Message}");
                    _errorLogged = true;
                }
            }
        }

        private void ApplyChams(Player player, Color color)
        {
            if (player == null)
                return;

            ChamsMode mode = PluginConfig.ChamsRenderMode.Value;

            foreach (var renderer in player.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (renderer == null || renderer.material == null)
                    continue;

                if (renderer.gameObject.name == OutlineObjectName)
                    continue;

                if (mode == ChamsMode.Outline)
                {
                    // Restore original shader on the main renderer if it was overridden
                    if (_originalShaders.ContainsKey(renderer))
                    {
                        renderer.material.shader = _originalShaders[renderer];
                        renderer.allowOcclusionWhenDynamic = true;
                        _originalShaders.Remove(renderer);
                    }
                    EnsureOutlineDuplicate(renderer, color);
                }
                else
                {
                    DestroyOutlineDuplicate(renderer);
                    int cullMode = mode == ChamsMode.CullFront ? 1 : 0;
                    ApplyShaderChams(renderer, color, cullMode);
                }
            }
        }

        private void ApplyLootChams(GameObject obj, Color color)
        {
            if (obj == null)
                return;

            ChamsMode mode = PluginConfig.LootChamsRenderMode.Value;

            foreach (var renderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer == null || renderer.material == null)
                    continue;

                if (renderer.gameObject.name == OutlineObjectName)
                    continue;

                if (mode == ChamsMode.Outline)
                {
                    if (_originalShaders.ContainsKey(renderer))
                    {
                        renderer.material.shader = _originalShaders[renderer];
                        renderer.allowOcclusionWhenDynamic = true;
                        _originalShaders.Remove(renderer);
                    }
                    EnsureOutlineDuplicate(renderer, color);
                }
                else
                {
                    DestroyOutlineDuplicate(renderer);
                    int cullMode = mode == ChamsMode.CullFront ? 1 : 0;
                    ApplyShaderChams(renderer, color, cullMode);
                }
            }
        }

        private void ApplyShaderChams(Renderer renderer, Color color, int cullMode)
        {
            renderer.forceRenderingOff = false;
            renderer.allowOcclusionWhenDynamic = false;

            if (renderer.material.shader != _chamsShader)
            {
                if (!_originalShaders.ContainsKey(renderer))
                    _originalShaders[renderer] = renderer.material.shader;

                renderer.material.shader = _chamsShader;
                renderer.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                renderer.material.SetInt("_ZWrite", 0);
                renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                renderer.material.renderQueue = 4000;
            }

            renderer.material.SetInt("_Cull", cullMode);

            float intensity = Mathf.Clamp(PluginConfig.ChamsIntensity.Value, 0.1f, 1f);
            float opacity = Mathf.Clamp(PluginConfig.ChamsOpacity.Value, 0.1f, 1f);
            Color adjusted = new Color(color.r * intensity, color.g * intensity, color.b * intensity, opacity);
            renderer.material.SetColor("_Color", adjusted);
        }

        private void EnsureOutlineDuplicate(Renderer renderer, Color color)
        {
            if (_outlineDuplicates.TryGetValue(renderer, out var existing) && existing != null)
            {
                // Update color on existing outline
                var outlineRenderer = existing.GetComponent<Renderer>();
                if (outlineRenderer != null && outlineRenderer.material != null)
                {
                    float intensity = Mathf.Clamp(PluginConfig.ChamsIntensity.Value, 0.1f, 1f);
                    float opacity = Mathf.Clamp(PluginConfig.ChamsOpacity.Value, 0.1f, 1f);
                    Color adjusted = new Color(color.r * intensity, color.g * intensity, color.b * intensity, opacity);
                    outlineRenderer.material.SetColor("_Color", adjusted);

                    float scale = Mathf.Clamp(PluginConfig.OutlineScale.Value, 1.01f, 1.15f);
                    existing.transform.localScale = new Vector3(scale, scale, scale);
                }
                return;
            }

            GameObject outline = null;
            if (renderer is SkinnedMeshRenderer skinned)
            {
                outline = CreateSkinnedOutline(skinned);
            }
            else if (renderer is MeshRenderer meshRenderer)
            {
                outline = CreateMeshOutline(meshRenderer);
            }

            if (outline != null)
            {
                // Apply color
                var outlineRenderer = outline.GetComponent<Renderer>();
                if (outlineRenderer != null && outlineRenderer.material != null)
                {
                    float intensity = Mathf.Clamp(PluginConfig.ChamsIntensity.Value, 0.1f, 1f);
                    float opacity = Mathf.Clamp(PluginConfig.ChamsOpacity.Value, 0.1f, 1f);
                    Color adjusted = new Color(color.r * intensity, color.g * intensity, color.b * intensity, opacity);
                    outlineRenderer.material.SetColor("_Color", adjusted);
                }

                _outlineDuplicates[renderer] = outline;
            }
        }

        private GameObject CreateSkinnedOutline(SkinnedMeshRenderer source)
        {
            var outlineObj = new GameObject(OutlineObjectName);
            outlineObj.transform.SetParent(source.transform, false);

            float scale = Mathf.Clamp(PluginConfig.OutlineScale.Value, 1.01f, 1.15f);
            outlineObj.transform.localScale = new Vector3(scale, scale, scale);

            var outlineSkinned = outlineObj.AddComponent<SkinnedMeshRenderer>();
            outlineSkinned.sharedMesh = source.sharedMesh;
            outlineSkinned.bones = source.bones;
            outlineSkinned.rootBone = source.rootBone;

            outlineSkinned.material = new Material(_chamsShader);
            outlineSkinned.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            outlineSkinned.material.SetInt("_ZWrite", 0);
            outlineSkinned.material.SetInt("_Cull", 1); // Cull front faces
            outlineSkinned.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            outlineSkinned.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            outlineSkinned.material.renderQueue = 4000;

            outlineSkinned.forceRenderingOff = false;
            outlineSkinned.allowOcclusionWhenDynamic = false;

            return outlineObj;
        }

        private GameObject CreateMeshOutline(MeshRenderer source)
        {
            var meshFilter = source.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
                return null;

            var outlineObj = new GameObject(OutlineObjectName);
            outlineObj.transform.SetParent(source.transform, false);

            float scale = Mathf.Clamp(PluginConfig.OutlineScale.Value, 1.01f, 1.15f);
            outlineObj.transform.localScale = new Vector3(scale, scale, scale);

            var outlineFilter = outlineObj.AddComponent<MeshFilter>();
            outlineFilter.sharedMesh = meshFilter.sharedMesh;

            var outlineMeshRenderer = outlineObj.AddComponent<MeshRenderer>();
            outlineMeshRenderer.material = new Material(_chamsShader);
            outlineMeshRenderer.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            outlineMeshRenderer.material.SetInt("_ZWrite", 0);
            outlineMeshRenderer.material.SetInt("_Cull", 1); // Cull front faces
            outlineMeshRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            outlineMeshRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            outlineMeshRenderer.material.renderQueue = 4000;

            outlineMeshRenderer.forceRenderingOff = false;
            outlineMeshRenderer.allowOcclusionWhenDynamic = false;

            return outlineObj;
        }

        private void DestroyOutlineDuplicate(Renderer renderer)
        {
            if (_outlineDuplicates.TryGetValue(renderer, out var outline))
            {
                if (outline != null)
                    UnityEngine.Object.Destroy(outline);
                _outlineDuplicates.Remove(renderer);
            }
        }

        private void DestroyOutlineDuplicatesByType<T>()
            where T : Renderer
        {
            var keysToRemove = new List<Renderer>();
            foreach (var kv in _outlineDuplicates)
            {
                if (kv.Key is T)
                {
                    keysToRemove.Add(kv.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                if (_outlineDuplicates.TryGetValue(key, out var outline))
                {
                    if (outline != null)
                        UnityEngine.Object.Destroy(outline);
                }
                _outlineDuplicates.Remove(key);
            }
        }

        private void ResetLootChams(GameObject obj)
        {
            if (obj == null)
                return;
            foreach (var renderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer.gameObject.name == OutlineObjectName)
                    continue;

                DestroyOutlineDuplicate(renderer);

                if (renderer != null && renderer.material != null && _originalShaders.ContainsKey(renderer))
                {
                    renderer.material.shader = _originalShaders[renderer];
                    renderer.allowOcclusionWhenDynamic = true;
                    _originalShaders.Remove(renderer);
                }
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

            var deadOutlines = new List<Renderer>();
            foreach (var kv in _outlineDuplicates)
            {
                if (kv.Key == null)
                {
                    if (kv.Value != null)
                        UnityEngine.Object.Destroy(kv.Value);
                    deadOutlines.Add(kv.Key);
                }
            }

            foreach (var r in deadOutlines)
            {
                _outlineDuplicates.Remove(r);
            }
        }

        private void ResetAllPlayerChams()
        {
            var keysToRemove = new List<Renderer>();
            foreach (var kv in _originalShaders)
            {
                if (kv.Key is SkinnedMeshRenderer)
                {
                    keysToRemove.Add(kv.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                try
                {
                    var renderer = key as SkinnedMeshRenderer;
                    if (renderer != null && renderer.material != null)
                    {
                        renderer.material.shader = _originalShaders[key];
                        renderer.allowOcclusionWhenDynamic = true;
                    }
                }
                catch (Exception)
                {
                    // Renderer may have been destroyed
                }
                _originalShaders.Remove(key);
            }

            DestroyOutlineDuplicatesByType<SkinnedMeshRenderer>();
        }

        private void ResetAllLootChams()
        {
            var keysToRemove = new List<Renderer>();
            foreach (var kv in _originalShaders)
            {
                if (kv.Key is MeshRenderer)
                {
                    keysToRemove.Add(kv.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                try
                {
                    var renderer = key as MeshRenderer;
                    if (renderer != null && renderer.material != null)
                    {
                        renderer.material.shader = _originalShaders[key];
                        renderer.allowOcclusionWhenDynamic = true;
                    }
                }
                catch (Exception)
                {
                    // Renderer may have been destroyed
                }
                _originalShaders.Remove(key);
            }

            DestroyOutlineDuplicatesByType<MeshRenderer>();
        }

        private void ResetChams(Player player)
        {
            if (player == null)
                return;
            foreach (var renderer in player.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (renderer.gameObject.name == OutlineObjectName)
                    continue;

                DestroyOutlineDuplicate(renderer);

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
