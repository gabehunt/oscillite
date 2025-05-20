using System;

using Oscillite.CleanRoom.LSM.Properties;


namespace Oscillite.CleanRoom.LSM
{
	
public class TriggerSource
    {
        private const string NoneString = "None";
        private const string CylinderString = "Cylinder";

        public string Value { get; set; }
        public SourceType Type { get; set; }

        public TriggerSource(SourceType type)
        {
            switch (type)
            {
                case SourceType.Cylinder:
                    Type = type;
                    Value = CylinderString;
                    break;
                case SourceType.None:
                    Type = type;
                    Value = NoneString;
                    break;
            }
        }

        public TriggerSource(Trace trace)
        {
            Type = SourceType.Trace;
            Value = trace.Id.ToString();
        }

        public TriggerSource(TriggerSource source)
        {
            Type = source.Type;
            switch (source.Type)
            {
                case SourceType.Trace:
                    var traceId = Convert.ToInt32(source.Value.Split(' ')[1]);
                    // seems to require TraceId prefix to be valid with other lists
                    Value = $"TraceId: {traceId}";
                    break;
                case SourceType.Cylinder:
                    Value = CylinderString;
                    break;
                case SourceType.None:
                    Value = NoneString;
                    break;
            }
        }
    }
}
