namespace Oscillite.CleanRoom.LSM
{
    public struct WaveformHeader
    {
        public int calibrationMultiplier;

        public int calibrationDivisor;

        public int calibrationOffset;

        public int numberOfPoints;

        public int pointSize;

        public double averageValue;

        public double minValue;

        public double maxValue;
    }
}
