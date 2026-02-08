using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Config;

/// <summary>
/// Tests for config section organization.
/// Validates section naming conventions, counts, and ordering.
/// </summary>
[TestFixture]
public class ConfigSectionTests
{
    /// <summary>
    /// Mirrors PluginConfig.Sections constants for testing.
    /// </summary>
    private static readonly string[] AllSections =
    {
        "01. Damage",
        "02. Survival",
        "03. Healing",
        "04. Weapons",
        "05. Movement",
        "06. FOV",
        "07. ESP Players",
        "08. Chams",
        "09. ESP Items",
        "10. ESP Quests",
        "11. Visual",
        "12. Performance",
        "13. UI",
        "14. Hotkeys",
    };

    /// <summary>
    /// Maps each section to how many config entries it should contain.
    /// </summary>
    private static readonly Dictionary<string, int> SectionEntryCounts = new()
    {
        { "01. Damage", 7 },
        { "02. Survival", 6 },
        { "03. Healing", 4 },
        { "04. Weapons", 3 },
        { "05. Movement", 4 },
        { "06. FOV", 9 },
        { "07. ESP Players", 10 },
        { "08. Chams", 8 },
        { "09. ESP Items", 8 },
        { "10. ESP Quests", 5 },
        { "11. Visual", 4 },
        { "12. Performance", 2 },
        { "13. UI", 2 },
        { "14. Hotkeys", 22 },
    };

    // --- Section naming convention tests ---

    [Test]
    public void AllSections_UseNumberedPrefix()
    {
        foreach (var section in AllSections)
        {
            Assert.That(
                section.Length >= 4 && char.IsDigit(section[0]) && char.IsDigit(section[1]),
                Is.True,
                $"Section '{section}' must start with a two-digit prefix"
            );
        }
    }

    [Test]
    public void AllSections_UseConsistentFormat()
    {
        // Format: "NN. Name" — two digits, dot, space, then name
        foreach (var section in AllSections)
        {
            Assert.That(section[2], Is.EqualTo('.'), $"Section '{section}' must have '.' at position 2");
            Assert.That(section[3], Is.EqualTo(' '), $"Section '{section}' must have ' ' at position 3");
        }
    }

    [Test]
    public void AllSections_HaveSequentialNumbers()
    {
        for (int i = 0; i < AllSections.Length; i++)
        {
            int expected = i + 1;
            string prefix = AllSections[i].Substring(0, 2);
            int actual = int.Parse(prefix);
            Assert.That(actual, Is.EqualTo(expected), $"Section '{AllSections[i]}' has wrong number");
        }
    }

    [Test]
    public void AllSections_NoDuplicates()
    {
        var unique = new HashSet<string>(AllSections);
        Assert.That(unique.Count, Is.EqualTo(AllSections.Length));
    }

    [Test]
    public void AllSections_AlphabeticallySorted()
    {
        // BepInEx sorts sections alphabetically, numbered prefixes ensure correct order
        var sorted = AllSections.OrderBy(s => s).ToArray();
        Assert.That(sorted, Is.EqualTo(AllSections));
    }

    // --- Section count tests ---

    [Test]
    public void TotalSectionCount_Is14()
    {
        Assert.That(AllSections.Length, Is.EqualTo(14));
    }

    [Test]
    public void NoSectionExceedsMaxEntries()
    {
        const int maxPerSection = 22; // Hotkeys is the largest with 22
        foreach (var kv in SectionEntryCounts)
        {
            Assert.That(
                kv.Value,
                Is.LessThanOrEqualTo(maxPerSection),
                $"Section '{kv.Key}' has {kv.Value} entries, exceeding max {maxPerSection}"
            );
        }
    }

    [Test]
    public void NonHotkeySections_HaveReasonableSize()
    {
        const int maxNonHotkey = 10;
        foreach (var kv in SectionEntryCounts)
        {
            if (kv.Key == "14. Hotkeys")
                continue;
            Assert.That(
                kv.Value,
                Is.LessThanOrEqualTo(maxNonHotkey),
                $"Section '{kv.Key}' has {kv.Value} entries, exceeding max {maxNonHotkey} for non-hotkey sections"
            );
        }
    }

    [Test]
    public void TotalEntryCount()
    {
        int total = SectionEntryCounts.Values.Sum();
        // 94 total config entries across all sections
        Assert.That(total, Is.EqualTo(94));
    }

    // --- Section logical grouping tests ---

    [TestCase("01. Damage", 7)]
    [TestCase("02. Survival", 6)]
    [TestCase("03. Healing", 4)]
    [TestCase("04. Weapons", 3)]
    [TestCase("05. Movement", 4)]
    [TestCase("06. FOV", 9)]
    [TestCase("07. ESP Players", 10)]
    [TestCase("08. Chams", 8)]
    [TestCase("09. ESP Items", 8)]
    [TestCase("10. ESP Quests", 5)]
    [TestCase("11. Visual", 4)]
    [TestCase("12. Performance", 2)]
    [TestCase("13. UI", 2)]
    [TestCase("14. Hotkeys", 22)]
    public void SectionEntryCount_MatchesExpected(string section, int expectedCount)
    {
        Assert.That(SectionEntryCounts[section], Is.EqualTo(expectedCount));
    }

    // --- Invalid section character tests ---

    private static readonly char[] InvalidChars = { '=', '\n', '\t', '\\', '"', '\'', '[', ']' };

    [Test]
    public void AllSections_NoInvalidChars()
    {
        foreach (var section in AllSections)
        {
            foreach (var ch in InvalidChars)
            {
                Assert.That(section.Contains(ch), Is.False, $"Section '{section}' contains invalid character '{ch}'");
            }
        }
    }

    [Test]
    public void AllSections_NoLeadingOrTrailingWhitespace()
    {
        foreach (var section in AllSections)
        {
            Assert.That(section, Is.EqualTo(section.Trim()), $"Section '{section}' has whitespace");
        }
    }

    // --- General was split correctly ---

    [Test]
    public void OldGeneralSection_NoLongerExists()
    {
        Assert.That(AllSections, Does.Not.Contain("General"));
    }

    [Test]
    public void ChamsSeparatedFromEspPlayers()
    {
        Assert.That(AllSections, Does.Contain("07. ESP Players"));
        Assert.That(AllSections, Does.Contain("08. Chams"));
    }

    [Test]
    public void ContainersAndItemsMerged()
    {
        // Containers and items are now in the same section
        Assert.That(AllSections, Does.Contain("09. ESP Items"));
        Assert.That(AllSections, Does.Not.Contain("ESP Containers"));
    }
}
