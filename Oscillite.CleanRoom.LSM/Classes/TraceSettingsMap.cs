namespace Oscillite.CleanRoom.LSM
{
    // These field names are embedded in the binary files as UTF-16 strings.
    // Their order and grouping were empirically recovered by inspecting real-world files
    // and identifying sequential ASCII/UTF-16 encoded strings.
    // The arrangement reflects the most common structure observed across multiple samples.

    public static class TraceSettingsMap
    {
        public static string[] GetFieldNames() => new[]
        {
            "m_Id", "m_Displayed", "m_InputType", "m_Position", "m_Scale", "m_Inverted",
            "m_LabelDisplayed", "m_LabelText", "m_SettingsDisplayed", "m_PositionIncrement",
            "m_MinPosition", "m_MaxPosition", "m_UnitsPerVolt", "m_Precision", "m_UnitsText",
            "m_bInvertedByChangeInProbeType", "m_AutoScale", "m_RasterSpacing", "m_CylinderCount", "m_CylinderDisplayed"
        };
    }
}
