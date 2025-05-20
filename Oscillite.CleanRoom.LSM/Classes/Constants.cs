using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oscillite.CleanRoom.LSM
{
    public static class Constants
    {
        // The first 4 bytes of all files tested; used to identify LSM files.
        public const string LSM_MAGIC = "MDLS";

        public const int LSM_HEADER_LENGTH = 3020;

        public const int NUMBER_OF_TRACES = 4;

        public const int NUMBER_OF_CHANNELS = 4;

        public const int NUMBER_OF_TRIGGERS = 6;

        public const int NUMBER_OF_CURSORS = 2;

        public const int DEFAULT_SWEEP_SETTING_COUNT = 24;

        public const int DEFAULT_UNIT_ADJUSTMENT_FACTOR = 1000;

        public const int MILLION_DIVISOR = 1000000;

        public const int ENCODING_NULL_BYTE_COUNT = 2;

        public const short EXTENDED_SWEEP_WIDTH = 600;

        public const short DEFAULT_SWEEP_WIDTH = 560;

        public const double SECONDS_PER_HOUR = 3600.0;

        public const double SECONDS_PER_MINUTE = 60.0;

        public const double SECONDS_PER_SECOND = 1.0;

        public const double SECONDS_PER_MILLISECOND = 0.001;

        public const double MILLISECONDS_PER_SECOND = 1000.0;

        public const double MICROSECONDS_PER_SECOND = 1000000.0;

    }
}
