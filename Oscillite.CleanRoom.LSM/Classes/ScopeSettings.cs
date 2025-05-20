using System;


namespace Oscillite.CleanRoom.LSM
{
	
public class ScopeSettings
    {
        public ScopeType ScopeType { get; set; } = ScopeType.ScopeType_LabScope;
        public SweepList Sweeps { get; set; }
        public Sweep Sweep { get; set; }
        public TraceList TraceList { get; set; }
        public TraceProbeList Probes { get; set; }
        public TriggerList TriggerList { get; set; }
        public Trigger SelectedTriggers
        {
            get => TriggerList?.SelectedTrigger;
            set { if (TriggerList != null) TriggerList.SelectedTrigger = value; }
        }
        public CursorList Cursors { get; set; }
        public PressureUnits PressureUnits { get; set; } = PressureUnits.psi;
        public VacuumUnits VacuumUnits { get; set; } = VacuumUnits.inHg;
        public TemperatureUnits TemperatureUnits { get; set; } = TemperatureUnits.degF;
        public ScopeHardwareType ScopeHardwareType { get; set; } = ScopeHardwareType.ScopeHardwareType_Unknown;

        public ScopeSettings(ScopeType scopeType, TraceCount traceCount, SweepList sweeps)
        {
            ScopeType = scopeType;
            Sweeps = sweeps;
            Sweep = sweeps?.DefaultSweep;
            switch (scopeType)
            {
                case ScopeType.ScopeType_LabScope:
                    Probes = new LabScopeTraceProbeList();
                    TraceList = new TraceList(scopeType, traceCount, Probes);
                    TriggerList = new TriggerList(TraceList);
                    break;
                case ScopeType.ScopeType_GraphingMeter:
                    Probes = new GraphingMeterTraceProbeList();
                    TraceList = new TraceList(scopeType, traceCount, Probes);
                    TriggerList = new TriggerList(new Trigger());
                    break;
                case ScopeType.ScopeType_DigitalMeter:
                    Probes = new DvomTraceProbeList();
                    TraceList = new TraceList(scopeType, traceCount, Probes);
                    TriggerList = new TriggerList(new Trigger());
                    break;
            }
            Cursors = new CursorList();
        }
    }
}
