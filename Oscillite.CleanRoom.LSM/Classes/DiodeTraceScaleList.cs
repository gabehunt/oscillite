using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class DiodeTraceScaleList : TraceScaleList
	{
		public DiodeTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_DIODE, hasAutoScale: false)
		{
			Add(new TraceScale(2.0, TraceScale.Units.V, probeGain));
			base.SelectedScale = base[2.0];
		}
	}
}
