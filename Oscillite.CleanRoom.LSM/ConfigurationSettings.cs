using System;


namespace Oscillite.CleanRoom.LSM
{
	
public class ConfigurationSettings
    {
        public Position<int> CurrentBufferPosition { get; set; }
        public Position<int> FrameSize { get; set; }
        public ScopeSettings ScopeSettings { get; set; }
        public WaveformBufferList WaveformData { get; set; }
    }
}
