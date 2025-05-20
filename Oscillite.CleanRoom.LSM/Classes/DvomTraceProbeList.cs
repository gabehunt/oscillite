using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class DvomTraceProbeList : TraceProbeList
	{
		public DvomTraceProbeList()
		{
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_VOLTS));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_VOLTS_DC));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_VOLTS_AC));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_OHMS));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_DIODE));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_CAPACITANCE));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_LOW_AMPS_20));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_LOW_AMPS_40));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_LOW_AMPS_60));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_HIGH_AMPS_200));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_HIGH_AMPS_2000));
		}
	}
}
