namespace MasterTool.Core;

/// <summary>
/// Pure screen-space logic extracted from EspRenderer for shared testing.
/// </summary>
public static class ScreenLogic
{
    public static bool IsOnScreen(float x, float y, float screenWidth, float screenHeight, float margin)
    {
        return x >= -margin && x <= screenWidth + margin && y >= -margin && y <= screenHeight + margin;
    }
}
