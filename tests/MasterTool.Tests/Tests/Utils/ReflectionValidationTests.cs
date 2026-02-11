using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Utils;

/// <summary>
/// Validates that all reflection-based member lookups in the codebase are registered
/// in <c>ReflectionHelper.KnownMembers</c> and use correct member names.
///
/// These tests provide layered defense against obfuscated member name mismatches:
/// <list type="bullet">
///   <item>Source scanning ensures raw reflection calls go through ReflectionHelper</item>
///   <item>Source scanning ensures Harmony <c>___param</c> injections are registered</item>
///   <item>DLL metadata validation (when libs/ present) verifies names against the game assembly</item>
/// </list>
/// </summary>
[TestFixture]
public class ReflectionValidationTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string ClientSrcDir = Path.Combine(RepoRoot, "src", "MasterTool");

    private static string FindRepoRoot()
    {
        var dir = TestContext.CurrentContext.TestDirectory;
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir, ".git")))
            {
                return dir;
            }
            dir = Directory.GetParent(dir)?.FullName;
        }
        return TestContext.CurrentContext.TestDirectory;
    }

    #region Source-scanning: raw reflection calls use ReflectionHelper

    /// <summary>
    /// Files that are allowed to contain raw reflection calls.
    /// ReflectionHelper.cs wraps them by design; ReflectionUtils.cs is intentionally
    /// dynamic for quest condition probing.
    /// </summary>
    private static readonly HashSet<string> ReflectionWhitelist = new() { "ReflectionHelper.cs", "ReflectionUtils.cs" };

    [Test]
    public void AllReflectionCalls_ShouldUseReflectionHelper()
    {
        var csFiles = Directory.GetFiles(ClientSrcDir, "*.cs", SearchOption.AllDirectories);

        // Match raw reflection patterns on game types:
        // - typeof(X).GetField(  / .GetProperty(
        // - AccessTools.Field(  / AccessTools.Property(
        var rawPatterns = new[]
        {
            new Regex(@"\.GetField\s*\(", RegexOptions.Compiled),
            new Regex(@"\.GetProperty\s*\(", RegexOptions.Compiled),
            new Regex(@"AccessTools\.(Field|Property)\s*\(", RegexOptions.Compiled),
        };

        var violations = new List<string>();

        foreach (var file in csFiles)
        {
            string fileName = Path.GetFileName(file);
            if (ReflectionWhitelist.Contains(fileName))
            {
                continue;
            }

            string content = File.ReadAllText(file);
            foreach (var pattern in rawPatterns)
            {
                var matches = pattern.Matches(content);
                if (matches.Count > 0)
                {
                    string relPath = Path.GetRelativePath(RepoRoot, file);
                    violations.Add(relPath + " (" + matches.Count + " " + pattern + " call(s))");
                }
            }
        }

        Assert.That(
            violations,
            Is.Empty,
            "Found raw reflection calls that should use ReflectionHelper instead:\n  " + string.Join("\n  ", violations)
        );
    }

    #endregion

    #region Source-scanning: Harmony ___param field names are registered

    [Test]
    public void AllHarmonyFieldInjections_AreRegisteredInReflectionHelper()
    {
        var csFiles = Directory.GetFiles(ClientSrcDir, "*.cs", SearchOption.AllDirectories);

        // Match parameters like: ___Player, ___Boss_1, ____allPlayers
        // Pattern: exactly 3 or 4 leading underscores followed by a letter
        var paramPattern = new Regex(@"(?<!\w)(_{3,4})([A-Za-z]\w*)", RegexOptions.Compiled);
        // Match PatchPrefix/PatchPostfix method signatures
        var patchMethodPattern = new Regex(@"static\s+\w+\s+(?:Patch(?:Prefix|Postfix)|Block\w+)\s*\(([^)]+)\)", RegexOptions.Compiled);

        var reflectionHelperSource = File.ReadAllText(Path.Combine(ClientSrcDir, "Utils", "ReflectionHelper.cs"));

        var unregistered = new List<string>();

        foreach (var file in csFiles)
        {
            string content = File.ReadAllText(file);
            var methodMatches = patchMethodPattern.Matches(content);

            foreach (Match methodMatch in methodMatches)
            {
                string paramList = methodMatch.Groups[1].Value;
                var fieldParams = paramPattern.Matches(paramList);

                foreach (Match fieldParam in fieldParams)
                {
                    string underscores = fieldParam.Groups[1].Value;
                    string identPart = fieldParam.Groups[2].Value;

                    // Reconstruct the actual field name:
                    // ___Player → field "Player" (3 underscores stripped by Harmony)
                    // ____allPlayers → field "_allPlayers" (3 underscores stripped, one remains)
                    string fieldName = underscores.Length > 3 ? new string('_', underscores.Length - 3) + identPart : identPart;

                    // Skip __instance and __result — these are Harmony builtins, not field injections
                    if (fieldName == "instance" || fieldName == "result" || fieldName == "_instance" || fieldName == "_result")
                    {
                        continue;
                    }

                    // Check if the field name appears in ReflectionHelper's KnownMembers
                    if (!reflectionHelperSource.Contains("\"" + fieldName + "\""))
                    {
                        string relPath = Path.GetRelativePath(RepoRoot, file);
                        unregistered.Add(relPath + ": " + underscores + identPart + " (field: " + fieldName + ")");
                    }
                }
            }
        }

        Assert.That(
            unregistered,
            Is.Empty,
            "Found Harmony field injections not registered in ReflectionHelper.KnownMembers:\n  "
                + string.Join("\n  ", unregistered)
                + "\n\nAdd entries to ReflectionHelper.KnownMembers for validation."
        );
    }

    #endregion

    #region DLL metadata validation

    /// <summary>
    /// Parses <c>ReflectionHelper.KnownMembers</c> from source code to extract
    /// (type simple name, member name, kind) tuples without requiring game assembly references.
    /// </summary>
    private static List<(string TypeName, string MemberName, string Kind, string Context)> ParseKnownMembersFromSource()
    {
        var helperPath = Path.Combine(ClientSrcDir, "Utils", "ReflectionHelper.cs");
        string source = File.ReadAllText(helperPath);

        // Match entries like:
        //   (typeof(Player), "ApplyDamageInfo", MemberKind.Method, "DamagePatches — damage blocking"),
        var entryPattern = new Regex(
            @"typeof\((?:[\w.]+\.)?(\w+)\),\s*""([^""]+)"",\s*MemberKind\.(\w+),\s*""([^""]+)""",
            RegexOptions.Compiled
        );

        var results = new List<(string, string, string, string)>();
        foreach (Match m in entryPattern.Matches(source))
        {
            results.Add((m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Value));
        }
        return results;
    }

    [Test]
    public void KnownMembers_MatchGameAssemblyMetadata()
    {
        var dllPath = Path.Combine(RepoRoot, "libs", "Assembly-CSharp.dll");
        if (!File.Exists(dllPath))
        {
            Assert.Ignore("Skipped: libs/Assembly-CSharp.dll not found (copy from SPT install for full validation)");
        }

        var knownMembers = ParseKnownMembersFromSource();
        Assert.That(knownMembers, Is.Not.Empty, "Failed to parse KnownMembers from ReflectionHelper.cs source");

        // Build dictionaries of type name → set of field/method names from the DLL
        var typeFields = new Dictionary<string, HashSet<string>>();
        var typeMethods = new Dictionary<string, HashSet<string>>();

        using var stream = File.OpenRead(dllPath);
        using var peReader = new PEReader(stream);
        var metadata = peReader.GetMetadataReader();

        foreach (var typeDef in metadata.TypeDefinitions)
        {
            var type = metadata.GetTypeDefinition(typeDef);
            string typeName = metadata.GetString(type.Name);

            if (!typeFields.ContainsKey(typeName))
            {
                typeFields[typeName] = new HashSet<string>();
                typeMethods[typeName] = new HashSet<string>();
            }

            foreach (var fieldHandle in type.GetFields())
            {
                var field = metadata.GetFieldDefinition(fieldHandle);
                typeFields[typeName].Add(metadata.GetString(field.Name));
            }

            foreach (var methodHandle in type.GetMethods())
            {
                var method = metadata.GetMethodDefinition(methodHandle);
                typeMethods[typeName].Add(metadata.GetString(method.Name));
            }
        }

        var failures = new List<string>();
        foreach (var (typeName, memberName, kind, context) in knownMembers)
        {
            // Skip backing fields
            if (memberName.StartsWith("<"))
            {
                continue;
            }

            if (kind == "Field" || kind == "Property")
            {
                // Properties compile to getter/setter methods + backing field.
                // Check fields first; for properties, also check for get_/set_ methods.
                if (typeFields.TryGetValue(typeName, out var fields))
                {
                    bool foundAsField = fields.Contains(memberName);
                    bool foundAsPropBacking = fields.Contains("<" + memberName + ">k__BackingField");
                    bool foundAsPropMethod = typeMethods.TryGetValue(typeName, out var methods) && methods.Contains("get_" + memberName);

                    if (!foundAsField && !foundAsPropBacking && !foundAsPropMethod)
                    {
                        string available = string.Join(", ", fields.OrderBy(f => f).Take(20));
                        failures.Add(
                            kind
                                + " '"
                                + memberName
                                + "' not found on "
                                + typeName
                                + " ("
                                + context
                                + "). Sample fields: ["
                                + available
                                + "]"
                        );
                    }
                }
                else
                {
                    TestContext.WriteLine("INFO: Type '" + typeName + "' not found in DLL (may be runtime-resolved). Skipping.");
                }
            }
            else if (kind == "Method")
            {
                if (typeMethods.TryGetValue(typeName, out var methods))
                {
                    if (!methods.Contains(memberName))
                    {
                        string available = string.Join(", ", methods.OrderBy(m => m).Take(20));
                        failures.Add(
                            "Method '"
                                + memberName
                                + "' not found on "
                                + typeName
                                + " ("
                                + context
                                + "). Sample methods: ["
                                + available
                                + "]"
                        );
                    }
                }
                else
                {
                    TestContext.WriteLine("INFO: Type '" + typeName + "' not found in DLL (may be runtime-resolved). Skipping.");
                }
            }
        }

        Assert.That(
            failures,
            Is.Empty,
            "Member names in ReflectionHelper.KnownMembers do not match game DLL:\n  " + string.Join("\n  ", failures)
        );
    }

    #endregion

    #region Registry completeness

    [Test]
    public void KnownMembers_HasExpectedMinimumEntryCount()
    {
        var knownMembers = ParseKnownMembersFromSource();
        Assert.That(knownMembers.Count, Is.GreaterThanOrEqualTo(10), "ReflectionHelper.KnownMembers should have at least 10 entries");
    }

    #endregion
}
