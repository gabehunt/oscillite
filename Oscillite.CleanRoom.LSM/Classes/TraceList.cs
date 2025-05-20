using System;
using System.Collections.Generic;

namespace Oscillite.CleanRoom.LSM
{
	
public class TraceList : List<Trace>
    {
        public TraceList(ScopeType scopeType, TraceCount traceCount, TraceProbeList probes)
        {
            for (int i = 0; i < (int)traceCount; i++)
            {
                Trace trace = new Trace();

                trace.Id = i + 1;
                trace.Probe = probes[TraceProbeType.TRACE_PROBE_VOLTS];
                trace.Scale = trace.Probe.TraceScaleList.SelectedScale;
                trace.Position = new Position<double>(trace.Scale.FullScaleValue / 10.0);
                trace.RasterSpacing = new Position<double>(trace.Scale.FullScaleValue / 20.0);

                trace.ThresholdSlope = new ThresholdSlope();

                if (trace.Probe.IsCalculatedProbe())
                {
                    if (trace.Probe.Type == TraceProbeType.TRACE_PROBE_INJECTOR_PULSE_WIDTH)
                    {
                        trace.ThresholdSlope = new ThresholdSlope();
                    }
                    else
                    {
                        trace.ThresholdSlope = new ThresholdSlope();
                    }
                }
                else
                {
                    trace.ThresholdSlope = new ThresholdSlope();
                }

                switch (scopeType)
                {
                    case ScopeType.ScopeType_GraphingMeter:
                        trace.PeakDetect = PeakDetectStatus.On;
                        trace.Filter = FilterState.On;
                        break;
                }

                Add(trace);
            }
        }
    }
}
