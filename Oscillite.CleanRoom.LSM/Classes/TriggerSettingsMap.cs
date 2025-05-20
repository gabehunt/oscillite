namespace Oscillite.CleanRoom.LSM
{
    // These field names are embedded in the binary files as UTF-16 strings.
    // Their order and grouping were empirically recovered by inspecting real-world files
    // and identifying sequential ASCII/UTF-16 encoded strings.
    // The arrangement reflects the most common structure observed across multiple samples.

    public static class TriggerSettingsMap
    {
        public static string[] GetFieldNames() => new[]
        {
            "m_TriggerSource", "m_TriggerMode", "m_TriggerDelay", "m_TriggerLevel", "m_TriggerSlope",
            "m_TriggerDisplayed", "m_TriggerDelayIncrement", "m_TriggerLevelIncrement",
            "m_TriggerMinDelay", "m_TriggerMaxDelay", "m_TriggerMinLevel", "m_TriggerMaxLevel",
            "m_TriggerActiveColor", "m_TriggerCylinder"
        };
    }

}
