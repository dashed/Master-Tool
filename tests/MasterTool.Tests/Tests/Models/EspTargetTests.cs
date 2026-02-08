using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Models;

/// <summary>
/// Tests for ESP target data models.
/// Uses <see cref="Vec2"/> and <see cref="Color"/> from MasterTool.Core (shared library).
/// </summary>
[TestFixture]
public class EspTargetTests
{
    // Standalone model mirrors (same fields as the real Models)
    private class EspTarget
    {
        public Vec2 ScreenPosition;
        public float Distance;
        public string Nickname;
        public string Side;
        public Color Color;
    }

    private class ItemEspTarget
    {
        public Vec2 ScreenPosition;
        public float Distance;
        public string Name;
        public Color Color;
    }

    private class QuestEspTarget
    {
        public Vec2 ScreenPosition;
        public float Distance;
        public string Name;
        public Color Color;
        public bool IsZone;
    }

    // --- EspTarget ---

    [Test]
    public void EspTarget_FieldAssignment_SetsAllFields()
    {
        var target = new EspTarget
        {
            ScreenPosition = new Vec2(100f, 200f),
            Distance = 42.5f,
            Nickname = "TestPlayer",
            Side = "BEAR",
            Color = new Color(1f, 0f, 0f, 1f),
        };

        Assert.That(target.ScreenPosition.X, Is.EqualTo(100f));
        Assert.That(target.ScreenPosition.Y, Is.EqualTo(200f));
        Assert.That(target.Distance, Is.EqualTo(42.5f));
        Assert.That(target.Nickname, Is.EqualTo("TestPlayer"));
        Assert.That(target.Side, Is.EqualTo("BEAR"));
        Assert.That(target.Color.R, Is.EqualTo(1f));
        Assert.That(target.Color.G, Is.EqualTo(0f));
        Assert.That(target.Color.B, Is.EqualTo(0f));
        Assert.That(target.Color.A, Is.EqualTo(1f));
    }

    [Test]
    public void EspTarget_DefaultValues_AreZeroAndNull()
    {
        var target = new EspTarget();

        Assert.That(target.ScreenPosition.X, Is.EqualTo(0f));
        Assert.That(target.ScreenPosition.Y, Is.EqualTo(0f));
        Assert.That(target.Distance, Is.EqualTo(0f));
        Assert.That(target.Nickname, Is.Null);
        Assert.That(target.Side, Is.Null);
        Assert.That(target.Color.R, Is.EqualTo(0f));
    }

    // --- ItemEspTarget ---

    [Test]
    public void ItemEspTarget_FieldAssignment_SetsAllFields()
    {
        var target = new ItemEspTarget
        {
            ScreenPosition = new Vec2(300f, 400f),
            Distance = 15.3f,
            Name = "M4A1",
            Color = new Color(0f, 1f, 0f, 0.8f),
        };

        Assert.That(target.ScreenPosition.X, Is.EqualTo(300f));
        Assert.That(target.ScreenPosition.Y, Is.EqualTo(400f));
        Assert.That(target.Distance, Is.EqualTo(15.3f));
        Assert.That(target.Name, Is.EqualTo("M4A1"));
        Assert.That(target.Color.G, Is.EqualTo(1f));
        Assert.That(target.Color.A, Is.EqualTo(0.8f));
    }

    [Test]
    public void ItemEspTarget_DefaultValues_AreZeroAndNull()
    {
        var target = new ItemEspTarget();

        Assert.That(target.ScreenPosition.X, Is.EqualTo(0f));
        Assert.That(target.Distance, Is.EqualTo(0f));
        Assert.That(target.Name, Is.Null);
    }

    // --- QuestEspTarget ---

    [Test]
    public void QuestEspTarget_FieldAssignment_SetsAllFields()
    {
        var target = new QuestEspTarget
        {
            ScreenPosition = new Vec2(500f, 600f),
            Distance = 120.7f,
            Name = "Quest Item Location",
            Color = new Color(0f, 0f, 1f, 1f),
            IsZone = true,
        };

        Assert.That(target.ScreenPosition.X, Is.EqualTo(500f));
        Assert.That(target.ScreenPosition.Y, Is.EqualTo(600f));
        Assert.That(target.Distance, Is.EqualTo(120.7f));
        Assert.That(target.Name, Is.EqualTo("Quest Item Location"));
        Assert.That(target.Color.B, Is.EqualTo(1f));
        Assert.That(target.IsZone, Is.True);
    }

    [Test]
    public void QuestEspTarget_DefaultValues_IsZoneIsFalse()
    {
        var target = new QuestEspTarget();

        Assert.That(target.IsZone, Is.False);
        Assert.That(target.Name, Is.Null);
        Assert.That(target.Distance, Is.EqualTo(0f));
    }

    [Test]
    public void QuestEspTarget_IsZoneFalse_ForNonZoneTarget()
    {
        var target = new QuestEspTarget { Name = "Find Item", IsZone = false };

        Assert.That(target.IsZone, Is.False);
        Assert.That(target.Name, Is.EqualTo("Find Item"));
    }
}
