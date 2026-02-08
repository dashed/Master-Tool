using System.Collections.Generic;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

/// <summary>
/// Tests for the quest zone ID extraction logic.
/// Duplicates the pure extraction since EFT quest condition types
/// cannot be used from net9.0 tests.
/// </summary>
[TestFixture]
public class QuestZoneExtractionTests
{
    private enum ConditionType
    {
        LeaveItemAtLocation,
        PlaceBeacon,
        VisitPlace,
        LaunchFlare,
        FindItem,
        CounterCreator,
    }

    private class FakeCondition
    {
        public ConditionType Type { get; set; }
        public string ZoneId { get; set; }
        public string Target { get; set; }
        public string[] ItemIds { get; set; }
    }

    /// <summary>
    /// Mirrors the zone ID extraction logic from QuestEsp.Update().
    /// Extracts zone IDs from conditions based on their type.
    /// </summary>
    private static HashSet<string> ExtractZoneIds(IEnumerable<FakeCondition> conditions)
    {
        var zoneIds = new HashSet<string>();
        foreach (var condition in conditions)
        {
            switch (condition.Type)
            {
                case ConditionType.LeaveItemAtLocation:
                case ConditionType.PlaceBeacon:
                case ConditionType.LaunchFlare:
                    if (!string.IsNullOrEmpty(condition.ZoneId))
                    {
                        zoneIds.Add(condition.ZoneId);
                    }

                    break;
                case ConditionType.VisitPlace:
                    if (!string.IsNullOrEmpty(condition.Target))
                    {
                        zoneIds.Add(condition.Target);
                    }

                    break;
            }
        }
        return zoneIds;
    }

    [Test]
    public void LeaveItemAtLocation_ExtractsZoneId()
    {
        var conditions = new[]
        {
            new FakeCondition { Type = ConditionType.LeaveItemAtLocation, ZoneId = "fuel1" },
        };
        var result = ExtractZoneIds(conditions);
        Assert.That(result, Contains.Item("fuel1"));
    }

    [Test]
    public void PlaceBeacon_ExtractsZoneId()
    {
        var conditions = new[]
        {
            new FakeCondition { Type = ConditionType.PlaceBeacon, ZoneId = "gazel" },
        };
        var result = ExtractZoneIds(conditions);
        Assert.That(result, Contains.Item("gazel"));
    }

    [Test]
    public void VisitPlace_ExtractsTarget()
    {
        var conditions = new[]
        {
            new FakeCondition { Type = ConditionType.VisitPlace, Target = "locked_office" },
        };
        var result = ExtractZoneIds(conditions);
        Assert.That(result, Contains.Item("locked_office"));
    }

    [Test]
    public void LaunchFlare_ExtractsZoneId()
    {
        var conditions = new[]
        {
            new FakeCondition { Type = ConditionType.LaunchFlare, ZoneId = "flare_zone_1" },
        };
        var result = ExtractZoneIds(conditions);
        Assert.That(result, Contains.Item("flare_zone_1"));
    }

    [Test]
    public void FindItem_NoZoneExtracted()
    {
        var conditions = new[]
        {
            new FakeCondition { Type = ConditionType.FindItem, ItemIds = new[] { "abc123" } },
        };
        var result = ExtractZoneIds(conditions);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void NullZoneId_Ignored()
    {
        var conditions = new[]
        {
            new FakeCondition { Type = ConditionType.PlaceBeacon, ZoneId = null },
        };
        var result = ExtractZoneIds(conditions);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void EmptyZoneId_Ignored()
    {
        var conditions = new[]
        {
            new FakeCondition { Type = ConditionType.LeaveItemAtLocation, ZoneId = "" },
        };
        var result = ExtractZoneIds(conditions);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void MultipleConditions_AllZonesExtracted()
    {
        var conditions = new[]
        {
            new FakeCondition { Type = ConditionType.PlaceBeacon, ZoneId = "gazel" },
            new FakeCondition { Type = ConditionType.VisitPlace, Target = "locked_office" },
            new FakeCondition { Type = ConditionType.LeaveItemAtLocation, ZoneId = "Q019_3" },
        };
        var result = ExtractZoneIds(conditions);
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result, Contains.Item("gazel"));
        Assert.That(result, Contains.Item("locked_office"));
        Assert.That(result, Contains.Item("Q019_3"));
    }

    [Test]
    public void DuplicateZoneIds_Deduplicated()
    {
        var conditions = new[]
        {
            new FakeCondition { Type = ConditionType.PlaceBeacon, ZoneId = "gazel" },
            new FakeCondition { Type = ConditionType.LeaveItemAtLocation, ZoneId = "gazel" },
        };
        var result = ExtractZoneIds(conditions);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result, Contains.Item("gazel"));
    }

    [Test]
    public void MixedValidAndInvalid_OnlyValidExtracted()
    {
        var conditions = new[]
        {
            new FakeCondition { Type = ConditionType.PlaceBeacon, ZoneId = "gazel" },
            new FakeCondition { Type = ConditionType.FindItem, ItemIds = new[] { "abc" } },
            new FakeCondition { Type = ConditionType.VisitPlace, Target = null },
            new FakeCondition { Type = ConditionType.LeaveItemAtLocation, ZoneId = "fuel1" },
        };
        var result = ExtractZoneIds(conditions);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result, Contains.Item("gazel"));
        Assert.That(result, Contains.Item("fuel1"));
    }
}
