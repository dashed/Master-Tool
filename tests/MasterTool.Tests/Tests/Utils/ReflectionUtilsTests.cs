using System.Collections;
using System.Collections.Generic;
using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Utils;

[TestFixture]
public class ReflectionUtilsTests
{
    [Test]
    public void TryExtractStrings_StringArray_ReturnsAllStrings()
    {
        var items = new List<string>();
        string[] input = { "alpha", "bravo", "charlie" };

        bool result = StringExtraction.TryExtractStrings(input, items);

        Assert.That(result, Is.True);
        Assert.That(items, Is.EqualTo(new[] { "alpha", "bravo", "charlie" }));
    }

    [Test]
    public void TryExtractStrings_IEnumerableString_ReturnsAllStrings()
    {
        var items = new List<string>();
        IEnumerable<string> input = new List<string> { "one", "two" };

        bool result = StringExtraction.TryExtractStrings(input, items);

        Assert.That(result, Is.True);
        Assert.That(items, Is.EqualTo(new[] { "one", "two" }));
    }

    [Test]
    public void TryExtractStrings_SingleString_ReturnsFalse_DueToIEnumerableCharMatch()
    {
        var items = new List<string>();

        bool result = StringExtraction.TryExtractStrings("hello", items);

        // A string is IEnumerable<char> which matches the IEnumerable branch.
        // That branch iterates chars (not strings), finds no string items,
        // and returns false. The single-string branch never executes.
        Assert.That(result, Is.False);
        Assert.That(items, Has.Count.EqualTo(0));
    }

    [Test]
    public void TryExtractStrings_Null_ReturnsFalse()
    {
        var items = new List<string>();

        bool result = StringExtraction.TryExtractStrings(null, items);

        Assert.That(result, Is.False);
        Assert.That(items, Is.Empty);
    }

    [Test]
    public void TryExtractStrings_EmptyArray_ReturnsFalse()
    {
        var items = new List<string>();
        string[] input = System.Array.Empty<string>();

        bool result = StringExtraction.TryExtractStrings(input, items);

        Assert.That(result, Is.False);
        Assert.That(items, Is.Empty);
    }

    [Test]
    public void TryExtractStrings_MixedEnumerable_ExtractsOnlyStrings()
    {
        var items = new List<string>();
        var input = new ArrayList { "hello", 42, "world", 3.14, null };

        bool result = StringExtraction.TryExtractStrings(input, items);

        Assert.That(result, Is.True);
        Assert.That(items, Is.EqualTo(new[] { "hello", "world" }));
    }

    [Test]
    public void TryExtractStrings_MixedEnumerableNoStrings_ReturnsFalse()
    {
        var items = new List<string>();
        var input = new ArrayList { 1, 2, 3.0 };

        bool result = StringExtraction.TryExtractStrings(input, items);

        Assert.That(result, Is.False);
        Assert.That(items, Is.Empty);
    }

    [Test]
    public void TryExtractStrings_SingleElementArray_ReturnsTrue()
    {
        var items = new List<string>();
        string[] input = { "only" };

        bool result = StringExtraction.TryExtractStrings(input, items);

        Assert.That(result, Is.True);
        Assert.That(items, Is.EqualTo(new[] { "only" }));
    }

    [Test]
    public void TryExtractStrings_NonStringNonEnumerable_ReturnsFalse()
    {
        var items = new List<string>();

        bool result = StringExtraction.TryExtractStrings(42, items);

        Assert.That(result, Is.False);
        Assert.That(items, Is.Empty);
    }
}
