namespace Oscillite.CleanRoom.LSM
{
    // These field names are embedded in the binary files as UTF-16 strings.
    // Their order and grouping were empirically recovered by inspecting real-world files
    // and identifying sequential ASCII/UTF-16 encoded strings.
    // The arrangement reflects the most common structure observed across multiple samples.

    public static class MainSettingsMap
    {
        public static string[] GetFieldNames() => new[]
        {
            "m_TraceSelection", "m_SweepRate", "m_TriggerSelection", "m_CursorSelection", "m_ViewType", "m_bGridOn",
            "m_TriggerTextDisplay", "m_ScaleLabelsDisplay", "m_ScaleDisplayMode", "m_BufferPointIndex",
            "m_BufferPointCount", "m_FrozenBufferPointIndex", "m_SweepWidth", "m_IgnitionType", "m_EngineStroke"
        };
    }
}
