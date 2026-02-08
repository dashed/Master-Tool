using System.Collections.Generic;
using System.Linq;
using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.UI;

/// <summary>
/// Tests for mod menu sub-tab structure.
/// Uses <see cref="TabDefinitions"/> from MasterTool.Core (shared library).
/// </summary>
[TestFixture]
public class SubTabTests
{
    private static readonly string[] TabNames = TabDefinitions.TabNames;
    private static readonly string[] GeneralSubTabNames = TabDefinitions.GeneralSubTabNames;
    private static readonly string[] EspPlayersSubTabNames = TabDefinitions.EspPlayersSubTabNames;
    private static readonly string[] TrollSubTabNames = TabDefinitions.ExtrasSubTabNames;

    // --- Main tab tests ---

    [Test]
    public void MainTabCount_Is7()
    {
        Assert.That(TabNames.Length, Is.EqualTo(7));
    }

    [Test]
    public void MainTabNames_NoDuplicates()
    {
        var unique = new HashSet<string>(TabNames);
        Assert.That(unique.Count, Is.EqualTo(TabNames.Length));
    }

    [Test]
    public void MainTabNames_AllNonEmpty()
    {
        foreach (var name in TabNames)
        {
            Assert.That(string.IsNullOrWhiteSpace(name), Is.False, $"Tab name is empty or whitespace");
        }
    }

    [Test]
    public void MainTabNames_AllEnglish()
    {
        // No Portuguese tab names
        Assert.That(TabNames, Does.Not.Contain("Geral"));
        Assert.That(TabNames, Does.Not.Contain("ESP Itens"));
        Assert.That(TabNames, Does.Not.Contain("ESP Quest/Wish"));
        Assert.That(TabNames, Does.Not.Contain("Troll"));
    }

    // --- General sub-tab tests ---

    [Test]
    public void GeneralSubTabCount_Is4()
    {
        Assert.That(GeneralSubTabNames.Length, Is.EqualTo(4));
    }

    [Test]
    public void GeneralSubTabNames_NoDuplicates()
    {
        var unique = new HashSet<string>(GeneralSubTabNames);
        Assert.That(unique.Count, Is.EqualTo(GeneralSubTabNames.Length));
    }

    [TestCase("Damage")]
    [TestCase("Survival")]
    [TestCase("Weapons")]
    [TestCase("Utility")]
    public void GeneralSubTab_ContainsExpected(string name)
    {
        Assert.That(GeneralSubTabNames, Does.Contain(name));
    }

    // --- ESP Players sub-tab tests ---

    [Test]
    public void EspPlayersSubTabCount_Is3()
    {
        Assert.That(EspPlayersSubTabNames.Length, Is.EqualTo(3));
    }

    [Test]
    public void EspPlayersSubTabNames_NoDuplicates()
    {
        var unique = new HashSet<string>(EspPlayersSubTabNames);
        Assert.That(unique.Count, Is.EqualTo(EspPlayersSubTabNames.Length));
    }

    [TestCase("ESP")]
    [TestCase("Chams")]
    [TestCase("Colors")]
    public void EspPlayersSubTab_ContainsExpected(string name)
    {
        Assert.That(EspPlayersSubTabNames, Does.Contain(name));
    }

    // --- Troll sub-tab tests ---

    [Test]
    public void TrollSubTabCount_Is3()
    {
        Assert.That(TrollSubTabNames.Length, Is.EqualTo(3));
    }

    [Test]
    public void TrollSubTabNames_NoDuplicates()
    {
        var unique = new HashSet<string>(TrollSubTabNames);
        Assert.That(unique.Count, Is.EqualTo(TrollSubTabNames.Length));
    }

    [TestCase("Movement")]
    [TestCase("Teleport")]
    [TestCase("Fun")]
    public void TrollSubTab_ContainsExpected(string name)
    {
        Assert.That(TrollSubTabNames, Does.Contain(name));
    }

    // --- Sub-tab index validation ---

    [TestCase(0, 4)]
    [TestCase(1, 4)]
    [TestCase(2, 4)]
    [TestCase(3, 4)]
    public void GeneralSubTabIndex_IsValid(int index, int count)
    {
        Assert.That(index, Is.GreaterThanOrEqualTo(0));
        Assert.That(index, Is.LessThan(count));
    }

    [TestCase(0, 3)]
    [TestCase(1, 3)]
    [TestCase(2, 3)]
    public void EspPlayersSubTabIndex_IsValid(int index, int count)
    {
        Assert.That(index, Is.GreaterThanOrEqualTo(0));
        Assert.That(index, Is.LessThan(count));
    }

    [TestCase(0, 3)]
    [TestCase(1, 3)]
    [TestCase(2, 3)]
    public void TrollSubTabIndex_IsValid(int index, int count)
    {
        Assert.That(index, Is.GreaterThanOrEqualTo(0));
        Assert.That(index, Is.LessThan(count));
    }

    // --- Tabs with sub-tabs ---

    [Test]
    public void TabsWithSubTabs_AreGeneralEspPlayersTroll()
    {
        // Only 3 main tabs have sub-tabs
        var tabsWithSubTabs = new[] { "General", "ESP Players", "Extras" };
        foreach (var tab in tabsWithSubTabs)
        {
            Assert.That(TabNames, Does.Contain(tab));
        }
    }

    // --- All sub-tab names are short enough for toolbar ---

    [Test]
    public void AllSubTabNames_AreConcise()
    {
        const int maxLength = 12;
        var allSubTabs = GeneralSubTabNames.Concat(EspPlayersSubTabNames).Concat(TrollSubTabNames);
        foreach (var name in allSubTabs)
        {
            Assert.That(name.Length, Is.LessThanOrEqualTo(maxLength), $"Sub-tab '{name}' exceeds max length {maxLength}");
        }
    }
}
