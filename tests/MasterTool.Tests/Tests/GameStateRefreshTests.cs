using System;
using NUnit.Framework;

namespace MasterTool.Tests.Tests;

/// <summary>
/// Tests for the GameState camera caching logic.
/// Duplicates the pure refresh decision since Unity Camera objects
/// cannot be instantiated from net9.0 tests.
/// </summary>
[TestFixture]
public class GameStateRefreshTests
{
    private class FakeCamera
    {
        public string Name { get; }

        public FakeCamera(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Mirrors the GameState.Refresh() camera logic.
    /// Always re-fetches the camera on every refresh cycle.
    /// </summary>
    private class FakeGameState
    {
        public FakeCamera MainCamera { get; private set; }
        public Func<FakeCamera> CameraProvider { get; set; }

        public void Refresh()
        {
            MainCamera = CameraProvider?.Invoke();
        }
    }

    private FakeGameState _state;

    [SetUp]
    public void SetUp()
    {
        _state = new FakeGameState();
    }

    [Test]
    public void NullCamera_FetchesNew()
    {
        var cam = new FakeCamera("FPS Camera");
        _state.CameraProvider = () => cam;

        _state.Refresh();

        Assert.That(_state.MainCamera, Is.SameAs(cam));
    }

    [Test]
    public void CameraAlreadySet_StillRefreshed()
    {
        var oldCam = new FakeCamera("FPS Camera");
        var newCam = new FakeCamera("Spectator Camera");
        _state.CameraProvider = () => oldCam;
        _state.Refresh();
        Assert.That(_state.MainCamera, Is.SameAs(oldCam));

        // Camera changes (e.g., spectator mode)
        _state.CameraProvider = () => newCam;
        _state.Refresh();

        Assert.That(_state.MainCamera, Is.SameAs(newCam), "Should pick up new camera even when old one is non-null");
    }

    [Test]
    public void CameraProviderReturnsNull_SetsNull()
    {
        var cam = new FakeCamera("FPS Camera");
        _state.CameraProvider = () => cam;
        _state.Refresh();
        Assert.That(_state.MainCamera, Is.Not.Null);

        // Camera destroyed (scene transition)
        _state.CameraProvider = () => null;
        _state.Refresh();

        Assert.That(_state.MainCamera, Is.Null, "Should set null when provider returns null");
    }

    [Test]
    public void MultipleRefreshes_AlwaysUsesLatest()
    {
        var cam1 = new FakeCamera("Camera 1");
        var cam2 = new FakeCamera("Camera 2");
        var cam3 = new FakeCamera("Camera 3");

        _state.CameraProvider = () => cam1;
        _state.Refresh();
        Assert.That(_state.MainCamera, Is.SameAs(cam1));

        _state.CameraProvider = () => cam2;
        _state.Refresh();
        Assert.That(_state.MainCamera, Is.SameAs(cam2));

        _state.CameraProvider = () => cam3;
        _state.Refresh();
        Assert.That(_state.MainCamera, Is.SameAs(cam3));
    }

    [Test]
    public void SameCameraReturned_NoIssue()
    {
        var cam = new FakeCamera("FPS Camera");
        _state.CameraProvider = () => cam;

        _state.Refresh();
        _state.Refresh();
        _state.Refresh();

        Assert.That(_state.MainCamera, Is.SameAs(cam));
    }

    [Test]
    public void NullProvider_CameraStaysNull()
    {
        _state.CameraProvider = null;

        _state.Refresh();

        Assert.That(_state.MainCamera, Is.Null);
    }

    [Test]
    public void CameraSwapAndBack_TracksCorrectly()
    {
        var fpsCam = new FakeCamera("FPS Camera");
        var spectatorCam = new FakeCamera("Spectator Camera");

        // Start with FPS camera
        _state.CameraProvider = () => fpsCam;
        _state.Refresh();
        Assert.That(_state.MainCamera, Is.SameAs(fpsCam));

        // Switch to spectator
        _state.CameraProvider = () => spectatorCam;
        _state.Refresh();
        Assert.That(_state.MainCamera, Is.SameAs(spectatorCam));

        // Switch back to FPS
        _state.CameraProvider = () => fpsCam;
        _state.Refresh();
        Assert.That(_state.MainCamera, Is.SameAs(fpsCam));
    }
}
