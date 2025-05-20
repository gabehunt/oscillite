namespace Oscillite.CleanRoom.LSM
{
    public static class UnitsHelper
    {
        // We ran into cases where the unit gain needed to be calculated based on time
        public static void GetUnitsFromSeconds(double seconds, out string unit, out double gain)
        {
            if (seconds >= Constants.SECONDS_PER_HOUR && seconds % Constants.SECONDS_PER_HOUR == 0.0)
            {
                unit = SweepLabels.SweepHours;
                gain = 1.0 / Constants.SECONDS_PER_HOUR;
            }
            else if (seconds >= Constants.SECONDS_PER_MINUTE && seconds % Constants.SECONDS_PER_MINUTE == 0.0)
            {
                unit = SweepLabels.SweepMinutes;
                gain = 1.0 / Constants.SECONDS_PER_MINUTE;
            }
            else if (seconds >= Constants.SECONDS_PER_SECOND)
            {
                unit = SweepLabels.SweepSeconds;
                gain = 1.0; // This appears to be the base so make it 1.0
            }
            else if (seconds >= Constants.SECONDS_PER_MILLISECOND)
            {
                unit = SweepLabels.SweepMilliseconds;
                gain = Constants.MILLISECONDS_PER_SECOND;
            }
            else
            {
                unit = SweepLabels.SweepMicroseconds;
                gain = Constants.MICROSECONDS_PER_SECOND;
            }
        }
    }

}
