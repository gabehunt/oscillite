using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oscillite
{
    public class TimeRuler
    {
        public float Time { get; set; } // normalized 0–1 time position
        public bool Active { get; set; } = true;
    }
}
