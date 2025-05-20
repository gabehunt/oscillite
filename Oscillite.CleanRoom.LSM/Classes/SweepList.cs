using System.Collections.Generic;
using System.Linq;

namespace Oscillite.CleanRoom.LSM
{
	
public class SweepList : List<Sweep>
    {
        public Sweep DefaultSweep { get; set; }
        public Sweep this[double seconds] => this.FirstOrDefault(s => s.Seconds == seconds);
        public Sweep this[SweepType type] => this.FirstOrDefault(s => s.Type == type);
    }
}
