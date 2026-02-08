using System.Collections;
using System.Collections.Generic;

namespace MasterTool.Core;

public static class StringExtraction
{
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
        {
            return false;
        }

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

        if (value is IEnumerable enumerable)
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
