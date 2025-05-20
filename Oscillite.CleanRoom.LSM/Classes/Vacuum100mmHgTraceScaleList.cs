using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class Vacuum100mmHgTraceScaleList : TraceScaleList
	{
		public Vacuum100mmHgTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_VACUUM_100, hasAutoScale: false)
		{
			Add(new TraceScale(100.0, TraceScale.Units.mmHg, probeGain));
			Add(new TraceScale(200.0, TraceScale.Units.mmHg, probeGain));
			Add(new TraceScale(500.0, TraceScale.Units.mmHg, probeGain));
			base.SelectedScale = base[Count - 1];
		}
	}
}
