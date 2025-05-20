using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class OhmsTraceScaleList : TraceScaleList
	{
		public OhmsTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_OHMS, hasAutoScale: true)
		{
			Add(new TraceScale(40.0, TraceScale.Units.Ohms, probeGain));
			Add(new TraceScale(400.0, TraceScale.Units.Ohms, probeGain));
			Add(new TraceScale(4000.0, TraceScale.Units.kOhms, probeGain));
			Add(new TraceScale(40000.0, TraceScale.Units.kOhms, probeGain));
			Add(new TraceScale(400000.0, TraceScale.Units.kOhms, probeGain));
			Add(new TraceScale(4000000.0, TraceScale.Units.MOhms, probeGain));
			Add(new TraceScale(40000000.0, TraceScale.Units.MOhms, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
