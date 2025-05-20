using System;
using System.Collections.Generic;
using System.Linq;

namespace Oscillite.CleanRoom.LSM
{
	
public class TraceProbeList : List<TraceProbe>
    {
        public TraceProbe this[TraceProbeType type] => this.FirstOrDefault(probe => probe.Type == type);
    }
}
