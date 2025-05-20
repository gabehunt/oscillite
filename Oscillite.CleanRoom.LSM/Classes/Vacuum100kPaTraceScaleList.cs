using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class Vacuum100kPaTraceScaleList : TraceScaleList
	{
		public Vacuum100kPaTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_VACUUM_100, hasAutoScale: false)
		{
			Add(new TraceScale(20.0, TraceScale.Units.kPa, probeGain));
			Add(new TraceScale(40.0, TraceScale.Units.kPa, probeGain));
			Add(new TraceScale(70.0, TraceScale.Units.kPa, probeGain));
			base.SelectedScale = base[Count - 1];
        }
	}
}
