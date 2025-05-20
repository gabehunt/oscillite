using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class Vacuum100inHgTraceScaleList : TraceScaleList
	{
		public Vacuum100inHgTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_VACUUM_100, hasAutoScale: false)
		{
			Add(new TraceScale(5.0, TraceScale.Units.inHg, probeGain));
			Add(new TraceScale(10.0, TraceScale.Units.inHg, probeGain));
			Add(new TraceScale(20.0, TraceScale.Units.inHg, probeGain));
			Add(new TraceScale(30.0, TraceScale.Units.inHg, probeGain));
			base.SelectedScale = base[Count - 1];
		}
	}
}
