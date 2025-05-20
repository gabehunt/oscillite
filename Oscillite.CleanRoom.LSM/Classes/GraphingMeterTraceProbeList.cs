using System;
using System.Collections.Generic;

namespace Oscillite.CleanRoom.LSM
{
	
	public class GraphingMeterTraceProbeList : TraceProbeList
	{
		public GraphingMeterTraceProbeList(PressureUnits pressureUnits = PressureUnits.psi, VacuumUnits vacuumUnits = VacuumUnits.inHg, TemperatureUnits temperatureUnits = TemperatureUnits.degF)
		{
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_VOLTS));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_MIXTURE_CONTROL_DWELL_60));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_MIXTURE_CONTROL_DWELL_90));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_INJECTOR_PULSE_WIDTH));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_PULSE_WIDTH));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_DUTY_CYCLE));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_FREQUENCY));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_VOLTS_AC));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_VOLTS_DC));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_VACUUM_100, vacuumUnits));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_PRESSURE_100, pressureUnits));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_PRESSURE_500, pressureUnits));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_PRESSURE_5000, pressureUnits));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_LOW_AMPS_20));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_LOW_AMPS_40));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_LOW_AMPS_60));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_HIGH_AMPS_200));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_HIGH_AMPS_2000));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_OHMS));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_EEDM506D_TEMPERATURE, temperatureUnits));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_MT5030_VACUUM, vacuumUnits));
			Add(new TraceProbe(TraceProbeType.TRACE_PROBE_MT5030_PRESSURE, pressureUnits));
            using (Enumerator enumerator = GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.TraceScaleList.HasAutoScale = false;
                }
            }
		}
	}
}
