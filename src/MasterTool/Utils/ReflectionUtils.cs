using System;
using System.Collections.Generic;
using System.Reflection;
using EFT.Quests;

namespace MasterTool.Utils
{
    /// <summary>
    /// Reflection helpers for extracting data from quest condition objects whose
    /// internal structure may vary across game versions.
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Extracts target item template IDs from a quest condition using reflection.
        /// Searches common property/field names ("target", "Target", "_target", etc.)
        /// and falls back to scanning all properties for string arrays of length 24 (MongoDB ObjectId).
        /// </summary>
        /// <param name="condition">The quest condition to inspect.</param>
        /// <returns>A list of item template ID strings found in the condition, or an empty list.</returns>
        public static List<string> GetConditionTargetItems(Condition condition)
        {
            var items = new List<string>();
            try
            {
                var conditionType = condition.GetType();

                string[] possibleNames = { "target", "Target", "_target", "targets", "Targets", "_targets" };

                foreach (var name in possibleNames)
                {
                    var prop = conditionType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (prop != null)
                    {
                        var value = prop.GetValue(condition);
                        if (TryExtractStrings(value, items))
                            return items;
                    }

                    var field = conditionType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (field != null)
                    {
                        var value = field.GetValue(condition);
                        if (TryExtractStrings(value, items))
                            return items;
                    }
                }

                foreach (var prop in conditionType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    try
                    {
                        var value = prop.GetValue(condition);
                        if (value is string[] strArray && strArray.Length > 0)
                        {
                            if (strArray[0].Length == 24)
                            {
                                items.AddRange(strArray);
                                return items;
                            }
                        }
                    }
                    catch { }
                }
            }
            catch (Exception) { }
            return items;
        }

        /// <summary>
        /// Attempts to extract string values from an object that may be a string array,
        /// an <see cref="IEnumerable{String}"/>, a generic enumerable containing strings,
        /// or a single string. Extracted values are added to <paramref name="items"/>.
        /// </summary>
        /// <param name="value">The object to extract strings from. May be null.</param>
        /// <param name="items">The list to populate with extracted string values.</param>
        /// <returns>True if at least one string was extracted; otherwise false.</returns>
        public static bool TryExtractStrings(object value, List<string> items)
        {
            if (value == null)
                return false;

            if (value is string[] strArray)
            {
                items.AddRange(strArray);
                return items.Count > 0;
            }

            if (value is IEnumerable<string> strEnumerable)
            {
                items.AddRange(strEnumerable);
                return items.Count > 0;
            }

            if (value is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is string str)
                    {
                        items.Add(str);
                    }
                }
                return items.Count > 0;
            }

            if (value is string singleStr && !string.IsNullOrEmpty(singleStr))
            {
                items.Add(singleStr);
                return true;
            }

            return false;
        }
    }
}
