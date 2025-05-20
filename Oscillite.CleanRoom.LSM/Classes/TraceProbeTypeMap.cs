using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oscillite.CleanRoom.LSM
{
    public static class TraceProbeTypeMap
    {
        /// <summary>
        /// Known good trace probe values used in LSM binary trace headers.
        /// Discovered by analyzing real-world files.
        /// May be missing some values not yet discovered.
        /// </summary>
        public static Dictionary<int, TraceProbeType> GetDefaultTraceProbes()
        {
            return new Dictionary<int, TraceProbeType>
            {
                { 0, TraceProbeType.TRACE_PROBE_LOW_AMPS_20 },
                { 1, TraceProbeType.TRACE_PROBE_LOW_AMPS_40 },
                { 2, TraceProbeType.TRACE_PROBE_VACUUM_100 },
                { 3, TraceProbeType.TRACE_PROBE_PRESSURE_100 },
                { 4, TraceProbeType.TRACE_PROBE_PRESSURE_500 },
                { 5, TraceProbeType.TRACE_PROBE_PRESSURE_5000 },
                { 6, TraceProbeType.TRACE_PROBE_IGNITION },
                { 7, TraceProbeType.TRACE_PROBE_VOLTS },
                { 11, TraceProbeType.TRACE_PROBE_MIXTURE_CONTROL_DWELL_60 },
                { 12, TraceProbeType.TRACE_PROBE_MIXTURE_CONTROL_DWELL_90 },
                { 13, TraceProbeType.TRACE_PROBE_INJECTOR_PULSE_WIDTH },
                { 14, TraceProbeType.TRACE_PROBE_PULSE_WIDTH },
                { 15, TraceProbeType.TRACE_PROBE_DUTY_CYCLE },
                { 16, TraceProbeType.TRACE_PROBE_FREQUENCY },
                { 18, TraceProbeType.TRACE_PROBE_OHMS },
                { 20, TraceProbeType.TRACE_PROBE_SHUNT },
                { 21, TraceProbeType.TRACE_PROBE_VOLTS_DC },
                { 22, TraceProbeType.TRACE_PROBE_VOLTS_AC },
                { 23, TraceProbeType.TRACE_PROBE_LOW_AMPS_60 },
                { 24, TraceProbeType.TRACE_PROBE_DIODE },
                { 25, TraceProbeType.TRACE_PROBE_CAPACITANCE },
                { 26, TraceProbeType.TRACE_PROBE_EEDM506D_TEMPERATURE },
                { 27, TraceProbeType.TRACE_PROBE_MT5030_PRESSURE },
                { 28, TraceProbeType.TRACE_PROBE_MT5030_VACUUM },
                { 29, TraceProbeType.TRACE_PROBE_HIGH_AMPS_200 },
                { 30, TraceProbeType.TRACE_PROBE_HIGH_AMPS_2000 }
            };
        }
    }
}
