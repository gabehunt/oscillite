namespace Oscillite.CleanRoom.LSM
{
    // These field names are embedded in the binary files as UTF-16 strings.
    // Their order and grouping were empirically recovered by inspecting real-world files
    // and identifying sequential ASCII/UTF-16 encoded strings.
    // The arrangement reflects the most common structure observed across multiple samples.
        
    public static class CursorSettingsMap
    {
        public static string[] GetFieldNames() => new[]
        {
            "m_CursorId", "m_CursorPosition", "m_CursorDisplayed", "m_CursorSelected",
            "m_CursorPositionIncrement", "m_CursorMinPosition", "m_CursorMaxPosition", "m_CursorActiveColor"
        };
    }
}
