using System;

namespace Oscillite.CleanRoom.LSM
{
	
	public class TestLeadFrequencyTraceScaleList : TraceScaleList
	{
		public TestLeadFrequencyTraceScaleList(float probeGain, float probeOffset)
			: base(TraceProbeType.TRACE_PROBE_FREQUENCY, hasAutoScale: false)
		{
			Add(new TraceScale(5.0, TraceScale.Units.Hz, probeGain));
			Add(new TraceScale(10.0, TraceScale.Units.Hz, probeGain));
			Add(new TraceScale(50.0, TraceScale.Units.Hz, probeGain));
			Add(new TraceScale(100.0, TraceScale.Units.Hz, probeGain));
			Add(new TraceScale(250.0, TraceScale.Units.Hz, probeGain));
			Add(new TraceScale(500.0, TraceScale.Units.Hz, probeGain));
			Add(new TraceScale(1000.0, TraceScale.Units.Hz, probeGain));
			Add(new TraceScale(2000.0, TraceScale.Units.kHz, probeGain));
			Add(new TraceScale(5000.0, TraceScale.Units.kHz, probeGain));
			Add(new TraceScale(10000.0, TraceScale.Units.kHz, probeGain));
			Add(new TraceScale(20000.0, TraceScale.Units.kHz, probeGain));
			Add(new TraceScale(50000.0, TraceScale.Units.kHz, probeGain));
			base.SelectedScale = base[base.Count - 1];
		}
	}
}
