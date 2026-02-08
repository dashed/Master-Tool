namespace MasterTool.Core;

/// <summary>
/// Pure logic for determining which body parts are protected by Keep 1 Health.
/// </summary>
public static class BodyPartProtection
{
    public const string SelectionAll = "All";
    public const string SelectionHeadAndThorax = "Head And Thorax";
    public const string SelectionVitals = "Vitals";
    public const string SelectionCustom = "Custom";

    public static readonly string[] AllSelections = { SelectionAll, SelectionHeadAndThorax, SelectionVitals, SelectionCustom };

    public const int BodyPartCount = 7;

    /// <summary>
    /// Determines whether a body part should be protected from lethal damage.
    /// </summary>
    /// <param name="selection">The active protection mode.</param>
    /// <param name="part">The body part being damaged.</param>
    /// <param name="customParts">Per-body-part toggles used when selection is "Custom". Index by (int)BodyPart.</param>
    public static bool ShouldProtect(string selection, BodyPart part, bool[] customParts)
    {
        switch (selection)
        {
            case SelectionAll:
                return true;

            case SelectionHeadAndThorax:
                return part == BodyPart.Head || part == BodyPart.Chest;

            case SelectionVitals:
                return part == BodyPart.Head || part == BodyPart.Chest || part == BodyPart.Stomach;

            case SelectionCustom:
                int index = (int)part;
                if (customParts == null || index < 0 || index >= customParts.Length)
                    return false;
                return customParts[index];

            default:
                return false;
        }
    }

    /// <summary>
    /// Cycles to the next selection mode in the list.
    /// </summary>
    public static string CycleSelection(string current)
    {
        for (int i = 0; i < AllSelections.Length; i++)
        {
            if (AllSelections[i] == current)
                return AllSelections[(i + 1) % AllSelections.Length];
        }
        return AllSelections[0];
    }
}
