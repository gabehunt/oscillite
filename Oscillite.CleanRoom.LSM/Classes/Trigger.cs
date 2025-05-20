using System;


namespace Oscillite.CleanRoom.LSM
{
	
public class Trigger
    {
        public TriggerSource Source { get; set; }
        public TriggerModeType Mode { get; set; } = TriggerModeType.Normal;
        public Position<double> Level { get; set; } = new Position<double>(0.0);
        public Position<double> Delay { get; set; } = new Position<double>(0.0);
        public SlopeType Slope { get; set; } = SlopeType.Up;
        public int CylinderNumber { get; set; } = 1;
        public Trace AssociatedTrace { get; set; }

        public Trigger(SourceType sourceType = SourceType.None)
        {
            Source = new TriggerSource(sourceType);
            Mode = (sourceType == SourceType.Cylinder)
                ? TriggerModeType.Auto
                : TriggerModeType.Normal;
            AssociatedTrace = null;
        }

        public Trigger(Trace trace)
        {
            Source = new TriggerSource(trace);
            Mode = TriggerModeType.Auto;
            AssociatedTrace = trace;
            trace.AssociatedTrigger = this;
        }
    }
}
