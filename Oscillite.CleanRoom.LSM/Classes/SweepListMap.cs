namespace Oscillite.CleanRoom.LSM
{
    public static class SweepListMap
    {
        public static readonly double[] AllSweepSeconds = new[]
        {
            0.00005, 0.0001, 0.0002, 0.0005,
            0.001, 0.002, 0.005, 0.01, 0.02, 0.05,
            0.1, 0.2, 0.5,
            10.0, 20.0, 30.0,
            60.0, 120.0, 180.0, 240.0, 300.0, 3600.0
        };

        public static SweepList GetSweepList(double minSeconds, double maxSeconds, double? defaultSeconds = null)
        {
            var sweepList = new SweepList();

            foreach (double seconds in AllSweepSeconds)
            {
                if (seconds >= minSeconds && seconds <= maxSeconds)
                    sweepList.Add(new Sweep { Seconds = seconds });
            }

            if (defaultSeconds.HasValue)
            {
                sweepList.DefaultSweep = sweepList[defaultSeconds.Value];
            }

            if (sweepList.DefaultSweep == null && sweepList.Count > 0)
            {
                sweepList.DefaultSweep = sweepList[sweepList.Count - 1];
            }

            return sweepList;
        }
    }
}
