using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using UnityEngine;

namespace MasterTool.Plugin
{
    /// <summary>
    /// Holds cached references to frequently accessed game objects.
    /// Refreshed periodically to handle scene transitions.
    /// </summary>
    public class GameState
    {
        public Player LocalPlayer { get; private set; }
        public ActiveHealthController LocalActiveHealthController { get; private set; }
        public Camera MainCamera { get; private set; }
        public GameWorld GameWorld { get; private set; }

        private float _nextRefresh;
        private const float RefreshIntervalSeconds = 4.0f;

        /// <summary>
        /// Checks whether the refresh interval has elapsed and, if so, re-caches game references.
        /// Call once per frame from the plugin's Update loop.
        /// </summary>
        public void Update()
        {
            if (Time.time < _nextRefresh) return;
            _nextRefresh = Time.time + RefreshIntervalSeconds;
            Refresh();
        }

        private void Refresh()
        {
            try
            {
                GameWorld = Singleton<GameWorld>.Instance;
                if (GameWorld == null) return;
                LocalPlayer = GameWorld.MainPlayer;
                if (LocalPlayer == null) return;
                LocalActiveHealthController = LocalPlayer.ActiveHealthController;
                if (MainCamera == null)
                    MainCamera = Camera.main ?? GameObject.Find("FPS Camera")?.GetComponent<Camera>();
            }
            catch { }
        }
    }
}
