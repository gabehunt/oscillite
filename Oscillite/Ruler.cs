using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oscillite
{
    public class Ruler
    {
        public float Voltage { get; set; }
        public bool Active { get; set; } = true;
        public bool IsAtTop { get; set; } = false;
    }
}
