namespace Oscillite.CleanRoom.LSM
{
    // These field names are embedded in the binary files as UTF-16 strings.
    // Their order and grouping were empirically recovered by inspecting real-world files
    // and identifying sequential ASCII/UTF-16 encoded strings.
    // The arrangement reflects the most common structure observed across multiple samples.

    public static class ChannelSettingsMap
    {
        public static string[] GetFieldNames() => new[]
        {
            "m_ChannelId", "m_ChannelProbeType", "m_ChannelCouplingType", "m_ChannelScale", "m_ChannelEnabled",
            "m_PeakDetectState", "m_ChannelFilter", "m_ThresholdValue", "m_ThresholdSlope", "m_PulseScale",
            "m_bPeakDetectByChangeInProbeType", "m_ChannelLeadResistance"
        };
    }

}
