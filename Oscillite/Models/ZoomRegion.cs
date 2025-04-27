namespace Oscillite
{
    public class ZoomRegion
    {
        public float TimeStart;
        public float TimeEnd;

        public float ViewportTopNorm;
        public float ViewportHeightNorm;

        public float? VoltageMin;
        public float? VoltageMax;

        public float TimeSpan => TimeEnd - TimeStart;
        public bool IsEmpty => TimeSpan <= 0 || ViewportHeightNorm <= 0;

        // 🔧 Add these computed properties to fix your errors:
        public float Left => TimeStart;
        public float Right => TimeEnd;
        public float Width => TimeSpan;
        public float Top => ViewportTopNorm;
        public float Height => ViewportHeightNorm;
    }
}
