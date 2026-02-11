using System;
using System.Reflection;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using MasterTool.Plugin;

namespace MasterTool.Utils
{
    /// <summary>
    /// Centralized reflection helpers with validation and error logging.
    ///
    /// BSG's deobfuscator uses inconsistent naming conventions that change between
    /// game updates. All lookups are case-sensitive and return <c>null</c> silently
    /// on mismatch. This helper logs errors loudly so member renames after game
    /// updates are caught immediately at startup.
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// The kind of member being looked up via reflection.
        /// </summary>
        public enum MemberKind
        {
            Field,
            Property,
            Method,
        }

        /// <summary>
        /// Known reflection member lookups. Validated at startup by
        /// <see cref="ValidateAllReflectionMembers"/>.
        /// Each entry: (target type, member name, kind, description of usage).
        ///
        /// Only includes string-literal lookups on game types. Members accessed
        /// via <c>nameof()</c> are compile-time safe and excluded. Self-referencing
        /// reflection (e.g. <c>typeof(DamagePatches).GetMethod</c>) is excluded.
        /// Chained lookups where the type is resolved at runtime are also excluded.
        /// </summary>
        private static readonly (Type Type, string MemberName, MemberKind Kind, string Context)[] KnownMembers = new[]
        {
            // Field lookups
            (typeof(Player), "ProceduralWeaponAnimation", MemberKind.Field, "VisionFeature — ADS detection"),
            // Property lookups
            (typeof(Player), "AIData", MemberKind.Property, "PeacefulPatches — bot AI access"),
            // Method lookups (string-literal, not nameof)
            (typeof(Player), "ApplyDamageInfo", MemberKind.Method, "DamagePatches — damage blocking"),
            // Note: Player.ApplyDamage is a belt-and-suspenders fallback that may not
            // exist in all game versions. Not registered — TryPatchDamageMethod handles it.
            (typeof(ActiveHealthController), "DoBleed", MemberKind.Method, "DamagePatches — bleed blocking"),
            (typeof(BotsGroup), "AddEnemy", MemberKind.Method, "PeacefulPatches — block enemy group"),
            (typeof(BotMemoryClass), "AddEnemy", MemberKind.Method, "PeacefulPatches — block bot memory"),
            (typeof(EnemyInfo), "ShallKnowEnemy", MemberKind.Method, "PeacefulPatches — knowledge override"),
            (typeof(EnemyInfo), "ShallKnowEnemyLate", MemberKind.Method, "PeacefulPatches — late knowledge"),
            (typeof(InventoryEquipment), "smethod_1", MemberKind.Method, "NoWeightFeature — weight calc"),
            // Harmony ___param field injections
            (typeof(ActiveHealthController), "Player", MemberKind.Field, "DamagePatches ___Player"),
        };

        private const BindingFlags AllInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Replacement for raw <c>typeof(T).GetField(name)</c> that logs an error
        /// when the field is not found instead of silently returning <c>null</c>.
        /// </summary>
        public static FieldInfo RequireField(Type type, string fieldName, string context = null)
        {
            FieldInfo field = type.GetField(fieldName, AllInstance);
            if (field == null)
            {
                LogMissing("Field", fieldName, type, context);
            }
            return field;
        }

        /// <summary>
        /// Replacement for raw <c>typeof(T).GetProperty(name)</c> that logs an error
        /// when the property is not found instead of silently returning <c>null</c>.
        /// </summary>
        public static PropertyInfo RequireProperty(Type type, string propertyName, string context = null)
        {
            PropertyInfo prop = type.GetProperty(propertyName, AllInstance);
            if (prop == null)
            {
                LogMissing("Property", propertyName, type, context);
            }
            return prop;
        }

        /// <summary>
        /// Replacement for raw <c>typeof(T).GetMethod(name)</c> or
        /// <c>AccessTools.Method(type, name)</c> that logs an error when the method
        /// is not found instead of silently returning <c>null</c>.
        /// </summary>
        public static MethodInfo RequireMethod(Type type, string methodName, string context = null)
        {
            MethodInfo method = type.GetMethod(methodName, AllInstance);
            if (method == null)
            {
                LogMissing("Method", methodName, type, context);
            }
            return method;
        }

        /// <summary>
        /// Validates all entries in <see cref="KnownMembers"/> against the loaded game assemblies.
        /// Call once during plugin startup (after game types are loaded).
        /// </summary>
        /// <returns>The number of validation failures (0 = all OK).</returns>
        public static int ValidateAllReflectionMembers()
        {
            int failures = 0;
            foreach (var (type, memberName, kind, context) in KnownMembers)
            {
                bool found = kind switch
                {
                    MemberKind.Field => type.GetField(memberName, AllInstance) != null,
                    MemberKind.Property => type.GetProperty(memberName, AllInstance) != null,
                    MemberKind.Method => type.GetMethod(memberName, AllInstance) != null,
                    _ => false,
                };

                if (!found)
                {
                    MasterToolPlugin.Log?.LogError(
                        "[ReflectionHelper] Validation FAILED: "
                            + kind
                            + " '"
                            + memberName
                            + "' not found on "
                            + type.FullName
                            + " ("
                            + context
                            + "). This member was likely renamed in a game update."
                    );
                    failures++;
                }
            }

            if (failures == 0)
            {
                MasterToolPlugin.Log?.LogInfo(
                    "[ReflectionHelper] Validation passed: all " + KnownMembers.Length + " member lookups verified."
                );
            }
            else
            {
                MasterToolPlugin.Log?.LogError(
                    "[ReflectionHelper] Validation: " + failures + " of " + KnownMembers.Length + " member lookups FAILED."
                );
            }

            return failures;
        }

        private static void LogMissing(string kind, string name, Type type, string context)
        {
            string msg = "[ReflectionHelper] " + kind + " '" + name + "' not found on " + type.FullName + ".";
            if (context != null)
            {
                msg += " Context: " + context + ".";
            }
            msg += " This member may have been renamed in a game update.";
            MasterToolPlugin.Log?.LogError(msg);
        }
    }
}
